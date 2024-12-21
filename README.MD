# VICONTAMP RESEARCH

## Общее

Этот проект показывает различные приёмы для разработки с использованием подходов "инверсии управления" и "шин сообщения".

Дополнительно показана замена стандартных Unity корутин более легкими конструктами для асинхронных процессов - "async-await".
Асинхронные процессы через "async-await" стали доступны после внедрения их в C# и обновления поддерживаемой версии .Net в Unity.

Отдельно показано внедрение парсера, основанного на генераторе из грамматик - "Обобщённый восходящий магазинный анализатор" (GLR).
Для простоты был внедрен парсер арифметических выражений со скобками с использованием простого стекового вычисления.

В проекте используются следующие сборки:

* [VContainer](https://vcontainer.hadashikick.jp)
* [UniTask](https://github.com/Cysharp/UniTask)
* [MessagePipe](https://github.com/Cysharp/MessagePipe#unity)
* [Hime parser generator](https://cenotelie.fr/projects/hime)

"Hime parser generator" не умеет генерировать пока напрямую в Unity и требует доработки.
Для внедрения в Unity в готовый код парсера были внесены соответствующие небольшие изменения, которые потеряются при регенерации.

В проекте показаны не "дистиллированные" подходы, а те, которые предполагают, что проект уже существовал и в него стали внедрять новые модули.

В некоторых компонентах и классах добавлен код, который показывает, что он может казаться правильным, но после длительной поддержки приводи к проблемам.
Этот код приводится как пример анти паттернов и "плохой практики срезания углов на дальней дистанции" и не предлагается использовать именно таким образом.
Весь код "пытается" учитывать множественную загрузку дополнительных сцен из одного и того же файла сцены с одним и тем же названием.

Ко многим классам и их методам даны пояснения в виде комментариев.

В коде есть простые классы и наследники MonoBehaviour. И смысл внедрения новых модулей сводится к тому, что нужно разделить взаимодействие строго на два уровня:

* Представление (UI, Renders, GameObject transforms) - реализуется полностью под управлением Unity
* Логика и данные (Calculations, AI Behaviour, Save-Restore) - реализуется полностью без использования Unity

Для взаимодействия двух уровней используются строго определенные входы-выходы без прямого перебрасывания ссылок на объекты Unity на уровень логики.

На каждом уровне можно разделять на небольшие модули представления и логику с данными, чтобы упростить параллельную разработку и тестирование.
Такой подход позволит также уменьшить сложность тестирования и при этот проводить тестирование на каждом уровне отдельно.
Дополнительно предлагается любые зависимости прокидывать не напрямую, а через интерфейсы, что позволит не привязываться к Unity и другим специфичным модулям,
а в Unity не придётся тянуть для тестов всю реализацию классов, можно будет "замокать пустышками" всё, что не нужно для проверки функционала.

`Debug.Log(...)` используется везде для упрощения кода и поэтому приходится везде использовать `using UnityEngine;`.
В корректно построенной системе логгер внедряется как зависимость и поэтому нет необходимости привязываться к Unity в обычных классах.

## Порядок изучения

Сформированы две сцены - одна основная (MainScene), другая дополнительная (SampleScene).
В сцены добавлены игровые объекты, стандартные для любого Unity проекта. А также добавлены "менеджеры", которые обычно добавляют в игру для глобального доступа.

Дополнительно в сцены добавлены игровые объекты с единственной компонентой, унаследованной от "LifetimeScope", которая унаследована сама от MonoBehaviour и реализует интерфейс IDisposable, чтобы для "дерева" экземпляров и областей видимости вызывать "обработчик удаления" (Dispose).
Эти объекты единственные, которые нужно добавить для внедрения новых подходов. Они определяют "жизненный цикл" и область видимости внедряемых экземпляров через DI.
Отдельные области видимости могут быть определены и для "сложносоставного" префаба, что облегчит реализацию модульности с использованием бандлов,
но более простым подходом является определение областей видимости только на уровне сцен и включения всех модулей в основную сборку (даже для DLC),
а ассетами являются только графические, звуковые и конфигурационные ресурсы.

Внутри каждой загруженной дополнительной сцены создаются свои игровые объекты и свои "менеджеры", которые "живут" до того момента, пока не будет выгружена дополнительная сцена.

Для внедрения зависимостей в экземпляры простых классов через их конструкторы достаточно зарегистрировать все классы, интерфейсы через Register интерфейса IContainerBuilder.

Для внедрения зависимостей в экземпляры MonoBehaviour-наследников необходимо использовать в различных случаях следующие варианты:

* Специальные методы регистрации (например, RegisterComponentOnNewGameObject), которые позволят создать игровой объект, которого нет на сцене. Так можно не нагружать лишними требованиями к настройке сцены.
* Добавить существующие на сцене объекты в список для регистрации LifetimeScope компоненты у соответствующего игрового объекта. Так можно проверять связанность системы после изменений в сцене.

Во время загрузки и инициализации дополнительной сцены LifetimeScope объект проходит стадию построения с использованием IContainerBuilder
и после окончания инициализации дополнительной сцены нет нормальной возможности определить регистрацию и поэтому нужно выстроить цепочку построения с помощью метода Enqueue,
в обработчик которого передается экземпляр IContainerBuilder для LifetimeScope объект в дополнительной сцене.

Хорошей практикой является определение класса родительской LifetimeScope компоненты в дополнительной сцене для её LifetimeScope объекта,
но это так же можно сделать через [EnqueueParent](https://vcontainer.hadashikick.jp/scoping/generate-child-via-scene).

В логику "игры" внедрены ограничения, которые отключают кнопку загрузки дополнительных сцен, если не выгружены все текущие дополнительные сцены.
Но так как загрузка дополнительной сцены идёт с искусственным запаздыванием на 3 секунды, то можно успеть нажать несколько раз на кнопку загрузки дополнительных сцен.
При нескольких нажатиях на кнопку будет отправлено несколько событий-намерений и произойдёт загрузка нескольких дополнительных сцен, которые будут работать независимо друг от друга.

Можно изучать скрипты в любом порядке, но для более быстрого погружения предлагается изучать в таком порядке:

* [GameLifetimeScope.cs](Assets/Scripts/GameLifetimeScope.cs)
* [AnimationCurveData.cs](Assets/Scripts/AnimationCurveData.cs) -> [Ожидание завершения задания загрузки](Assets/Scripts/GameLifetimeScope.cs#L90)
* [CubeAssetData.cs](Assets/Scripts/CubeAssetData.cs) -> [Ожидание завершения дженерик-задания загрузки](Assets/Scripts/GameLifetimeScope.cs#L91)
* [Кусок кода в GameLifetimeScope.cs](Assets/Scripts/GameLifetimeScope.cs#L94) `using (Enqueue(innerBuilder => { ... })){ await SceneManager.LoadSceneAsync... }`
* [LoadAdditiveSceneIntention.cs](Assets/Scripts/LoadAdditiveSceneIntention.cs)
* [AdditiveSceneLoadedEvent.cs](Assets/Scripts/AdditiveSceneLoadedEvent.cs)
* [AdditiveSceneUnloadedEvent.cs](Assets/Scripts/AdditiveSceneUnloadedEvent.cs)
* [AdditiveSceneLoader.cs](Assets/Scripts/AdditiveSceneLoader.cs)
* [MainScenePresenter.cs](Assets/Scripts/MainScenePresenter.cs)
* [MainUiViewController.cs](Assets/Scripts/MainUiViewController.cs)
* [MainUiEventSystemAdapter.cs](Assets/Scripts/MainUiEventSystemAdapter.cs)
* [HelloWorldService.cs](Assets/Scripts/HelloWorldService.cs)
* [AuxServicedController.cs](Assets/Scripts/AuxServicedController.cs)
* [MessagePipePubSub.cs](Assets/Scripts/MessagePipePubSub.cs)
* [AnimationSwitchEvent.cs](Assets/Scripts/AnimationSwitchEvent.cs)
* [AdditiveLifetimeScope.cs](Assets/Scripts/AdditiveLifetimeScope.cs)
* [CurvedAnimationController.cs](Assets/Scripts/CurvedAnimationController.cs)
* [AdditiveScenePresenter.cs](Assets/Scripts/AdditiveScenePresenter.cs)
* [AdditiveUiViewController.cs](Assets/Scripts/AdditiveUiViewController.cs)
* [AdditiveScenePresenterFactory.cs](Assets/Scripts/AdditiveScenePresenterFactory.cs)
* [CreateCubeIntention.cs](Assets/Scripts/CreateCubeIntention.cs)
* [CubeCreator.cs](Assets/Scripts/CubeCreator.cs)
* [AdditiveUiEventSystemAdapter.cs](Assets/Scripts/AdditiveUiEventSystemAdapter.cs)
* [MathExp/Evaluator.cs](Assets/Scripts/MathExp/Evaluator.cs)

## Что почитать

* <https://habr.com/ru/articles/759958/>
* <https://habr.com/ru/articles/652483/>
* <https://habr.com/ru/companies/otus/articles/754296/>
