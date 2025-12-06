using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing.Tools;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace DrawingApp.UI.Drawing;

public class CanvasInteraction
{
    private readonly Canvas _canvas;

    public IDrawTool? CurrentTool { get; set; }
    public StrokeStyle CurrentStyle { get; set; } = new();

    public event Action<Shape>? ShapeCompleted;

    private bool _isDrawing;

    public CanvasInteraction(Canvas canvas)
    {
        _canvas = canvas;

        _canvas.PointerPressed += OnPressed;
        _canvas.PointerMoved += OnMoved;
        _canvas.PointerReleased += OnReleased;
        _canvas.DoubleTapped += OnDoubleTapped;
        _canvas.Tapped += OnTapped;
    }

    private void OnPressed(object sender, PointerRoutedEventArgs e)
    {
        if (CurrentTool == null) return;

        var p = e.GetCurrentPoint(_canvas).Position;
        _isDrawing = true;

        CurrentTool.Begin(p, CloneStyle(CurrentStyle));
        if (CurrentTool.Preview != null && !_canvas.Children.Contains(CurrentTool.Preview))
            _canvas.Children.Add(CurrentTool.Preview);
    }

    private void OnMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentTool == null) return;
        var p = e.GetCurrentPoint(_canvas).Position;
        CurrentTool.Update(p);
    }

    private void OnReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentTool == null) return;

        var p = e.GetCurrentPoint(_canvas).Position;

        // Polygon: không finalize ở release
        if (CurrentTool is PolygonTool) return;

        var shape = CurrentTool.End(p);
        _isDrawing = false;

        if (shape != null)
            ShapeCompleted?.Invoke(shape);
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (CurrentTool is PolygonTool poly)
        {
            var p = e.GetPosition(_canvas);
            poly.AddPoint(p);
        }
    }

    private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (CurrentTool is PolygonTool poly)
        {
            var p = e.GetPosition(_canvas);
            var shape = poly.End(p);
            _isDrawing = false;

            if (shape != null)
                ShapeCompleted?.Invoke(shape);
        }
    }

    private static StrokeStyle CloneStyle(StrokeStyle s)
        => new()
        {
            StrokeColor = s.StrokeColor,
            FillColor = s.FillColor,
            Thickness = s.Thickness,
            Dash = s.Dash
        };
}
