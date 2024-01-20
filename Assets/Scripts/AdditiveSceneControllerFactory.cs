using MessagePipe;
using System;
using UnityEngine;
using VContainer;

namespace MyGame
{
    public class AdditiveSceneControllerFactory
    {
        readonly IUiViewController uiViewController;
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;
        
        public AdditiveSceneControllerFactory(IUiViewController uiViewController, IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent)
        {
            this.uiViewController = uiViewController;
            this.publisherOfAdditiveSceneLoadedEvent = publisherOfAdditiveSceneLoadedEvent;
        }

        public AdditiveSceneController Create(string sceneName) {
            Debug.Log(String.Format("Additive scene '{0}' intent to load!", sceneName));            
            return new AdditiveSceneController(sceneName, publisherOfAdditiveSceneLoadedEvent, uiViewController);
        }
    }
}
