using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using NGWebGal.Types;
using NGWebGal.Layer;
using NGWebGal.Services;
using NGWebGal.Example;
using GameMouseButton = NGWebGal.Handler.Event.MouseButton;
using GameMouseStatus = NGWebGal.Handler.Event.MouseStatus;
using GameKeyModifiers = NGWebGal.Handler.Event.KeyModifiers;
using GameKeyStatus = NGWebGal.Handler.Event.KeyStatus;
using MouseEventData = NGWebGal.Handler.Event.MouseEventData;
using KeyboardEventData = NGWebGal.Handler.Event.KeyboardEventData;

namespace NGWebGal.Views;

public partial class GameView : UserControl
{
    private readonly Stopwatch _frameTimer = Stopwatch.StartNew();
    private readonly Stopwatch _fpsTimer = Stopwatch.StartNew();
    private ulong _frameCount = 0;
    private double _currentFps = 0;
    private long _lastFrameTime = 0;
    private double _deltaTime = 0;

    // Input state
    private Point _mousePosition;
    private Point _lastMousePosition;
    private bool _isLeftButtonPressed = false;
    private bool _isRightButtonPressed = false;

    // Services
    private readonly GameManager? _gameManager;

    // Example UI page
    private readonly ExamplePage _examplePage = new();

    public GameView()
    {
        InitializeComponent();
        ClipToBounds = true;
        Focusable = true;

        // Get GameManager from DI
        if (Application.Current is App app && app.Services != null)
        {
            _gameManager = app.Services.GetService(typeof(GameManager)) as GameManager;
        }

        // Attach input event handlers
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerMoved += OnPointerMoved;
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        Console.WriteLine("[GameView] Event handlers attached");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Console.WriteLine("[GameView] Attached to visual tree");
        Focus();
        _frameTimer.Restart();
        _fpsTimer.Restart();
    }

    // Input event handlers
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _lastMousePosition = _mousePosition;
        _mousePosition = point.Position;
        Console.WriteLine($"[PointerPressed] Position: {point.Position.X}, {point.Position.Y}");

        var button = GameMouseButton.Empty;
        var status = GameMouseStatus.Down;

        if (point.Properties.IsLeftButtonPressed)
        {
            _isLeftButtonPressed = true;
            button = GameMouseButton.LButton;
        }
        if (point.Properties.IsRightButtonPressed)
        {
            _isRightButtonPressed = true;
            button = GameMouseButton.RButton;
        }
        if (point.Properties.IsMiddleButtonPressed)
            button = GameMouseButton.MButton;

        var eventData = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Move = new IVector(
                (int)(_mousePosition.X - _lastMousePosition.X),
                (int)(_mousePosition.Y - _lastMousePosition.Y)
            ),
            Button = button,
            Status = status
        };

        _gameManager?.ProcessEvent(eventData);
        var mouseEvent = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Button = GameMouseButton.LButton,
            Status = GameMouseStatus.Hold
        };
        _examplePage.ProcessEvent(mouseEvent);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _mousePosition = point.Position;
        Console.WriteLine($"[PointerReleased] Position: {point.Position.X}, {point.Position.Y}");
        var updateKind = point.Properties.PointerUpdateKind;

        var button = GameMouseButton.Empty;
        var status = GameMouseStatus.Up;

        if (updateKind == PointerUpdateKind.LeftButtonReleased)
        {
            _isLeftButtonPressed = false;
            button = GameMouseButton.LButton;
        }
        if (updateKind == PointerUpdateKind.RightButtonReleased)
        {
            _isRightButtonPressed = false;
            button = GameMouseButton.RButton;
        }
        if (updateKind == PointerUpdateKind.MiddleButtonReleased)
            button = GameMouseButton.MButton;

        var eventData = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Move = new IVector(0, 0),
            Button = button,
            Status = status
        };

        _gameManager?.ProcessEvent(eventData);
        var mouseEvent = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Button = GameMouseButton.LButton,
            Status = GameMouseStatus.Release
        };
        _examplePage.ProcessEvent(mouseEvent);
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _lastMousePosition = _mousePosition;
        _mousePosition = point.Position;
        Console.WriteLine($"[PointerMoved] Position: {point.Position.X}, {point.Position.Y}");

        var button = GameMouseButton.Empty;
        if (_isLeftButtonPressed) button |= GameMouseButton.LButton;
        if (_isRightButtonPressed) button |= GameMouseButton.RButton;

        var eventData = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Move = new IVector(
                (int)(_mousePosition.X - _lastMousePosition.X),
                (int)(_mousePosition.Y - _lastMousePosition.Y)
            ),
            Button = button,
            Status = _isLeftButtonPressed || _isRightButtonPressed ? GameMouseStatus.Hold : GameMouseStatus.Release
        };

        _gameManager?.ProcessEvent(eventData);
        var mouseEvent = new MouseEventData
        {
            Position = new IVector((int)_mousePosition.X, (int)_mousePosition.Y),
            Button = _isLeftButtonPressed ? GameMouseButton.LButton : GameMouseButton.Empty,
            Status = _isLeftButtonPressed ? GameMouseStatus.Hold : GameMouseStatus.Release
        };
        _examplePage.ProcessEvent(mouseEvent);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var modifiers = GameKeyModifiers.None;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt))
            modifiers |= GameKeyModifiers.Alt;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
            modifiers |= GameKeyModifiers.Control;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
            modifiers |= GameKeyModifiers.Shift;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Meta))
            modifiers |= GameKeyModifiers.Meta;

        var eventData = new KeyboardEventData
        {
            Key = e.Key,
            Status = GameKeyStatus.Down,
            Modifiers = modifiers,
            Text = null
        };

        _gameManager?.ProcessEvent(eventData);
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var modifiers = GameKeyModifiers.None;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt))
            modifiers |= GameKeyModifiers.Alt;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
            modifiers |= GameKeyModifiers.Control;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
            modifiers |= GameKeyModifiers.Shift;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Meta))
            modifiers |= GameKeyModifiers.Meta;

        var eventData = new KeyboardEventData
        {
            Key = e.Key,
            Status = GameKeyStatus.Up,
            Modifiers = modifiers,
            Text = null
        };

        _gameManager?.ProcessEvent(eventData);
    }

    // Custom draw operation for SkiaSharp rendering
    private class GameViewDrawOp : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly double _elapsedSeconds;
        private readonly double _fps;
        private readonly double _deltaTime;
        private readonly Point _mousePosition;
        private readonly ExamplePage _examplePage;

        private readonly bool _isLeftButtonPressed;
        private readonly bool _isRightButtonPressed;

        public GameViewDrawOp(Rect bounds, double elapsedSeconds, double fps, double deltaTime, Point mousePosition, ExamplePage examplePage, bool isLeftButtonPressed, bool isRightButtonPressed)
        {
            _bounds = bounds;
            _elapsedSeconds = elapsedSeconds;
            _fps = fps;
            _deltaTime = deltaTime;
            _mousePosition = mousePosition;
            _examplePage = examplePage;
            _isLeftButtonPressed = isLeftButtonPressed;
            _isRightButtonPressed = isRightButtonPressed;
        }

        public void Dispose() { }

        public Rect Bounds => _bounds;

        public bool HitTest(Point p) => false;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            // Delegate all rendering to ExamplePage
            _examplePage.Render(canvas, _bounds, _elapsedSeconds, _fps, _deltaTime, _mousePosition, _isLeftButtonPressed, _isRightButtonPressed);
        }








    }

    // Render method - called every frame
    public override void Render(DrawingContext context)
    {
        // Update frame timing
        long currentTime = _frameTimer.ElapsedMilliseconds;
        _deltaTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;

        // Update FPS counter
        _frameCount++;
        if (_fpsTimer.ElapsedMilliseconds >= 1000)
        {
            _currentFps = _frameCount * 1000.0 / _fpsTimer.ElapsedMilliseconds;
            _frameCount = 0;
            _fpsTimer.Restart();
        }

        // Create custom draw operation
        var drawOp = new GameViewDrawOp(
            new Rect(0, 0, Bounds.Width, Bounds.Height),
            _frameTimer.Elapsed.TotalSeconds,
            _currentFps,
            _deltaTime,
            _mousePosition,
            _examplePage,
            _isLeftButtonPressed,
            _isRightButtonPressed
        );
        Console.WriteLine($"[Render] Mouse position: {_mousePosition.X}, {_mousePosition.Y}");

        context.Custom(drawOp);

        // Queue next frame - 60 FPS render loop
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}
