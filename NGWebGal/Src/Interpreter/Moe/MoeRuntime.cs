using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Stack frame for function execution
/// </summary>
public class MoeStackFrame
{
    public Dictionary<string, MoeVariable> LVariable { get; set; } = [];
    public Stack<int> PC { get; set; } = new();
    public Stack<ProgramNode> CodeBlock { get; set; } = new();
    public Stack<List<string>> BlockVarName { get; set; } = new();
    public MoeVariable ReturnData { get; set; } = new();
}

/// <summary>
/// Runtime state for the interpreter
/// </summary>
public class MoeRuntime
{
    public Dictionary<string, MoeVariable> GVariables { get; set; } = [];
    public Dictionary<string, FunctionNode> Functions { get; set; } = [];
    public Stack<MoeStackFrame> ActiveTasks { get; set; } = new();

    public void Clear()
    {
        GVariables.Clear();
        Functions.Clear();
        ActiveTasks.Clear();
    }
}
