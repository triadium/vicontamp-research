using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MyGame
{
    // Интерфейс для использования в соответствующем Presenter
    public interface IAdditiveUiViewController
    {
        AdditiveScenePresenter Presenter { get; set; }
        void SetSpecText(string text);
    }

    public class AdditiveUiViewController : MonoBehaviour, IAdditiveUiViewController
    {
        // Так не будет работать потому, что у нас presenter должен создаваться фабрикой,
        // а стандартный механизм не знает где достать значения параметров для фабрики.
        // [Inject]
        // Ещё одна особенность, которую нужно решить - на стороне presenter интерфейс должен 
        // быть WeakReference, чтобы позволить Unity удалить элемент из памяти.
        private AdditiveScenePresenter presenter;
        string sceneName;
        AdditiveScenePresenter IAdditiveUiViewController.Presenter
        {
            get => presenter;
            set
            {
                presenter = value;
                // Не удаляем имя сцены после первого успешного определения. См. OnDestroy
                sceneName = presenter?.sceneName ?? sceneName;
            }
        }

        public Text specText;


        // Сегрегируем функционал, используя общий контекст MonoBehaviour
        void IAdditiveUiViewController.SetSpecText(string text)
        {
            specText.text = text;
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
                    specText = go.transform.Find("SpecText").GetComponent<Text>();
                }
            }

            presenter.SayHello();
        }

        void Update()
        {
            // Размещаем постоянную анимацию дополнения сцены здесь
        }

        void OnDestroy()
        {
            // presenter может быть null и поэтому имя сцены нельзя будет получить.
            Debug.Log(String.Format("Additive scene '{0}' view controller disposed!", this.sceneName));
            this.presenter = null;
        }
    }
}
