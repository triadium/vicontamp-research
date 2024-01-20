using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{   
    public class GameController : ITickable, IStartable, IDisposable
    {
        readonly HelloWorldService helloWorldService;
        readonly IPublisher<int> publisher;
        readonly ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriber;        
        IDisposable disposable;

        public GameController(HelloWorldService helloWorldService, IPublisher<int> publisher, ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriber)
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
            subscriber.Subscribe(AnimationSwitchEventSubscriber.Second, OnAnimationSwitchEvent).AddTo(d);
            disposable = d.Build();            
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
            Debug.Log("GameController Disposed!");
        }

        void ITickable.Tick()
        {
            helloWorldService.Noop();          
        }        
                
    }
}
