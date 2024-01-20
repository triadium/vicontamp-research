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
        private MessagePipeOptions _optionsOfMessagePipe;
        private AnimationCurveData _cubeAnimationParameters;
        private CubeAssetData _cubePrefabs;
        public MessagePipeOptions OptionsOfMessagePipe => _optionsOfMessagePipe;        
        public AnimationCurveData CubeAnimationParameters => _cubeAnimationParameters;
        public CubeAssetData CubePrefabs => _cubePrefabs;

        protected override async void Configure(IContainerBuilder builder)
        {
            Debug.Log("Async awaiters started!");
            var cubeAnimationParametersTask = LoadAsyncAsAnimationCurveData("CubeAnimationParameters");
            var cubePrefabsTask = LoadAssetDataAsync<CubeAssetData>("CubePrefabsAsset");

            Debug.Log("Registrations!");
            builder.Register<HelloWorldService>(Lifetime.Singleton);

            _optionsOfMessagePipe = builder.RegisterMessagePipe(c => {
                c.InstanceLifetime = InstanceLifetime.Scoped;
                // c.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
            });
            // Setup GlobalMessagePipe to enable diagnostics window and global function
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            // RegisterMessageBroker: Register for IPublisher<T>/ISubscriber<T>, includes async and buffered.
            // Интерфейсы IAsyncPublisher<T>/IAsyncSubscriber<T> позволяют обрабатывать асинхронные операции последовательно и параллельно,
            // в зависимости от указанной стратегии публикации. В основном нужны для организации сетевого обмена по событиям,
            // но можно применять и для загрузки большого числа ресурсов разными обработчиками подписчика.
            builder.RegisterMessageBroker<int>(OptionsOfMessagePipe);
            builder.RegisterMessageBroker<LoadAdditiveSceneIntention>(OptionsOfMessagePipe);
            builder.RegisterMessageBroker<AdditiveSceneLoadedEvent>(OptionsOfMessagePipe);
            builder.RegisterMessageBroker<AdditiveSceneUnloadedEvent>(OptionsOfMessagePipe);
            builder.Register<AdditiveSceneLoader>(Lifetime.Singleton).AsImplementedInterfaces();

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
