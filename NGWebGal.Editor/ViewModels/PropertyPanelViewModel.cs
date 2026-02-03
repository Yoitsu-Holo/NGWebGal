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
    /// Loads a layer and creates property editors for it
    /// </summary>
    /// <param name="layer">The layer to edit</param>
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
                catch (NotSupportedException)
                {
                    // Skip unsupported property types
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
    /// Handles value changes from property editors
    /// </summary>
    private void OnEditorValueChanged(object? sender, EventArgs e)
    {
        if (sender is IPropertyEditor editor)
        {
            System.Diagnostics.Debug.WriteLine($"[PropertyPanel] Property changed: {editor.Descriptor.Name}");
        }
        OnPropertyChanged();
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

    /// <summary>
    /// Triggers bitmap invalidation when a property changes
    /// </summary>
    /// <remarks>
    /// This method is called by property editors when values change.
    /// It finds the corresponding WidgetViewModel and calls InvalidateBitmap().
    /// </remarks>
    public void OnPropertyChanged()
    {
        if (SelectedLayer == null)
        {
            System.Diagnostics.Debug.WriteLine("[PropertyPanel] OnPropertyChanged: SelectedLayer is null");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[PropertyPanel] OnPropertyChanged: Looking for widget with layer type {SelectedLayer.GetType().Name}");

        // Find the WidgetViewModel that contains this layer
        var widget = _mainViewModel.Widgets.FirstOrDefault(w => w.CoreLayer == SelectedLayer);
        if (widget != null)
        {
            System.Diagnostics.Debug.WriteLine($"[PropertyPanel] Found WidgetViewModel, calling InvalidateBitmap");
            widget.InvalidateBitmap();
            _mainViewModel.MarkDirty();
            System.Diagnostics.Debug.WriteLine($"[PropertyPanel] InvalidateBitmap called successfully");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[PropertyPanel] WARNING: Could not find WidgetViewModel for layer");
        }
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
