using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// Logger provider for <see cref="RingBufferLogger" />.
/// </summary>
public sealed class RingBufferLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Logs storage.
    /// </summary>
    public RingBufferLogsStorage Storage { get; } = new();

    /// <summary>
    /// Logs list.
    /// </summary>
    public RingBufferObservableList<LogItem> Logs => Storage.Logs;

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new RingBufferLogger(Storage, categoryName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
