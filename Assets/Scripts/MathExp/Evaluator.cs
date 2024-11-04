using System.Collections.Generic;
using Hime.Redist;
using MathExp;

// Имплементируем интерфейс действий для парсера математических выражений
public sealed class Evaluator : MathExpParser.IActions
{
    // Реализация основывается просто на стековой машине
    // Терминалы - кладём на верх стека, конвертируя из строки в float число (known as System.Single)
    // Операции - снимаем необходимое количество операндов с верха стека и производим операцию
    // Результат операции - кладём на верх стека
    private readonly Stack<float> stack = new Stack<float>();

    public float Result { get { return stack.Peek(); } }

    public void OnNumber(Symbol head, SemanticBody body)
    {
        stack.Push(float.Parse(body[0].Value));
    }

    public void OnMult(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left * right);
    }

    public void OnDiv(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left / right);
    }

    public void OnPlus(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left + right);
    }

    public void OnMinus(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left - right);
    }
}