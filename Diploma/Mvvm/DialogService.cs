using System.Windows;
using Microsoft.Win32;

namespace Diploma.Mvvm;

/// <summary>
/// WPF implementation of <see cref="IDialogService"/>.
/// Register as singleton — it holds no mutable state.
/// </summary>
public sealed class DialogService : IDialogService
{
    // Maps ViewModel types to their corresponding Window types.
    // Register during app startup: dialogService.Register<MyVm, MyWindow>()
    private readonly Dictionary<Type, Func<Window>> _windowFactories = new();

    /// <summary>
    /// Registers a window factory for a given ViewModel type.
    /// </summary>
    public void Register<TViewModel, TWindow>()
        where TWindow : Window, new()
        where TViewModel : BaseViewModel
        => _windowFactories[typeof(TViewModel)] = () => new TWindow();

    public string? OpenFile(string title, string filter)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            Filter = filter
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string title, string filter, string defaultExtension)
    {
        var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
            DefaultExt = defaultExtension,
            AddExtension = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowMessage(string message, string title = "Информация")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message, string title = "Ошибка")
        => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "Подтверждение")
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
           == MessageBoxResult.Yes;

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : BaseViewModel
    {
        if (!_windowFactories.TryGetValue(typeof(TViewModel), out var factory))
            throw new InvalidOperationException(
                $"No window registered for ViewModel type '{typeof(TViewModel).Name}'. " +
                $"Call dialogService.Register<{typeof(TViewModel).Name}, YourWindow>() at startup.");

        var window = factory();
        window.DataContext = viewModel;
        window.Owner = Application.Current.MainWindow;
        return window.ShowDialog();
    }
}