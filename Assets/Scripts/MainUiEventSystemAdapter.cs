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
        readonly IPublisher<AnimationSwitchEventSubscriber, AnimationSwitchEvent> publisherOfAnimationSwitchEvent;
        [Inject]
        readonly IPublisher<CreateCubeIntention> publisherOfCreateCubeIntention;

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
            publisherOfCreateCubeIntention.Publish(new CreateCubeIntention(Random.Range(-3f,3f), Random.Range(-3f, 3f), Random.Range(1, 101) >=50 ));
        }

        public async void OnButtonUnloadAdditive()
        {
            OnToggleTwoValueChanged(true);
            OnToggleTwoValueChanged(false);
            await SceneManager.UnloadSceneAsync("SampleScene");
            OnToggleTwoValueChanged(true);
            OnToggleTwoValueChanged(false);
        }
    }
}
