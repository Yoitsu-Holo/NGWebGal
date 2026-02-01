using System;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Variable type enumeration for Moe script variables
/// </summary>
public enum MoeVariableType
{
    Void,
    Int,
    Float,
    String,
    Error,
}

/// <summary>
/// Variable access modifier enumeration
/// </summary>
public enum MoeVariableAccess
{
    Const,
    Static,
    Variable,
    Reference,
    Error,
}

/// <summary>
/// Token types for lexical analysis
/// </summary>
public enum TokenType
{
    Void,

    VarAccess,
    VarType,
    Function,
    Return,
    FuncName,
    VarName,
    VarDelimiter,
    IntNumber,
    FloatNumber,
    String,

    IF,
    ELIF,
    ELSE,
    WHILE,
    CONTINUE,
    BREAK,
    Operator,
    AssignmentOperator,

    LeftParen,
    RightParen,
    LeftRange,
    RightRange,
    LeftCodeBlock,
    RightCodeBlock,

    LineEnd,
    EOF,

    Error,
}

/// <summary>
/// Operator types for expression evaluation
/// </summary>
public enum OperatorType
{
    Void,
    POW,
    MUL,
    DIV,
    MOD,
    ADD,
    SUB,
    bAND,
    bOR,
    XOR,
    bNOT,
    SHL,
    SHR,

    EQ,
    NEQ,
    GT,
    LT,
    EGT,
    ELT,
    AND,
    OR,
    NOT,

    Minus,

    LeftParen,
    RightParen,
    LeftRange,
    RightRange,
    Variable,
    Number,
    String,
    Error,
}

/// <summary>
/// AST node type enumeration
/// </summary>
public enum ASTNodeType
{
    Void,
    VariableDeclaration,
    Conditional,
    Loop,
    LoopControl,
    Assignment,
    FunctionCall,
    Program,
    Return,
    Error,
}

/// <summary>
/// Function call type enumeration
/// </summary>
public enum FuncCallType
{
    Positional,
    Keyword,
}

/// <summary>
/// ELF segment type enumeration
/// </summary>
public enum MoeELFSegment
{
    Void,
    FILE,
    TABLE,
    DATA,
    FORM,
    START,
    Error,
}

/// <summary>
/// File type enumeration for resource management
/// </summary>
[Flags]
public enum MoeFileType : ulong
{
    Void = 0,
    Image = 0x000F,
    Audio = 0x00F0,
    Text = 0x0F00,
    Bin = 0xF000,

    Image_png = 0x1,
    Image_jpg = 0x2,
    Image_bmp = 0x4,

    Audio_wav = 0x10,
    Audio_mp3 = 0x20,
    Audio_flac = 0x40,
    Audio_ogg = 0x8,

    Text_script = 0x100,
    Text_form = 0x200,
    Text_opera = 0x400,

    Bin_font = 0x1000,
    Bin_bin = 0x2000,

    Error,
}
