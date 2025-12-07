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
    private const double MIN_SEL_SIZE = 12;

    // ==========================
    // 1) Drag-box selecting state
    // ==========================
    private bool _selecting;
    private Point _start;

    // ==========================
    // 2) Paint-like selection visuals
    // ==========================
    private Rectangle? _selectionRect;            // dashed main rect
    private readonly List<Rectangle> _handles = new(); // 8 handles

    private const double HANDLE_SIZE = 10;

    private Rect _selectionBounds;               // current bounds of selection group
    private Rect _selectionStartBounds;          // snapshot for move/resize start

    // ==========================
    // 3) Group drag/resize state
    // ==========================
    private bool _movingGroup;
    private bool _resizingGroup;
    private HandleType _activeHandle = HandleType.None;

    private Point _gestureStart;                 // pointer start for move/resize

    // snapshot geometry for ALL selected shapes
    private readonly Dictionary<Shape, ShapeGeom> _groupSnapshot = new();

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

                // attach behaviors for loaded shapes
                foreach (var s in DrawCanvas.Children.OfType<Shape>())
                    AttachShapeBehaviors(s);

                ClearSelectionVisuals();
                return;
            }

            if (args.TemplateId.HasValue)
            {
                var shape = await ViewModel.LoadTemplateAsShapeAsync(args.TemplateId.Value);
                if (shape != null)
                {
                    ViewModel.RuntimeShapes.Add(shape);

                    if (DrawCanvas != null && !DrawCanvas.Children.Contains(shape))
                        DrawCanvas.Children.Add(shape);

                    AttachShapeBehaviors(shape);

                    ViewModel.CurrentBoardId = null;
                }

                ClearSelectionVisuals();
            }
        }
    }

    private void DrawingPage_Loaded(object sender, RoutedEventArgs e)
    {
        _interaction = new CanvasInteraction(DrawCanvas);

        _interaction.ShapeCompleted += shape =>
        {
            if (!ViewModel.RuntimeShapes.Contains(shape))
                ViewModel.RuntimeShapes.Add(shape);

            if (!DrawCanvas.Children.Contains(shape))
                DrawCanvas.Children.Add(shape);

            AttachShapeBehaviors(shape);

            // nếu đang selection mode mà vừa tạo shape mới
            // thì không auto select, chỉ refresh visuals khi user chọn lại
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

            // ✅ Toggle selection mode => toggle selection visuals too
            if (args.PropertyName == nameof(ViewModel.IsSelectionMode))
            {
                _interaction.IsEnabled = !ViewModel.IsSelectionMode;

                if (!ViewModel.IsSelectionMode)
                {
                    ViewModel.SetSelectedShapes(Array.Empty<Shape>());
                    ClearSelectionVisuals();
                }
                else
                {
                    // nếu bật lại mà đang có selection sẵn
                    if (ViewModel.HasSelection)
                        BuildSelectionFromShapes();
                }
            }
        };

        _interaction.ReloadFrom(ViewModel.RuntimeShapes);

        foreach (var s in DrawCanvas.Children.OfType<Shape>())
            AttachShapeBehaviors(s);

        _interaction.IsEnabled = !ViewModel.IsSelectionMode;

        ClearSelectionVisuals();
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

    private void AttachShapeBehaviors(Shape shape)
    {
        shape.IsHitTestVisible = true;

        shape.Tapped += (_, __) =>
        {
            if (ViewModel.IsFillMode)
                ViewModel.ApplyFillTo(shape);
        };

        // NOTE:
        // Chúng ta không dùng pointer drag tại shape nữa để move group,
        // vì muốn giống Paint: kéo khung selection để move/resize.
        // Nhưng vẫn có thể giữ single click select nếu bạn muốn.
        shape.PointerPressed += Shape_SingleSelect_Pressed;
    }

    // ==========================
    // Single shape quick select (optional)
    // ==========================
    private void Shape_SingleSelect_Pressed(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsSelectionMode) return;
        if (sender is not Shape hit) return;

        // click 1 shape => select nó
        if (!ViewModel.SelectedShapes.Contains(hit))
        {
            ViewModel.SetSelectedShapes(new[] { hit });
            BuildSelectionFromShapes();
        }

        e.Handled = true;
    }

    // ==========================
    // SelectionOverlay Handlers
    // ==========================

    private void Selection_Pressed(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsSelectionMode) return;

        var p = e.GetCurrentPoint(SelectionOverlay).Position;

        // 1) nếu đang có selection rect => check hit handles/inside rect
        if (_selectionRect != null && ViewModel.HasSelection)
        {
            var hitHandle = HitTestHandle(p);
            if (hitHandle != HandleType.None)
            {
                BeginResize(hitHandle, p, e);
                return;
            }

            if (PointInRect(p, _selectionBounds))
            {
                BeginMoveGroup(p, e);
                return;
            }
        }

        // 2) fallback => start drag-box select
        _selecting = true;
        _start = p;

        EnsureSelectionRect();
        HideHandles();

        UpdateSelectionRectVisual(GetRect(_start, _start));

        e.Handled = true;
    }

    private void Selection_Moved(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsSelectionMode) return;

        var p = e.GetCurrentPoint(SelectionOverlay).Position;

        // drag-box selecting
        if (_selecting)
        {
            UpdateSelectionRectVisual(GetRect(_start, p));
            e.Handled = true;
            return;
        }

        // moving group
        if (_movingGroup)
        {
            var dx = p.X - _gestureStart.X;
            var dy = p.Y - _gestureStart.Y;

            ApplyGroupMove(dx, dy);

            e.Handled = true;
            return;
        }

        // resizing group
        if (_resizingGroup)
        {
            var newBounds = ComputeResizedBounds(_selectionStartBounds, _activeHandle, p);

            // tránh width/height 0
            if (newBounds.Width < 2 || newBounds.Height < 2)
            {
                e.Handled = true;
                return;
            }

            ApplyGroupResize(_selectionStartBounds, newBounds);

            e.Handled = true;
            return;
        }
    }

    private void Selection_Released(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsSelectionMode) return;

        var p = e.GetCurrentPoint(SelectionOverlay).Position;

        // end drag-box select
        if (_selecting)
        {
            _selecting = false;

            var rect = GetRect(_start, p);
            UpdateSelectionRectVisual(rect);

            var selected = FindShapesInRect(rect);
            ViewModel.SetSelectedShapes(selected);

            if (ViewModel.HasSelection)
            {
                
                BuildSelectionFromRect(rect);
            }
            else
            {
                ClearSelectionVisuals(keepRectForBox: false);
            }

            e.Handled = true;
            return;
        }

        // end move/resize
        if (_movingGroup || _resizingGroup)
        {
            _movingGroup = false;
            _resizingGroup = false;
            _activeHandle = HandleType.None;
            _groupSnapshot.Clear();

            // refresh bounds from actual shapes
            if (ViewModel.HasSelection)
                UpdateSelectionRectVisual(_selectionBounds);
                UpdateHandlesPositions(_selectionBounds);
                ShowHandles();

            SelectionOverlay.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }

    // ==========================
    // Begin Move / Resize
    // ==========================

    private void BeginMoveGroup(Point p, PointerRoutedEventArgs e)
    {
        SnapshotGroup();

        _movingGroup = true;
        _resizingGroup = false;
        _activeHandle = HandleType.None;

        _gestureStart = p;
        _selectionStartBounds = _selectionBounds;

        SelectionOverlay.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void BeginResize(HandleType handle, Point p, PointerRoutedEventArgs e)
    {
        SnapshotGroup();

        _resizingGroup = true;
        _movingGroup = false;
        _activeHandle = handle;

        _gestureStart = p;
        _selectionStartBounds = _selectionBounds;

        SelectionOverlay.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void SnapshotGroup()
    {
        _groupSnapshot.Clear();

        foreach (var s in ViewModel.SelectedShapes)
            _groupSnapshot[s] = CaptureShapeGeom(s);
    }

    // ==========================
    // Build Paint-like selection from shapes
    // ==========================

    private void BuildSelectionFromShapes()
    {
        EnsureSelectionRect();
        EnsureHandles();

        _selectionBounds = GetUnionBounds(ViewModel.SelectedShapes);

        UpdateSelectionRectVisual(_selectionBounds);
        UpdateHandlesPositions(_selectionBounds);
        ShowHandles();
    }

    private Rect GetUnionBounds(IEnumerable<Shape> shapes)
    {
        var list = shapes.ToList();
        if (list.Count == 0) return new Rect(0, 0, 0, 0);

        Rect union = GetShapeBounds(list[0]);
        for (int i = 1; i < list.Count; i++)
        {
            var b = GetShapeBounds(list[i]);
            union = Union(union, b);
        }

        // padding nhẹ cho đẹp như Paint
        union = new Rect(union.X - 2, union.Y - 2, union.Width + 4, union.Height + 4);

        return union;
    }

    private static Rect Union(Rect a, Rect b)
    {
        var x1 = Math.Min(a.X, b.X);
        var y1 = Math.Min(a.Y, b.Y);
        var x2 = Math.Max(a.X + a.Width, b.X + b.Width);
        var y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);
        return new Rect(x1, y1, Math.Max(1, x2 - x1), Math.Max(1, y2 - y1));
    }

    // ==========================
    // Group Move
    // ==========================

    private void ApplyGroupMove(double dx, double dy)
    {
        // update shapes from snapshot
        foreach (var s in ViewModel.SelectedShapes)
        {
            if (_groupSnapshot.TryGetValue(s, out var g))
                ApplyShapeGeomWithOffset(s, g, dx, dy);
        }

        // update selection rect visual
        var moved = new Rect(
            _selectionStartBounds.X + dx,
            _selectionStartBounds.Y + dy,
            _selectionStartBounds.Width,
            _selectionStartBounds.Height);

        _selectionBounds = moved;
        UpdateSelectionRectVisual(moved);
        UpdateHandlesPositions(moved);
    }

    // ==========================
    // Group Resize (scale)
    // ==========================

    private void ApplyGroupResize(Rect oldBounds, Rect newBounds)
    {
        double sx = newBounds.Width / oldBounds.Width;
        double sy = newBounds.Height / oldBounds.Height;

        foreach (var s in ViewModel.SelectedShapes)
        {
            if (_groupSnapshot.TryGetValue(s, out var g))
                ApplyShapeGeomScaled(s, g, oldBounds, newBounds, sx, sy);
        }

        _selectionBounds = newBounds;
        UpdateSelectionRectVisual(newBounds);
        UpdateHandlesPositions(newBounds);
    }

    private Rect ComputeResizedBounds(Rect start, HandleType handle, Point current)
    {
        double left = start.X;
        double top = start.Y;
        double right = start.X + start.Width;
        double bottom = start.Y + start.Height;

        // 1) apply raw drag based on handle
        switch (handle)
        {
            case HandleType.TopLeft:
                left = current.X; top = current.Y; break;
            case HandleType.Top:
                top = current.Y; break;
            case HandleType.TopRight:
                right = current.X; top = current.Y; break;
            case HandleType.Right:
                right = current.X; break;
            case HandleType.BottomRight:
                right = current.X; bottom = current.Y; break;
            case HandleType.Bottom:
                bottom = current.Y; break;
            case HandleType.BottomLeft:
                left = current.X; bottom = current.Y; break;
            case HandleType.Left:
                left = current.X; break;
        }

        // 2) prevent inversion horizontally
        double w = right - left;

        bool affectsLeft =
            handle is HandleType.Left or HandleType.TopLeft or HandleType.BottomLeft;
        bool affectsRight =
            handle is HandleType.Right or HandleType.TopRight or HandleType.BottomRight;

        if (w < MIN_SEL_SIZE)
        {
            if (affectsLeft && !affectsRight)
                left = right - MIN_SEL_SIZE;
            else
                right = left + MIN_SEL_SIZE;
        }

        // 3) prevent inversion vertically
        double h = bottom - top;

        bool affectsTop =
            handle is HandleType.Top or HandleType.TopLeft or HandleType.TopRight;
        bool affectsBottom =
            handle is HandleType.Bottom or HandleType.BottomLeft or HandleType.BottomRight;

        if (h < MIN_SEL_SIZE)
        {
            if (affectsTop && !affectsBottom)
                top = bottom - MIN_SEL_SIZE;
            else
                bottom = top + MIN_SEL_SIZE;
        }

        // 4) return stable non-flipped rect
        return new Rect(left, top, right - left, bottom - top);
    }

    // ==========================
    // Find shapes inside selection drag-box
    // ==========================

    private IEnumerable<Shape> FindShapesInRect(Rect r)
    {
        // ✅ ưu tiên runtime shapes để không dính preview/tool artifacts
        var shapes = ViewModel.RuntimeShapes;

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

    // ==========================
    // Shape bounds
    // ==========================

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
                    Math.Max(1, s.Width),
                    Math.Max(1, s.Height)
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

    // ==========================
    // Paint-like selection visuals
    // ==========================

    private void EnsureSelectionRect()
    {
        if (_selectionRect != null) return;

        _selectionRect = new Rectangle
        {
            Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 215)),
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 4, 2 },
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(15, 0, 120, 215)),
            IsHitTestVisible = false // hit test do overlay handle
        };

        SelectionOverlay.Children.Add(_selectionRect);
    }

    private void EnsureHandles()
    {
        if (_handles.Count > 0) return;

        // 8 handles
        CreateHandle(HandleType.TopLeft);
        CreateHandle(HandleType.Top);
        CreateHandle(HandleType.TopRight);
        CreateHandle(HandleType.Right);
        CreateHandle(HandleType.BottomRight);
        CreateHandle(HandleType.Bottom);
        CreateHandle(HandleType.BottomLeft);
        CreateHandle(HandleType.Left);

        HideHandles();
    }

    private void CreateHandle(HandleType type)
    {
        var h = new Rectangle
        {
            Width = HANDLE_SIZE,
            Height = HANDLE_SIZE,
            Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 215)),
            StrokeThickness = 1,
            Tag = type
        };

        _handles.Add(h);
        SelectionOverlay.Children.Add(h);
    }

    private void UpdateSelectionRectVisual(Rect rect)
    {
        EnsureSelectionRect();

        Canvas.SetLeft(_selectionRect!, rect.X);
        Canvas.SetTop(_selectionRect!, rect.Y);
        _selectionRect!.Width = rect.Width;
        _selectionRect!.Height = rect.Height;
    }

    private void UpdateHandlesPositions(Rect r)
    {
        EnsureHandles();

        double x = r.X;
        double y = r.Y;
        double w = r.Width;
        double h = r.Height;

        // positions center on corners/edges
        SetHandle(HandleType.TopLeft, x, y);
        SetHandle(HandleType.Top, x + w / 2, y);
        SetHandle(HandleType.TopRight, x + w, y);

        SetHandle(HandleType.Right, x + w, y + h / 2);
        SetHandle(HandleType.BottomRight, x + w, y + h);
        SetHandle(HandleType.Bottom, x + w / 2, y + h);

        SetHandle(HandleType.BottomLeft, x, y + h);
        SetHandle(HandleType.Left, x, y + h / 2);
    }

    private void SetHandle(HandleType type, double cx, double cy)
    {
        var h = _handles.First(x => (HandleType)x.Tag! == type);

        Canvas.SetLeft(h, cx - HANDLE_SIZE / 2);
        Canvas.SetTop(h, cy - HANDLE_SIZE / 2);
    }

    private void ShowHandles()
    {
        foreach (var h in _handles)
            h.Visibility = Visibility.Visible;
    }

    private void HideHandles()
    {
        foreach (var h in _handles)
            h.Visibility = Visibility.Collapsed;
    }

    private void ClearSelectionVisuals(bool keepRectForBox = true)
    {
        if (_selectionRect != null)
        {
            if (keepRectForBox)
            {
                _selectionRect.Visibility = Visibility.Collapsed;
                _selectionRect.Width = 0;
                _selectionRect.Height = 0;
            }
            else
            {
                SelectionOverlay.Children.Remove(_selectionRect);
                _selectionRect = null;
            }
        }

        foreach (var h in _handles)
            SelectionOverlay.Children.Remove(h);

        _handles.Clear();
        _selectionBounds = new Rect(0, 0, 0, 0);
    }

    // ==========================
    // Handle hit test
    // ==========================

    private HandleType HitTestHandle(Point p)
    {
        foreach (var h in _handles)
        {
            if (h.Visibility != Visibility.Visible) continue;

            var left = Canvas.GetLeft(h);
            var top = Canvas.GetTop(h);

            var rect = new Rect(left, top, h.Width, h.Height);
            if (PointInRect(p, rect))
                return (HandleType)h.Tag!;
        }

        return HandleType.None;
    }

    private static bool PointInRect(Point p, Rect r)
        => p.X >= r.X && p.X <= r.X + r.Width
        && p.Y >= r.Y && p.Y <= r.Y + r.Height;

    // ==========================
    // Drag-box rect helper
    // ==========================

    private static Rect GetRect(Point a, Point b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var w = Math.Abs(a.X - b.X);
        var h = Math.Abs(a.Y - b.Y);
        return new Rect(x, y, w, h);
    }

    // ==========================
    // Geometry snapshot models
    // ==========================

    private abstract record ShapeGeom;

    private record LineGeom(double X1, double Y1, double X2, double Y2) : ShapeGeom;
    private record RectGeom(double Left, double Top, double Width, double Height) : ShapeGeom;
    private record PolyGeom(List<Point> Points) : ShapeGeom;

    private void BuildSelectionFromRect(Rect rect)
    {
        EnsureSelectionRect();
        EnsureHandles();

        _selectionBounds = rect;

        UpdateSelectionRectVisual(_selectionBounds);
        UpdateHandlesPositions(_selectionBounds);
        ShowHandles();
    }


    private ShapeGeom CaptureShapeGeom(Shape s)
    {
        return s switch
        {
            Line l => new LineGeom(l.X1, l.Y1, l.X2, l.Y2),

            Microsoft.UI.Xaml.Shapes.Rectangle or Ellipse =>
                new RectGeom(
                    Canvas.GetLeft(s),
                    Canvas.GetTop(s),
                    s.Width,
                    s.Height),

            Polygon p =>
                new PolyGeom(p.Points.Select(pt => new Point(pt.X, pt.Y)).ToList()),

            _ =>
                new RectGeom(Canvas.GetLeft(s), Canvas.GetTop(s), s.Width, s.Height)
        };
    }

    // offset move
    private void ApplyShapeGeomWithOffset(Shape s, ShapeGeom g, double dx, double dy)
    {
        switch (s)
        {
            case Line l when g is LineGeom lg:
                l.X1 = lg.X1 + dx; l.Y1 = lg.Y1 + dy;
                l.X2 = lg.X2 + dx; l.Y2 = lg.Y2 + dy;
                break;

            case Microsoft.UI.Xaml.Shapes.Rectangle or Ellipse when g is RectGeom rg:
                Canvas.SetLeft(s, rg.Left + dx);
                Canvas.SetTop(s, rg.Top + dy);
                break;

            case Polygon p when g is PolyGeom pg:
                p.Points.Clear();
                foreach (var pt in pg.Points)
                    p.Points.Add(new Point(pt.X + dx, pt.Y + dy));
                break;
        }
    }

    // scale resize
    private void ApplyShapeGeomScaled(Shape s, ShapeGeom g, Rect oldBounds, Rect newBounds, double sx, double sy)
    {
        var ox = oldBounds.X;
        var oy = oldBounds.Y;

        switch (s)
        {
            case Line l when g is LineGeom lg:
                {
                    l.X1 = newBounds.X + (lg.X1 - ox) * sx;
                    l.Y1 = newBounds.Y + (lg.Y1 - oy) * sy;
                    l.X2 = newBounds.X + (lg.X2 - ox) * sx;
                    l.Y2 = newBounds.Y + (lg.Y2 - oy) * sy;
                    break;
                }

            case Microsoft.UI.Xaml.Shapes.Rectangle r when g is RectGeom rg:
                {
                    var newLeft = newBounds.X + (rg.Left - ox) * sx;
                    var newTop = newBounds.Y + (rg.Top - oy) * sy;

                    Canvas.SetLeft(r, newLeft);
                    Canvas.SetTop(r, newTop);

                    r.Width = Math.Max(1, rg.Width * sx);
                    r.Height = Math.Max(1, rg.Height * sy);
                    break;
                }

            case Ellipse e when g is RectGeom eg:
                {
                    var newLeft = newBounds.X + (eg.Left - ox) * sx;
                    var newTop = newBounds.Y + (eg.Top - oy) * sy;

                    Canvas.SetLeft(e, newLeft);
                    Canvas.SetTop(e, newTop);

                    e.Width = Math.Max(1, eg.Width * sx);
                    e.Height = Math.Max(1, eg.Height * sy);
                    break;
                }

            case Polygon p when g is PolyGeom pg:
                {
                    p.Points.Clear();
                    foreach (var pt in pg.Points)
                    {
                        var nx = newBounds.X + (pt.X - ox) * sx;
                        var ny = newBounds.Y + (pt.Y - oy) * sy;
                        p.Points.Add(new Point(nx, ny));
                    }
                    break;
                }
        }
    }

    // ==========================
    // Optional: ensure NumberBox binding
    // ==========================

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
    // Handle enum
    // ==========================
    private enum HandleType
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left
    }
}
