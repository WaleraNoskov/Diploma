namespace Diploma.Mvvm;

/// <summary>
/// Represents MVVM relay command implementation.
/// </summary>
public class RelayCommand : DisposableCommand
{
    private Action<object> _execute;
    private Func<object, bool> _canExecute;
    
    /// <summary>
    /// Constructor for <see cref="RelayCommand"/>.
    /// </summary>
    /// <param name="execute">Action that command executes.</param>
    /// <param name="canExecute">Execution availability checking method. Will be triggered by WPF <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> event.</param>
    /// <exception cref="ArgumentNullException">Throws if execute parameter is null.</exception>
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <inheritdoc cref="DisposableCommand.CanExecute"/>
    public override bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc cref="DisposableCommand.Execute"/>
    public override void Execute(object parameter) => _execute?.Invoke(parameter);
    
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

        if(disposing)
        {
            _execute = null;
            _canExecute = null;
        }
        
        base.Dispose(disposing);
        _disposed = true;
    }

    ~RelayCommand()
    {
        Dispose(false);
    }

    #endregion
}