using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Lexical analyzer for Moe scripts
/// </summary>
public class Lexer
{
    private static readonly HashSet<string> AccessSet =
    [
        "var", "static", "const", "ref",
    ];

    private static readonly HashSet<string> TypeSet =
    [
        "void", "int", "double", "string", "dictionary",
    ];

    private static readonly HashSet<string> KeywordsSet =
    [
        "func", "return",
        "if", "elif", "else", "while", "continue", "break",
    ];

    private static readonly HashSet<string> OperatorSet =
    [
        "=",
        "+", "-", "*", "/", "%", "^^",
        "~", "|", "&", "^", "<<", ">>",
        "<", ">", ">=", "<=", "==",
        "||", "&&", "!",
    ];

    private int _line = 0;
    private int _inputPos = 0;
    private int _complexTokenPos = 0;

    private List<string> _input = [];
    public List<Token> Tokens { get; private set; } = [];
    public Statement CodeStatement { get; private set; } = new();

    public Lexer(string input) => SetInput(input);
    public Lexer(List<string> input) => SetInput(input);

    public void SetInput(string input) =>
        SetInput(new List<string>(input.Split('\n', StringSplitOptions.TrimEntries)));

    public void SetInput(List<string> input)
    {
        Clear();
        _input = input;
    }

    public void Clear()
    {
        _line = _inputPos = _complexTokenPos = 0;
        _input = [];
        Tokens = [];
        CodeStatement = new();
    }

    public void AddInput(string input)
    {
        var tempInput = new List<string>(input.Split('\n', StringSplitOptions.TrimEntries));
        _input.AddRange(tempInput);
    }

    public void Parse()
    {
        ParseTokens();
        CodeStatement = ParseStatement();
    }

    public void ParseTokens()
    {
        while (true)
        {
            Token token = NextToken();
            if (token.Type == TokenType.Error)
                throw new InvalidOperationException("Error Token: " + token);

            if (token.Type == TokenType.EOF)
                break;

            Tokens.Add(token);
        }

        for (int i = 0; i < Tokens.Count; i++)
        {
            if (Tokens[i].Type == TokenType.Function && Tokens[i + 2].Type == TokenType.VarName)
                Tokens[i + 2].Type = TokenType.FuncName;
            else if (Tokens.Count > i + 1 && Tokens[i].Type == TokenType.VarName && Tokens[i + 1].Type == TokenType.LeftParen)
                Tokens[i].Type = TokenType.FuncName;
        }
    }

    private Statement ParseStatement()
    {
        Statement statement = new() { IsCodeblock = true, CodeBlock = [new()], };
        while (_complexTokenPos < Tokens.Count)
        {
            Token token = Tokens[_complexTokenPos++];

            if (token.Type == TokenType.RightCodeBlock)
                break;

            if (token.Type == TokenType.LeftCodeBlock)
            {
                if (statement.CodeBlock[^1].CodeBlock.Count != 0 || statement.CodeBlock[^1].Tokens.Count != 0)
                    statement.CodeBlock.Add(new());

                statement.CodeBlock[^1] = ParseStatement();
                statement.CodeBlock.Add(new());
                continue;
            }

            if (token.Type == TokenType.LineEnd)
            {
                if (statement.CodeBlock[^1].CodeBlock.Count != 0 || statement.CodeBlock[^1].Tokens.Count != 0)
                    statement.CodeBlock.Add(new());
                continue;
            }

            statement.CodeBlock[^1].Tokens.Add(token);
        }
        if (statement.CodeBlock[^1].CodeBlock.Count == 0 && statement.CodeBlock[^1].Tokens.Count == 0)
            statement.CodeBlock.RemoveAt(statement.CodeBlock.Count - 1);
        return statement;
    }

    public Token NextToken()
    {
        if (_inputPos >= _input[_line].Length)
        {
            if (_line >= _input.Count)
                return new() { Type = TokenType.Error };
            _line++;
            _inputPos = 0;
        }

        Token ret = new() { Line = _line, Type = TokenType.Void };
        if (_line >= _input.Count)
        {
            ret.Type = TokenType.EOF;
            return ret;
        }

        int start = _inputPos;

        if (_input[_line] == "" || char.IsWhiteSpace(_input[_line][_inputPos]))
        {
            _inputPos++;
            return NextToken();
        }
        else if (char.IsLetter(_input[_line][_inputPos]) || _input[_line][_inputPos] == '_')
        {
            ret = ParseIdentifier(start);
        }
        else if (char.IsDigit(_input[_line][_inputPos]))
        {
            ret = ParseNumber(start);
        }
        else if (OperatorSet.Contains(_input[_line][start..(_inputPos + 1)]))
        {
            ret = ParseOperator(start);
        }
        else
        {
            ret = ParseSpecialChar(start);
        }

        return ret;
    }

    private Token ParseIdentifier(int start)
    {
        Token ret = new() { Line = _line };
        while (_inputPos < _input[_line].Length &&
               (char.IsLetterOrDigit(_input[_line][_inputPos]) || _input[_line][_inputPos] == '_'))
            _inputPos++;

        string value = _input[_line][start.._inputPos];

        if (KeywordsSet.Contains(value))
        {
            ret.Type = value switch
            {
                "func" => TokenType.Function,
                "return" => TokenType.Return,
                "while" => TokenType.WHILE,
                "continue" => TokenType.CONTINUE,
                "break" => TokenType.BREAK,
                "if" => TokenType.IF,
                "elif" => TokenType.ELIF,
                "else" => TokenType.ELSE,
                _ => TokenType.Error,
            };
        }
        else if (AccessSet.Contains(value))
            ret.Type = TokenType.VarAccess;
        else if (TypeSet.Contains(value))
            ret.Type = TokenType.VarType;
        else
            ret.Type = TokenType.VarName;

        ret.Value = value;
        return ret;
    }

    private Token ParseNumber(int start)
    {
        Token ret = new() { Line = _line };
        while (_inputPos < _input[_line].Length && char.IsDigit(_input[_line][_inputPos]))
            _inputPos++;
        ret.Type = TokenType.IntNumber;

        if (_inputPos < _input[_line].Length && _input[_line][_inputPos] == '.')
        {
            _inputPos++;
            while (_inputPos < _input[_line].Length && char.IsDigit(_input[_line][_inputPos]))
                _inputPos++;
            ret.Type = TokenType.FloatNumber;
        }

        ret.Value = _input[_line][start.._inputPos];
        return ret;
    }

    private Token ParseOperator(int start)
    {
        Token ret = new() { Line = _line };
        while (_inputPos < _input[_line].Length &&
               OperatorSet.Contains(_input[_line][start..(_inputPos + 1)]))
            _inputPos++;

        string opString = _input[_line][start.._inputPos];
        ret.Type = opString == "=" ? TokenType.AssignmentOperator : TokenType.Operator;
        ret.Value = opString;
        return ret;
    }

    private Token ParseSpecialChar(int start)
    {
        Token ret = new() { Line = _line };
        char c = _input[_line][_inputPos];

        switch (c)
        {
            case ';':
                ret.Type = TokenType.LineEnd;
                _inputPos++;
                break;
            case ',':
                ret.Type = TokenType.VarDelimiter;
                _inputPos++;
                break;
            case '(':
                ret.Type = TokenType.LeftParen;
                _inputPos++;
                break;
            case ')':
                ret.Type = TokenType.RightParen;
                _inputPos++;
                break;
            case '{':
                ret.Type = TokenType.LeftCodeBlock;
                _inputPos++;
                break;
            case '}':
                ret.Type = TokenType.RightCodeBlock;
                _inputPos++;
                break;
            case '[':
                ret.Type = TokenType.LeftRange;
                _inputPos++;
                break;
            case ']':
                ret.Type = TokenType.RightRange;
                _inputPos++;
                break;
            case '\"':
                return ParseString(start);
            default:
                ret.Type = TokenType.Error;
                _inputPos++;
                break;
        }

        ret.Value = _input[_line][start.._inputPos];
        return ret;
    }

    private Token ParseString(int start)
    {
        Token ret = new() { Line = _line, Type = TokenType.String };
        _inputPos++;
        while (_inputPos < _input[_line].Length && _input[_line][_inputPos] != '\"')
        {
            if (_input[_line][_inputPos] == '\\')
                _inputPos++;
            _inputPos++;
        }
        ret.Value = _input[_line][(start + 1).._inputPos];
        _inputPos++;
        return ret;
    }
}