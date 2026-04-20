using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

/// <summary>
/// Logger provider for <see cref="SimpleFileLogger" />.
/// </summary>
public sealed class SimpleFileLoggerProvider(string fileName) : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleFileLogger(categoryName, fileName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
