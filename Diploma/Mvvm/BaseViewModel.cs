using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Diploma.Mvvm;

/// <summary> Base View Model </summary>
public class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    #region INotifyPropertyChanged

    /// <summary>
    /// Notifies that some property in view model has been changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises <see cref="PropertyChanged"/>.
    /// </summary>
    /// <param name="propertyName">Name of changed property that will be set to <see cref="PropertyChanged"/> args.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets field value and raises <see cref="PropertyChanged"/>.
    /// </summary>
    /// <param name="field">Filed to set value.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="propertyName">Property name that will be set to <see cref="PropertyChanged"/> args.</param>
    /// <typeparam name="T">Type of set field.</typeparam>
    /// <returns>Whether field value was changed.</returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    #endregion
    
    #region Disposing

    protected bool _disposed;
    
    /// <summary>
    /// Disposes object and unbinds <see cref="PropertyChanged"/> listeners.
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// <inheritdoc cref="Dispose"/>
    /// </summary>
    /// <param name="disposing">Need to free resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            PropertyChanged = null;
        }

        _disposed = true;
    }

    ~BaseViewModel()
    {
        Dispose(false);
    }

    #endregion
}