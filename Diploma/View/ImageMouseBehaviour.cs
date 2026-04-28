using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Diploma.View;

public class ImageMouseBehavior : Behavior<FrameworkElement>
{
    public ICommand ClickCommand { get; set; }
    public ICommand MouseMoveCommand { get; set; }
    public ICommand WheelCommand { get; set; }

    protected override void OnAttached()
    {
        AssociatedObject.MouseLeftButtonDown += OnMouseDown;
        AssociatedObject.MouseMove += OnMouseMove;
        AssociatedObject.MouseWheel += OnMouseWheel;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(AssociatedObject);
        ClickCommand?.Execute(pos);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(AssociatedObject);
        MouseMoveCommand?.Execute(pos);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        WheelCommand?.Execute(e.Delta);
    }
}