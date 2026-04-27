namespace Diploma.Mvvm;

/// <summary>
/// Abstracts all dialog interactions so ViewModels stay testable.
/// Implementations live in the Presentation layer; ViewModels get this injected.
/// </summary>
public interface IDialogService
{
    /// <summary>Shows an Open File dialog. Returns the selected path or null.</summary>
    string? OpenFile(string title, string filter);
 
    /// <summary>Shows a Save File dialog. Returns the selected path or null.</summary>
    string? SaveFile(string title, string filter, string defaultExtension);
 
    /// <summary>Shows an information message box.</summary>
    void ShowMessage(string message, string title = "Информация");
 
    /// <summary>Shows an error message box.</summary>
    void ShowError(string message, string title = "Ошибка");
 
    /// <summary>Shows a Yes/No confirmation dialog. Returns true if user confirmed.</summary>
    bool Confirm(string message, string title = "Подтверждение");
 
    /// <summary>
    /// Shows a modal dialog whose DataContext is <paramref name="viewModel"/>.
    /// The window type is resolved from a registered mapping.
    /// Returns true if the user confirmed (closed with DialogResult = true).
    /// </summary>
    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : BaseViewModel;
}
