using System.Windows;
using System.Windows.Input;
using ImageAnalysis.Domain.ValueObjects;
using Microsoft.Xaml.Behaviors;

namespace Diploma.View;

public class ImageMouseBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.Register(
            nameof(ClickCommand),
            typeof(ICommand),
            typeof(ImageMouseBehavior));

    public static readonly DependencyProperty MouseMoveCommandProperty =
        DependencyProperty.Register(
            nameof(MouseMoveCommand),
            typeof(ICommand),
            typeof(ImageMouseBehavior));

    public static readonly DependencyProperty WheelCommandProperty =
        DependencyProperty.Register(
            nameof(WheelCommand),
            typeof(ICommand),
            typeof(ImageMouseBehavior));

    public ICommand ClickCommand
    {
        get => (ICommand)GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public ICommand MouseMoveCommand
    {
        get => (ICommand)GetValue(MouseMoveCommandProperty);
        set => SetValue(MouseMoveCommandProperty, value);
    }

    public ICommand WheelCommand
    {
        get => (ICommand)GetValue(WheelCommandProperty);
        set => SetValue(WheelCommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseWheel += OnMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
        AssociatedObject.MouseMove -= OnMouseMove;
        AssociatedObject.MouseWheel -= OnMouseWheel;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(AssociatedObject);
        ClickCommand?.Execute(new PixelPoint((int)pos.X, (int)pos.Y));
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(AssociatedObject);
        MouseMoveCommand?.Execute(new PixelPoint((int)pos.X, (int)pos.Y));
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        WheelCommand?.Execute(e.Delta);
    }
}