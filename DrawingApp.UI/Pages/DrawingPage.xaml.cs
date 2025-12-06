using DrawingApp.Core.Enums;
using DrawingApp.UI.Drawing;
using DrawingApp.UI.Drawing.Tools;
using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace DrawingApp.UI.Pages;

public sealed partial class DrawingPage : Page
{
    public DrawingViewModel ViewModel { get; }
    private CanvasInteraction? _interaction;

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

        if (e.Parameter is Guid boardId)
        {
            await ViewModel.LoadBoardAsync(boardId);

            // nếu canvas interaction đã có thì render lại
            _interaction?.ReloadFrom(ViewModel.RuntimeShapes);
        }
    }

    private void DrawingPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var canvas = (Canvas)FindName("DrawCanvas")!;
        _interaction = new CanvasInteraction(canvas);

        _interaction.ShapeCompleted += shape =>
        {
            // 1) Add vào runtime
            ViewModel.RuntimeShapes.Add(shape);

            // 2) Đảm bảo shape có trong canvas (nếu tool trả về shape mới)
            if (!canvas.Children.Contains(shape))
                canvas.Children.Add(shape);

            // 3) Enable hit test + fill behavior
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
            if (args.PropertyName == nameof(ViewModel.CurrentTool))
                ApplyTool(ViewModel.CurrentTool);

            if (args.PropertyName == nameof(ViewModel.StrokeColor)
                || args.PropertyName == nameof(ViewModel.FillColor)
                || args.PropertyName == nameof(ViewModel.Thickness))
            {
                _interaction!.CurrentStyle = ViewModel.BuildStyle();
            }
        };
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

    private void BoardW_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (DataContext is DrawingViewModel vm && !double.IsNaN(args.NewValue))
            vm.BoardWidth = args.NewValue;
    }

    private void BoardH_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (DataContext is DrawingViewModel vm && !double.IsNaN(args.NewValue))
            vm.BoardHeight = args.NewValue;
    }
}
