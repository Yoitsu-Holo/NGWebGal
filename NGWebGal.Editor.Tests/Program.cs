using System;
using System.IO;
using NGWebGal.Editor.Models;
using NGWebGal.Editor.Services;

namespace NGWebGal.Editor.Tests;

/// <summary>
/// Simple test to verify serialization works correctly.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== NGWebGal.Editor Functionality Test ===\n");

        // Create test layout
        var layout = new EditorLayout
        {
            Name = "TestLayout",
            CanvasWidth = 1920,
            CanvasHeight = 1080
        };

        // Add test widgets
        layout.Widgets.Add(new EditorWidget
        {
            Type = WidgetType.Button,
            Name = "StartButton",
            X = 810,
            Y = 450,
            Width = 300,
            Height = 60,
            Text = "Start Game",
            Visible = true,
            Enable = true,
            ZIndex = 10
        });

        layout.Widgets.Add(new EditorWidget
        {
            Type = WidgetType.TextBox,
            Name = "TitleText",
            X = 660,
            Y = 200,
            Width = 600,
            Height = 100,
            Text = "Welcome to WebGal",
            Visible = true,
            Enable = true,
            ZIndex = 5
        });

        Console.WriteLine($"Created layout: {layout.Name}");
        Console.WriteLine($"Canvas size: {layout.CanvasWidth}x{layout.CanvasHeight}");
        Console.WriteLine($"Widget count: {layout.Widgets.Count}\n");

        // Test XML serialization
        var xmlSerializer = new XmlLayoutSerializer();
        var xmlPath = Path.Combine(Path.GetTempPath(), "test_layout.xml");

        try
        {
            Console.WriteLine("Testing XML serialization...");
            xmlSerializer.Save(layout, xmlPath);
            Console.WriteLine($"✓ Saved to: {xmlPath}");

            var loadedLayout = xmlSerializer.Load(xmlPath);
            Console.WriteLine($"✓ Loaded: {loadedLayout.Name} with {loadedLayout.Widgets.Count} widgets");

            // Verify data integrity
            bool dataIntact = loadedLayout.Name == layout.Name &&
                              loadedLayout.Widgets.Count == layout.Widgets.Count &&
                              loadedLayout.Widgets[0].Name == "StartButton";

            if (dataIntact)
            {
                Console.WriteLine("✓ Data integrity verified\n");
            }
            else
            {
                Console.WriteLine("✗ Data integrity check failed\n");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ XML serialization failed: {ex.Message}\n");
            return;
        }

        // Test TXT export
        var txtExporter = new TxtExporter();
        var txtPath = Path.Combine(Path.GetTempPath(), "test_export.txt");

        try
        {
            Console.WriteLine("Testing TXT export...");
            txtExporter.Export(layout, txtPath);
            Console.WriteLine($"✓ Exported to: {txtPath}");

            var txtContent = File.ReadAllText(txtPath);
            bool containsExpected = txtContent.Contains("StartButton") &&
                                    txtContent.Contains("TitleText") &&
                                    txtContent.Contains("Widget Count: 2");

            if (containsExpected)
            {
                Console.WriteLine("✓ TXT export format verified\n");
            }
            else
            {
                Console.WriteLine("✗ TXT export format check failed\n");
                return;
            }

            Console.WriteLine("Preview of exported TXT:");
            Console.WriteLine(new string('-', 60));
            var lines = txtContent.Split('\n');
            for (int i = 0; i < Math.Min(20, lines.Length); i++)
            {
                Console.WriteLine(lines[i]);
            }
            Console.WriteLine(new string('-', 60));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ TXT export failed: {ex.Message}\n");
            return;
        }

        Console.WriteLine("\n=== All Tests Passed! ===");
        Console.WriteLine("\nFiles created:");
        Console.WriteLine($"  XML: {xmlPath}");
        Console.WriteLine($"  TXT: {txtPath}");
    }
}
