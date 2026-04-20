namespace Dashik.Shared.Infrastructure.Updates;

internal sealed class StubAppUpdateService : IAppUpdateService
{
    /// <inheritdoc />
    public Task<string> CheckUpdatesAsync(CancellationToken cancellationToken) => Task.FromResult(string.Empty);

    /// <inheritdoc />
    public Task UpdateAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
