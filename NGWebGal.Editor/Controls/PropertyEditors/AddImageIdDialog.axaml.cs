using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NGWebGal.Editor.Controls.PropertyEditors;

public partial class AddImageIdDialog : Window
{
    public AddImageIdDialog()
    {
        InitializeComponent();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        var numeric = this.FindControl<NumericUpDown>("IdNumeric")!;
        Close((int?)numeric.Value);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
