namespace Dashik.Sdk.Abstract;

public interface IWidgetUpdate
{
    /// <summary>
    /// Update widget's UI state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
