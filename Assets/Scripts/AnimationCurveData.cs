using UnityEngine;
using System.Collections;

namespace MyGame
{
    // ��������� �������� ������ � ��������� ����� � ����� �������� ������� �������� �� �������� �������������
    // ������������� ��������� ��������-����� � ������� ������� ��������� ����.
    // ������� �������� ������ ���������� �����������, � ��������� � ������������� ���������.
    // ������ GameLifetimeScope

    [CreateAssetMenu]
    public class AnimationCurveData : ScriptableObject
    {
        public AnimationCurve curve;
    }
}