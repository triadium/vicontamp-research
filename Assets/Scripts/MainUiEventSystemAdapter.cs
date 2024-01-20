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
    public class MainUiEventSystemAdapter : MonoBehaviour
    {
        [Inject]
        readonly IAsyncPublisher<LoadAdditiveSceneIntention> publisherOfLoadAdditiveSceneIntention;

        public async void OnButtonLoadAdditive()
        {
            // Комбинация async и sync кода позволяет формировать императивный код без "танцев" вокруг проблем корутин.

            // Ждём некоторое время и потом только публикуем асинхронное событие.
            await UniTask.Delay(3000, DelayType.Realtime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy(), true).SuppressCancellationThrow();
            // Какой-то селектор в UI позволил определить, что есть намерение загрузить "SampleScene".
            // Например, была выбрана миссия и через событие, что миссия выбрана на глобальной карте
            // был обновлен общий контекст, который может храниться, например, в контроллере сцены.
            // Асинхронное событие должно отправляться и приниматься только асинхронным издателем и подписчиком,
            // иначе это будут разные шины и "отлов" событий не будет происходить.
            // Publish - не асинхронный метод и работает как "кинул и забыл"
            // и поэтому последующий вывода в лог произойдет почти моментально.
            //publisherOfLoadAdditiveSceneIntention.Publish(new LoadAdditiveSceneIntention("SampleScene"), this.GetCancellationTokenOnDestroy());

            // Так как мы ждём обработку асинхронного события, то мы как бы "помним", что у нас идёт обработка
            // и поэтому последующий вывода в лог произойдет с ощутимой задержкой.
            await publisherOfLoadAdditiveSceneIntention.PublishAsync(new LoadAdditiveSceneIntention("SampleScene"), this.GetCancellationTokenOnDestroy());

            Debug.Log("Intention to load additive scene was handled!");
        }
    }
}
