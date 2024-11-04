using UnityEngine;

namespace MyGame
{
    public class SimpleController : MonoBehaviour
    {
        private bool isAnimationOn = false;
        public void SwitchAnimation(int phase, bool on = true)
        {
            // Still C# object fields can be changed for destroyed unity object.
            this.isAnimationOn = on;

            // But there is no longer access to the game object for destroyed unity object!
            if (!Globals.IsNull(this) && gameObject.activeInHierarchy)
            {
                gameObject.transform.position = gameObject.transform.forward * phase;
                Debug.LogWarningFormat("Animation switched {0}", phase);
            }
        }

        void OnDestroy()
        {
            Debug.Log("SimpleController Disposed!");
        }
    }
}