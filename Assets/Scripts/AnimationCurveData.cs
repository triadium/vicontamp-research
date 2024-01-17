using UnityEngine;
using System.Collections;

namespace MyGame
{
    // Настройки анимации храним в отдельном файле и чтобы отделить процесс загрузки от процесса использования
    // регистриуется отдельный синглтон-ассет в базовой области видимости игры.
    // Процесс загрузки ассета происходит ассинхронно, а внедрение и использование синхронно.
    // Смотри GameLifetimeScope

    [CreateAssetMenu]
    public class AnimationCurveData : ScriptableObject
    {
        public AnimationCurve curve;
    }
}