using NGWebGal.Types;

namespace NGWebGal.Driver.Data;

/// <summary> Layer ID information </summary>
public record struct LayerIdInfo
{
    public int LayoutID { get; set; }
    public int LayerID { get; set; }
}

/// <summary> Basic request type </summary>
public enum RequestType
{
    // Get, Set, Delete
    Get, Set, Delete,

    // Any type, Void type, Auto parse, Error
    Void, Any, Auto, Error,
}

/// <summary> Basic response type </summary>
public enum ResponseType
{
    Success, Fail,

    // Any type, Void type, Auto parse, Error
    Void, Any, Auto, Error,
}

public enum ControllerStatus
{
    // Normal, Hover, Pressed, Focused (highlighted)
    Normal, Hover, Pressed, Focused,
}

/// <summary> Response structure header </summary>
public record struct Response
{
    public Response()
    {
        Type = ResponseType.Success;
        Message = "";
    }

    public Response(string message, ResponseType type = ResponseType.Fail)
    {
        Type = type;
        Message = message;
    }

    public ResponseType Type { get; set; }
    public string Message { get; set; }
}

/// <summary> Game settings </summary>
public record struct GameInfo
{
    public RequestType Request { get; set; }

    public string Name { get; set; }
    public IVector Resolution { get; set; }
}

public record struct ImageInfo
{
    public string ImageName { get; set; }
    public IRect SubRect { get; set; }
}
