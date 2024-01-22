using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    // EverythingEverywhereAndAtOnce
    public class GameLifetimeScope : LifetimeScope
    {
        private MessagePipeOptions _commonOptionsOfMessagePipe;
        private AnimationCurveData _cubeAnimationParameters;
        private CubeAssetData _cubePrefabs;
        public MessagePipeOptions CommonOptionsOfMessagePipe => _commonOptionsOfMessagePipe;        
        public AnimationCurveData CubeAnimationParameters => _cubeAnimationParameters;
        public CubeAssetData CubePrefabs => _cubePrefabs;

        protected override async void Configure(IContainerBuilder builder)
        {
            Debug.Log("Async awaiters started!");
            var cubeAnimationParametersTask = LoadAsyncAsAnimationCurveData("CubeAnimationParameters");
            var cubePrefabsTask = LoadAssetDataAsync<CubeAssetData>("CubePrefabsAsset");

            Debug.Log("Main Registrations started!");
            builder.Register<HelloWorldService>(Lifetime.Singleton);

            // Значения СommonOptionsOfMessagePipe и внедренного MessagePipeOptions объекта будут одинаковыми,
            // так как MessagePipeOptions внедряется как Singleton, но для использования нужен класс-фабрика.
            // См. AdditiveLifetimeScope
            _commonOptionsOfMessagePipe = builder.RegisterMessagePipe(c => {
                // Определяем, что все шины будут создаваться для каждой LifetimeScope свои и следовательно работать
                // следующим образом:
                // События одной шины одной области видимости будут ходить внутри области видимости.
                // Для формирования "общения" между областями видимости необходимо создавать InstanceLifetime.Singleton шины.
                c.InstanceLifetime = InstanceLifetime.Scoped;
                // c.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
            });

            // Общие шины лучше делать Singleton всегда, а для дополнений лучше делать Scoped
            var singletonOptionsOfMessagePipe = new MessagePipeOptions()
            {
                InstanceLifetime = InstanceLifetime.Singleton
            };

            // Setup GlobalMessagePipe to enable diagnostics window and global function
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            // RegisterMessageBroker: Register for IPublisher<T>/ISubscriber<T>, includes async and buffered.
            // Интерфейсы IAsyncPublisher<T>/IAsyncSubscriber<T> позволяют обрабатывать асинхронные операции последовательно и параллельно,
            // в зависимости от указанной стратегии публикации. В основном нужны для организации сетевого обмена по событиям,
            // но можно применять и для загрузки большого числа ресурсов разными обработчиками подписчика, для загрузки асинхронно сцен.
            // Издатель и подписчик, зарегистрированные в корневой области видимости в виде синглтонов, для дополнений сцен с дочерними областями видимости
            // будут глобальными и будут рассылать и обрабатывать события для всех обработчиков во всех сценах.
            // Так можно организовать "общение" между сценами и уровнями.
            // Если сделать всех подписчиков и издателей "Scoped", то потеряется связь от дополнений сцены до основной сцены, потому что подписчик не знает,
            // что появился ещё один издатель, так как издатель будет жить в своей шине своей области видимости.

            // Если закомментировать следующую строку и раскомментировать ниже похожую в Enqueue, но Scoped (CommonOptionsOfMessagePipe) и в AdditiveSceneLoader
            // такую же строку как в Enqueue раскомментировать, то можно будет увидеть, что теперь каждая сцена общается независимо такими событиями.
            builder.RegisterMessageBroker<int>(singletonOptionsOfMessagePipe);
            builder.RegisterMessageBroker<LoadAdditiveSceneIntention>(singletonOptionsOfMessagePipe);
            builder.RegisterMessageBroker<AdditiveSceneLoadedEvent>(singletonOptionsOfMessagePipe);
            builder.RegisterMessageBroker<AdditiveSceneUnloadedEvent>(singletonOptionsOfMessagePipe);
            builder.Register<AdditiveSceneLoader>(Lifetime.Singleton).AsImplementedInterfaces();

            // Регистрируем компоненту для создания GameObject с указанным именем и с одной регистрируемой компонентой.
            // Это хорошо подходит для всяких моно-синглтонов, в сцене и дополнениях к сцене.
            // Регистрируем в иерархии текущей области видимости, чтобы при выгрузки дополнения
            // все зависимые элементы были очищены и не остались в основной сцене.
            // При использовании Lifetime.Scoped для каждого LifetimeScope будет создан объект, что хорошо видно,
            // если у MainUiViewController и MainScenePresenter установить Lifetime.Scoped.
            // Если Lifetime.Scoped определения потом резолвит только один синглтон, то не будет создано много экземпляров,
            // но лучше их правильно синхронизировать по областям видимости и не смешивать. Смешение показано для примера.
            builder.RegisterComponentOnNewGameObject<MainUiViewController>(Lifetime.Scoped, "UIViewController").UnderTransform(this.transform).AsImplementedInterfaces();
            // Лучше не использовать одновременно и классовую и интерфейсную маркировку, чтобы не путать себя и других. В основном ссылаются на маркировочные интерфейсы,
            // которые по своей природе должны быть уникальными для каждой MVP(MVC, MVI) группы, например. И нет привязки для тестов к конкретной реализации.
            //X.As<MainUiViewController>();
            builder.Register<MainScenePresenter>(Lifetime.Singleton).AsImplementedInterfaces();

            Debug.Log("Main Registrations ended!");

            // При использовании асинхронных операций ресурсы должны быть получены перед самой дополнительной регистрацией.
            // Если сразу их попробовать получить, то будет неправильная последовательность регистрации и ничего не сработает.
            // В целом, если придерживаться принципа, что все "await" операции выполняются в конце Configure после всех остальных регистраций,
            // то будет работать стабильно. Самый правильный способ, использовать IAsyncStartable https://vcontainer.hadashikick.jp/integrations/unitask
            // А всё потому, что Configure не является async изначально и не обрабатывает ожидание

            this._cubeAnimationParameters = (await cubeAnimationParametersTask);
            this._cubePrefabs = (await cubePrefabsTask);
            Debug.Log("Async awaiters stoped?!");
                        
            using (Enqueue(innerBuilder =>
            {
                // В двух местах с локальной областью видимости регистрируются издатель и подписчик для "int" события.
                // Запуск цепочки будет реагировать только на определенные кубики, которые были созданы в рамках очередной загруженной сцены.
                // См. AdditiveSceneLoader
                // Если закомментировать эти регистрации и раскомментировать общую одну регистрацию, то можно будет увидеть при нескольких сценах,
                // что выгрузка очередной сцены влият на остальные кубики.
                // innerBuilder.RegisterMessageBroker<int>(CommonOptionsOfMessagePipe);
                innerBuilder.RegisterInstance(CubeAnimationParameters);
                innerBuilder.RegisterInstance(CubePrefabs);
            }))
            {
                await SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive).WithCancellation(this.GetCancellationTokenOnDestroy());
            }
        }

        async UniTask<AnimationCurveData> LoadAsyncAsAnimationCurveData(string path)
        {
            var resource = await Resources.LoadAsync<AnimationCurveData>(path).WithCancellation(this.GetCancellationTokenOnDestroy());
            return (resource as AnimationCurveData);
        }

        
        // Надо проверять на IL2CPP сборке будет ли всё это работать без "бубнов"
        async UniTask<T> LoadAssetDataAsync<T>(string path) where T : UnityEngine.Object
        {
            var resource = await Resources.LoadAsync<T>(path).WithCancellation(this.GetCancellationTokenOnDestroy());
            return (resource as T) ?? throw new System.InvalidOperationException(String.Format("Asset with '{0}' not found", path));
        }        

    }

}
