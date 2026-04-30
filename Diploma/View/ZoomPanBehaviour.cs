using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Diploma.ViewModel;
using Microsoft.Xaml.Behaviors;

public class ZoomPanBehavior : Behavior<FrameworkElement>
{
    public ImageViewerViewModel ViewModel
        => AssociatedObject.DataContext as ImageViewerViewModel;

    private ScrollViewer _scroll;
    private Point? _last;

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += (_, _) =>
        {
            _scroll = FindScrollViewer(AssociatedObject.Parent);
        };

        AssociatedObject.MouseWheel += OnWheel;
        AssociatedObject.MouseLeftButtonDown += OnDown;
        AssociatedObject.MouseLeftButtonUp += OnUp;
        AssociatedObject.MouseMove += OnMove;
    }

    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        if (ViewModel == null || _scroll == null) return;

        var pos = e.GetPosition(AssociatedObject);
        double zoom = e.Delta > 0 ? 1.1 : 0.9;

        // 1. координата в изображении ДО зума
        var before = ViewModel.ScreenToImage(pos);

        // 2. меняем масштаб (LayoutTransform)
        ViewModel.Scale *= zoom;

        // 3. координата ПОСЛЕ зума
        var after = ViewModel.ScreenToImage(pos);

        // 4. компенсируем сдвиг через ScrollViewer
        var shift = after - before;

        _scroll.ScrollToHorizontalOffset(_scroll.HorizontalOffset + shift.X);
        _scroll.ScrollToVerticalOffset(_scroll.VerticalOffset + shift.Y);
    }

    private void OnDown(object sender, MouseButtonEventArgs e)
    {
        _last = e.GetPosition(_scroll);
        AssociatedObject.CaptureMouse();
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        _last = null;
        AssociatedObject.ReleaseMouseCapture();
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        if (_last == null || _scroll == null) return;

        var current = e.GetPosition(_scroll);
        var delta = current - _last.Value;

        _scroll.ScrollToHorizontalOffset(_scroll.HorizontalOffset - delta.X);
        _scroll.ScrollToVerticalOffset(_scroll.VerticalOffset - delta.Y);

        _last = current;
    }

    private ScrollViewer FindScrollViewer(DependencyObject d)
    {
        if (d is ScrollViewer sv) return sv;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            var result = FindScrollViewer(child);
            if (result != null) return result;
        }

        return null;
    }
}