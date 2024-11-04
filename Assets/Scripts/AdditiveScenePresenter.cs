using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;

namespace MyGame
{

    // Кусочек из MVP архитектуры:
    // Presenter не должен знать напрямую класс представления, а должен только использовать выделенный из представления контракт.
    // Ничего из Unity для обработки представления не используем. Всё через контрактный интерфейс с представлением.
    // Интерфейс I(X)ViewController называется View Controller, а не просто View, потому что Unity компоненты являются контроллерами, 
    // а не представлением. View Controller и по совмемстительству Mono Behaviour обращается к GameObject, которые фактически и являются View.
    // View фактически "тупые объекты", которые не имеют логики и в большинстве для "гибкости" системы не должны иметь, потому что чем больше
    // логики во View, тем сложнее такую систему адаптировать ("подгонять") под разные случаи.
    // Отличие этого Presenter класса в том, что он имеет в конструкторе параметры, которые нужно устанавливать извне во время создания.
    // Такое может сделать только код, который вне дополнительной сцены находится и знает какие параметры передать, но сам Prsenter инстанс
    // должен жить в пределах области видимости дополнения сцены и поэтому есть такой механизм, как предварительное дополнительное построение контейнера
    // и внутри этого построения используется фабрика. См. AdditiveScenePresenterFactory
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
            // Вот проблема - придётся этот код писать, если не сделать "слабой" ссылку на uiViewController.
            // Разрываем цикличные ссылки, чтобы не было "утечки" и чтобы GC спокойно собрал "мусор".
            // Если ссылку на uiViewController сделать слабой, то любой вызов метода интерфейса превратиться 
            // в более длинную цепочку TryGetTarget -> Use local as target
            uiViewController.Presenter = null;
            Debug.Log(String.Format("Additive scene '{0}' presenter disposed!", this.sceneName));
        }

        void IPostStartable.PostStart()
        {
            // Подвох такого подхода в том, что мы нигде в объектах не должны использовать IPostStartable интерфейс 
            // на зависимых объектах от этого события и от состояния Presenter инстанса.
            // Но по IPostStartable можно легко отследить какие инстансы будут обрабатывать этот этап.
            // uiViewController.SetSpecText(this.sceneName);
            uiViewController.SetSpecText(Application.consoleLogPath);
            publisherOfAdditiveSceneLoadedEvent.Publish(new AdditiveSceneLoadedEvent(this.sceneName));
            Debug.Log(String.Format("Additive scene '{0}' loaded!", this.sceneName));
        }

        public void SayHello() {
            Debug.Log(String.Format("Hello! I am Presenter of additive scene '{0}' !", this.sceneName));
        }
    }
}
