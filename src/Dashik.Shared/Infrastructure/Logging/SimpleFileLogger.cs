using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// Log messages to a file.
/// </summary>
public sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _file;
    private static readonly Lock _objLock = new();

    public SimpleFileLogger(string categoryName, string file)
    {
        _categoryName = categoryName;
        _file = file;
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
        lock (_objLock)
        {
            var logLevelShort = LogLevelFormatter.GetShortName(logLevel);
            File.AppendAllText(_file, $"{DateTime.UtcNow:s} {logLevelShort} [{_categoryName}] {message}\n");
        }
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;
}
