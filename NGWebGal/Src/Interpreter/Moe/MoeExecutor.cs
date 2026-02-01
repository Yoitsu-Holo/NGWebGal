using System;
using System.Collections.Generic;
using System.Reflection;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Executor for Moe interpreter - handles statement execution
/// </summary>
public partial class MoeInterpreter
{
    /// <summary>
    /// Call a function with parameters
    /// </summary>
    public object Call(FunctionNode function, List<MoeVariable> paramList)
    {
        var header = function.Header;
        var body = function.Body;
        var frame = new MoeStackFrame();
        _runtime.ActiveTasks.Push(frame);

        if (header.CallParam.Count != paramList.Count)
        {
            Console.WriteLine($"Parameter count mismatch {function.Header}\n {header.CallParam.Count} : {paramList.Count}");
            return new object();
        }

        Console.WriteLine(function.Header);
        for (int i = 0; i < header.CallParam.Count; i++)
        {
            if (header.CallParam[i].Type == paramList[i].Type)
            {
                frame.LVariable[header.CallParam[i].Name] = new MoeVariable();
                frame.LVariable[header.CallParam[i].Name].CloneFrom(paramList[i]);
            }
            else if (paramList[i].Type == MoeVariableType.Void)
                continue;
            else
            {
                Console.WriteLine($"Parameter type mismatch {header.CallParam[i].Type}:{paramList[i].Type}\n{function.Header}");
                return new object();
            }
        }

        frame.PC.Push(0);
        frame.CodeBlock.Push(body);
        frame.BlockVarName.Push([]);

        Run(frame);
        var returnData = frame.ReturnData;
        _runtime.ActiveTasks.Pop();

        if (returnData.Size != 0)
            return returnData[0];
        return returnData;
    }

    /// <summary>
    /// Call a function by name
    /// </summary>
    public object Call(FunctionCallNode function)
    {
        // System call - keyword parameters only
        if (function.FunctionName.StartsWith("_"))
        {
            var parametersList = new Dictionary<string, MoeVariable>();
            foreach (var (name, exp) in function.KeywordParams)
                parametersList[name] = ParseCallValue(exp);

            var obj = UserCall(function.FunctionName[1..], parametersList);
            return obj ?? new object();
        }

        // Non-system call - try local call
        var paramList = new List<MoeVariable>();
        var functionNode = _runtime.Functions[function.FunctionName];

        if (function.CallType == FuncCallType.Positional)
        {
            foreach (var exp in function.PositionalParams)
                paramList.Add(ParseCallValue(exp));
        }
        else if (function.CallType == FuncCallType.Keyword)
        {
            var callParam = functionNode.Header.CallParam;
            foreach (var param in callParam)
            {
                string paramName = param.Name;
                if (function.KeywordParams.TryGetValue(paramName, out var exp))
                    paramList.Add(ParseCallValue(exp));
                else
                    paramList.Add(new MoeVariable());
            }
        }

        return Call(functionNode, paramList);
    }

    /// <summary>
    /// User call handler
    /// </summary>
    public object? UserCall(string usercall, Dictionary<string, MoeVariable> paramList)
    {
        if (usercall.Length == 0)
        {
            Console.WriteLine($"Invalid user function name: {usercall}");
            return null;
        }

        if (usercall.StartsWith("_"))
        {
            SysCall(usercall[1..], paramList);
            return null;
        }

        Console.WriteLine("User function not implemented");
        return InnerCall(usercall, typeof(MoeSyscall), paramList);
    }

    /// <summary>
    /// System call handler
    /// </summary>
    public object? SysCall(string syscall, Dictionary<string, MoeVariable> paramList)
    {
        if (syscall.Length == 0)
        {
            Console.WriteLine($"Invalid system function name: {syscall}");
            return null;
        }

        return InnerCall(syscall, typeof(MoeSyscall), paramList);
    }

    /// <summary>
    /// Inner call using reflection
    /// </summary>
    public object? InnerCall(string syscall, Type type, Dictionary<string, MoeVariable> paramList)
    {
        var method = type.GetMethod(syscall, BindingFlags.Public | BindingFlags.Static);
        if (method is null)
        {
            Console.WriteLine($"Unknown system function: {syscall}");
            return null;
        }

        var parameters = method.GetParameters();
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            if (paramList.TryGetValue(parameters[i].Name!, out var value))
                args[i] = value;
            else
                args[i] = new MoeVariable();
        }

        return method.Invoke(null, args);
    }

    /// <summary>
    /// Run a code block
    /// </summary>
    public void Run(MoeStackFrame frame)
    {
        while (frame.PC.Count != 0)
        {
            int index = frame.PC.Peek();
            var now = frame.CodeBlock.Peek();

            if (index >= now.Statements.Count) // End of code block
            {
                foreach (var varName in frame.BlockVarName.Peek())
                    frame.LVariable.Remove(varName);
                frame.PC.Pop();
                frame.CodeBlock.Pop();
                frame.BlockVarName.Pop();
                continue;
            }

            var ast = now.Statements[index];

            frame.PC.Pop();
            frame.PC.Push(index + 1); // Next statement

            if (ast.ASTType != ASTNodeType.Void)
                Execute(ast, frame); // Execute current statement
        }
    }

    /// <summary>
    /// Execute a single AST node
    /// </summary>
    public void Execute(ASTNode ast, MoeStackFrame frame)
    {
        if (ast.ASTType == ASTNodeType.Void)
            return;
        if (ast.ASTType == ASTNodeType.Error)
        {
            Console.WriteLine("Error node in AST");
            return;
        }

        // Program block
        if (ast.ASTType == ASTNodeType.Program && ast.Program is not null)
        {
            frame.PC.Push(0);
            frame.CodeBlock.Push(ast.Program);
            frame.BlockVarName.Push([]);
        }
        // Variable declaration
        else if (ast.ASTType == ASTNodeType.VariableDeclaration && ast.VarDefine is not null)
        {
            ExecuteVariableDeclaration(ast.VarDefine, frame);
        }
        // Assignment
        else if (ast.ASTType == ASTNodeType.Assignment && ast.Assignment is not null)
        {
            ExecuteAssignment(ast.Assignment, frame);
        }
        // Conditional (if/else)
        else if (ast.ASTType == ASTNodeType.Conditional && ast.IfCase is not null)
        {
            ExecuteConditional(ast.IfCase, frame);
        }
        // Loop
        else if (ast.ASTType == ASTNodeType.Loop && ast.Loop is not null)
        {
            ExecuteLoop(ast.Loop, frame);
        }
        // Loop control (break/continue)
        else if (ast.ASTType == ASTNodeType.LoopControl && ast.LoopControl is not null)
        {
            Console.WriteLine("Loop control not implemented");
        }
        // Function call
        else if (ast.ASTType == ASTNodeType.FunctionCall && ast.FunctionCall is not null)
        {
            Call(ast.FunctionCall);
        }
        // Return statement
        else if (ast.ASTType == ASTNodeType.Return && ast.Return is not null)
        {
            ExecuteReturn(ast.Return, frame);
        }
        else
        {
            Console.WriteLine($"Unknown AST node type: {ast}");
        }
    }

    private void ExecuteVariableDeclaration(VariableDefineNode varDefine, MoeStackFrame frame)
    {
        foreach (var variable in varDefine.Variables)
        {
            if (frame.BlockVarName.Peek().Contains(variable.Name) || _runtime.GVariables.ContainsKey(variable.Name))
            {
                Console.WriteLine($"Variable name already defined: {variable}");
                continue;
            }

            frame.BlockVarName.Peek().Add(variable.Name);
            var lvar = new MoeVariable();
            lvar.CloneFrom(variable);
            frame.LVariable.Add(variable.Name, lvar);
        }
    }

    private void ExecuteAssignment(AssignmentNode assignment, MoeStackFrame frame)
    {
        var leftVar = assignment.LeftVar;
        var rightExp = assignment.RightExp;
        MoeVariable? left = null;
        object right = new();

        if (left is null)
            _runtime.GVariables.TryGetValue(leftVar.Name, out left);
        if (left is null)
            frame.LVariable.TryGetValue(leftVar.Name, out left);

        if (left is null)
        {
            Console.WriteLine($"Variable not found: {leftVar.Name}");
            return;
        }

        if (left.Access == MoeVariableAccess.Const)
        {
            Console.WriteLine($"Cannot modify constant: {left}");
            return;
        }

        if (rightExp is not null)
        {
            var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
            right = executor.Parse(rightExp);
        }
        else if (assignment.FuncCall is not null)
            right = Call(assignment.FuncCall);

        var index = new List<int>();
        foreach (var exp in leftVar.Index)
        {
            var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
            index.Add((int)executor.Parse(exp));
        }

        if (right is int && left.Type == MoeVariableType.Int)
            left[index] = right;
        else if (right is float && left.Type == MoeVariableType.Float)
            left[index] = right;
        else if (right is string && left.Type == MoeVariableType.String)
            left[index] = right;
        else
            Console.WriteLine($"Type mismatch\nRight Type: {right.GetType()}\n{left} <:: {right}");
    }

    private void ExecuteConditional(IfCaseNode ifCase, MoeStackFrame frame)
    {
        for (int i = 0; i < ifCase.If.Count; i++)
        {
            var conditional = ifCase.If[i];
            var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
            var result = executor.Parse(conditional.Conditional);

            if (result is not int && result is not float)
            {
                Console.WriteLine("Conditional expression must be numeric");
                continue;
            }

            if (result is int resultInt && resultInt == 0)
                continue;
            if (result is float resultFloat && resultFloat == 0)
                continue;

            frame.PC.Push(0);
            frame.CodeBlock.Push(conditional.Program);
            frame.BlockVarName.Push([]);
            break;
        }
    }

    private void ExecuteLoop(LoopNode loop, MoeStackFrame frame)
    {
        var conditional = loop.Loop;
        var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
        var result = executor.Parse(conditional.Conditional);

        if (result is not int && result is not float)
        {
            Console.WriteLine("Loop condition must be numeric");
            return;
        }

        if (result is int resultInt && resultInt == 0)
            return;
        if (result is float resultFloat && resultFloat == 0)
            return;

        int index = frame.PC.Pop();
        frame.PC.Push(index - 1);

        frame.PC.Push(0);
        frame.CodeBlock.Push(conditional.Program);
        frame.BlockVarName.Push([]);
    }

    private void ExecuteReturn(ReturnNode returnNode, MoeStackFrame frame)
    {
        object obj;
        if (returnNode.ReturnExp.Tokens.Count > 0)
        {
            var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
            obj = executor.Parse(returnNode.ReturnExp);
        }
        else
        {
            obj = new object();
        }
        frame.ReturnData = new MoeVariable(obj);
        frame.PC.Clear();
    }

    /// <summary>
    /// Parse call value from expression
    /// </summary>
    public MoeVariable ParseCallValue(ExpressionNode exp)
    {
        if (exp.IsVarName)
        {
            string varName = exp.Tokens[0].Var.Name;
            if (_runtime.GVariables.TryGetValue(varName, out var gvalue))
                return gvalue;
            else if (_runtime.ActiveTasks.Peek().LVariable.TryGetValue(varName, out var lvalue))
                return lvalue;
            else
                Console.WriteLine("Static parameter passing not fully implemented");
            return new MoeVariable();
        }
        else
        {
            var frame = _runtime.ActiveTasks.Peek();
            var executor = new ExpressionsExecutor(_runtime.GVariables, frame.LVariable);
            var value = executor.Parse(exp);
            var variable = new MoeVariable();

            if (value is int vint)
            {
                variable = new MoeVariable(MoeVariableAccess.Variable, MoeVariableType.Int)
                {
                    Name = value.GetHashCode().ToString(),
                    Dimension = [1],
                };
                variable[0] = vint;
            }
            else if (value is float vfloat)
            {
                variable = new MoeVariable(MoeVariableAccess.Variable, MoeVariableType.Float)
                {
                    Name = value.GetHashCode().ToString(),
                    Dimension = [1],
                };
                variable[0] = vfloat;
            }
            else if (value is string vstring)
            {
                variable = new MoeVariable(MoeVariableAccess.Variable, MoeVariableType.String)
                {
                    Name = value.GetHashCode().ToString(),
                    Dimension = [1],
                };
                variable[0] = vstring;
            }
            else
                Console.WriteLine("Unmatched parameter type");

            return variable;
        }
    }
}
