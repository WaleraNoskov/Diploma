using System.Windows;
using System.Windows.Input;
using Diploma.ViewModel;
using Microsoft.Xaml.Behaviors;

public class ZoomPanBehavior : Behavior<FrameworkElement>
{
    public ImageViewerViewModel ViewModel
        => AssociatedObject.DataContext as ImageViewerViewModel;

    private Point? _lastDragPoint;

    protected override void OnAttached()
    {
        AssociatedObject.MouseWheel += OnWheel;
        AssociatedObject.MouseLeftButtonDown += OnDown;
        AssociatedObject.MouseLeftButtonUp += OnUp;
        AssociatedObject.MouseMove += OnMove;
    }

    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        if (ViewModel == null) return;

        var pos = e.GetPosition(AssociatedObject);
        double zoom = e.Delta > 0 ? 1.1 : 0.9;

        ViewModel.ZoomAt(pos, zoom);
    }

    private void OnDown(object sender, MouseButtonEventArgs e)
    {
        _lastDragPoint = e.GetPosition(AssociatedObject);
        AssociatedObject.CaptureMouse();
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        _lastDragPoint = null;
        AssociatedObject.ReleaseMouseCapture();
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (_lastDragPoint == null || ViewModel == null) return;

        var current = e.GetPosition(AssociatedObject);
        var delta = current - _lastDragPoint.Value;

        ViewModel.Pan(delta);

        _lastDragPoint = current;
    }
}