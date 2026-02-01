using System;
using NGWebGal.Types;

namespace NGWebGal.Handler.Event;

public enum MouseButton : ushort
{
    Empty = 0x0000,
    LButton = 0x0001,
    MButton = 0x0002,
    RButton = 0x0004,
    Button4 = 0x0008,
    Button5 = 0x0010,
    Button6 = 0x0011,
    Button7 = 0x0012,
    Button8 = 0x0014,
    MouseChord = 0x0018,
    MouseWheel = 0x0020,
    Any = 0xFFFF
}

public enum MouseStatus : byte
{
    Release = 0x00,
    Hold = 0x01,
    Up = 0x02,
    Down = 0x04,
    Click = 0x08,
    WheelUp = 0x10,
    WheelDown = 0x11,
    Any = 0xFF
}

public class MouseEventData : EventArgs
{
    public IVector Position;
    public IVector Move;
    public MouseButton Button;
    public MouseStatus Status;
}
