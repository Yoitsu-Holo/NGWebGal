using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NGWebGal.Editor.Controls.PropertyEditors;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Layer;

namespace NGWebGal.Editor.ViewModels;

/// <summary>
/// ViewModel for the PropertyPanel that manages dynamic property editors
/// </summary>
public partial class PropertyPanelViewModel : ObservableObject
{
    private readonly ReflectionEngine _reflectionEngine;
    private readonly PropertyEditorFactory _editorFactory;
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// Gets or sets the currently bound layer
    /// </summary>
    [ObservableProperty]
    private ILayer? _selectedLayer;

    /// <summary>
    /// Gets or sets the currently bound widget view model
    /// </summary>
    [ObservableProperty]
    private WidgetViewModel? _selectedWidget;

    /// <summary>
    /// Gets the collection of property editors grouped by type hierarchy
    /// </summary>
    public ObservableCollection<PropertyGroupViewModel> PropertyGroups { get; } = new();

    /// <summary>
    /// Initializes a new instance of the PropertyPanelViewModel
    /// </summary>
    /// <param name="mainViewModel">Reference to the main view model</param>
    public PropertyPanelViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        var widgetLookup = new Func<ILayer, Models.EditorWidget?>(layer =>
            _mainViewModel.Widgets.FirstOrDefault(w => w.CoreLayer == layer)?.ToModel()
        );
        _reflectionEngine = new ReflectionEngine(widgetLookup);
        _editorFactory = new PropertyEditorFactory(widgetLookup);
    }

    /// <summary>
    /// Loads a layer and creates property editors for it.
    /// [DEPRECATED] Use LoadWidget() for WidgetViewModel-based binding.
    /// </summary>
    /// <param name="layer">The layer to edit</param>
    [Obsolete("Use LoadWidget() for better data flow. LoadLayer() is kept for backwards compatibility.")]
    public void LoadLayer(ILayer? layer)
    {
        // Unbind all existing editors
        foreach (var group in PropertyGroups)
        {
            foreach (var editor in group.Editors)
            {
                editor.ValueChanged -= OnEditorValueChanged;
                editor.Unbind();
            }
        }

        PropertyGroups.Clear();
        SelectedLayer = layer;

        if (layer == null)
            return;

        // Discover properties using reflection
        var properties = _reflectionEngine.DiscoverProperties(layer.GetType());

        // Group properties by DeclaringType
        var groupedProperties = properties
            .GroupBy(p => p.DeclaringType)
            .OrderBy(g => properties.First(p => p.DeclaringType == g.Key).HierarchyDepth);

        // Create groups and editors
        foreach (var group in groupedProperties)
        {
            var groupViewModel = new PropertyGroupViewModel
            {
                TypeName = group.Key.Name,
                HierarchyDepth = properties.First(p => p.DeclaringType == group.Key).HierarchyDepth
            };

            foreach (var property in group)
            {
                try
                {
                    // Create editor with abbreviated name
                    var abbreviatedProperty = new PropertyDescriptor
                    {
                        Name = GetDisplayName(property.Name),
                        PropertyPath = property.PropertyPath,
                        PropertyType = property.PropertyType,
                        DeclaringType = property.DeclaringType,
                        HierarchyDepth = property.HierarchyDepth,
                        IsReadOnly = property.IsReadOnly,
                        IsExpandable = property.IsExpandable,
                        Category = property.Category,
                        Getter = property.Getter,
                        Setter = property.Setter
                    };

                    var editor = _editorFactory.CreateEditor(abbreviatedProperty);

                    // Skip if no editor available for this property type
                    if (editor == null)
                        continue;

                    // Bind editor to layer
                    editor.Bind(layer);

                    // Subscribe to value changes
                    editor.ValueChanged += OnEditorValueChanged;

                    groupViewModel.Editors.Add(editor);
                }
                catch (NotImplementedException)
                {
                    // Skip editors not yet implemented
                    continue;
                }
            }

            // Only add group if it has editors
            if (groupViewModel.Editors.Count > 0)
            {
                PropertyGroups.Add(groupViewModel);
            }
        }
    }

    /// <summary>
    /// Loads a WidgetViewModel and creates property editors for it.
    /// Properties from ViewModel (Position, Size, Offset) take precedence over CoreLayer properties.
    /// </summary>
    /// <param name="widget">The WidgetViewModel to edit</param>
    public void LoadWidget(WidgetViewModel? widget)
    {
        // Unbind all existing editors
        foreach (var group in PropertyGroups)
        {
            foreach (var editor in group.Editors)
            {
                editor.ValueChanged -= OnEditorValueChanged;
                editor.Unbind();
            }
        }

        PropertyGroups.Clear();
        SelectedWidget = widget;
        SelectedLayer = widget?.CoreLayer; // Keep SelectedLayer for backwards compatibility

        if (widget == null)
            return;

        // Discover properties using ViewModel-aware reflection
        var properties = _reflectionEngine.DiscoverViewModelProperties(widget);

        // Group properties by DeclaringType
        var groupedProperties = properties
            .GroupBy(p => p.DeclaringType)
            .OrderBy(g => properties.First(p => p.DeclaringType == g.Key).HierarchyDepth);

        // Create groups and editors
        foreach (var group in groupedProperties)
        {
            var groupViewModel = new PropertyGroupViewModel
            {
                GroupName = FormatGroupName(group.Key),
                HierarchyDepth = properties.First(p => p.DeclaringType == group.Key).HierarchyDepth
            };

            foreach (var prop in group.OrderBy(p => p.Category).ThenBy(p => p.Name))
            {
                try
                {
                    // Create editor with abbreviated name
                    var abbreviatedProperty = new PropertyDescriptor
                    {
                        Name = GetDisplayName(prop.Name),
                        PropertyPath = prop.PropertyPath,
                        PropertyType = prop.PropertyType,
                        DeclaringType = prop.DeclaringType,
                        HierarchyDepth = prop.HierarchyDepth,
                        IsReadOnly = prop.IsReadOnly,
                        IsExpandable = prop.IsExpandable,
                        Category = prop.Category,
                        Getter = prop.Getter,
                        Setter = prop.Setter
                    };

                    var editor = _editorFactory.CreateEditor(abbreviatedProperty);
                    if (editor != null)
                    {
                        // IMPORTANT: Bind to WidgetViewModel, not CoreLayer
                        editor.Bind(widget);
                        editor.ValueChanged += OnEditorValueChanged;
                        groupViewModel.Editors.Add(editor);
                    }
                }
                catch (NotSupportedException ex)
                {
                    // Skip properties that don't have editors (e.g., Object type, complex types)
                    System.Diagnostics.Debug.WriteLine($"[PropertyPanelViewModel] Skipping property {prop.Name} ({prop.PropertyType.Name}): {ex.Message}");
                    continue;
                }
            }

            if (groupViewModel.Editors.Count > 0)
                PropertyGroups.Add(groupViewModel);
        }

        System.Diagnostics.Debug.WriteLine($"[PropertyPanelViewModel] Loaded {PropertyGroups.Sum(g => g.Editors.Count)} editors for widget {widget.Name} (Type: {widget.Type})");
    }

    /// <summary>
    /// Formats the type name into a user-friendly group name
    /// </summary>
    private string FormatGroupName(Type type)
    {
        var name = type.Name;

        // Special handling for WidgetViewModel
        if (name == "WidgetViewModel")
            return "LayerBase"; // Show as LayerBase for consistency with CoreLayer properties

        // Remove "Widget" or "Controller" prefix
        if (name.StartsWith("Widget"))
            name = name.Substring(6);
        else if (name.StartsWith("Controller"))
            name = name.Substring(10);

        // Add spaces before capitals
        return System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>
    /// Handles value changes from property editors
    /// </summary>
    private void OnEditorValueChanged(object? sender, EventArgs e)
    {
        if (sender is IPropertyEditor editor)
        {
            System.Diagnostics.Debug.WriteLine($"[PropertyPanel] Property changed: {editor.Descriptor.Name}");

            // Only invalidate bitmap for properties that change visual content
            // Position and Offset only affect drawing location, not bitmap content
            var propertyName = editor.Descriptor.Name;
            bool needsBitmapInvalidation = propertyName != "Pos" &&
                                          propertyName != "Position" &&
                                          propertyName != "Offset";

            if (needsBitmapInvalidation)
            {
                SelectedWidget?.InvalidateBitmap();
            }

            _mainViewModel.MarkDirty();
        }
    }

    /// <summary>
    /// Gets abbreviated display name for property
    /// </summary>
    private static string GetDisplayName(string propertyName)
    {
        return propertyName switch
        {
            "Position" => "Pos",
            "Rotation" => "Rot",
            "Opacity" => "Opa",
            "Background" => "Bg",
            "Foreground" => "Fg",
            "Padding" => "Pad",
            "Margin" => "Mar",
            _ => propertyName
        };
    }

}

/// <summary>
/// ViewModel representing a group of properties from the same declaring type
/// </summary>
public partial class PropertyGroupViewModel : ObservableObject
{
    /// <summary>
    /// Name of the declaring type
    /// </summary>
    [ObservableProperty]
    private string _typeName = string.Empty;

    /// <summary>
    /// Formatted group name for display
    /// </summary>
    [ObservableProperty]
    private string _groupName = string.Empty;

    /// <summary>
    /// Hierarchy depth (0 = concrete type, higher = base types)
    /// </summary>
    [ObservableProperty]
    private int _hierarchyDepth;

    /// <summary>
    /// Whether the group is expanded
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// Collection of editors in this group
    /// </summary>
    public ObservableCollection<IPropertyEditor> Editors { get; } = new();
}
