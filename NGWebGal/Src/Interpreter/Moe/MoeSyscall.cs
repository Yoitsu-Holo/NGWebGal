using System;
using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// System calls for Moe interpreter
/// </summary>
public static partial class MoeSyscall
{
    // Shared state - will be injected by interpreter
    private static ElfHeader? _elfHeader;
    private static MoeInterpreter? _interpreter;

    public static void Initialize(MoeInterpreter interpreter)
    {
        _interpreter = interpreter;
        _elfHeader = interpreter.ElfHeader;
    }
}
