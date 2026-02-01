using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Expression evaluator for Moe scripts
/// </summary>
public class ExpressionsExecutor
{
    private readonly Dictionary<string, MoeVariable> _globalVariables;
    private readonly Dictionary<string, MoeVariable> _localVariables;

    public ExpressionsExecutor(
        Dictionary<string, MoeVariable> globalVariables,
        Dictionary<string, MoeVariable> localVariables)
    {
        _globalVariables = globalVariables;
        _localVariables = localVariables;
    }

    public object Parse(ExpressionNode expression) => Parse(expression.Tokens);

    public object Parse(List<ExpressionToken> expression)
    {
        var tokens = new ExpressionTokenEnumerator(expression);
        tokens.MoveNext();
        object result = Level15(tokens);
        if (!tokens.IsEnd)
            throw new InvalidOperationException($"Unexpected token {tokens.Current}");
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ExpressionToken ConsumeToken(OperatorType type, ExpressionTokenEnumerator tokens)
    {
        if (tokens.IsEnd)
            throw new InvalidOperationException("End of expression");
        var token = tokens.Current;
        tokens.MoveNext();
        if (token.Type != type)
            throw new InvalidOperationException($"Unexpected token: {token.Type} != {type}");
        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level15(ExpressionTokenEnumerator tokens) => Level12(tokens);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level12(ExpressionTokenEnumerator tokens)
    {
        object result = Level11(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while (opType == OperatorType.OR && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            object value = Level11(tokens);
            result = Calc(result, value, opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level11(ExpressionTokenEnumerator tokens)
    {
        object result = Level10(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while (opType == OperatorType.AND && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            object value = Level10(tokens);
            result = Calc(result, value, opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level10(ExpressionTokenEnumerator tokens)
    {
        object result = Level9(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while (opType == OperatorType.bOR && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level9(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level9(ExpressionTokenEnumerator tokens)
    {
        object result = Level8(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while (opType == OperatorType.XOR && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level8(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level8(ExpressionTokenEnumerator tokens)
    {
        object result = Level7(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while (opType == OperatorType.bAND && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level7(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level7(ExpressionTokenEnumerator tokens)
    {
        object result = Level6(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while ((opType == OperatorType.EQ || opType == OperatorType.NEQ) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level6(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level6(ExpressionTokenEnumerator tokens)
    {
        object result = Level5(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while ((opType == OperatorType.GT || opType == OperatorType.LT ||
                opType == OperatorType.EGT || opType == OperatorType.ELT) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level5(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level5(ExpressionTokenEnumerator tokens)
    {
        object result = Level4(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while ((opType == OperatorType.SHL || opType == OperatorType.SHR) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level4(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level4(ExpressionTokenEnumerator tokens)
    {
        object result = Level3(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while ((opType == OperatorType.ADD || opType == OperatorType.SUB) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level3(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level3(ExpressionTokenEnumerator tokens)
    {
        object result = Level2(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        while ((opType == OperatorType.MUL || opType == OperatorType.DIV ||
                opType == OperatorType.MOD) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            result = Calc(result, Level2(tokens), opType);
            opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level2(ExpressionTokenEnumerator tokens)
    {
        object result = Level1(tokens);
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        if ((opType == OperatorType.Minus || opType == OperatorType.bNOT ||
             opType == OperatorType.NOT) && !tokens.IsEnd)
        {
            ConsumeToken(opType, tokens);
            return Calc(Level1(tokens), 0, opType);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object Level1(ExpressionTokenEnumerator tokens)
    {
        var opType = tokens.IsEnd ? OperatorType.Void : tokens.Current.Type;
        if (opType == OperatorType.Number)
            return ConsumeToken(OperatorType.Number, tokens).Number;
        if (opType == OperatorType.String)
            return ConsumeToken(OperatorType.String, tokens).String;
        if (opType == OperatorType.Variable)
            return GetVariableValue(tokens);
        if (opType == OperatorType.LeftParen)
        {
            ConsumeToken(OperatorType.LeftParen, tokens);
            object result = Level15(tokens);
            ConsumeToken(OperatorType.RightParen, tokens);
            return result;
        }
        throw new InvalidOperationException($"Unexpected token: {tokens.Current}");
    }

    private object GetVariableValue(ExpressionTokenEnumerator tokens)
    {
        var variableInfo = tokens.Current.Var;
        ConsumeToken(OperatorType.Variable, tokens);

        _globalVariables.TryGetValue(variableInfo.Name, out MoeVariable? variable);
        if (variable is null)
            _localVariables.TryGetValue(variableInfo.Name, out variable);

        if (variable is null)
            throw new InvalidOperationException($"Variable not found: {variableInfo}");

        List<int> indices = [];
        foreach (var item in variableInfo.Index)
        {
            object ret = Parse(item);
            if (ret is int index)
                indices.Add(index);
            else
                throw new InvalidOperationException($"Array index must be integer: {ret}");
        }
        return variable[indices];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object Calc(object v1, object v2, OperatorType type)
    {
        int v1i, v2i;
        float v1f, v2f;
        string v1s, v2s;
        int flag = 0;

        if (v1 is int vv1i)
            (v1i, v1f, v1s) = (vv1i, vv1i, vv1i.ToString());
        else if (v1 is float vv1f)
            (v1i, v1f, v1s, flag) = ((int)vv1f, vv1f, vv1f.ToString(), 1);
        else if (v1 is string vv1s)
            (v1i, v1f, v1s, flag) = (0, 0, vv1s, 2);
        else
            throw new InvalidOperationException($"Invalid type: {v1.GetType()}");

        if (v2 is int vv2i && flag != 2)
            (v2i, v2f, v2s) = (vv2i, vv2i, vv2i.ToString());
        else if (v2 is float vv2f && flag != 2)
            (v2i, v2f, v2s, flag) = ((int)vv2f, vv2f, vv2f.ToString(), 1);
        else if (v2 is string vv2s)
            (v2i, v2f, v2s, flag) = (0, 0, vv2s, 2);
        else
            throw new InvalidOperationException($"Invalid type: {v2.GetType()}");

        return flag switch
        {
            0 => CalcInt(v1i, v2i, type),
            1 => CalcFloat(v1f, v2f, type),
            2 => CalcString(v1s, v2s, type),
            _ => throw new InvalidOperationException("Unknown type flag")
        };
    }

    private static object CalcInt(int v1, int v2, OperatorType type) => type switch
    {
        OperatorType.ADD => v1 + v2,
        OperatorType.SUB => v1 - v2,
        OperatorType.MUL => v1 * v2,
        OperatorType.DIV => v1 / v2,
        OperatorType.MOD => v1 % v2,
        OperatorType.POW => (int)Math.Pow(v1, v2),
        OperatorType.bAND => v1 & v2,
        OperatorType.bOR => v1 | v2,
        OperatorType.XOR => v1 ^ v2,
        OperatorType.SHL => v1 << v2,
        OperatorType.SHR => v1 >> v2,
        OperatorType.EQ => v1 == v2 ? 1 : 0,
        OperatorType.NEQ => v1 != v2 ? 1 : 0,
        OperatorType.GT => v1 > v2 ? 1 : 0,
        OperatorType.LT => v1 < v2 ? 1 : 0,
        OperatorType.EGT => v1 >= v2 ? 1 : 0,
        OperatorType.ELT => v1 <= v2 ? 1 : 0,
        OperatorType.AND => (v1 != 0 && v2 != 0) ? 1 : 0,
        OperatorType.OR => (v1 != 0 || v2 != 0) ? 1 : 0,
        OperatorType.Minus => -v1,
        OperatorType.bNOT => ~v1,
        OperatorType.NOT => v1 == 0 ? 1 : 0,
        _ => throw new InvalidOperationException($"Operator {type} not implemented for int")
    };

    private static object CalcFloat(float v1, float v2, OperatorType type) => type switch
    {
        OperatorType.ADD => v1 + v2,
        OperatorType.SUB => v1 - v2,
        OperatorType.MUL => v1 * v2,
        OperatorType.DIV => v1 / v2,
        OperatorType.MOD => v1 % v2,
        OperatorType.POW => Math.Pow(v1, v2),
        OperatorType.EQ => v1 == v2 ? 1 : 0,
        OperatorType.NEQ => v1 != v2 ? 1 : 0,
        OperatorType.GT => v1 > v2 ? 1 : 0,
        OperatorType.LT => v1 < v2 ? 1 : 0,
        OperatorType.EGT => v1 >= v2 ? 1 : 0,
        OperatorType.ELT => v1 <= v2 ? 1 : 0,
        OperatorType.AND => (v1 != 0 && v2 != 0) ? 1 : 0,
        OperatorType.OR => (v1 != 0 || v2 != 0) ? 1 : 0,
        OperatorType.Minus => -v1,
        OperatorType.NOT => v1 == 0 ? 1 : 0,
        _ => throw new InvalidOperationException($"Operator {type} not implemented for float")
    };

    private static object CalcString(string v1, string v2, OperatorType type) => type switch
    {
        OperatorType.ADD => v1 + v2,
        OperatorType.EQ => v1 == v2 ? 1 : 0,
        OperatorType.NEQ => v1 != v2 ? 1 : 0,
        _ => throw new InvalidOperationException($"Operator {type} not implemented for string")
    };
}
