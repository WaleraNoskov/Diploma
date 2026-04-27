namespace Diploma.Mvvm;

/// <summary>
/// Extends <see cref="BaseViewModel"/> with a unified busy/error state.
/// Used by ViewModels that perform async operations.
/// </summary>
public abstract class BaseBusyViewModel : BaseViewModel
{
    private bool _isBusy;
    private string? _busyMessage;
    private string? _errorMessage;

    /// <summary>True while an async operation is in progress.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetField(ref _isBusy, value);
    }

    /// <summary>Human-readable progress message shown while IsBusy is true.</summary>
    public string? BusyMessage
    {
        get => _busyMessage;
        private set => SetField(ref _busyMessage, value);
    }

    /// <summary>Non-null when the last operation produced an error.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        protected set => SetField(ref _errorMessage, value);
    }

    public bool HasError => ErrorMessage is not null;

    /// <summary>
    /// Wraps an async operation with busy state management.
    /// Automatically resets IsBusy on completion or failure.
    /// </summary>
    protected async Task RunBusyAsync(Func<Task> operation, string busyMessage = "Обработка...")
    {
        if (IsBusy) return;

        IsBusy = true;
        BusyMessage = busyMessage;
        ErrorMessage = null;

        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    /// <summary>Clears the current error state.</summary>
    public void ClearError() => ErrorMessage = null;
}