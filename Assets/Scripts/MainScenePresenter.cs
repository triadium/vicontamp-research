using System;
using UnityEngine;
using VContainer.Unity;
using MessagePipe;
using Random = UnityEngine.Random;
using Hime.Redist;
using MathExp; // default namespace for the parser is the grammar's name


namespace MyGame
{
    public class MainScenePresenter : IStartable, IPostStartable, IDisposable
    {
        readonly IMainUiViewController uiViewController;
        readonly ISubscriber<AdditiveSceneLoadedEvent> subscriberOfAdditiveSceneLoadedEvent;
        readonly ISubscriber<AdditiveSceneUnloadedEvent> subscriberOfAdditiveSceneUnloadedEvent;
        IDisposable disposable;
        int additiveSceneCount = 0;

        public MainScenePresenter(
            ISubscriber<AdditiveSceneLoadedEvent> subscriberOfAdditiveSceneLoadedEvent,
            ISubscriber<AdditiveSceneUnloadedEvent> subscriberOfAdditiveSceneUnloadedEvent,
            IMainUiViewController uiViewController
        )
        {
            this.subscriberOfAdditiveSceneLoadedEvent = subscriberOfAdditiveSceneLoadedEvent;
            this.subscriberOfAdditiveSceneUnloadedEvent = subscriberOfAdditiveSceneUnloadedEvent;
            this.uiViewController = uiViewController;
        }

        void IStartable.Start()
        {
            var d = DisposableBag.CreateBuilder();
            subscriberOfAdditiveSceneLoadedEvent.Subscribe(_ =>
            {
                this.additiveSceneCount++;
                // Вот кластеризация кода на маленькие фрагменты функционала -
                // это обычная "проблема" унификации подхода к управлению UI и другими элементами представления
                this.uiViewController.SetAdditiveSceneCount(this.additiveSceneCount);
                this.uiViewController.TurnLoadAdditive(this.additiveSceneCount <= 0);
            }).AddTo(d);
            subscriberOfAdditiveSceneUnloadedEvent.Subscribe(_ =>
            {
                this.additiveSceneCount--;
                // Вот кластеризация кода на маленькие фрагменты функционала -
                // это обычная "проблема" унификации подхода к управлению UI и другими элементами представления
                this.uiViewController.SetAdditiveSceneCount(this.additiveSceneCount);
                this.uiViewController.TurnLoadAdditive(this.additiveSceneCount <= 0);

                string expression = String.Format("{1} * ({0} + {1}) * {2}", Random.Range(1, 100), Random.Range(1, 100), Random.Range(1, 100));

                // Для тестов внедрения парсера выражений по описанной грамматике и сгенерированным автоматонам с помощью himecc
                Evaluator evaluator = new Evaluator();
                MathExpLexer lexer = new MathExpLexer(expression);
                MathExpParser parser = new MathExpParser(lexer, evaluator);
                // Executes the parsing                
                ParseResult result = parser.Parse();
                MathExpParser.Visit(result, new MathExpVisitor());
                var resultStr = String.Format("{0} = {1}\n", expression, evaluator.Result);

                Debug.Log(resultStr);
                uiViewController.SetAdditiveSceneText(resultStr);

            }).AddTo(d);
            disposable = d.Build();
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
            Debug.Log("Main scene controller disposed!");
        }

        void IPostStartable.PostStart()
        {
            uiViewController.TurnLoadAdditive(this.additiveSceneCount <= 0);
            Debug.Log("Main scene loaded!");
        }
    }
}
