using System;
using UnityEngine;
using System.Collections;
using VContainer;
using MessagePipe;

namespace MyGame
{
    // ����������, ����������� ��������� ��������� ������� GameObject.
    // ��������� ������������ ����� ���� � ������ � MonoBehaviour �� ������������ ������������� ��� ���� ������� ��������, ��� �������� �����.
    // ���� �� ����� ��� ���� ��������� �������� �������, � ���������� �������� ����� �������� �����������,
    // �� ����� ������� ������ - �������� � ������ "Auto Inject Game Objects" � ���������� LifetimeScope �������.
    // ��������, Cube ��� ���� � �������������� ����� � ������� ����� �������� ��� � AdditiveLifetimeScope.
    // � MonoBehaviour ��� ������������, ������� ����� ������� ������ - �������� ����� ����, ������� ����� ���� readonly.
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
        

        // ��������� ����� ����� � ����������� ������ �������� ��� ������� �������, � ��� MonoBehaviour ������ ����� ���� ����� ��������� ��������,
        // ��� ��� ��� ������ ����������� ������� � readonly � ��������� �������� ����� ���������.
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

        // ��������� API ������ ������ ��� ����������� �������� � �� ����������� ������������������
        // ��� ��������� �������, ����� �� ����������� ����������� ��� � ��� ������� ��������� � �������
        // ���������� ����������� ��� ����� �������� �������������.
        public void TurnAnimation(bool on = true) {
            this.isAnimOn = on;
        }

        void Start()
        {
            startPosition = transform.position;
            // ����� ����� ������ �������� � ����� �������� ����� ��������� � ������-���������� �� ScriptableObject.
            // ����� ������ ��� ������� ��� �������.
            curve = curveData?.curve ?? throw new System.InvalidOperationException("Asset with 'Animation Curve' not found");
            service.Hello();
            // ������� ������ "�����", ������� �� ������ ������� ������� ��� �������� � �� ����� ����� �������� ������ �������.
            // ��� ���� ��� ������������� ��� �������� "������ � ���� �����". �� ����� ������� �������, ������� ����� ��� ����������
            // �������� �������� � ������ ������ �������� �������. ������� � OnDestroy � � IDisposable.Dispose �������� Dispose ��� ��� ����.
            var d = DisposableBag.CreateBuilder();
            // ��������� ������ ���� (��������) ��� ����� ���������� "������" � ���� �������, � ��� ������� ���� �������, ����� ��� "������".
            // �� ��������� ����� ��������� � ���� � ������ �������� ������� ������ ������������ �������.
            // ��������� ��� ��������� ���������� ������� ���������� �� ���� ����������������� � ���� ������������ � �������� �� � ��������� �� ����������� ���������� �������.
            // ������-������� � ������ ����� ���� ������������� �������. � ������ ������ ��� ������-������� ������������, ����������� ������ ������� "�� �����".
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