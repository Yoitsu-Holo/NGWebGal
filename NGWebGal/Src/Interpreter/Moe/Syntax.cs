using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Syntax parser for Moe scripts - converts tokens to AST
/// </summary>
public class Syntax
{
    public static List<FunctionNode> ParseFile(Statement fileStatement)
    {
        Statement temp = new();
        List<FunctionNode> functions = [];

        foreach (var statement in fileStatement.CodeBlock)
        {
            if (!statement.IsCodeblock && statement.Tokens[0].Type == TokenType.Function)
            {
                if (temp.CodeBlock.Count == 0)
                    temp.CodeBlock.Add(statement);
                else
                    throw new InvalidOperationException("Unexpected function header");
            }
            else if (statement.IsCodeblock)
            {
                if (temp.CodeBlock.Count == 1)
                {
                    temp.CodeBlock.Add(statement);
                    functions.Add(ParseFunction(temp));
                    temp = new();
                }
                else
                    throw new InvalidOperationException("Unexpected code block");
            }
            else
                throw new InvalidOperationException("Unexpected statement");
        }

        return functions;
    }

    public static FunctionNode ParseFunction(Statement funcStatement)
    {
        return new FunctionNode
        {
            Header = ParseFunctionHeader(funcStatement.CodeBlock[0].Tokens.GetEnumerator()),
            Body = ParseProgram(funcStatement.CodeBlock[1])
        };
    }

    public static ProgramNode ParseProgram(Statement programStatement)
    {
        return ParseProgramInternal(programStatement, null);
    }

    public static FunctionHeader ParseFunctionHeader(IEnumerator<Token> tokens)
    {
        if (!tokens.MoveNext())
            throw new InvalidOperationException("No function definition");
        if (tokens.Current.Type != TokenType.Function)
            throw new InvalidOperationException("Invalid function definition");

        if (!tokens.MoveNext())
            throw new InvalidOperationException("No return type");
        var varType = tokens.Current;
        if (varType.Type != TokenType.VarType)
            throw new InvalidOperationException($"Invalid return type: {varType}");

        if (!tokens.MoveNext())
            throw new InvalidOperationException("No function name");
        var funcName = tokens.Current;
        if (funcName.Type != TokenType.FuncName)
            throw new InvalidOperationException($"Invalid function name: {funcName}");

        var header = new FunctionHeader
        {
            ReturnType = varType.Value switch
            {
                "void" => MoeVariableType.Void,
                "int" => MoeVariableType.Int,
                "float" => MoeVariableType.Float,
                "string" => MoeVariableType.String,
                _ => MoeVariableType.Error,
            },
            FuncName = funcName.Value,
            CallParam = [],
        };

        if (!tokens.MoveNext() || tokens.Current.Type != TokenType.LeftParen)
            throw new InvalidOperationException("No parameter list");

        List<Token> param = [];
        bool close = false;
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type != TokenType.VarDelimiter && token.Type != TokenType.RightParen)
                param.Add(token);
            else
            {
                if (param.Count != 0)
                {
                    var variable = ParseSingleVar(param.GetEnumerator()).Variables[0];
                    header.CallParam.Add(variable);
                }
                param.Clear();
            }
            if (token.Type == TokenType.RightParen)
            {
                close = true;
                break;
            }
        }

        if (!close)
            throw new InvalidOperationException("Unclosed parameter list");

        return header;
    }

    public static VariableDefineNode ParseSingleVar(IEnumerator<Token> tokens)
    {
        var ret = new VariableDefineNode();
        var info = VarType(tokens);

        List<Token> tempToken = [];
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type != TokenType.VarDelimiter)
                tempToken.Add(token);
            if (token.Type == TokenType.VarDelimiter)
                break;
        }

        var variable = new MoeVariable(info.Access, info.Type)
        {
            Name = tempToken[0].Value,
            Dimension = VarDimension(tempToken.GetRange(1, tempToken.Count - 1).GetEnumerator()),
        };
        ret.Variables.Add(variable);
        return ret;
    }

    public static VariableDefineNode ParseMultiVar(IEnumerator<Token> tokens)
    {
        var ret = new VariableDefineNode();
        var info = VarType(tokens);

        List<Token> tempToken = [];
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type != TokenType.VarDelimiter)
                tempToken.Add(token);

            if (token.Type == TokenType.VarDelimiter)
            {
                var variable = new MoeVariable(info.Access, info.Type)
                {
                    Name = tempToken[0].Value,
                    Dimension = VarDimension(tempToken.GetRange(1, tempToken.Count - 1).GetEnumerator()),
                };
                ret.Variables.Add(variable);
                tempToken.Clear();
            }
        }
        if (tempToken.Count != 0)
        {
            var variable = new MoeVariable(info.Access, info.Type)
            {
                Name = tempToken[0].Value,
                Dimension = VarDimension(tempToken.GetRange(1, tempToken.Count - 1).GetEnumerator()),
            };
            ret.Variables.Add(variable);
        }
        return ret;
    }

    public static VarTypeNode VarType(IEnumerator<Token> tokens)
    {
        if (!tokens.MoveNext()) return new();
        var access = tokens.Current;
        if (access.Type != TokenType.VarAccess)
            throw new InvalidOperationException($"Expected access modifier: {access}");

        if (!tokens.MoveNext())
            throw new InvalidOperationException("Expected type");
        var type = tokens.Current;
        if (type.Type != TokenType.VarType)
            throw new InvalidOperationException($"Expected type: {type}");

        return new VarTypeNode
        {
            Access = access.Value switch
            {
                "var" => MoeVariableAccess.Variable,
                "const" => MoeVariableAccess.Const,
                "static" => MoeVariableAccess.Static,
                "ref" => MoeVariableAccess.Reference,
                _ => MoeVariableAccess.Error,
            },
            Type = type.Value switch
            {
                "int" => MoeVariableType.Int,
                "float" => MoeVariableType.Float,
                "string" => MoeVariableType.String,
                _ => MoeVariableType.Error,
            },
        };
    }

    public static List<int> VarDimension(IEnumerator<Token> tokens)
    {
        int state = 0;
        List<int> dimension = [];
        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (state == 0 && token.Type == TokenType.LeftRange)
                state = 1;
            else if (state == 1 && token.Type == TokenType.IntNumber)
            {
                dimension.Add(Convert.ToInt32(token.Value));
                state = 2;
            }
            else if (state == 2 && token.Type == TokenType.RightRange)
                state = 0;
            else
                throw new InvalidOperationException("Invalid array dimension: " + token);
        }
        return dimension;
    }

    private static ProgramNode ParseProgramInternal(Statement funcStatement, ConditionalNode? preWhile)
    {
        var programNode = new ProgramNode();

        for (int i = 0; i < funcStatement.CodeBlock.Count; i++)
        {
            var statement = funcStatement.CodeBlock[i];
            var node = new ASTNode();
            var tokens = statement.Tokens;

            if (tokens[0].Type == TokenType.VarAccess)
            {
                node.ASTType = ASTNodeType.VariableDeclaration;
                node.VarDefine = ParseVariableDefine(statement);
                programNode.Statements.Add(node);
            }
            else if (tokens[0].Type == TokenType.IF)
            {
                if (i + 1 >= funcStatement.CodeBlock.Count)
                    throw new InvalidOperationException("Missing if body");
                var conditional = ParseConditional(statement, funcStatement.CodeBlock[i + 1], preWhile);
                i++;
                node.ASTType = ASTNodeType.Conditional;
                node.IfCase = new IfCaseNode { If = [conditional] };
                programNode.Statements.Add(node);
            }
            else if (tokens[0].Type == TokenType.ELIF)
            {
                node = programNode.Statements[^1];
                if (node.ASTType != ASTNodeType.Conditional || node.IfCase is null)
                    throw new InvalidOperationException("No preceding if");
                if (i + 1 >= funcStatement.CodeBlock.Count)
                    throw new InvalidOperationException("Missing elif body");
                var conditional = ParseConditional(statement, funcStatement.CodeBlock[i + 1], preWhile);
                i++;
                node.IfCase.If.Add(conditional);
                programNode.Statements[^1] = node;
            }
            else if (tokens[0].Type == TokenType.ELSE)
            {
                node = programNode.Statements[^1];
                if (node.ASTType != ASTNodeType.Conditional || node.IfCase is null)
                    throw new InvalidOperationException("No preceding if");
                if (i + 1 >= funcStatement.CodeBlock.Count)
                    throw new InvalidOperationException("Missing else body");
                var conditional = ParseConditional(statement, funcStatement.CodeBlock[i + 1], preWhile);
                i++;
                node.IfCase.If.Add(conditional);
                programNode.Statements[^1] = node;
            }
            else
            {
                ParseRemainingStatements(funcStatement, ref i, tokens, node, programNode, preWhile);
            }
        }
        return programNode;
    }

    private static void ParseRemainingStatements(Statement funcStatement, ref int i,
        List<Token> tokens, ASTNode node, ProgramNode programNode, ConditionalNode? preWhile)
    {
        if (tokens[0].Type == TokenType.WHILE)
        {
            var conditional = ParseConditional(tokens, funcStatement.CodeBlock[i + 1], preWhile);
            i++;
            node.ASTType = ASTNodeType.Loop;
            node.Loop = new LoopNode { Loop = conditional };
            programNode.Statements.Add(node);
        }
        else if (tokens[0].Type == TokenType.CONTINUE || tokens[0].Type == TokenType.BREAK)
        {
            if (preWhile is null)
                throw new InvalidOperationException("No preceding while loop");
            node.ASTType = ASTNodeType.LoopControl;
            node.LoopControl = new LoopControlNode
            {
                Loop = preWhile,
                ContinueFlag = tokens[0].Type == TokenType.CONTINUE
            };
            programNode.Statements.Add(node);
        }
        else if (tokens[0].Type == TokenType.VarName)
        {
            node.ASTType = ASTNodeType.Assignment;
            node.Assignment = ParseAssignment(tokens);
            programNode.Statements.Add(node);
        }
        else if (tokens[0].Type == TokenType.FuncName)
        {
            node.ASTType = ASTNodeType.FunctionCall;
            node.FunctionCall = ParseFunctionCall(tokens);
            programNode.Statements.Add(node);
        }
        else if (tokens[0].Type == TokenType.Return)
        {
            node.ASTType = ASTNodeType.Return;
            node.Return = new ReturnNode
            {
                ReturnExp = ParseExpression(new TokenEnumerator(tokens.GetRange(1, tokens.Count - 1))),
            };
            programNode.Statements.Add(node);
        }
        else
        {
            throw new InvalidOperationException($"Unknown statement: {tokens[0]}");
        }
    }

    public static ConditionalNode ParseConditional(Statement statement, Statement program, ConditionalNode? preWhile)
    {
        return ParseConditional(statement.Tokens, program, preWhile);
    }

    public static ConditionalNode ParseConditional(List<Token> tokens, Statement program, ConditionalNode? preWhile)
    {
        if (tokens[0].Type != TokenType.ELSE)
            if (tokens.Count < 4 || tokens[1].Type != TokenType.LeftParen || tokens[^1].Type != TokenType.RightParen)
                throw new InvalidOperationException("Invalid conditional syntax");

        var conditional = new ConditionalNode();

        if (tokens[0].Type != TokenType.ELSE)
            conditional.Conditional = ParseExpression(new TokenEnumerator(tokens.GetRange(2, tokens.Count - 3)));
        else
            conditional.Conditional.Tokens = [new() { Type = OperatorType.Number, Number = 1 }];

        if (tokens[0].Type == TokenType.WHILE)
            conditional.Program = ParseProgramInternal(program, conditional);
        else
            conditional.Program = ParseProgramInternal(program, preWhile);

        return conditional;
    }

    public static VariableDefineNode ParseVariableDefine(Statement statement)
    {
        return ParseMultiVar(statement.Tokens.GetEnumerator());
    }

    public static AssignmentNode ParseAssignment(List<Token> tokens)
    {
        var assignment = new AssignmentNode();
        int demerger;
        for (demerger = 0; tokens[demerger].Type != TokenType.AssignmentOperator; demerger++) ;

        var preTokens = tokens.GetRange(0, demerger);
        var expTokens = tokens.GetRange(demerger + 1, tokens.Count - demerger - 1);

        assignment.LeftVar = new VariableInfo { Name = preTokens[0].Value };

        var range = new TokenEnumerator(preTokens.GetRange(1, preTokens.Count - 1));
        while (range.TryGetNext(out Token? t))
        {
            if (t!.Type == TokenType.LeftRange)
                assignment.LeftVar.Index.Add(RangeExpression(range));
            else
                break;
        }

        if (expTokens.Count >= 2 && expTokens[0].Type == TokenType.FuncName)
            assignment.FuncCall = ParseFunctionCall(expTokens);
        else if (preTokens[0].Type == TokenType.VarName)
            assignment.RightExp = ParseExpression(new TokenEnumerator(expTokens));

        return assignment;
    }

    public static FunctionCallNode ParseFunctionCall(List<Token> tokens)
    {
        var functionCall = new FunctionCallNode();
        if (tokens[0].Type != TokenType.FuncName)
            throw new InvalidOperationException("Function call must start with function name");
        functionCall.FunctionName = tokens[0].Value;

        var restTokens = tokens.GetRange(2, tokens.Count - 3);
        if (restTokens.Count != 0)
        {
            if (restTokens[0].Type == TokenType.LeftRange && restTokens[^1].Type == TokenType.RightRange)
            {
                functionCall.CallType = FuncCallType.Keyword;
                functionCall.KeywordParams = KeywordCall(restTokens.GetRange(1, restTokens.Count - 2));
            }
            else
            {
                functionCall.CallType = FuncCallType.Positional;
                functionCall.PositionalParams = PositionalCall(restTokens);
            }
        }
        return functionCall;
    }

    public static List<ExpressionNode> PositionalCall(List<Token> tokens)
    {
        List<ExpressionNode> paramList = [];
        List<Token> expTokens = [];
        for (int i = 0; i <= tokens.Count; i++)
        {
            if (i == tokens.Count || tokens[i].Type == TokenType.VarDelimiter)
            {
                paramList.Add(ParseExpression(new TokenEnumerator(expTokens)));
                expTokens.Clear();
            }
            else
                expTokens.Add(tokens[i]);
        }
        return paramList;
    }

    public static Dictionary<string, ExpressionNode> KeywordCall(List<Token> tokens)
    {
        Dictionary<string, ExpressionNode> paramList = [];
        List<Token> expTokens = [];
        for (int i = 0; i <= tokens.Count; i++)
        {
            if (i == tokens.Count || tokens[i].Type == TokenType.VarDelimiter)
            {
                var assignment = ParseAssignment(expTokens);
                if (assignment.RightExp is not null)
                    paramList[assignment.LeftVar.Name] = assignment.RightExp;
                expTokens.Clear();
            }
            else
                expTokens.Add(tokens[i]);
        }
        return paramList;
    }

    public static ExpressionNode ParseExpression(TokenEnumerator tokens)
    {
        List<ExpressionToken> math = [];
        int opCount = 1;

        while (tokens.MoveNext())
        {
            var token = tokens.Current;
            if (token.Type == TokenType.IntNumber && opCount != 0)
            {
                math.Add(new() { Type = OperatorType.Number, Number = int.Parse(token.Value) });
                opCount = 0;
            }
            else if (token.Type == TokenType.FloatNumber && opCount != 0)
            {
                math.Add(new() { Type = OperatorType.Number, Number = double.Parse(token.Value) });
                opCount = 0;
            }
            else if (token.Type == TokenType.String && opCount != 0)
            {
                math.Add(new() { Type = OperatorType.String, String = token.Value });
                opCount = 0;
            }
            else if (token.Type == TokenType.VarName && opCount != 0)
            {
                var variable = new VariableInfo { Name = token.Value };
                while (tokens.TryGetNext(out Token? t))
                {
                    if (t!.Type == TokenType.LeftRange)
                        variable.Index.Add(RangeExpression(tokens));
                    else
                        break;
                }
                math.Add(new() { Type = OperatorType.Variable, Var = variable });
                opCount = 0;
            }
            else if (token.Type == TokenType.LeftRange || token.Type == TokenType.RightRange)
                break;
            else if (token.Type == TokenType.LeftParen && opCount != 0)
            {
                math.Add(new() { Type = OperatorType.LeftParen });
                opCount = 1;
            }
            else if (token.Type == TokenType.RightParen && opCount == 0)
            {
                math.Add(new() { Type = OperatorType.RightParen });
                opCount = 0;
            }
            else if (token.Type == TokenType.Operator)
            {
                math.Add(ParseOperatorToken(token.Value, opCount));
                opCount++;
            }
            else
                throw new InvalidOperationException("Invalid expression: " + token);
        }
        return new ExpressionNode { Tokens = math };
    }

    private static ExpressionToken ParseOperatorToken(string value, int opCount)
    {
        return new ExpressionToken
        {
            Type = value switch
            {
                "+" => OperatorType.ADD,
                "-" => opCount == 1 ? OperatorType.Minus : OperatorType.SUB,
                "*" => OperatorType.MUL,
                "/" => OperatorType.DIV,
                "%" => OperatorType.MOD,
                "^^" => OperatorType.POW,
                "<<" => OperatorType.SHL,
                ">>" => OperatorType.SHR,
                "==" => OperatorType.EQ,
                "!=" => OperatorType.NEQ,
                ">=" => OperatorType.EGT,
                "<=" => OperatorType.ELT,
                ">" => OperatorType.GT,
                "<" => OperatorType.LT,
                "&&" => OperatorType.AND,
                "||" => OperatorType.OR,
                "!" => OperatorType.NOT,
                "&" => OperatorType.bAND,
                "|" => OperatorType.bOR,
                "~" => OperatorType.bNOT,
                "^" => OperatorType.XOR,
                _ => throw new InvalidOperationException("Invalid operator"),
            },
        };
    }

    public static ExpressionNode RangeExpression(TokenEnumerator tokens)
    {
        if (!tokens.MoveNext())
            throw new InvalidOperationException("No more tokens");
        if (tokens.Current.Type != TokenType.LeftRange)
            throw new InvalidOperationException($"Expected '[': {tokens.Current}");

        var expNode = ParseExpression(tokens);

        if (tokens.Current.Type != TokenType.RightRange)
            throw new InvalidOperationException($"Expected ']': {tokens.Current}");

        return expNode;
    }
}
