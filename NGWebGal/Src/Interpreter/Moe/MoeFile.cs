namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Represents a file reference in the Moe script system
/// </summary>
public class MoeFile
{
    public string Name { get; set; } = "";
    public MoeFileType Type { get; set; } = MoeFileType.Void;
    public string URL { get; set; } = "";

    public override string ToString() => $"FileName: {Name}, \tFileType: {Type}, \tFileURL: {URL}";
}
