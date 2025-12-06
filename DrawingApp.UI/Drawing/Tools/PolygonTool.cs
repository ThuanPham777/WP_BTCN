using DrawingApp.Core.Enums;
using DrawingApp.Core.Models;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace DrawingApp.UI.Drawing.Tools;

public class PolygonTool : IDrawTool
{
    public ShapeType Type => ShapeType.Polygon;

    private Polygon? _poly;
    private StrokeStyle? _style;

    public Shape? Preview => _poly;

    public void Begin(Point start, StrokeStyle style)
    {
        _style = style;

        _poly = new Polygon();
        ShapeFactory.ApplyStroke(_poly, style);

        // Polygon mode: không auto thêm start như line/rect
        // Người dùng sẽ tap để thêm điểm
    }

    // Với Polygon, Update sẽ dùng để preview điểm cuối theo mouse move
    public void Update(Point current)
    {
        if (_poly == null) return;

        // Nếu đang có ít nhất 1 điểm:
        // - Điểm cuối cùng được coi là "preview"
        if (_poly.Points.Count == 0) return;

        // Nếu đã có preview point thì update nó
        // Quy ước:
        //   - Khi AddPoint: thêm 1 điểm thật
        //   - Sau đó thêm 1 "preview point" nữa để Update kéo theo chuột
        //   => Update sẽ sửa last point
        var lastIndex = _poly.Points.Count - 1;
        _poly.Points[lastIndex] = current;
    }

    public Shape? End(Point end)
    {
        if (_poly == null) return null;

        // Nếu có preview point, bỏ nó để chốt polygon gọn
        if (_poly.Points.Count >= 2)
        {
            // Nếu điểm cuối trùng logic preview, ta remove nó
            // Cách đơn giản: remove last point rồi add end nếu cần
            _poly.Points.RemoveAt(_poly.Points.Count - 1);
        }

        return _poly.Points.Count >= 3 ? _poly : null;
    }

    // Được gọi từ CanvasInteraction.OnTapped
    public void AddPoint(Point p)
    {
        if (_poly == null) return;

        // Nếu chưa có điểm nào:
        if (_poly.Points.Count == 0)
        {
            _poly.Points.Add(p); // điểm thật đầu tiên
            _poly.Points.Add(p); // preview point
            return;
        }

        // Nếu đang có preview point ở cuối:
        // - biến preview thành điểm thật bằng cách set nó = p
        var lastIndex = _poly.Points.Count - 1;
        _poly.Points[lastIndex] = p;

        // Thêm preview mới
        _poly.Points.Add(p);
    }
}
