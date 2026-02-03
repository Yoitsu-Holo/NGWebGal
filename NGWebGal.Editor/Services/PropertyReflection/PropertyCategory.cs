namespace NGWebGal.Editor.Services.PropertyReflection;

/// <summary>
/// Categories for organizing properties in the editor
/// </summary>
public enum PropertyCategory
{
    Identity,      // Name, Id
    Transform,     // Position, Size, Offset
    Display,       // Visible, Enable, ZIndex
    Content,       // Text, TextSize, Typeface
    Appearance,    // Color, Style
    Advanced       // Everything else
}
