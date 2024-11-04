using Hime.Redist;
using MathExp;
using UnityEngine;

// Имплементируем интерфейс визитёра для парсера математических выражений
public sealed class MathExpVisitor : MathExpParser.IVisitor
{
    public void OnTerminalGet(ASTNode node)
    {
        Debug.LogFormat("OnTerminalGet ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnTerminalNumber(ASTNode node)
    {
        Debug.LogFormat("OnTerminalNumber ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnTerminalSeparator(ASTNode node)
    {
        Debug.LogFormat("OnTerminalSeparator ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnTerminalSymbol(ASTNode node)
    {
        Debug.LogFormat("OnTerminalSymbol ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableAdditionOperator(ASTNode node)
    {
        Debug.LogFormat("OnVariableAdditionOperator ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableExp(ASTNode node)
    {
        Debug.LogFormat("OnVariableExp ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableExpAtom(ASTNode node)
    {
        Debug.LogFormat("OnVariableExpAtom ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableExpFactor(ASTNode node)
    {
        Debug.LogFormat("OnVariableExpFactor ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableExpTerm(ASTNode node)
    {
        Debug.LogFormat("OnVariableExpTerm ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }

    public void OnVariableFactorOperator(ASTNode node)
    {
        Debug.LogFormat("OnVariableFactorOperator ==> Symbol: {0} and value: {1}", node.Symbol.Name, node.Value);
    }
}