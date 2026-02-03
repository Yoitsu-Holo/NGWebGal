using Avalonia;
using Avalonia.Controls;
using NGWebGal.Editor.ViewModels;

namespace NGWebGal.Editor.Views;

/// <summary>
/// UserControl for editing properties of the selected widget in the editor.
/// Provides a property panel with form controls bound to the SelectedWidget's properties.
/// </summary>
public partial class PropertyPanel : UserControl
{
    /// <summary>
    /// Defines the <see cref="SelectedWidget"/> attached property.
    /// </summary>
    public static readonly StyledProperty<WidgetViewModel?> SelectedWidgetProperty =
        AvaloniaProperty.Register<PropertyPanel, WidgetViewModel?>(nameof(SelectedWidget));

    /// <summary>
    /// Gets or sets the currently selected widget to display and edit properties for.
    /// When null, displays a "No widget selected" message.
    /// </summary>
    public WidgetViewModel? SelectedWidget
    {
        get => GetValue(SelectedWidgetProperty);
        set => SetValue(SelectedWidgetProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyPanel"/> class.
    /// </summary>
    public PropertyPanel()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedWidgetProperty)
        {
            DataContext = this;
        }
    }
}
