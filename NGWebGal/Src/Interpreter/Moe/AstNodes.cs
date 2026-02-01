using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Lexical token
/// </summary>
public class Token
{
    public TokenType Type { get; set; } = TokenType.Void;
    public string Value { get; set; } = "";
    public int Line { get; set; } = 0;

    public override string ToString() => $"{Line}:{Type}: {Value}";
}

/// <summary>
/// Statement containing tokens and nested code blocks
/// </summary>
public class Statement
{
    public bool IsCodeblock { get; set; } = false;
    public List<Token> Tokens { get; set; } = [];
    public List<Statement> CodeBlock { get; set; } = [];

    public override string ToString()
    {
        string ret = "";
        foreach (var token in Tokens)
            ret += token.Type + ":" + token.Value + " ";
        return ret;
    }
}

/// <summary>
/// Variable type information node
/// </summary>
public class VarTypeNode
{
    public MoeVariableType Type { get; set; }
    public MoeVariableAccess Access { get; set; }
}

/// <summary>
/// Variable reference information
/// </summary>
public class VariableInfo
{
    public string Name { get; set; } = "";
    public List<ExpressionNode> Index { get; set; } = [];

    public override string ToString()
    {
        string ret = Name;
        foreach (var sz in Index)
            ret += $"[{sz}]";
        return ret;
    }
}

/// <summary>
/// Expression token for expression evaluation
/// </summary>
public class ExpressionToken
{
    public OperatorType Type { get; set; } = OperatorType.Void;
    public VariableInfo Var { get; set; } = new();
    public object Number { get; set; } = 0;
    public string String { get; set; } = "";

    public override string ToString()
    {
        return Type switch
        {
            OperatorType.LeftParen => "( ",
            OperatorType.RightParen => " )",
            OperatorType.Number => $"{Number}",
            OperatorType.String => $"{String}",
            OperatorType.Variable => $"{Var}",
            _ => $"{Type}"
        };
    }
}

/// <summary>
/// Expression node containing tokens
/// </summary>
public class ExpressionNode
{
    public List<ExpressionToken> Tokens { get; set; } = [];

    public bool IsVarName
    {
        get
        {
            if (Tokens.Count == 1 && Tokens[0].Type == OperatorType.Variable)
            {
                var varinfo = Tokens[0].Var;
                if (varinfo.Index.Count == 0)
                    return true;
            }
            return false;
        }
    }

    public override string ToString()
    {
        string ret = " ";
        foreach (var exp in Tokens)
            ret += $"{exp} ";
        return ret;
    }
}

/// <summary>
/// Function call node
/// </summary>
public class FunctionCallNode
{
    public string FunctionName { get; set; } = "";
    public FuncCallType CallType { get; set; } = FuncCallType.Positional;
    public List<ExpressionNode> PositionalParams { get; set; } = [];
    public Dictionary<string, ExpressionNode> KeywordParams { get; set; } = [];

    public override string ToString()
    {
        string ret = $"CallFunc: {FunctionName}\t: ";
        if (CallType == FuncCallType.Positional)
        {
            ret += "Positional Call\n";
            foreach (var param in PositionalParams)
                ret += $"\t{param}\n";
        }
        else
        {
            ret += "Keyword Call\n";
            foreach (var param in KeywordParams)
                ret += $"\t{param}\n";
        }
        return ret;
    }
}

/// <summary>
/// Assignment node
/// </summary>
public class AssignmentNode
{
    public VariableInfo LeftVar { get; set; } = new();
    public ExpressionNode? RightExp { get; set; }
    public FunctionCallNode? FuncCall { get; set; }

    public override string ToString()
    {
        string ret = LeftVar + " = ";
        if (RightExp is not null)
            ret += RightExp;
        else if (FuncCall is not null)
            ret += FuncCall;
        ret += "\n";
        return ret;
    }
}

/// <summary>
/// Conditional node for if/while statements
/// </summary>
public class ConditionalNode
{
    public ExpressionNode Conditional { get; set; } = new();
    public ProgramNode Program { get; set; } = new();

    public override string ToString() => $"{Conditional} Hash:{GetHashCode()}\n{Program}";
}

/// <summary>
/// If-case node containing multiple conditional branches
/// </summary>
public class IfCaseNode
{
    public List<ConditionalNode> If { get; set; } = [];

    public override string ToString()
    {
        string ret = "";
        foreach (var ifcase in If)
            ret += "IF: " + ifcase;
        return ret;
    }
}

/// <summary>
/// Loop node
/// </summary>
public class LoopNode
{
    public ConditionalNode Loop { get; set; } = new();
    public override string ToString() => "WHILE: " + Loop;
}

/// <summary>
/// Loop control node (break/continue)
/// </summary>
public class LoopControlNode
{
    public ConditionalNode Loop { get; set; } = new();
    public bool ContinueFlag { get; set; } = true;
    public override string ToString() => (ContinueFlag ? "CONTINUE" : "BREAK") + " Hash: " + Loop.GetHashCode() + "\n";
}

/// <summary>
/// Variable definition node
/// </summary>
public class VariableDefineNode
{
    public List<MoeVariable> Variables { get; set; } = [];

    public override string ToString()
    {
        string ret = "";
        foreach (var variable in Variables)
            ret += $"{variable}\n";
        return ret;
    }
}

/// <summary>
/// Return node
/// </summary>
public class ReturnNode
{
    public ExpressionNode ReturnExp { get; set; } = new();
    public override string ToString() => "Return: " + ReturnExp;
}

/// <summary>
/// Program node containing statements
/// </summary>
public class ProgramNode
{
    public List<ASTNode> Statements { get; set; } = [];

    public override string ToString()
    {
        string ret = "";
        foreach (var ast in Statements)
            ret += ast.ToString();
        if (Statements.Count == 0)
            ret += "Without Program";
        return ret;
    }
}

/// <summary>
/// AST node - the main executable unit
/// </summary>
public class ASTNode
{
    public ASTNodeType ASTType { get; set; } = ASTNodeType.Void;
    public VariableDefineNode? VarDefine { get; set; }
    public AssignmentNode? Assignment { get; set; }
    public IfCaseNode? IfCase { get; set; }
    public LoopNode? Loop { get; set; }
    public LoopControlNode? LoopControl { get; set; }
    public FunctionCallNode? FunctionCall { get; set; }
    public ProgramNode? Program { get; set; }
    public ReturnNode? Return { get; set; }

    public override string ToString()
    {
        return ASTType switch
        {
            ASTNodeType.Void => "$ Void AST\n",
            ASTNodeType.VariableDeclaration when VarDefine is not null => VarDefine.ToString(),
            ASTNodeType.FunctionCall when FunctionCall is not null => FunctionCall.ToString(),
            ASTNodeType.Assignment when Assignment is not null => Assignment.ToString(),
            ASTNodeType.Conditional when IfCase is not null => IfCase.ToString(),
            ASTNodeType.Loop when Loop is not null => Loop.ToString(),
            ASTNodeType.LoopControl when LoopControl is not null => LoopControl.ToString(),
            ASTNodeType.Program when Program is not null => Program.ToString(),
            ASTNodeType.Return when Return is not null => Return.ToString(),
            _ => ">>> error line\n"
        };
    }
}

/// <summary>
/// Function header
/// </summary>
public class FunctionHeader
{
    public string FileName { get; set; } = "main.moe";
    public string FuncName { get; set; } = "main";
    public MoeVariableType ReturnType { get; set; }
    public List<MoeVariable> CallParam { get; set; } = [];

    public override string ToString()
    {
        string ret = $"FileName: {FileName}\t FuncName: {FuncName}\t ReturnType: {ReturnType}";
        foreach (var call in CallParam)
            ret += $"\n\tParam: {call}";
        if (CallParam.Count == 0)
            ret += "\n\tParam: Null";
        return ret;
    }
}

/// <summary>
/// Function node containing header and body
/// </summary>
public class FunctionNode
{
    public FunctionHeader Header { get; set; } = new();
    public ProgramNode Body { get; set; } = new();
    public Dictionary<string, MoeVariable> SVariable { get; set; } = [];

    public override string ToString() => Header.ToString() + "\n" + Body.ToString() + "\n";
}
