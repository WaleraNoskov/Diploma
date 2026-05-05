using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Diploma.ViewModel;
using ImageAnalysis.Domain.ValueObjects;
using Microsoft.Xaml.Behaviors;

public class ZoomPanBehavior : Behavior<FrameworkElement>
{
    public ImageViewerViewModel ViewModel
        => AssociatedObject.DataContext as ImageViewerViewModel;

    private ScrollViewer? _scroll;
    private Image? _image;
    private bool _isDragging;
    private Point? _mouseDownPoint;
    private Point? _lastScrolling;
    private Point? _lastImage;

    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnAssociatedObjectOnLoaded;

        AssociatedObject.MouseWheel += OnWheel;
        AssociatedObject.MouseLeftButtonDown += OnDown;
        AssociatedObject.MouseLeftButtonUp += OnUp;
        AssociatedObject.MouseMove += OnMove;
    }

    private void OnAssociatedObjectOnLoaded(object o, RoutedEventArgs routedEventArgs)
    {
        _scroll = (ScrollViewer)FindElement<ScrollViewer>(AssociatedObject.Parent);
        _image = (Image)FindElement<Image>(AssociatedObject.Parent);
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
        _lastScrolling = e.GetPosition(_scroll);
        _lastImage = e.GetPosition(_image);
        _mouseDownPoint = e.GetPosition(_scroll);
        AssociatedObject.CaptureMouse();
    }

    private void OnUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging && _lastImage is not null)
            ViewModel.TakeMeasurementAsyncCommand.Execute(new PixelPoint((int)_lastImage.Value.X, 
                (int)_lastImage.Value.Y));

        _isDragging = false;
        _mouseDownPoint = null;
        _lastScrolling = null;
        _lastImage = null;

        AssociatedObject.ReleaseMouseCapture();
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(AssociatedObject);
        var imagePoint = ViewModel.ScreenToImage(pos);
        
        ViewModel.CursorPosition = imagePoint;
        ViewModel.IsCursorVisible = true;
        
        if (_lastScrolling == null || _scroll == null) 
            return;

        var current = e.GetPosition(_scroll);
        var delta = current - _lastScrolling.Value;

        if ((current - _mouseDownPoint!).Value.Length > 5)
            _isDragging = true;

        _scroll.ScrollToHorizontalOffset(_scroll.HorizontalOffset - delta.X);
        _scroll.ScrollToVerticalOffset(_scroll.VerticalOffset - delta.Y);

        _lastScrolling = current;
        _lastImage = e.GetPosition(_image);
    }

    private object FindElement<T>(DependencyObject d)
    {
        if (d is T sv) return sv;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
        {
            var child = VisualTreeHelper.GetChild(d, i);
            var result = FindElement<T>(child);
            if (result != null) return result;
        }

        return null;
    }
}