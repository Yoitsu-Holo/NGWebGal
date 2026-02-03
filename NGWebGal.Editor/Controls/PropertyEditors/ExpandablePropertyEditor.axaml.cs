using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using NGWebGal.Editor.Services.PropertyReflection;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for complex expandable types with nested properties
/// </summary>
public partial class ExpandablePropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private object? _nestedTarget;
    private readonly ReflectionEngine _reflectionEngine;
    private readonly PropertyEditorFactory _editorFactory;
    private readonly ObservableCollection<IPropertyEditor> _childEditors;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public ExpandablePropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _reflectionEngine = new ReflectionEngine();
        _editorFactory = new PropertyEditorFactory();
        _childEditors = new ObservableCollection<IPropertyEditor>();

        InitializeComponent();

        // Configure header
        HeaderTextBlock.Text = descriptor.Name;

        // Set up child properties control
        ChildPropertiesControl.ItemsSource = _childEditors;
    }

    public void Bind(object target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

        // Get nested object
        _nestedTarget = Descriptor.Getter(_target);

        if (_nestedTarget == null)
        {
            PropertyExpander.IsEnabled = false;
            return;
        }

        PropertyExpander.IsEnabled = true;

        // Discover child properties
        var childProperties = _reflectionEngine.DiscoverProperties(_nestedTarget.GetType());

        // Create editors for each child property
        foreach (var childProperty in childProperties)
        {
            try
            {
                // Skip read-only for now
                if (childProperty.IsReadOnly)
                    continue;

                var editor = _editorFactory.CreateEditor(childProperty);
                editor.Bind(_nestedTarget);
                editor.ValueChanged += OnChildEditorValueChanged;

                _childEditors.Add(editor);
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
    }

    public void Unbind()
    {
        // Unbind all child editors
        foreach (var editor in _childEditors)
        {
            editor.ValueChanged -= OnChildEditorValueChanged;
            editor.Unbind();
        }

        _childEditors.Clear();
        _target = null;
        _nestedTarget = null;
    }

    private void OnChildEditorValueChanged(object? sender, EventArgs e)
    {
        // When a nested property changes, propagate the event upward
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
