using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Diploma.ViewModel;
using ImageAnalysis.Application.Queries;
using ImageAnalysis.Domain.Entities;
using MediatR;

namespace Diploma.View;

public class OverlayVisualHost : FrameworkElement
{
    private readonly VisualCollection _visuals;
    private DrawingVisual _visual;

    public OverlayVisualHost()
    {
        _visuals = new VisualCollection(this);
        _visual = new DrawingVisual();
        _visuals.Add(_visual);

        Loaded += (_, _) => Redraw();
    }

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index)
        => _visuals[index];

    public void Redraw()
    {
        if (DataContext is not ImageViewerViewModel vm)
            return;

        using var dc = _visual.RenderOpen();

        var pen = new Pen(Brushes.Lime, 2);

        foreach (var m in vm.Measurements)
        {
            var start = new Point(m.From.X, m.From.Y);
            var end = new Point(m.To.X, m.To.Y);
            dc.DrawLine(pen, start, end);
        }
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        Redraw();
    }
}