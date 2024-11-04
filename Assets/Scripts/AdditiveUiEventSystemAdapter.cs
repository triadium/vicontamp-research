using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using VContainer;
using MessagePipe;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;


namespace MyGame
{
    // Адаптеры UI Event System обычно определяются отдельно от UI View Controller,
    // потому что не хочется смешивать глобальные намерения с локальными действиями.
    // В то же время для однозначного намерения делать цепочку - 
    // UI View Controller -> Presenter -> Сообщение - тоже не очень хочется и прибегают 
    // к такому подходу. Им нельзя злоупотреблять, потому что очень легко скатиться к отправке
    // сообщений по каждому "чиху" из интерфейса по глобальным шинам и обработку всего через события.
    // Фактически, 
    // UI Event System передаёт события и намерения, которые предназначены 
    // не только текущим представлениям, которыми управляет UI View Controller,
    // но и другим подсистемам. 
    // UI View Controller не должен отправлять события и намерения, а передавать локальные события
    // инстансу Presenter через его интерфейс. Например, что-то там выбрано в списке и нужно как-то на это локально
    // отреагировать, но это никак должно не затрагивает другие части системы. А если надо, то это забота Presenter.    
    public class AdditiveUiEventSystemAdapter : MonoBehaviour
    {
        [Inject]
        readonly IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisherOfAnimationSwitchEvent;
        [Inject]
        readonly IPublisher<CreateCubeIntention> publisherOfCreateCubeIntention;
        [Inject]
        readonly IPublisher<AdditiveSceneUnloadedEvent> publisherOfAdditiveSceneUnloadedEvent;
        [Inject]
        readonly AdditiveScenePresenter additiveScenePresenter;

        public SimpleController simpleController;

        IEnumerator Start()
        {
            object obj = simpleController;
            Debug.LogWarningFormat("1) SimpleController ref is NULL: {0}", obj == null);
            Debug.LogWarningFormat("1) SimpleController is NULL: {0}", Globals.IsNull(simpleController));
            if (obj != null) { simpleController.SwitchAnimation(1); }
            Destroy(simpleController);
            obj = simpleController;
            Debug.LogWarningFormat("2) SimpleController ref is NULL: {0}", obj == null);
            Debug.LogWarningFormat("2) SimpleController is NULL: {0}", Globals.IsNull(simpleController));
            if (obj != null) { simpleController.SwitchAnimation(2); }
            yield return new WaitForEndOfFrame();
            obj = simpleController;
            Debug.LogWarningFormat("3) SimpleController ref is NULL: {0}", obj == null);
            Debug.LogWarningFormat("3) SimpleController is NULL: {0}", Globals.IsNull(simpleController));
            if (obj != null) { simpleController.SwitchAnimation(3); }

            // GC let's do it!
            simpleController = null;
            obj = simpleController;
            Debug.LogWarningFormat("4) SimpleController ref is NULL: {0}", obj == null);
            Debug.LogWarningFormat("4) SimpleController is NULL: {0}", Globals.IsNull(simpleController));
            if (obj != null) { simpleController.SwitchAnimation(4); }
        }

        public void OnToggleOneValueChanged(bool value)
        {
            Debug.Log(String.Format("Toggle One! New value: {0}", value));
            publisherOfAnimationSwitchEvent.Publish(AnimationSwitchEventSubscriber.First, new AnimationSwitchEvent(value));
        }

        public void OnToggleTwoValueChanged(bool value)
        {
            Debug.Log(String.Format("Toggle Two! New value: {0}", value));
            publisherOfAnimationSwitchEvent.Publish(AnimationSwitchEventSubscriber.Second, new AnimationSwitchEvent(value));
        }

        public void OnButtonOne()
        {
            // Предполагаем, что система UI каким-то образом определяет позицию в пространстве.
            // Чаще всего здесь не чистая позиция объекта, который собираемся создать, в пространстве объектов игры,
            // а позиция, которую может указывать UI. На стороне "Создателя" обычно есть функция конвертации одной
            // системы координат в другую.
            // В некоторых "сложных" случаях вся конвертация  
            publisherOfCreateCubeIntention.Publish(new CreateCubeIntention(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(1, 101) >= 50));
        }

        public async void OnButtonUnloadAdditive()
        {
            OnToggleTwoValueChanged(true);
            OnToggleTwoValueChanged(false);
            await SceneManager.UnloadSceneAsync(additiveScenePresenter.sceneName);
            // Как раз без дополнительного намерения (intention) может выйти казус - асинхронная операция на выгруженной сцене может не сработать.
            // Здесь использую синхронную передачи сообщения и поток правильно работает.
            publisherOfAdditiveSceneUnloadedEvent.Publish(new AdditiveSceneUnloadedEvent(additiveScenePresenter.sceneName));
            OnToggleTwoValueChanged(true);
            OnToggleTwoValueChanged(false);
        }
    }
}
