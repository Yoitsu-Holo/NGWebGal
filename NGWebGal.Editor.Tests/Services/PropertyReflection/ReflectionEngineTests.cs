using System;
using System.Linq;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Handler;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using Xunit;

namespace NGWebGal.Editor.Tests.Services.PropertyReflection;

public class ReflectionEngineTests
{
    private readonly ReflectionEngine _engine = new();

    [Fact]
    public void DiscoverProperties_WidgetColorBox_ReturnsPositionSizeVisibleEnable()
    {
        // Arrange
        var type = typeof(WidgetColorBox);

        // Act
        var properties = _engine.DiscoverProperties(type);

        // Assert
        Assert.NotEmpty(properties);

        // Should find Position from LayerBase
        var position = properties.FirstOrDefault(p => p.Name == "Position");
        Assert.NotNull(position);
        Assert.Equal(typeof(LayerBase), position.DeclaringType);
        Assert.False(position.IsReadOnly);
        Assert.Equal(PropertyCategory.Transform, position.Category);

        // Should find Size from LayerBase
        var size = properties.FirstOrDefault(p => p.Name == "Size");
        Assert.NotNull(size);
        Assert.Equal(typeof(LayerBase), size.DeclaringType);
        Assert.False(size.IsReadOnly);

        // Should find Visible from LayerBase
        var visible = properties.FirstOrDefault(p => p.Name == "Visible");
        Assert.NotNull(visible);
        Assert.Equal(typeof(LayerBase), visible.DeclaringType);
        Assert.Equal(PropertyCategory.Display, visible.Category);

        // Should find Enable from LayerBase
        var enable = properties.FirstOrDefault(p => p.Name == "Enable");
        Assert.NotNull(enable);
        Assert.Equal(typeof(LayerBase), enable.DeclaringType);
        Assert.Equal(PropertyCategory.Display, enable.Category);

        // Should find Name from LayerBase
        var name = properties.FirstOrDefault(p => p.Name == "Name");
        Assert.NotNull(name);
        Assert.Equal(typeof(LayerBase), name.DeclaringType);
        Assert.Equal(PropertyCategory.Identity, name.Category);
    }

    [Fact]
    public void DiscoverProperties_WidgetTextBox_IncludesStyleNestedProperties()
    {
        // Arrange
        var type = typeof(WidgetTextBox);

        // Act
        var properties = _engine.DiscoverProperties(type);

        // Assert
        Assert.NotEmpty(properties);

        // Should find Text from LayerBase
        var text = properties.FirstOrDefault(p => p.Name == "Text");
        Assert.NotNull(text);
        Assert.Equal(typeof(string), text.PropertyType);
        Assert.Equal(PropertyCategory.Content, text.Category);

        // Should find TextSize from LayerBase
        var textSize = properties.FirstOrDefault(p => p.Name == "TextSize");
        Assert.NotNull(textSize);
        Assert.Equal(typeof(int), textSize.PropertyType);
        Assert.Equal(PropertyCategory.Content, textSize.Category);

        // Should find Style from WidgetTextBox
        var style = properties.FirstOrDefault(p => p.Name == "Style");
        Assert.NotNull(style);
        Assert.True(style.IsExpandable); // Style should be expandable
        Assert.Equal(PropertyCategory.Appearance, style.Category);
    }

    [Fact]
    public void DiscoverProperties_FiltersSKPaintButAllowsSKColor()
    {
        // Arrange
        var type = typeof(WidgetTextBox);

        // Act
        var properties = _engine.DiscoverProperties(type);

        // Assert
        // Should NOT find TextPaint (SKPaint is excluded)
        var textPaint = properties.FirstOrDefault(p => p.Name == "TextPaint");
        Assert.Null(textPaint);

        // Should NOT find TextFont (SKFont is excluded)
        var textFont = properties.FirstOrDefault(p => p.Name == "TextFont");
        Assert.Null(textFont);

        // Should NOT find Typeface (SKTypeface is excluded)
        var typeface = properties.FirstOrDefault(p => p.Name == "Typeface");
        Assert.Null(typeface);

        // Note: SKColor would be whitelisted if exposed as a direct property
        // (SetColor is a method, not a property, so it won't be discovered)
    }

    [Fact]
    public void DiscoverProperties_RespectsDepthLimit()
    {
        // Arrange
        var type = typeof(WidgetTextBox);

        // Act
        var properties = _engine.DiscoverProperties(type);

        // Assert
        // All properties should have HierarchyDepth within reasonable bounds
        // LayerBase is depth 0, WidgetLayerBase is depth 1, WidgetTextBox is depth 2
        var maxDepth = properties.Max(p => p.HierarchyDepth);
        Assert.True(maxDepth <= TypeFilters.MaxDepth);

        // Hierarchy should be: LayerBase (0) -> WidgetLayerBase (1) -> WidgetTextBox (2)
        var layerBaseProps = properties.Where(p => p.DeclaringType == typeof(LayerBase));
        Assert.All(layerBaseProps, p => Assert.Equal(0, p.HierarchyDepth));

        var widgetLayerBaseProps = properties.Where(p => p.DeclaringType == typeof(WidgetLayerBase));
        Assert.All(widgetLayerBaseProps, p => Assert.Equal(1, p.HierarchyDepth));

        var widgetTextBoxProps = properties.Where(p => p.DeclaringType == typeof(WidgetTextBox));
        Assert.All(widgetTextBoxProps, p => Assert.Equal(2, p.HierarchyDepth));
    }

    [Fact]
    public void DiscoverProperties_CachesResultsByType()
    {
        // Arrange
        var type = typeof(WidgetColorBox);

        // Act
        var firstCall = _engine.DiscoverProperties(type);
        var secondCall = _engine.DiscoverProperties(type);

        // Assert
        // Should return exact same reference (cached)
        Assert.Same(firstCall, secondCall);
    }

    [Fact]
    public void DiscoverProperties_CompiledGettersWork()
    {
        // Arrange
        var type = typeof(WidgetColorBox);
        var properties = _engine.DiscoverProperties(type);
        var nameProperty = properties.First(p => p.Name == "Name");

        var instance = new WidgetColorBox { Name = "TestName" };

        // Act
        var value = nameProperty.Getter(instance);

        // Assert
        Assert.Equal("TestName", value);
    }

    [Fact]
    public void DiscoverProperties_CompiledSettersWork()
    {
        // Arrange
        var type = typeof(WidgetColorBox);
        var properties = _engine.DiscoverProperties(type);
        var nameProperty = properties.First(p => p.Name == "Name");

        var instance = new WidgetColorBox();

        // Act
        nameProperty.Setter!(instance, "NewName");

        // Assert
        Assert.Equal("NewName", instance.Name);
    }

    [Fact]
    public void DiscoverProperties_ReadOnlyPropertiesHaveNullSetter()
    {
        // Arrange
        var type = typeof(WidgetColorBox);
        var properties = _engine.DiscoverProperties(type);

        // Act
        var windowProperty = properties.FirstOrDefault(p => p.Name == "Window");

        // Assert
        Assert.NotNull(windowProperty);
        Assert.True(windowProperty.IsReadOnly);
        Assert.Null(windowProperty.Setter);
    }

    [Fact]
    public void DiscoverProperties_StopsAtLayerBase()
    {
        // Arrange
        var type = typeof(WidgetColorBox);

        // Act
        var properties = _engine.DiscoverProperties(type);

        // Assert
        // Should NOT find properties from HandlerBase or Object
        var handlerBaseProps = properties.Where(p =>
            p.DeclaringType == typeof(HandlerBase) ||
            p.DeclaringType == typeof(object));

        Assert.Empty(handlerBaseProps);

        // Should include LayerBase properties
        var layerBaseProps = properties.Where(p => p.DeclaringType == typeof(LayerBase));
        Assert.NotEmpty(layerBaseProps);
    }

    [Fact]
    public void DiscoverProperties_HandlesCircularReferences()
    {
        // Arrange
        var type = typeof(WidgetColorBox);

        // Act & Assert - Should not throw StackOverflowException
        var exception = Record.Exception(() => _engine.DiscoverProperties(type));
        Assert.Null(exception);
    }

    [Fact]
    public void ClearCache_RemovesCachedResults()
    {
        // Arrange
        var type = typeof(WidgetColorBox);
        var firstCall = _engine.DiscoverProperties(type);

        // Act
        _engine.ClearCache();
        var secondCall = _engine.DiscoverProperties(type);

        // Assert
        // Should return different references after cache clear
        Assert.NotSame(firstCall, secondCall);
        // But should have same content
        Assert.Equal(firstCall.Length, secondCall.Length);
    }
}
