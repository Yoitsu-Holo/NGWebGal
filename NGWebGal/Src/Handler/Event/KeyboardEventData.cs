using System;
using Avalonia.Input;

namespace NGWebGal.Handler.Event;

public enum KeyModifiers : byte
{
    None = 0x00,
    Alt = 0x01,
    Control = 0x02,
    Shift = 0x04,
    Meta = 0x08
}

public enum KeyStatus : byte
{
    Release = 0x00,
    Hold = 0x01,
    Up = 0x02,
    Down = 0x04,
    Press = 0x08
}

public class KeyboardEventData : EventArgs
{
    public Key Key;
    public KeyStatus Status;
    public KeyModifiers Modifiers;
    public string? Text;
}
