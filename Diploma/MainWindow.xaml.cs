using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Diploma.Model;
using Diploma.Mvvm;
using Diploma.ViewModel;

namespace Diploma;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel Vm => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        // Keyboard shortcuts
        InputBindings.Add(new KeyBinding(
            new RelayCommand(_ => Vm.OpenImageCommand.Execute(null)),
            new KeyGesture(Key.O, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(
            new RelayCommand(_ => Vm.UndoCommand.Execute(null)),
            new KeyGesture(Key.Z, ModifierKeys.Control)));
        InputBindings.Add(new KeyBinding(
            new RelayCommand(_ => Vm.ActiveMode = InteractionMode.View),
            new KeyGesture(Key.Escape)));
    }

    // ── Canvas mouse events → ViewModel ────────────────────────────────────

    private void ImageCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var pos = e.GetPosition(ImageCanvas);
        Vm.Canvas.OnMouseWheel(pos, e.Delta);
        e.Handled = true;
    }

    private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        Vm.Canvas.OnMouseMove(e.GetPosition(ImageCanvas));
    }

    private void ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ImageCanvas.CaptureMouse();
        Vm.Canvas.OnMouseDown(e.GetPosition(ImageCanvas), e.ChangedButton);
    }

    private void ImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        ImageCanvas.ReleaseMouseCapture();
        Vm.Canvas.OnMouseUp(e.GetPosition(ImageCanvas), e.ChangedButton);
    }
}