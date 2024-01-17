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
            // ���������� IAsyncPublisher<T>/IAsyncSubscriber<T> ��������� ������������ ����������� �������� ��������������� � �����������,
            // � ����������� �� ��������� ��������� ����������. � �������� ����� ��� ����������� �������� ������ �� ��������,
            // �� ����� ��������� � ��� �������� �������� ����� �������� ������� ������������� ����������.
            builder.RegisterMessageBroker<int>(messagePipeOptions);
            builder.RegisterMessageBroker<AdditiveSceneLoadedEvent>(messagePipeOptions);

            // ������������ ����� � �������-�������� ��� ����, ����� �������� ������������� ����������� � ������������
            builder.Register<AdditiveSceneControllerFactory>(Lifetime.Singleton);
            // ������������ �����-�������
            builder.RegisterFactory<string, AdditiveSceneController>(container => container.Resolve<AdditiveSceneControllerFactory>().Create, Lifetime.Singleton);
            
            // ��� ������������� ����������� �������� ������� ������ ���� �������� ����� ����� �������������� ������������.
            // ���� ����� �� ����������� ��������, �� ����� ������������ ������������������ ����������� � ������ �� ���������.
            // � �����, ���� �������������� ��������, ��� ��� "await" �������� ����������� � ����� Configure ����� ���� ��������� �����������,
            // �� ����� �������� ���������. ����� ���������� ������, ������������ IAsyncStartable https://vcontainer.hadashikick.jp/integrations/unitask
            // � �� ������, ��� Configure �� �������� async ���������� � �� ������������ ��������
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

        
        // ���� ��������� �� IL2CPP ������ ����� �� �� ��� �������� ��� "������"
        async UniTask<T> LoadAssetDataAsync<T>(string path) where T : UnityEngine.Object
        {
            var resource = await Resources.LoadAsync<T>(path).WithCancellation(this.GetCancellationTokenOnDestroy());
            return (resource as T) ?? throw new System.InvalidOperationException(String.Format("Asset with '{0}' not found", path));
        }        

    }

}