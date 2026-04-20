using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// The special logger that uses ring buffer observable static
/// list to collect all logs.
/// </summary>
public sealed class RingBufferLogger : ILogger
{
    private readonly string _categoryName;
    private readonly RingBufferLogsStorage _storage;

    public RingBufferLogger(RingBufferLogsStorage storage, string categoryName)
    {
        _storage = storage;
        _categoryName = categoryName;
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        _storage.AddLog(new LogItem(logLevel, _categoryName, message, DateTime.UtcNow, exception));
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
