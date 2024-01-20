using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    public interface IUiViewController {
        void SetText(string text);
    }

    public class UiViewController: MonoBehaviour, IUiViewController
    {
        public Text specText;

        public void SetText(string text)
        {
            specText.text = text;
        }

        // Use this for initialization
        void Start()
        {
            specText = GameObject.Find("SpecText").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
