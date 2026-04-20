using Avalonia.Controls;

namespace Dashik.Sdk.Mvvm;

/// <summary>
/// Interface to interact with main application window.
/// </summary>
public interface IMvvmService
{
    /// <summary>
    /// Find control (window) by view model using view locator.
    /// </summary>
    /// <param name="viewModel">View model instance.</param>
    /// <returns>Control or null if not found.</returns>
    Control? FindControlByViewModel(object viewModel);

    /// <summary>
    /// Get application main window.
    /// </summary>
    /// <returns>Avalonia main window handle.</returns>
    Window? GetMainWindow();

    /// <summary>
    /// Create a new instance of view model.
    /// </summary>
    /// <param name="type">View model type.</param>
    /// <param name="parameters">Any constructor parameters to be passed to the view model.</param>
    /// <returns>Created view model.</returns>
    object CreateViewModel(Type type, params object[] parameters);

    /// <summary>
    /// Open window.
    /// </summary>
    /// <param name="viewModel">View model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task OpenAsync(
        object viewModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new instance of view model and open it.
    /// </summary>
    /// <param name="viewModel">View model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task<DialogResult> OpenAsync<TDialogResult>(
        IDialogViewModel<TDialogResult> viewModel,
        CancellationToken cancellationToken = default);
}
