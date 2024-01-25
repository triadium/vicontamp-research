using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    // Используеся Instantiate интерфейса контейнера (IObjectResolver)
    // и это позволяет внедрять зависимости из областей видимости в новый объект Unity.
    // Если на объекте нет компонент с внедряемыми зависимостями, то лучше использовать "родной" Unity метод.
    public class CubeCreator: IStartable, IDisposable
    {
        readonly IObjectResolver container;
        readonly ISubscriber<CreateCubeIntention> subscriber;
        readonly CubeAssetData data;
        IDisposable disposable;

        public CubeCreator(IObjectResolver container, ISubscriber<CreateCubeIntention> subscriber, CubeAssetData data)
        {
            this.container = container;
            this.subscriber = subscriber;
            this.data = data;
        }

        void OnCreateCubeIntention(CreateCubeIntention e)
        {
            var prefab = data?.cube ?? throw new NullReferenceException("Cube prefab not found!");
            var go = container.Instantiate(prefab, new Vector3(e.x, e.y, prefab.transform.position.z), Quaternion.identity);
            var controller = go.GetComponent<CurvedAnimationController>();
            controller.TurnAnimation(e.on);
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