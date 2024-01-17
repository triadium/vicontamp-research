using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{  
    public class GameLifetimeScope : LifetimeScope
    {
        public MessagePipeOptions messagePipeOptions;

        protected override async void Configure(IContainerBuilder builder)
        {
            Debug.Log("Async awaiters started!");
            var animationCurveDataTask = LoadAsyncAsAnimationCurveData("CubeAnimationParameters");
            var cubePrefabsTask = LoadAssetDataAsync<CubeAssetData>("CubePrefabsAsset");

            Debug.Log("Registrations!");
            builder.Register<HelloWorldService>(Lifetime.Singleton);

            messagePipeOptions = builder.RegisterMessagePipe(c => {
                c.InstanceLifetime = InstanceLifetime.Scoped;
                // c.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
            });
            // Setup GlobalMessagePipe to enable diagnostics window and global function
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            // RegisterMessageBroker: Register for IPublisher<T>/ISubscriber<T>, includes async and buffered.
            // Интерфейсы IAsyncPublisher<T>/IAsyncSubscriber<T> позволяют обрабатывать асинхронные операции последовательно и параллельно,
            // в зависимости от указанной стратегии публикации. В основном нужны для организации сетевого обмена по событиям,
            // но можно применять и для загрузки большого числа ресурсов разными обработчиками подписчика.
            builder.RegisterMessageBroker<int>(messagePipeOptions);
            builder.RegisterMessageBroker<AdditiveSceneLoadedEvent>(messagePipeOptions);

            // Регистрируем класс с методом-фабрикой для того, чтобы получить автоматически зависимости в конструкторе
            builder.Register<AdditiveSceneControllerFactory>(Lifetime.Singleton);
            // Регистрируем метод-фабрику
            builder.RegisterFactory<string, AdditiveSceneController>(container => container.Resolve<AdditiveSceneControllerFactory>().Create, Lifetime.Singleton);
            
            // При использовании асинхронных операций ресурсы должны быть получены перед самой дополнительной регистрацией.
            // Если сразу их попробовать получить, то будет неправильная последовательность регистрации и ничего не сработает.
            // В целом, если придерживаться принципа, что все "await" операции выполняются в конце Configure после всех остальных регистраций,
            // то будет работать стабильно. Самый правильный способ, использовать IAsyncStartable https://vcontainer.hadashikick.jp/integrations/unitask
            // А всё потому, что Configure не является async изначально и не обрабатывает ожидание
            var cubeAnimationParameters = (await animationCurveDataTask);
            var cubePrefabs = (await cubePrefabsTask);
            Debug.Log("Async awaiters stoped?!");
            
            using (Enqueue(innerBuilder =>
            {
                innerBuilder.RegisterInstance(cubeAnimationParameters);
                innerBuilder.RegisterInstance(cubePrefabs);
                
                var factoryOfAdditiveSceneController = this.Container.Resolve<Func<string, AdditiveSceneController>>();
                innerBuilder.RegisterInstance(factoryOfAdditiveSceneController("SampleScene")).AsImplementedInterfaces();
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