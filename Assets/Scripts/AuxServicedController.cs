using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{   
    // Для тестирования ITickable и шины с топиками.
    public class AuxServicedController : ITickable, IStartable, IDisposable
    {
        readonly HelloWorldService helloWorldService;
        readonly IPublisher<int> publisher;
        readonly ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriber;        
        IDisposable disposable;

        public AuxServicedController(HelloWorldService helloWorldService, IPublisher<int> publisher, ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriber)
        {            
            this.helloWorldService = helloWorldService;
            this.publisher = publisher;
            this.subscriber = subscriber;
        }

        void OnAnimationSwitchEvent(AnimationSwitchEvent e)
        {
            if (e.isOn)
            {
                publisher.Publish(0x0A1);
            }
            else
            {
                publisher.Publish(0x0FF);
            }            
        }

        void IStartable.Start()
        {
            var d = DisposableBag.CreateBuilder();
            // Подписываемся на шину с топиком, равному AnimationSwitchEventSubscriber.Second
            subscriber.Subscribe(AnimationSwitchEventSubscriber.Second, OnAnimationSwitchEvent).AddTo(d);
            disposable = d.Build();            
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
            Debug.Log("AuxServicedController Disposed!");
        }

        void ITickable.Tick()
        {
            helloWorldService.Noop();          
        }        
                
    }
}
