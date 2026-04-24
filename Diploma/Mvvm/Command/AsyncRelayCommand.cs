using System.Diagnostics;
using Diploma.Mvvm.Command.Base;

namespace Diploma.Mvvm.Command;

/// <summary>
/// Represents asynchronous MVVM relay command implementation.
/// </summary>
public class AsyncRelayCommand : DisposableCommand, IAsyncCommand
{
    private bool _isExecuting;
    private Func<object, Task> _executeAsync;
    private Func<object, bool> _canExecute;

    public AsyncRelayCommand(Func<object, Task> executeAsync, Func<object, bool> canExecute = null)
    {
        _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        _canExecute = canExecute;
    }

    /// <inheritdoc cref="IAsyncCommand.CanExecute"/>
    public override bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    /// <inheritdoc cref="IAsyncCommand.Execute"/>
    public override async void Execute(object parameter) => await ExecuteAsync(parameter);

    /// <inheritdoc cref="IAsyncCommand.ExecuteAsync"/>
    public async Task ExecuteAsync(object parameter)
    {
        if (_isExecuting)
            return;

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _executeAsync(parameter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AsyncRelayCommand] Exception: {ex}");
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }
    
    #region Disposing
    
    /// <summary>
    /// Disposes command and unbinds all <see cref="CanExecuteChanged"/> listeners.
    /// </summary>
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// <inheritdoc cref="Dispose"/>
    /// </summary>
    /// <param name="disposing">Need to unbind <see cref="CanExecuteChanged"/> listeners.</param>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _executeAsync = null;
            _canExecute = null;
        }
        
        base.Dispose(disposing);
        _disposed = true;
    }

    ~AsyncRelayCommand()
    {
        Dispose(false);
    }

    #endregion
}