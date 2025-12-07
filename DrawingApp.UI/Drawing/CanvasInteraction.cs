using DrawingApp.Core.Models;
using DrawingApp.UI.Drawing.Tools;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.Foundation;

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

        _canvas.Tapped += OnTapped;
        _canvas.DoubleTapped += OnDoubleTapped;
    }

    private void OnPressed(object sender, PointerRoutedEventArgs e)
    {
        if (CurrentTool == null) return;

        // nếu đang selection mode thì page sẽ chặn overlay
        var p = e.GetCurrentPoint(_canvas).Position;
        _isDrawing = true;

        CurrentTool.Begin(p, CloneStyle(CurrentStyle));

        if (CurrentTool.Preview != null && !_canvas.Children.Contains(CurrentTool.Preview))
            _canvas.Children.Add(CurrentTool.Preview);

        _canvas.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentTool == null) return;

        var p = e.GetCurrentPoint(_canvas).Position;
        CurrentTool.Update(p);

        e.Handled = true;
    }

    private void OnReleased(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDrawing || CurrentTool == null) return;

        var p = e.GetCurrentPoint(_canvas).Position;

        // Polygon không finalize ở release
        if (CurrentTool is PolygonTool)
        {
            e.Handled = true;
            return;
        }

        var shape = CurrentTool.End(p);

        _isDrawing = false;
        _canvas.ReleasePointerCapture(e.Pointer);

        if (shape != null)
        {
            // shape ở đây chính là preview đã nằm trên canvas
            ShapeCompleted?.Invoke(shape);
        }

        e.Handled = true;
    }

    private void OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (CurrentTool is PolygonTool poly)
        {
            var p = e.GetPosition(_canvas);

            // nếu user tap trước khi pressed
            if (poly.Preview == null)
            {
                poly.Begin(p, CloneStyle(CurrentStyle));
                if (poly.Preview != null && !_canvas.Children.Contains(poly.Preview))
                    _canvas.Children.Add(poly.Preview);
            }

            poly.AddPoint(p);
            e.Handled = true;
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

            e.Handled = true;
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

    public void ReloadFrom(IEnumerable<Shape> shapes)
    {
        _canvas.Children.Clear();
        foreach (var s in shapes)
            _canvas.Children.Add(s);
    }
}
