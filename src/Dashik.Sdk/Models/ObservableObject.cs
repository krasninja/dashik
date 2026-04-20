using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dashik.Sdk.Models;

/// <summary>
/// Object that implements <see cref="INotifyPropertyChanged" /> and <see cref="INotifyPropertyChanging" /> interfaces.
/// It helps to implement properties with change notifications.
/// </summary>
public class ObservableObject : INotifyPropertyChanged, INotifyPropertyChanging
{
    #region INotifyPropertyChanged

    /// <summary>
    /// Object property changed event. Occurs when any property updated.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// On property changed internal method.
    /// </summary>
    /// <param name="propertyName">Name of property.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region INotifyPropertyChanging

    /// <summary>
    /// On property changing internal method.
    /// </summary>
    /// <param name="propertyName">Name of property.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    #endregion

    /// <summary>
    /// The method checks if the property was really changed and updates
    /// the property backing field internally. Fires the property changes event.
    /// It helps to implement <see cref="INotifyPropertyChanged" /> interface.
    /// </summary>
    /// <param name="backingField">Backing field of the property.</param>
    /// <param name="newValue">New value.</param>
    /// <param name="propertyName">Property name. Usually automatically provided.</param>
    /// <typeparam name="TProperty">Property type.</typeparam>
    /// <returns>True if property was changed, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool RaiseAndSetIfChanged<TProperty>(
        ref TProperty backingField,
        TProperty newValue,
        [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        if (EqualityComparer<TProperty>.Default.Equals(backingField, newValue))
        {
            return false;
        }
        OnPropertyChanging(propertyName);
        backingField = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}
