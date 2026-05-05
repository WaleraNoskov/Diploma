using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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
    private readonly DrawingVisual _visual;

    public OverlayVisualHost()
    {
        _visuals = new VisualCollection(this);
        _visual = new DrawingVisual();
        _visuals.Add(_visual);

        // Перерисовываем, когда меняется DataContext или размеры
        DataContextChanged += OnDataContextChanged;
        SizeChanged += (_, _) => Redraw(true);
    }

    private void OnDataContextChanged(object o, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        if (DataContext is INotifyPropertyChanged vm)
            vm.PropertyChanged += (_, _) => Redraw(true);

        Redraw(true);
    }

    protected override int VisualChildrenCount => _visuals.Count;
    protected override Visual GetVisualChild(int index) => _visuals[index];

    public void Redraw(bool redrawMeasurementsAndRoi)
    {
        if (DataContext is not ImageViewerViewModel vm) return;

        using (var dc = _visual.RenderOpen())
        {
            var scale = ((MatrixTransform)RenderTransform).Matrix.M11;

            var radius = 4 / scale;

            if (vm.FirstPoint is not null)
                dc.DrawEllipse(
                    Brushes.Orange,
                    null,
                    new Point(vm.FirstPoint.X, vm.FirstPoint.Y),
                    radius,
                    radius);

            if (vm.IsCursorVisible)
                dc.DrawEllipse(
                    Brushes.Red,
                    null,
                    vm.CursorPosition,
                    radius,
                    radius);

            if (vm.FirstPoint is not null && vm.IsCursorVisible)
            {
                var from = new Point(vm.FirstPoint.X, vm.FirstPoint.Y);
                var to = new Point(vm.CursorPosition.X, vm.CursorPosition.Y);
                dc.DrawText(new FormattedText($"{Math.Round((from - to).Length)} px",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        12,
                        Brushes.Orange),
                    from - (from - to) / 2);
                dc.DrawLine(new Pen(Brushes.Orange, 2), from, to);
            }

            if (!redrawMeasurementsAndRoi)
                return;

            // Рисуем прозрачный фон, если нужно, чтобы контрол имел физический размер, 
            // но так как IsHitTestVisible="False", это не перекроет мышь.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, ActualWidth, ActualHeight));

            var pen = new Pen(Brushes.Lime, 2);
            // Чтобы толщина линии не менялась при зуме, можно сделать так:
            // pen.Thickness = 2 / ((MatrixTransform)RenderTransform).Matrix.M11;

            foreach (var m in vm.Measurements)
            {
                var from = new Point(m.From.X, m.From.Y);
                var to = new Point(m.To.X, m.To.Y);
                dc.DrawLine(pen, from, to);
                dc.DrawText(new FormattedText($"{Math.Round((from - to).Length)} px",
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Verdana"),
                        12,
                        Brushes.Lime),
                    from - (from - to) / 2);
            }
        }
    }

    // OnRender нам больше не нужен, так как мы используем DrawingVisual
}