using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    public class AdditiveSceneController: IPostStartable
    {        
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;
        readonly string sceneName;

        public AdditiveSceneController(string sceneName, IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent)
        {
            this.sceneName = sceneName;
            this.publisherOfAdditiveSceneLoadedEvent = publisherOfAdditiveSceneLoadedEvent;
        }

        void IPostStartable.PostStart()
        {
            publisherOfAdditiveSceneLoadedEvent.Publish(new AdditiveSceneLoadedEvent(this.sceneName));
            Debug.Log(String.Format("Additive scene '{0}' loaded!", this.sceneName));
        }
    }
}