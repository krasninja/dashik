namespace Dashik.Sdk.Mvvm;

/// <summary>
/// View model that is used for dialog windows. Allow to return a value.
/// </summary>
/// <typeparam name="TDialogResult">Dialog result type.</typeparam>
public interface IDialogViewModel<out TDialogResult>
{
    /// <summary>
    /// Dialog result value.
    /// </summary>
    TDialogResult ResultValue { get; }

    /// <summary>
    /// Dialog result (OK, Cancel, etc.).
    /// </summary>
    DialogResult Result { get; }
}
