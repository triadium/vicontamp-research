using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public class AdditiveSceneLoader: IStartable, IDisposable
    {
        readonly GameLifetimeScope scope;
        readonly IAsyncSubscriber<LoadAdditiveSceneIntention> subscriberOfLoadAdditiveSceneIntention;        
        IDisposable disposable;
        
        public AdditiveSceneLoader(GameLifetimeScope scope, IAsyncSubscriber<LoadAdditiveSceneIntention> subscriberOfLoadAdditiveSceneIntention)
        {
            this.scope = scope;
            this.subscriberOfLoadAdditiveSceneIntention = subscriberOfLoadAdditiveSceneIntention;            
        }

        async UniTask OnLoadAdditiveSceneIntention(LoadAdditiveSceneIntention e, CancellationToken cancellationToken)
        {
            using (LifetimeScope.Enqueue(innerBuilder =>
            {
                innerBuilder.RegisterInstance(scope.CubeAnimationParameters);
                innerBuilder.RegisterInstance(scope.CubePrefabs);
            }))
            {
                await SceneManager.LoadSceneAsync(e.name, LoadSceneMode.Additive).WithCancellation(cancellationToken);
            }

            await UniTask.Delay(3000, DelayType.Realtime, PlayerLoopTiming.Update, cancellationToken, true).SuppressCancellationThrow();
        }

        void IStartable.Start()
        {
            var d = DisposableBag.CreateBuilder();
            subscriberOfLoadAdditiveSceneIntention.Subscribe(OnLoadAdditiveSceneIntention).AddTo(d);
            disposable = d.Build();
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
            Debug.Log( String.Format("{0} Disposed!", this.GetType().Name));
        }
    }
}
