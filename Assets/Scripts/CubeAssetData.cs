using UnityEngine;
using System.Collections;

namespace MyGame
{
    // ��������� �������� ������ � ��������� ����� � ����� �������� ������� �������� �� �������� �������������
    // ������������� ��������� ��������-����� � ������� ������� ��������� ����.
    // ������� �������� ������ ���������� �����������, � ��������� � ������������� ���������.
    // ������ GameLifetimeScope

    [CreateAssetMenu]
    public class CubeAssetData : ScriptableObject
    {
        public GameObject cube;
    }
}