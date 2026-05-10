namespace Dashik.Sdk.Abstract;

/// <summary>
/// The interface indicates that widget should be updated for period of time.
/// </summary>
public interface IWidgetUpdate
{
    /// <summary>
    /// Update widget's UI state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
