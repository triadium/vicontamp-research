namespace MyGame
{
    public struct AdditiveSceneLoadedEvent
    {
        // ����� ������������ ������� ��� ������������� �������� ��������.
        // ������� ��� ������ ������ ������������������ ����� ����������� � ���� readonly.
        // � ��������� �� ������������ Unity ��������� � ������ ��� �������� �������, ����� ��������� ��������� ������������.
        public readonly string name;

        public AdditiveSceneLoadedEvent(string name)
        {
            this.name = name;
        }
    }
}