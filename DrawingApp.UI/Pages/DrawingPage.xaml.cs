using DrawingApp.Core.Enums;
using DrawingApp.UI.Drawing;
using DrawingApp.UI.Drawing.Tools;
using DrawingApp.UI.Navigation;
using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace DrawingApp.UI.Pages;

public sealed partial class DrawingPage : Page
{
    public DrawingViewModel ViewModel { get; }
    private CanvasInteraction? _interaction;

    // selection visual
    private Rectangle? _selectionRect;
    private bool _selecting;
    private Point _start;

    public DrawingPage()
    {
        InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<DrawingViewModel>();
        DataContext = ViewModel;

        Loaded += DrawingPage_Loaded;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is DrawingNavigationArgs args)
        {
            if (args.BoardId.HasValue)
            {
                await ViewModel.LoadBoardAsync(args.BoardId.Value);
                _interaction?.ReloadFrom(ViewModel.RuntimeShapes);
                return;
            }

            if (args.TemplateId.HasValue)
            {
                var shape = await ViewModel.LoadTemplateAsShapeAsync(args.TemplateId.Value);
                if (shape != null)
                {
                    ViewModel.RuntimeShapes.Add(shape);

                    // DrawCanvas có thể chưa init visual fully
                    if (DrawCanvas != null && !DrawCanvas.Children.Contains(shape))
                        DrawCanvas.Children.Add(shape);

                    ViewModel.CurrentBoardId = null;
                }
            }
        }
    }

    private void DrawingPage_Loaded(object sender, RoutedEventArgs e)
    {
        _interaction = new CanvasInteraction(DrawCanvas);

        // Khi hoàn tất 1 shape do tool tạo
        _interaction.ShapeCompleted += shape =>
        {
            // 1) add vào runtime collection
            if (!ViewModel.RuntimeShapes.Contains(shape))
                ViewModel.RuntimeShapes.Add(shape);

            // 2) add vào canvas nếu chưa có
            if (!DrawCanvas.Children.Contains(shape))
                DrawCanvas.Children.Add(shape);

            // 3) enable hit test để fill
            shape.IsHitTestVisible = true;
            shape.Tapped += (_, __) =>
            {
                if (ViewModel.IsFillMode)
                    ViewModel.ApplyFillTo(shape);
            };
        };

        ApplyTool(ViewModel.CurrentTool);
        _interaction.CurrentStyle = ViewModel.BuildStyle();

        ViewModel.PropertyChanged += (_, args) =>
        {
            if (_interaction == null) return;

            if (args.PropertyName == nameof(ViewModel.CurrentTool))
                ApplyTool(ViewModel.CurrentTool);

            if (args.PropertyName == nameof(ViewModel.StrokeColor)
                || args.PropertyName == nameof(ViewModel.FillColor)
                || args.PropertyName == nameof(ViewModel.Thickness))
            {
                _interaction.CurrentStyle = ViewModel.BuildStyle();
            }

            // Khi load board đổi width/height => reload overlay layout ok
            // Canvas binding đã tự update Width/Height.
        };

        // initial render (nếu RuntimeShapes đã có từ trước)
        _interaction.ReloadFrom(ViewModel.RuntimeShapes);
    }

    private void ApplyTool(ShapeType type)
    {
        if (_interaction == null) return;

        _interaction.CurrentTool = type switch
        {
            ShapeType.Line => new LineTool(),
            ShapeType.Rectangle => new RectangleTool(),
            ShapeType.Oval => new EllipseTool(ShapeType.Oval),
            ShapeType.Circle => new EllipseTool(ShapeType.Circle),
            ShapeType.Triangle => new TriangleTool(),
            ShapeType.Polygon => new PolygonTool(),
            _ => new LineTool()
        };
    }

    // Optional: đảm bảo NumberBox update VM chắc chắn
    private void BoardW_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.BoardWidth = args.NewValue;
    }

    private void BoardH_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (!double.IsNaN(args.NewValue))
            ViewModel.BoardHeight = args.NewValue;
    }

    // ==========================
    // Selection handlers
    // ==========================

    private void Selection_Pressed(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsSelectionMode) return;

        _selecting = true;
        _start = e.GetCurrentPoint(SelectionOverlay).Position;

        EnsureSelectionVisual();
        UpdateSelectionVisual(_start, _start);

        e.Handled = true;
    }

    private void Selection_Moved(object sender, PointerRoutedEventArgs e)
    {
        if (!_selecting || !ViewModel.IsSelectionMode) return;

        var cur = e.GetCurrentPoint(SelectionOverlay).Position;
        UpdateSelectionVisual(_start, cur);

        e.Handled = true;
    }

    private void Selection_Released(object sender, PointerRoutedEventArgs e)
    {
        if (!_selecting || !ViewModel.IsSelectionMode) return;

        _selecting = false;
        var end = e.GetCurrentPoint(SelectionOverlay).Position;

        var rect = GetRect(_start, end);
        UpdateSelectionVisual(_start, end);

        // tìm shapes trong rect
        var selected = FindShapesInRect(rect);

        // cập nhật selection qua VM method
        ViewModel.SetSelectedShapes(selected);

        e.Handled = true;
    }

    private IEnumerable<Shape> FindShapesInRect(Rect r)
    {
        var shapes = DrawCanvas.Children.OfType<Shape>();

        foreach (var s in shapes)
        {
            var b = GetShapeBounds(s);
            if (Intersects(r, b))
                yield return s;
        }
    }

    private static bool Intersects(Rect a, Rect b)
    {
        return a.X < b.X + b.Width &&
               a.X + a.Width > b.X &&
               a.Y < b.Y + b.Height &&
               a.Y + a.Height > b.Y;
    }

    private Rect GetShapeBounds(Shape s)
    {
        return s switch
        {
            Line l => new Rect(
                Math.Min(l.X1, l.X2),
                Math.Min(l.Y1, l.Y2),
                Math.Max(1, Math.Abs(l.X1 - l.X2)),
                Math.Max(1, Math.Abs(l.Y1 - l.Y2))
            ),

            Microsoft.UI.Xaml.Shapes.Rectangle or Ellipse =>
                new Rect(
                    Canvas.GetLeft(s),
                    Canvas.GetTop(s),
                    s.Width,
                    s.Height
                ),

            Polygon p => GetPolygonBounds(p),

            _ => new Rect(0, 0, 0, 0)
        };
    }

    private static Rect GetPolygonBounds(Polygon p)
    {
        if (p.Points == null || p.Points.Count == 0)
            return new Rect(0, 0, 0, 0);

        double minX = p.Points.Min(pt => pt.X);
        double minY = p.Points.Min(pt => pt.Y);
        double maxX = p.Points.Max(pt => pt.X);
        double maxY = p.Points.Max(pt => pt.Y);

        return new Rect(minX, minY, Math.Max(1, maxX - minX), Math.Max(1, maxY - minY));
    }

    private void EnsureSelectionVisual()
    {
        if (_selectionRect != null) return;

        _selectionRect = new Rectangle
        {
            Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 215)),
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 0, 120, 215))
        };

        SelectionOverlay.Children.Add(_selectionRect);
    }

    private void UpdateSelectionVisual(Point start, Point end)
    {
        EnsureSelectionVisual();

        var rect = GetRect(start, end);

        Canvas.SetLeft(_selectionRect!, rect.X);
        Canvas.SetTop(_selectionRect!, rect.Y);
        _selectionRect!.Width = rect.Width;
        _selectionRect!.Height = rect.Height;
    }

    private static Rect GetRect(Point a, Point b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var w = Math.Abs(a.X - b.X);
        var h = Math.Abs(a.Y - b.Y);
        return new Rect(x, y, w, h);
    }
}
