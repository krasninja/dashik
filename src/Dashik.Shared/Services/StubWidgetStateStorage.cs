using Dashik.Abstractions;

namespace Dashik.Shared.Services;

public sealed class StubWidgetStateStorage : IWidgetsStateStorage
{
    public static readonly StubWidgetStateStorage Instance = new();

    /// <inheritdoc />
    public Task SetStateAsync(object state, string instanceId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<object?> GetStateAsync(Type stateType, string instanceId, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);
}
