using System;
using System.Collections.Generic;
using Hime.Redist;
using MathExp;
class Evaluator : MathExpParser.Actions
{
    private Stack<float> stack = new Stack<float>();

    public float Result { get { return stack.Peek(); } }

    // we override the base semantic actions (that do nothing)
    public override void OnNumber(Symbol head, SemanticBody body)
    {
        stack.Push(Single.Parse(body[0].Value));
    }

    public override void OnMult(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left * right);
    }

    public override void OnDiv(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left / right);
    }

    public override void OnPlus(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left + right);
    }

    public override void OnMinus(Symbol head, SemanticBody body)
    {
        float right = stack.Pop();
        float left = stack.Pop();
        stack.Push(left - right);
    }
}