namespace MyGame
{
    public struct AdditiveSceneUnloadedEvent
    {
        // Лучше использовать события как иммутабельный источник значений.
        // Поэтому все данные должны инициализироваться через конструктор и быть readonly.
        // И стараться не использовать Unity структуры и классы при передачи события, чтобы уменьшить сложность тестирования.
        public readonly string name;

        public AdditiveSceneUnloadedEvent(string name)
        {
            this.name = name;
        }
    }
}
