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
        if(DataContext is INotifyPropertyChanged vm)
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
            if (vm.IsCursorVisible)
            {
                var scale = ((MatrixTransform)RenderTransform).Matrix.M11;

                double radius = 4 / scale;

                dc.DrawEllipse(
                    Brushes.Red,
                    null,
                    vm.CursorPosition,
                    radius,
                    radius);
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
                dc.DrawLine(pen, new Point(m.From.X, m.From.Y), new Point(m.To.X, m.To.Y));
            }
        }
    }

    // OnRender нам больше не нужен, так как мы используем DrawingVisual
}