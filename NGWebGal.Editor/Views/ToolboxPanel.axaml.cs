using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Views;

/// <summary>
/// UserControl that displays a toolbox panel with draggable widget types.
/// </summary>
public partial class ToolboxPanel : UserControl
{
    /// <summary>
    /// Defines the ToolboxItems attached property for binding toolbox items.
    /// </summary>
    public static readonly StyledProperty<ObservableCollection<ToolboxItem>> ToolboxItemsProperty =
        AvaloniaProperty.Register<ToolboxPanel, ObservableCollection<ToolboxItem>>(
            nameof(ToolboxItems),
            defaultValue: CreateDefaultToolboxItems());

    /// <summary>
    /// Gets or sets the collection of toolbox items displayed in the panel.
    /// </summary>
    public ObservableCollection<ToolboxItem> ToolboxItems
    {
        get => GetValue(ToolboxItemsProperty);
        set => SetValue(ToolboxItemsProperty, value);
    }

    private ListBox? _toolboxListBox;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolboxPanel"/> class.
    /// </summary>
    public ToolboxPanel()
    {
        InitializeComponent();
        InitializeEventHandlers();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _toolboxListBox = this.FindControl<ListBox>("ToolboxListBox");

        if (_toolboxListBox != null)
        {
            _toolboxListBox.ItemsSource = ToolboxItems;
        }
    }

    private void InitializeEventHandlers()
    {
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_toolboxListBox != null)
        {
            _toolboxListBox.AddHandler(PointerPressedEvent, OnListBoxItemPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    /// <summary>
    /// Handles pointer pressed events on list box items to initiate drag-and-drop.
    /// </summary>
    private async void OnListBoxItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control control)
        {
            // Find the ToolboxItem from the data context
            var item = control.DataContext as ToolboxItem;
            if (item == null)
            {
                // Walk up the visual tree to find the ListBoxItem
                var parent = control.Parent as StyledElement;
                while (parent != null && item == null)
                {
                    if (parent is Control parentControl)
                    {
                        item = parentControl.DataContext as ToolboxItem;
                    }
                    parent = parent.Parent as StyledElement;
                }
            }

            if (item != null)
            {
                // Create data object with the WidgetType
                #pragma warning disable CS0618 // Type or member is obsolete
                var dataObject = new DataObject();
                dataObject.Set("WidgetType", item.Type);
                dataObject.Set("ToolboxItem", item);

                // Initiate drag-and-drop operation
                var result = await DragDrop.DoDragDrop(e, dataObject, DragDropEffects.Copy);
                #pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }

    /// <summary>
    /// Creates the default collection of toolbox items with all available widget types.
    /// </summary>
    /// <returns>An observable collection containing all default toolbox items.</returns>
    private static ObservableCollection<ToolboxItem> CreateDefaultToolboxItems()
    {
        return new ObservableCollection<ToolboxItem>
        {
            new ToolboxItem
            {
                Type = WidgetType.ImageBox,
                DisplayName = "Image Box",
                Description = "Display an image"
            },
            new ToolboxItem
            {
                Type = WidgetType.TextBox,
                DisplayName = "Text Box",
                Description = "Display text"
            },
            new ToolboxItem
            {
                Type = WidgetType.ColorBox,
                DisplayName = "Color Box",
                Description = "Solid color rectangle"
            },
            new ToolboxItem
            {
                Type = WidgetType.ProgressBar,
                DisplayName = "Progress Bar",
                Description = "Progress indicator"
            },
            new ToolboxItem
            {
                Type = WidgetType.Button,
                DisplayName = "Button",
                Description = "Clickable button"
            },
            new ToolboxItem
            {
                Type = WidgetType.Toggle,
                DisplayName = "Toggle",
                Description = "On/Off switch"
            },
            new ToolboxItem
            {
                Type = WidgetType.Checkbox,
                DisplayName = "Checkbox",
                Description = "Checkable option"
            },
            new ToolboxItem
            {
                Type = WidgetType.SliderHorizontal,
                DisplayName = "H-Slider",
                Description = "Horizontal slider"
            },
            new ToolboxItem
            {
                Type = WidgetType.SliderVertical,
                DisplayName = "V-Slider",
                Description = "Vertical slider"
            },
            new ToolboxItem
            {
                Type = WidgetType.InputField,
                DisplayName = "Input Field",
                Description = "Text input"
            }
        };
    }
}
