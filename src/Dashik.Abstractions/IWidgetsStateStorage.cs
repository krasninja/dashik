namespace Dashik.Abstractions;

/// <summary>
/// Interface for widgets state storage. State is an object that contains all necessary information to restore
/// widget to the same state as it was before.
/// </summary>
public interface IWidgetsStateStorage
{
    /// <summary>
    /// Set new state.
    /// </summary>
    /// <param name="state">State object. Must be serializable.</param>
    /// <param name="instanceId">Widget file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task SetStateAsync(object? state, string instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get state.
    /// </summary>
    /// <param name="stateType">State object type.</param>
    /// <param name="instanceId">Widget file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>State object.</returns>
    Task<object?> GetStateAsync(Type stateType, string instanceId, CancellationToken cancellationToken = default);
}
