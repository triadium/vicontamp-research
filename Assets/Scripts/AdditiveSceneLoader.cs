using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    public class AdditiveSceneLoader: IStartable, IDisposable
    {
        readonly IObjectResolver container;
        readonly ISubscriber<CreateCubeIntention> subscriber;
        readonly CubeAssetData data;
        IDisposable disposable;

        public AdditiveSceneLoader(IObjectResolver container, ISubscriber<CreateCubeIntention> subscriber, CubeAssetData data)
        {
            this.container = container;
            this.subscriber = subscriber;
            this.data = data;
        }

        void OnCreateCubeIntention(CreateCubeIntention e)
        {
            var prefab = data?.cube ?? throw new NullReferenceException("Cube prefab not found!");
            container.Instantiate(prefab, new Vector3(e.x, e.y, prefab.transform.position.z), Quaternion.identity);
        }

        void IStartable.Start()
        {
            var d = DisposableBag.CreateBuilder();
            subscriber.Subscribe(OnCreateCubeIntention).AddTo(d);
            disposable = d.Build();
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
            Debug.Log( String.Format("{0} Disposed!", this.GetType().Name));
        }

    }
}