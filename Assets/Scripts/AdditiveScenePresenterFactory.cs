using MessagePipe;
using System;
using UnityEngine;
using VContainer;

namespace MyGame
{
    public class AdditiveScenePresenterFactory
    {
        readonly IAdditiveUiViewController uiViewController;
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;
        
        public AdditiveScenePresenterFactory(IAdditiveUiViewController uiViewController, IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent)
        {
            this.uiViewController = uiViewController;
            this.publisherOfAdditiveSceneLoadedEvent = publisherOfAdditiveSceneLoadedEvent;
        }

        public AdditiveScenePresenter Create(string sceneName) {
            Debug.Log(String.Format("Additive scene '{0}' intent to load!", sceneName));            
            return new AdditiveScenePresenter(sceneName, publisherOfAdditiveSceneLoadedEvent, uiViewController);
        }
    }
}
