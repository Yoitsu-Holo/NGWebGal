using System;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Audio-related syscalls
/// </summary>
public static partial class MoeSyscall
{
    public static void RegAudioContext(MoeVariable contextID)
        => RawRegAudioContext(contextID);

    public static void RegAudioNode(MoeVariable contextID, MoeVariable nodeID, MoeVariable type)
        => RawRegAudioNode(contextID, nodeID, type);

    public static void BGM(MoeVariable file)
        => RawBGM(file);

    public static void VO(MoeVariable file)
        => RawVO(file);

    public static void SE(MoeVariable file)
        => RawSE(file);

    private static void RawRegAudioContext(int contextID)
    {
        // TODO: Implement audio context registration
        Console.WriteLine($"RegAudioContext: {contextID}");
    }

    private static void RawRegAudioNode(int contextID, int nodeID, string type)
    {
        // TODO: Implement audio node registration
        Console.WriteLine($"RegAudioNode: ctx={contextID}, node={nodeID}, type={type}");
    }

    private static void RawBGM(MoeVariable file)
    {
        // TODO: Implement BGM playback
        Console.WriteLine($"BGM: {file}");
    }

    private static void RawVO(MoeVariable file)
    {
        // TODO: Implement voice playback
        Console.WriteLine($"VO: {file}");
    }

    private static void RawSE(MoeVariable file)
    {
        // TODO: Implement sound effect playback
        Console.WriteLine($"SE: {file}");
    }
}
