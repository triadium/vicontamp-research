using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    public class AdditiveScenePresenter: IPostStartable, IDisposable
    {
        public readonly string sceneName;

        readonly IAdditiveUiViewController uiViewController;
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;

        public AdditiveScenePresenter(string sceneName, IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent, IAdditiveUiViewController uiViewController)
        {
            this.sceneName = sceneName;
            this.publisherOfAdditiveSceneLoadedEvent = publisherOfAdditiveSceneLoadedEvent;
            this.uiViewController = uiViewController;
        }

        void IDisposable.Dispose()
        {
            Debug.Log(String.Format("Additive scene '{0}' controller disposed!", this.sceneName));
        }

        void IPostStartable.PostStart()
        {
            uiViewController.SetSpecText(this.sceneName);
            publisherOfAdditiveSceneLoadedEvent.Publish(new AdditiveSceneLoadedEvent(this.sceneName));
            Debug.Log(String.Format("Additive scene '{0}' loaded!", this.sceneName));
        }
    }
}
