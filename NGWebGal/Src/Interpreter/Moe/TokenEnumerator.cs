using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Token enumerator with peek capability
/// </summary>
public class TokenEnumerator
{
    private readonly List<Token> _tokens;
    private int _position = -1;

    public TokenEnumerator(List<Token> tokens) => _tokens = tokens;

    public Token Current => _position >= 0 && _position < _tokens.Count
        ? _tokens[_position]
        : new Token { Type = TokenType.EOF };

    public bool IsEnd => _position >= _tokens.Count;

    public bool MoveNext()
    {
        _position++;
        return _position < _tokens.Count;
    }

    public bool TryGetNext(out Token? token)
    {
        if (_position + 1 < _tokens.Count)
        {
            token = _tokens[_position + 1];
            _position++;
            return true;
        }
        token = null;
        return false;
    }

    public Token? Peek() =>
        _position + 1 < _tokens.Count ? _tokens[_position + 1] : null;
}
