using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NGWebGal.Editor.Services;
using NGWebGal.Editor.ViewModels;

namespace NGWebGal.Editor.Views;

/// <summary>
/// Main application window that hosts the WebGal Editor interface.
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
        SetupKeyboardShortcuts();
    }

    /// <summary>
    /// Initializes the services and ViewModel for the editor.
    /// </summary>
    private void InitializeServices()
    {
        // Initialize services
        var layoutSerializer = new XmlLayoutSerializer();
        var txtExporter = new TxtExporter();
        var widgetFactory = new WidgetFactory();

        // Create MainViewModel with services and storage provider
        _viewModel = new MainViewModel(
            layoutSerializer,
            txtExporter,
            StorageProvider);

        // Set DataContext
        DataContext = _viewModel;
    }

    /// <summary>
    /// Sets up keyboard shortcuts for common operations.
    /// </summary>
    private void SetupKeyboardShortcuts()
    {
        // Keyboard shortcuts are primarily handled via InputGesture in XAML Menu items
        // Additional custom shortcuts can be handled here
        this.KeyDown += OnWindowKeyDown;
    }

    /// <summary>
    /// Handles window-level keyboard shortcuts.
    /// </summary>
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (_viewModel == null)
            return;

        // Ctrl+N - New Layout
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.N)
        {
            _viewModel.NewLayoutCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+O - Open Layout
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.O)
        {
            _viewModel.OpenLayoutCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+S - Save Layout
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.S)
        {
            _viewModel.SaveLayoutCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+Shift+S - Save As
        else if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) && e.Key == Key.S)
        {
            _viewModel.SaveAsLayoutCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+E - Export TXT
        else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.E)
        {
            _viewModel.ExportTxtCommand.Execute(null);
            e.Handled = true;
        }
        // Delete - Delete selected widget
        else if (e.Key == Key.Delete)
        {
            _viewModel.DeleteWidgetCommand.Execute(null);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles the window closing event to prompt for unsaved changes.
    /// </summary>
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_viewModel?.IsDirty == true)
        {
            // Cancel the close to show dialog
            e.Cancel = true;

            // Show confirmation dialog
            var result = await ShowUnsavedChangesDialog();

            if (result == UnsavedChangesResult.Save)
            {
                // Save and close
                await _viewModel.SaveLayoutCommand.ExecuteAsync(null);
                if (!_viewModel.IsDirty) // Save succeeded
                {
                    Close();
                }
            }
            else if (result == UnsavedChangesResult.Discard)
            {
                // Close without saving
                _viewModel.IsDirty = false; // Prevent re-prompting
                Close();
            }
            // Cancel does nothing (already cancelled above)
        }
    }

    /// <summary>
    /// Shows a dialog asking the user what to do with unsaved changes.
    /// </summary>
    /// <returns>The user's choice.</returns>
    private async Task<UnsavedChangesResult> ShowUnsavedChangesDialog()
    {
        // TODO: Replace with proper dialog when available
        // For now, use a simple message box approach
        var dialog = new Window
        {
            Title = "Unsaved Changes",
            Width = 400,
            Height = 150,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var result = UnsavedChangesResult.Cancel;
        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 15
        };

        panel.Children.Add(new TextBlock
        {
            Text = "You have unsaved changes. Do you want to save before closing?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var saveButton = new Button { Content = "Save", Width = 80 };
        saveButton.Click += (_, _) =>
        {
            result = UnsavedChangesResult.Save;
            dialog.Close();
        };

        var discardButton = new Button { Content = "Discard", Width = 80 };
        discardButton.Click += (_, _) =>
        {
            result = UnsavedChangesResult.Discard;
            dialog.Close();
        };

        var cancelButton = new Button { Content = "Cancel", Width = 80 };
        cancelButton.Click += (_, _) =>
        {
            result = UnsavedChangesResult.Cancel;
            dialog.Close();
        };

        buttonPanel.Children.Add(saveButton);
        buttonPanel.Children.Add(discardButton);
        buttonPanel.Children.Add(cancelButton);

        panel.Children.Add(buttonPanel);
        dialog.Content = panel;

        await dialog.ShowDialog(this);
        return result;
    }

    /// <summary>
    /// Handles the Exit menu item click.
    /// </summary>
    private void OnExitMenuItemClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Result of the unsaved changes dialog.
    /// </summary>
    private enum UnsavedChangesResult
    {
        Save,
        Discard,
        Cancel
    }
}
