using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NGWebGal.Editor.Models;
using NGWebGal.Editor.Services;

namespace NGWebGal.Editor.ViewModels;

/// <summary>
/// Main application ViewModel that manages the editor state and operations.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILayoutSerializer _layoutSerializer;
    private readonly TxtExporter _txtExporter;
    private readonly IStorageProvider? _storageProvider;
    private readonly CoreWidgetFactory _coreFactory = new();

    /// <summary>
    /// Gets the property panel view model
    /// </summary>
    public PropertyPanelViewModel PropertyPanelVM { get; }

    /// <summary>
    /// Gets the collection of all widgets in the current layout.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WidgetViewModel> _widgets = new();

    /// <summary>
    /// Gets or sets the currently selected widget.
    /// </summary>
    [ObservableProperty]
    private WidgetViewModel? _selectedWidget;

    /// <summary>
    /// Gets or sets the current layout data.
    /// </summary>
    [ObservableProperty]
    private EditorLayout _currentLayout = new();

    /// <summary>
    /// Gets or sets the path to the current file.
    /// </summary>
    [ObservableProperty]
    private string? _currentFilePath;

    /// <summary>
    /// Gets or sets whether the layout has unsaved changes.
    /// </summary>
    [ObservableProperty]
    private bool _isDirty;

    /// <summary>
    /// Gets or sets the canvas width in pixels.
    /// </summary>
    [ObservableProperty]
    private int _canvasWidth = 1280;

    /// <summary>
    /// Gets or sets the canvas height in pixels.
    /// </summary>
    [ObservableProperty]
    private int _canvasHeight = 720;

    /// <summary>
    /// Gets or sets the canvas zoom level (0.1 to 5.0).
    /// </summary>
    [ObservableProperty]
    private double _zoomLevel = 1.0;

    /// <summary>
    /// Gets or sets whether to show percentage guide lines.
    /// </summary>
    [ObservableProperty]
    private bool _showPercentageLines = true;

    /// <summary>
    /// Gets or sets whether to show grid lines.
    /// </summary>
    [ObservableProperty]
    private bool _showGridLines = false;

    /// <summary>
    /// Gets or sets the grid spacing in pixels.
    /// </summary>
    [ObservableProperty]
    private int _gridSpacing = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="layoutSerializer">The layout serialization service.</param>
    /// <param name="txtExporter">The text export service.</param>
    /// <param name="storageProvider">Optional storage provider for file dialogs.</param>
    public MainViewModel(
        ILayoutSerializer layoutSerializer,
        TxtExporter txtExporter,
        IStorageProvider? storageProvider = null)
    {
        _layoutSerializer = layoutSerializer ?? throw new ArgumentNullException(nameof(layoutSerializer));
        _txtExporter = txtExporter ?? throw new ArgumentNullException(nameof(txtExporter));
        _storageProvider = storageProvider;

        // Initialize PropertyPanelVM
        PropertyPanelVM = new PropertyPanelViewModel(this);
    }

    /// <summary>
    /// Creates a new empty layout, clearing all widgets.
    /// </summary>
    [RelayCommand]
    private void NewLayout()
    {
        Widgets.Clear();
        CurrentLayout = new EditorLayout();
        CurrentFilePath = null;
        IsDirty = false;
        SelectedWidget = null;
    }

    /// <summary>
    /// Opens a layout file from disk.
    /// </summary>
    [RelayCommand]
    private async Task OpenLayout()
    {
        if (_storageProvider == null)
            return;

        var files = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Layout",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("WebGal Layout")
                {
                    Patterns = new[] { "*.xml" }
                },
                FilePickerFileTypes.All
            }
        });

        if (files.Count == 0)
            return;

        var file = files[0];
        var filePath = file.Path.LocalPath;

        try
        {
            var layout = _layoutSerializer.Load(filePath);
            CurrentLayout = layout;
            CurrentFilePath = filePath;

            // Convert model widgets to ViewModels
            Widgets.Clear();
            foreach (var widget in layout.Widgets)
            {
                var vm = new WidgetViewModel(widget, this);
                // TEMPORARY: Disable CoreLayer creation to diagnose freeze
                // vm.CoreLayer = _coreFactory.CreateLayer(widget);
                Widgets.Add(vm);
            }

            IsDirty = false;
            SelectedWidget = null;
        }
        catch (Exception ex)
        {
            // TODO: Show error dialog to user
            Console.Error.WriteLine($"Failed to open layout: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves the current layout to the current file or shows save dialog if no file is set.
    /// </summary>
    [RelayCommand]
    private async Task SaveLayout()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            await SaveAsLayout();
            return;
        }

        try
        {
            SaveToFile(CurrentFilePath);
        }
        catch (Exception ex)
        {
            // TODO: Show error dialog to user
            Console.Error.WriteLine($"Failed to save layout: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows save dialog and saves the current layout to a new file.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsLayout()
    {
        if (_storageProvider == null)
            return;

        var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Layout As",
            DefaultExtension = "xml",
            SuggestedFileName = string.IsNullOrEmpty(CurrentLayout.Name) ? "layout.xml" : $"{CurrentLayout.Name}.xml",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("WebGal Layout")
                {
                    Patterns = new[] { "*.xml" }
                }
            }
        });

        if (file == null)
            return;

        var filePath = file.Path.LocalPath;

        try
        {
            SaveToFile(filePath);
            CurrentFilePath = filePath;
        }
        catch (Exception ex)
        {
            // TODO: Show error dialog to user
            Console.Error.WriteLine($"Failed to save layout: {ex.Message}");
        }
    }

    /// <summary>
    /// Exports the current layout to a human-readable text file.
    /// </summary>
    [RelayCommand]
    private async Task ExportTxt()
    {
        if (_storageProvider == null)
            return;

        var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export to Text",
            DefaultExtension = "txt",
            SuggestedFileName = string.IsNullOrEmpty(CurrentLayout.Name) ? "layout.txt" : $"{CurrentLayout.Name}.txt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Text File")
                {
                    Patterns = new[] { "*.txt" }
                }
            }
        });

        if (file == null)
            return;

        var filePath = file.Path.LocalPath;

        try
        {
            // Update layout with current widgets
            SyncLayoutFromViewModels();
            _txtExporter.Export(CurrentLayout, filePath);
        }
        catch (Exception ex)
        {
            // TODO: Show error dialog to user
            Console.Error.WriteLine($"Failed to export layout: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes the currently selected widget.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteWidget))]
    private void DeleteWidget()
    {
        if (SelectedWidget == null)
            return;

        Widgets.Remove(SelectedWidget);
        SelectedWidget = null;
        IsDirty = true;
    }

    /// <summary>
    /// Determines whether a widget can be deleted.
    /// </summary>
    /// <returns>True if a widget is selected; otherwise, false.</returns>
    private bool CanDeleteWidget() => SelectedWidget != null;

    /// <summary>
    /// Adds a new widget to the canvas at the specified position.
    /// </summary>
    /// <param name="parameter">Tuple containing (WidgetType type, Point position).</param>
    [RelayCommand]
    private void AddWidget(object? parameter)
    {
        if (parameter is not (WidgetType type, Avalonia.Point position))
        {
            return;
        }

        var widget = new EditorWidget
        {
            Type = type,
            Name = $"{type}_{Widgets.Count + 1}",
            X = (int)position.X,
            Y = (int)position.Y,
            Width = 100,
            Height = 100,
            Visible = true,
            Enable = true,
            ZIndex = Widgets.Count
        };

        var viewModel = new WidgetViewModel(widget, this);
        viewModel.CoreLayer = _coreFactory.CreateLayer(widget);

        // CRITICAL: Sync Position/Size to CoreLayer immediately after creation
        if (viewModel.CoreLayer != null)
        {
            viewModel.CoreLayer.Position = new NGWebGal.Types.IVector(viewModel.X, viewModel.Y);
            viewModel.CoreLayer.Size = new NGWebGal.Types.IVector(viewModel.Width, viewModel.Height);
        }

        // Set default color/placeholder based on widget type
        if (viewModel.CoreLayer != null)
        {
            try
            {
                // Only set color for widgets that support SetColor
                if (type == WidgetType.ColorBox ||
                    type == WidgetType.TextBox ||
                    type == WidgetType.ProgressBar)
                {
                    // Use the SAME colors as defined in WidgetTypeToBrushConverter
                    var defaultColor = type switch
                    {
                        WidgetType.ColorBox => new SkiaSharp.SKColor(240, 128, 128),       // LightCoral
                        WidgetType.TextBox => new SkiaSharp.SKColor(144, 238, 144),        // LightGreen
                        WidgetType.ProgressBar => new SkiaSharp.SKColor(250, 250, 210),    // LightGoldenrodYellow
                        _ => new SkiaSharp.SKColor(200, 200, 200)                          // Default gray
                    };

                    viewModel.CoreLayer.SetColor(defaultColor, 0);
                    widget.Color = EditorColor.FromSKColor(defaultColor);
                    System.Diagnostics.Debug.WriteLine($"[MainViewModel] Set default color for {type}: {defaultColor}");
                }
                else if (type == WidgetType.ImageBox)
                {
                    // For ImageBox, create a placeholder bitmap with LightBlue color
                    // Use the SAME color as defined in WidgetTypeToBrushConverter
                    var placeholderColor = new SkiaSharp.SKColor(173, 216, 230); // LightBlue
                    var bitmap = new SkiaSharp.SKBitmap(100, 100);

                    try
                    {
                        var canvas = new SkiaSharp.SKCanvas(bitmap);
                        canvas.Clear(placeholderColor); // Fill entire bitmap with LightBlue
                        canvas.Dispose();

                        // Pass ownership of bitmap to CoreLayer (it will manage disposal)
                        viewModel.CoreLayer.SetImage(bitmap, 0);
                        System.Diagnostics.Debug.WriteLine($"[MainViewModel] Set placeholder image for ImageBox (100x100, color: {placeholderColor})");
                    }
                    catch
                    {
                        // If SetImage fails, dispose the bitmap ourselves
                        bitmap.Dispose();
                        throw;
                    }
                }
                // For other widgets (Button, Toggle, Checkbox, Sliders), use default rendering

                viewModel.InvalidateBitmap();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Failed to set default appearance: {ex.Message}");
                // Continue anyway - widget will just use default rendering
            }
        }

        Widgets.Add(viewModel);

        SelectedWidget = viewModel;
        IsDirty = true;
    }

    /// <summary>
    /// Marks the layout as having unsaved changes.
    /// </summary>
    internal void MarkDirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Saves the current layout to the specified file path.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    private void SaveToFile(string filePath)
    {
        // Sync ViewModels back to model
        SyncLayoutFromViewModels();

        // Update timestamps
        CurrentLayout.ModifiedAt = DateTime.UtcNow;

        // Save to file
        _layoutSerializer.Save(CurrentLayout, filePath);
        IsDirty = false;
    }

    /// <summary>
    /// Synchronizes the CurrentLayout model with the current WidgetViewModels.
    /// </summary>
    private void SyncLayoutFromViewModels()
    {
        CurrentLayout.Widgets.Clear();
        CurrentLayout.Widgets.AddRange(Widgets.Select(vm => vm.ToModel()));
    }

    partial void OnSelectedWidgetChanged(WidgetViewModel? value)
    {
        // Notify that DeleteWidget command can execute state may have changed
        DeleteWidgetCommand.NotifyCanExecuteChanged();

        // Update PropertyPanelVM with the selected widget (enables ViewModel property binding)
        PropertyPanelVM.LoadWidget(value);
    }

    /// <summary>
    /// Shows the canvas settings dialog.
    /// </summary>
    [RelayCommand]
    private async Task ShowCanvasSettings()
    {
        var dialog = new Views.CanvasSettingsDialog(CanvasWidth, CanvasHeight);

        // Get the main window from the application lifetime
        var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow == null)
            return;

        // Show dialog and get result
        var result = await dialog.ShowDialog<bool>(mainWindow);

        if (result)
        {
            // User clicked OK, update canvas size
            CanvasWidth = (int)dialog.CanvasWidth;
            CanvasHeight = (int)dialog.CanvasHeight;
        }
    }
}
