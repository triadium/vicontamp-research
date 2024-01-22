using VContainer;
using VContainer.Unity;
using MessagePipe;
using System;

namespace MyGame
{
    // Дополнительная область видимости для подгружаемого дополнения текущей сцены (Additive Scene).
    // Дополнительные области (Scoped) полезны для регистрации синглтонов и ресурсов только конкретного класса сцен.
    // Все дополнительные синглтоны будут "жить", пока "живо" дополнение сцены.
    // Например, есть сцены с миссиями в виде лабиринта и сцены с отсеками командного центра в виде UI форм.
    // Процессы разные, классы разные, синглтоны тоже нужны разные для этих двух видов сцен.
    // Для всех сцен миссий можно использовать один и тот же LifetimeScope класс-наследник со своим процессом конфигурирования.
    // Для всех сцен с отсеками командного центра скорее всего можно использовать другой, но тоже один и тот же LifetimeScope класс-наследник.
    // Для очень специфичных отсеков командного цента и миссий можно создать отдельные LifetimeScope классы-наследник со своим набором регистрации.
    // Например, MissionLifetimeScope, CompartmentLifetimeScope и если надо, то VerySpecialMissionLifetimeScope, VerySpecialCompartmentLifetimeScope
    // Дополнительные области могут внедрять зависимости, которые есть в области более высокого уровня. Например, в игровой области видимости.
    // Для этого необходимо при настройке дополнительной области указать класс родительской области.
    // Например, в AdditiveLifetimeScope в поле компоненты через редактор Unity нужно указать класс GameLifetimeScope.
    // При активации дополнительной сцены контейнер попробует найти в иерархии и связать текущую дополнительную область с указанной в качестве родителя.
    // В случае, если в иерархии объектов не будет найден соответствующий объект, то выведется ошибка.
    public class AdditiveLifetimeScope : LifetimeScope
    {
        // Не будет работать, так как LifetimeScope должен быть указан в родительском для внедрения, MessagePipeOptions нужен для IContainerBuilder,
        // который используется для регистрации шин с соответствующими опциями. Наверное, самое правильное - через класс-фабрику внедрять опции шин.
        // Но можно проще и более прямолинейно найти нужный нам объект с нужной компонентой в иерархии как сделано в Configure.
        // [Inject]
        // MessagePipeOptions optionsOfMessagePipe;

        protected override void Configure(IContainerBuilder builder)
        {
            // Получаем основную область видимости, чтобы взять оттуда общие настройки для шины событий.
            var parent = LifetimeScope.Find<GameLifetimeScope>() as GameLifetimeScope;

            // Регистрируем компоненту для создания GameObject с указанным именем и с одной регистрируемой компонентой.
            // Это хорошо подходит для всяких моно-синглтонов, в сцене и дополнениях к сцене.
            // Регистрируем в иерархии текущей области видимости, чтобы при выгрузки дополнения
            // все зависимые элементы были очищены и не остались в основной сцене.
            builder.RegisterComponentOnNewGameObject<AdditiveUiViewController>(Lifetime.Scoped, "UIViewController").UnderTransform(this.transform).AsImplementedInterfaces();
            // Регистрируем класс-фабрику
            builder.Register<AdditiveScenePresenterFactory>(Lifetime.Scoped);
            // Регистрируем метод-фабрику
            builder.RegisterFactory<string, AdditiveScenePresenter>(container => container.Resolve<AdditiveScenePresenterFactory>().Create, Lifetime.Scoped);

            // Пока идёт регистрация нет никакого ещё контейнера и поэтому любые дополнительные цепочки регистрации,
            // где уже нужны резолвы делаем через делегат-регистрацию. На выходе мы должны отдать экземпляр для дополнительной регистрации.
            builder.Register(container =>
            {
                // Можно использовать класс-фабрику и привязываться к конкретной маркировке класса
                // var factoryOfAdditiveSceneController = container.Resolve<AdditiveScenePresenterFactory>();
                // return factoryOfAdditiveSceneController.Create("SampleScene");

                // Можно использовать метод-фабрику и не привязываться к конкретной маркировке класса,
                // а привязаться к обобщеному определению метода - на входе такие-то параметры, на выходе новый инициализированный экземпляр!
                // Такой подход освобождает от каких-то дополнительных имён, но не позволяет иметь несколько разных методов-фабрик одновременно.
                // Но обычно этого не нужно, а если надо всё-таки, то входной параметр делают структурой с уникальным именем и так маркируют метод-фабрику.
                var factoryOfAdditiveSceneController = container.Resolve<Func<string, AdditiveScenePresenter>>();
                return factoryOfAdditiveSceneController(this.gameObject.scene.name);
            }, Lifetime.Scoped).AsImplementedInterfaces().As<AdditiveScenePresenter>();

            // Регистрируем для дополнительной сцены синглтоны
            // MessagePipePubSub делаем только "Post Startable" - т.е. будет выполнен код старта после того, как все остальные уже пройдут инициализацию.
            // В примере отправляется событие при старте и если отправить его простов Start, то не все подписчики получат и обработают его, так как порядок не определен.
            // "Жизненный цикл" указан в документации https://vcontainer.hadashikick.jp/integrations/entrypoint
            builder.Register<MessagePipePubSub>(Lifetime.Scoped).As<IPostStartable>();
            // Для GameController регистрируем все интерфейсы.
            builder.Register<AuxServicedController>(Lifetime.Scoped).AsImplementedInterfaces();
            // Метод AsImplementedInterfaces полезен для тех случаев, когда есть полная уверенность, что все интерфейсы должны регистироваться
            // и тогда можно не писать в коде длинные цепочки As<IStartable>().As<ITickable>().As<System.IDisposable>()
            // Например, .As<System.IDisposable>() лишнее звено, резолвер контейнера и так проверяет на IDisposable, чтобы вызывать принудительно очистку,
            // и можно тогда оставить только As<IStartable>().As<ITickable>()

            builder.Register<CubeCreator>(Lifetime.Scoped).As<IStartable>();

            // Не можем зарегистрировать просто AdditiveSceneController класс для создания экземпляра,
            // так как конструктор класса помимо зависимостей имеет параметр - имя сцены и поэтому нужно
            // регистрировать экземпляр через родительскую область видимости, создавая экземпляр с помощью
            // класса-фабрики, чтобы пробросить все необходимые зависимости без бойлерплейта.
            // builder.Register<AdditiveSceneController>(Lifetime.Scoped).AsImplementedInterfaces();

            // Регистрировать компоненты брокера - издателя, подписчика - лучше в рамках дополнительного скоупа, чтобы после выгрузки все подписки были автоматом очищены.
            // Смотри класс GameController. 
            // Метод RegisterMessageBroker<TKey, TMessage> используется для организации идентифицируемых шин событий по ключу TKey.
            // Например, в этом случае перечисление AnimationSwitchEventSubscriber будет определять шину, по которой будет отправляться событие и "отлавливаться" подписчиком.
            // Обычно в других системах с передачей событий TKey называют "топиком".
            builder.RegisterMessageBroker<AnimationSwitchEventSubscriber, AnimationSwitchEvent>(parent.CommonOptionsOfMessagePipe);
            
            // Метод RegisterMessageBroker<TMessage> используется в большинстве случаев для передачи системных и общих событий, которые не требуют разделения по "топикам".
            builder.RegisterMessageBroker<CreateCubeIntention>(parent.CommonOptionsOfMessagePipe);
        }
    }
}
