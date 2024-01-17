namespace MyGame
{
    public struct AdditiveSceneLoadedEvent
    {
        // Ћучше использовать событи€ как иммутабельный источник значений.
        // ѕоэтому все данные должны инициализироватьс€ через конструктор и быть readonly.
        // » старатьс€ не использовать Unity структуры и классы при передачи событи€, чтобы уменьшить сложность тестировани€.
        public readonly string name;

        public AdditiveSceneLoadedEvent(string name)
        {
            this.name = name;
        }
    }
}