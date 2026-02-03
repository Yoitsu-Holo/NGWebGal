namespace NGWebGal.Editor.Models;

/// <summary>
/// Represents an item in the toolbox for creating new widgets.
/// </summary>
public class ToolboxItem
{
    /// <summary>
    /// Gets or sets the widget type this toolbox item creates.
    /// </summary>
    public WidgetType Type { get; set; }

    /// <summary>
    /// Gets or sets the display name shown in the toolbox.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Gets or sets the description of the widget type.
    /// </summary>
    public string Description { get; set; } = "";
}
