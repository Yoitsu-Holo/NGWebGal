using System;
using System.Collections.Generic;
using NGWebGal.Driver;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Main Moe script interpreter - refactored to instance-based
/// </summary>
public partial class MoeInterpreter
{
    private readonly ElfHeader _elfHeader = new();
    private readonly MoeRuntime _runtime = new();
    private readonly Dictionary<string, MoeVariable> _localVariables = [];
    private Driver.Driver? _driver;

    public ElfHeader ElfHeader => _elfHeader;
    public MoeRuntime Runtime => _runtime;
    public Driver.Driver? Driver => _driver;

    public void SetDriver(Driver.Driver driver)
    {
        _driver = driver;
    }

    public void Clear()
    {
        _elfHeader.Clear();
        _runtime.Clear();
        _localVariables.Clear();
    }
}
