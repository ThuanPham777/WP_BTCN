using DrawingApp.Core.Enums;
using DrawingApp.UI.Drawing;
using DrawingApp.UI.Drawing.Tools;
using DrawingApp.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace DrawingApp.UI.Pages;

public sealed partial class DrawingPage : Page
{
    public DrawingViewModel ViewModel { get; }
    private CanvasInteraction? _interaction;

    public DrawingPage()
    {
        this.InitializeComponent();

        ViewModel = App.Host.Services.GetRequiredService<DrawingViewModel>();
        DataContext = ViewModel;

        Loaded += DrawingPage_Loaded;
    }

    private void DrawingPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var canvas = (Canvas)FindName("DrawCanvas")!;
        _interaction = new CanvasInteraction(canvas);

        _interaction.ShapeCompleted += shape =>
        {
            ViewModel.RuntimeShapes.Add(shape);
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
}
