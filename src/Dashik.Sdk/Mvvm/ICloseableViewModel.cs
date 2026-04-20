namespace Dashik.Sdk.Mvvm;

/// <summary>
/// View model that can be closed.
/// </summary>
public interface ICloseableViewModel
{
    /// <summary>
    /// Close event. Should be called to close the window.
    /// </summary>
    event EventHandler? CloseRequest;
}
