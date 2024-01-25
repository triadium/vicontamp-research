using MessagePipe;
using System;
using UnityEngine;
using VContainer;

namespace MyGame
{
    // Класс-фабрика нужен лишь для маркированного получения зависимостей без лишнего кода резолвингов
    // и дальнейшей передачи этих зависимостей в конструктор создаваемого инстанса
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
            var instance = new AdditiveScenePresenter(sceneName, publisherOfAdditiveSceneLoadedEvent, uiViewController);            
            uiViewController.Presenter = instance;
            return instance;
        }
    }
}
