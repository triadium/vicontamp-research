using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    public interface IMainUiViewController {
        void TurnLoadAdditive(bool on);
        void SetAdditiveSceneCount(int count);
        void SetAdditiveSceneText(string text);
    }

    public class MainUiViewController: MonoBehaviour, IMainUiViewController
    {
        public Button btnLoadAdditive;
        public TMP_Text txtLoadAdditive;
        public TMP_Text txtAdditiveSceneCounter;

        string initLoadAdditiveText;

        void IMainUiViewController.TurnLoadAdditive(bool on)
        {
            btnLoadAdditive.enabled = on;
            txtLoadAdditive.text = on ? String.Format("Try {0}", initLoadAdditiveText) : String.Format("{0}\nDisabled For Now", initLoadAdditiveText);
        }

        void IMainUiViewController.SetAdditiveSceneCount(int count)
        {
            txtAdditiveSceneCounter.text = count.ToString();
        }

        void IMainUiViewController.SetAdditiveSceneText(string text)
        {
            txtAdditiveSceneCounter.text = text;
        }        

        // Use this for initialization
        void Start()
        {
            // Статичный метод GameObject.Find не работает при одинаково загружаемых сценах одновременно
            // и не очень хорошо работает, когда надо заменить одну сцену на другую, у которых могут пересекаться объекты по имени и тэгу.
            // Можно сделать моно-"сундучок" и все необходимые ссылки, которые нужно быстро 
            var rgoArray = this.gameObject.scene.GetRootGameObjects();
            foreach (GameObject go in rgoArray)
            {
                // Вот поэтому и должно быть в начальной сцене мало корневых объектов.
                // Даже можно какие-то игровые области, если их немного, формировать в корне сцены, а все остальные как дочерние.
                // Если областей много, то лучше их располагать под каким-то общим "пустым" объектом, выравненным по "нулю"
                // с глобальными координатами.
                // Можно искать по тэгу. В целом обычный поиск подойдёт, если только одна сцена будет загружена дополнительная,
                // но это сильное ограничение стурктуры.
                // Можно сформировать компоненту "карта ссылок", навесить на объект с компонентой области видимости (LifetimeScope)
                // и использовать его для быстрого получения и проверки всё ли инициализировано, что нужно функционалу сцены.
                if (go.name == "Canvas")
                {
                    btnLoadAdditive = go.transform.Find("LoadAdditiveButton").GetComponent<Button>();
                    txtLoadAdditive = btnLoadAdditive.GetComponentInChildren<TMP_Text>();
                    txtAdditiveSceneCounter = go.transform.Find("AdditiveSceneCounter").GetComponent<TMP_Text>();

                    initLoadAdditiveText = txtLoadAdditive.text;

                    break;
                }
            }            
        }
        
        void Update()
        {
            // Размещаем постоянную анимацию главной сцены здесь
        }
    }
}
