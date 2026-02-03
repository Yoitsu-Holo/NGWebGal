using System;
using System.Linq;
using System.Reflection;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using Xunit;
using Xunit.Abstractions;

namespace NGWebGal.Editor.Tests.Services.PropertyReflection;

public class DiagnosticTests
{
    private readonly ITestOutputHelper _output;

    public DiagnosticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DebugGetPropertiesDirectly()
    {
        var type = typeof(LayerBase);
        var props = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        _output.WriteLine($"LayerBase declared properties count: {props.Length}");
        foreach (var prop in props)
        {
            _output.WriteLine($"  {prop.Name} : {prop.PropertyType.Name}");
        }
    }

    [Fact]
    public void DebugDiscoverProperties_WidgetColorBox()
    {
        // Arrange
        var engine = new ReflectionEngine();
        var type = typeof(WidgetColorBox);

        // Act
        var properties = engine.DiscoverProperties(type);

        // Assert & Debug Output
        _output.WriteLine($"Total properties discovered: {properties.Length}");
        foreach (var prop in properties.OrderBy(p => p.HierarchyDepth).ThenBy(p => p.Name))
        {
            _output.WriteLine($"  [{prop.HierarchyDepth}] {prop.DeclaringType.Name}.{prop.Name} : {prop.PropertyType.Name} ({prop.Category}) ReadOnly={prop.IsReadOnly}");
        }

        // Check for specific expected properties
        var hasPosition = properties.Any(p => p.Name == "Position");
        var hasSize = properties.Any(p => p.Name == "Size");
        var hasName = properties.Any(p => p.Name == "Name");
        var hasVisible = properties.Any(p => p.Name == "Visible");
        var hasOffset = properties.Any(p => p.Name == "Offset");

        _output.WriteLine($"\nExpected properties:");
        _output.WriteLine($"  Position: {hasPosition}");
        _output.WriteLine($"  Size: {hasSize}");
        _output.WriteLine($"  Name: {hasName}");
        _output.WriteLine($"  Visible: {hasVisible}");
        _output.WriteLine($"  Offset: {hasOffset}");

        Assert.NotEmpty(properties);
    }
}
