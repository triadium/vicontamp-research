using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{
    public class AdditiveSceneController: IPostStartable, IDisposable
    {
        public readonly string sceneName;

        readonly IUiViewController uiViewController;
        readonly IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent;

        public AdditiveSceneController(string sceneName, IPublisher<AdditiveSceneLoadedEvent> publisherOfAdditiveSceneLoadedEvent, IUiViewController uiViewController)
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
            uiViewController.SetText(this.sceneName);
            publisherOfAdditiveSceneLoadedEvent.Publish(new AdditiveSceneLoadedEvent(this.sceneName));
            Debug.Log(String.Format("Additive scene '{0}' loaded!", this.sceneName));
        }
    }
}
