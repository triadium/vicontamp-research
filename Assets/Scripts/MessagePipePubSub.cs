using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    // Для тестов шин с топиками.    
    public class MessagePipePubSub : ITickable, IPostStartable
    {        
        readonly IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisher;
        public MessagePipePubSub(IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisher)
        {                        
            this.publisher = publisher;        
        }

        void IPostStartable.PostStart()
        {
            // Отправляем сообщение через шину с топиком AnimationSwitchEventSubscriber.First
            publisher.Publish(AnimationSwitchEventSubscriber.First, new AnimationSwitchEvent(true));
        }

        void ITickable.Tick()
        {
            Debug.Log("No chance?!");
        }     
    }
}