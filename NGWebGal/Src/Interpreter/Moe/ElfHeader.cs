using System.Collections.Generic;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// ELF header containing loaded script data
/// </summary>
public class ElfHeader
{
    public Dictionary<string, MoeFile> ImageFiles { get; set; } = [];
    public Dictionary<string, MoeFile> AudioFiles { get; set; } = [];
    public Dictionary<string, MoeFile> TextFiles { get; set; } = [];
    public Dictionary<string, MoeFile> BinFiles { get; set; } = [];
    public Dictionary<string, FunctionNode> Functions { get; set; } = [];
    public Dictionary<string, MoeVariable> Datas { get; set; } = [];
    public string Start { get; set; } = "main";

    public void Clear()
    {
        ImageFiles.Clear();
        AudioFiles.Clear();
        TextFiles.Clear();
        BinFiles.Clear();
        Functions.Clear();
        Datas.Clear();
        Start = "main";
    }
}
