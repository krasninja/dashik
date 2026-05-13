namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget with persistent state.
/// </summary>
public interface IWidgetState
{
    /// <summary>
    /// Set new state.
    /// </summary>
    /// <param name="state">State object. Must be serializable.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task SetStateAsync(object state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get state.
    /// </summary>
    /// <param name="stateType">State object type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>State object.</returns>
    Task<object> GetStateAsync(Type stateType, CancellationToken cancellationToken = default);
}
