using Dashik.Abstractions;

namespace Dashik.Shared.Services;

internal sealed class StubSystemUtils : ISystemUtils
{
    /// <inheritdoc />
    public Task<bool> IsLaunchAtStartupEnabledAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);

    /// <inheritdoc />
    public Task EnableLaunchAtStartupAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task DisableLaunchAtStartupAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
