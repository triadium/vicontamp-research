using System;
using UnityEngine;
using System.Collections;
using VContainer;
using MessagePipe;

namespace MyGame
{
    // Компонента, управляющая анимацией изменения позиции GameObject.
    // Внедрение зависимостей через поля и методы в MonoBehaviour не производится автоматически для всех игровых объектов, это накладно очень.
    // Если на сцене уже есть экземпляр игрового объекта, в компоненты которого нужно внедрять зависимости,
    // то самый простой способ - добавить в список "Auto Inject Game Objects" в компоненте LifetimeScope объекта.
    // Например, Cube уже есть в дополнительной сцене и поэтому можно добавить его в AdditiveLifetimeScope.
    // У MonoBehaviour нет конструктора, поэтому самый простой способ - внедрить через поля, которые могут быть readonly.
    public class CurvedAnimationController : MonoBehaviour
    {
        public AnimationCurve curve;

        [Inject]
        readonly AnimationCurveData curveData;
        [Inject]
        readonly HelloWorldService service;
        [Inject]
        readonly ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriberOfAnimationSwitchEvent;
        [Inject]
        readonly ISubscriber<int> subscriberOfInt;

        Vector3 startPosition;

        IDisposable disposable;
        bool isAnimOn = false;
        

        // Внедрение через метод и конструктор хорошо работают для обычных классов, а для MonoBehaviour только через поля можно нормально внедрять,
        // так как нет другой возможности сделать и readonly и назначить значение через внедрение.
        //
        //[Inject]
        //void Construct(AnimationCurveData curveData, HelloWorldService service, ISubscriber<AnimationSwitchEventSubscriber, AnimationSwitchEvent> subscriberOfAnimationSwitchEvent, ISubscriber<int> subscriberOfInt)
        //{
        //    // Compiler Error CS0191
        //    this.curveData = curveData;
        //    this.service = service;
        //    this.subscriberOfAnimationSwitchEvent = subscriberOfAnimationSwitchEvent;
        //    this.subscriberOfInt = subscriberOfInt;
        //}

        // Публичное API должно делать все необходимые действия и по возможности переиспользоваться
        // при обработке событий, чтобы не приходилось дублировать код и все цепочки намерений и событий
        // оставались стабильными при любом сценарии использования.
        public void TurnAnimation(bool on = true) {
            this.isAnimOn = on;
        }

        void Start()
        {
            startPosition = transform.position;
            // Можно здесь убрать проверку и после загрузки сразу проверять в классе-наследнике от ScriptableObject.
            // Здесь только для примера это сделано.
            curve = curveData?.curve ?? throw new System.InvalidOperationException("Asset with 'Animation Curve' not found");
            service.Hello();
            // Создаем единый "мешок", который по единой команде очистит все подписки и не нужно будет отдельно делать отписку.
            // При этом нет необходимости все подписки "пихать в один мешок". Их можно создать столько, сколько нужно для управления
            // группами подписок в рамках одного игрового объекта. Главное в OnDestroy и в IDisposable.Dispose вызывать Dispose для них всех.
            var d = DisposableBag.CreateBuilder();
            // Подписчик всегда один (синглтон) для одной комбинации "топика" и типа события, и для каждого типа события, когда нет "топика".
            // Но подписчик может добавлять к себе в список рассылки сколько угодно обработчиков событий.
            // Подписчик при получении очередного события проходится по всем зарегестированным у него обработчикам и вызывает их с передачей им полученного экземпляра события.
            // Лямбда-функция и методы могут быть обработчиками событий. В данном случае две лямбда-функции используется, обрабатывая данные объекта "на месте".
            subscriberOfAnimationSwitchEvent.Subscribe<AnimationSwitchEventSubscriber, AnimationSwitchEvent>(AnimationSwitchEventSubscriber.First, v => this.TurnAnimation(v.isOn)).AddTo(d);
            subscriberOfInt.Subscribe(x => {
                Debug.Log("CurvedAnimationView:" + x);
                var delta = x * 0.001f;
                startPosition.x += delta; // 
                transform.position = new Vector3(transform.position.x + delta, transform.position.y, transform.position.z);
            }).AddTo(d);
            disposable = d.Build();
        }

        void Update()
        {
            if (isAnimOn)
            {
                transform.position = new Vector3(startPosition.x, startPosition.y + curve.Evaluate(Time.time), startPosition.z);
            }            
        }

        void OnDestroy()
        {
            disposable.Dispose();
            Debug.Log("CurvedAnimationView Disposed!");
        }
    }
}