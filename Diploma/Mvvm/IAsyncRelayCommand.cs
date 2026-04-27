using System.Windows.Input;

namespace Diploma.Mvvm;

/// <summary>
/// Represents the asynchronously <see cref="ICommand"/>.
/// </summary>
public interface IAsyncCommand : ICommand
{
    /// <summary>
    /// Triggers command execution asynchronously.
    /// </summary>
    /// <param name="parameter">Any parameter for execution logic.</param>
    Task ExecuteAsync(object parameter);
}