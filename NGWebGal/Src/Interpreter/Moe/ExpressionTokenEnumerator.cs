using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Expression token enumerator for expression evaluation
/// </summary>
public class ExpressionTokenEnumerator
{
    private readonly List<ExpressionToken> _tokens;
    private int _position = -1;

    public ExpressionTokenEnumerator(List<ExpressionToken> tokens) => _tokens = tokens;

    public ExpressionToken Current => _position >= 0 && _position < _tokens.Count
        ? _tokens[_position]
        : new ExpressionToken { Type = OperatorType.Void };

    public bool IsEnd => _position >= _tokens.Count;

    public bool MoveNext()
    {
        _position++;
        return _position < _tokens.Count;
    }
}
