using MessagePipe;
using System;
using UnityEngine;

namespace MyGame
{
    public class AdditiveSceneControllerFactory
    {
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;

        public AdditiveSceneControllerFactory(IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent)
        {
            this.publisherOfAdditiveSceneLoadedEvent = publisherOfAdditiveSceneLoadedEvent;
        }

        public AdditiveSceneController Create(string sceneName) {
            Debug.Log(String.Format("Additive scene '{0}' intent to load!", sceneName));
            return new AdditiveSceneController(sceneName, publisherOfAdditiveSceneLoadedEvent);
        }
    }
}