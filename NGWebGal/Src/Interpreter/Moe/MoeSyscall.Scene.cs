using System;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Scene-related syscalls
/// </summary>
public static partial class MoeSyscall
{
    public static void ParseSceneList(MoeVariable sceneName)
        => RawParseSceneList(sceneName);

    public static void SetSceneList(MoeVariable sceneName)
        => RawSetSceneList(sceneName);

    public static void OnClick()
        => RawOnClick();

    public static void LoadScene(MoeVariable sceneid)
        => RawLoadScene(sceneid);

    public static void BG(MoeVariable file, MoeVariable subx, MoeVariable suby, MoeVariable width, MoeVariable height)
        => RawBG(file, subx, suby, width, height);

    public static void TEXT(MoeVariable name, MoeVariable text)
        => RawTEXT(name, text);

    public static void LATEX(MoeVariable text)
        => RawLATEX(text);

    private static void RawParseSceneList(string sceneName)
    {
        // TODO: Implement scene list parsing
        Console.WriteLine($"ParseSceneList: {sceneName}");
    }

    private static void RawSetSceneList(string sceneName)
    {
        // TODO: Implement set active scene list
        Console.WriteLine($"SetSceneList: {sceneName}");
    }

    private static void RawOnClick()
    {
        // TODO: Implement click handler
        Console.WriteLine("OnClick");
    }

    private static void RawLoadScene(int sceneid = 0)
    {
        // TODO: Implement scene loading
        Console.WriteLine($"LoadScene: {sceneid}");
    }

    private static void RawBG(string file, int subx, int suby, int width, int height)
    {
        // TODO: Implement background setting
        Console.WriteLine($"BG: file={file}, sub=({subx},{suby}), size=({width},{height})");
    }

    private static void RawTEXT(string name, string text)
    {
        // TODO: Implement text display
        Console.WriteLine($"TEXT: name={name}, text={text}");
    }

    private static void RawLATEX(string text)
    {
        // TODO: Implement LaTeX rendering
        Console.WriteLine($"LATEX: {text}");
    }
}
