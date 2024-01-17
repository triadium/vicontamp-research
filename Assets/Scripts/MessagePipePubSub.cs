using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    public class MessagePipePubSub : ITickable, IPostStartable
    {        
        readonly IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisher;
        public MessagePipePubSub(IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisher)
        {                        
            this.publisher = publisher;        
        }

        void IPostStartable.PostStart()
        {
            publisher.Publish(AnimationSwitchEventSubscriber.First, new AnimationSwitchEvent(true));
        }

        void ITickable.Tick()
        {
            Debug.Log("No chance?!");
        }     
    }
}