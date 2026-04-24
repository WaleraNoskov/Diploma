using System.Windows.Input;

namespace Diploma.Mvvm.Command.Base;

/// <summary>
/// Represents base MVVM command implementation as a disposable object.
/// </summary>
public abstract class DisposableCommand : ICommand, IDisposable
{
    private EventHandler? _canExecuteChangedHandlers;

    /// <summary>
    /// Gets that command can execute.
    /// </summary>
    /// <param name="parameter">Any parameter for checking logic.</param>
    /// <returns></returns>
    public abstract bool CanExecute(object parameter);

    /// <summary>
    /// Triggers command execution.
    /// </summary>
    /// <param name="parameter">Any parameter for execution logic.</param>
    public abstract void Execute(object parameter);

    /// <summary>
    /// Invokes when need to check execution availability.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add
        {
            CommandManager.RequerySuggested += value;
            _canExecuteChangedHandlers += value;
        }
        remove
        {
            CommandManager.RequerySuggested -= value;
            _canExecuteChangedHandlers -= value;
        }
    }

    /// <summary>
    /// Notifies that command can or can not be executed.
    /// </summary>
    public void RaiseCanExecuteChanged() => _canExecuteChangedHandlers?.Invoke(this, EventArgs.Empty);
    
    #region Disposing

    protected bool _disposed;

    /// <summary>
    /// Disposes command and unbinds all <see cref="CanExecuteChanged"/> listeners.
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <inheritdoc cref="Dispose"/>
    /// </summary>
    /// <param name="disposing">Need to unbind <see cref="CanExecuteChanged"/> listeners.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && _canExecuteChangedHandlers is not null)
        {
            foreach (var handler in _canExecuteChangedHandlers.GetInvocationList())
                CommandManager.RequerySuggested -= (EventHandler)handler;

            _canExecuteChangedHandlers = null;
        }

        _disposed = true;
    }

    ~DisposableCommand()
    {
        Dispose(false);
    }

    #endregion
}