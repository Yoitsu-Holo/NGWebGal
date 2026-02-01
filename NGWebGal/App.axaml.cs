using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NGWebGal.Services;
using NGWebGal.Interpreter.Moe;
using NGWebGal.Audio;

namespace NGWebGal;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<IResourceManager>(sp =>
            new FileResourceManager(Path.Combine(Environment.CurrentDirectory, "Resources")));
        services.AddSingleton<AudioManager>();
        services.AddSingleton<LayoutManager>();
        services.AddSingleton<Driver.Driver>();
        services.AddSingleton<MoeInterpreter>();
        services.AddSingleton<GameManager>();

        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}