using Microsoft.Extensions.Logging;
using Velopack.Logging;

namespace Dashik.Desktop.Infrastructure.Updates;

internal sealed class MicrosoftExtensionsVelopackLogger : IVelopackLogger
{
    private readonly ILogger _logger;

    public MicrosoftExtensionsVelopackLogger(ILogger logger)
    {
        this._logger = logger;
    }

    /// <inheritdoc />
    public void Log(VelopackLogLevel logLevel, string? message, Exception? exception)
    {
        var velopackLogLevel = logLevel switch
        {
            VelopackLogLevel.Critical => LogLevel.Critical,
            VelopackLogLevel.Error => LogLevel.Error,
            VelopackLogLevel.Warning => LogLevel.Warning,
            VelopackLogLevel.Information => LogLevel.Information,
            VelopackLogLevel.Debug => LogLevel.Debug,
            VelopackLogLevel.Trace => LogLevel.Trace,
            _ => LogLevel.None
        };

        _logger.Log(velopackLogLevel, exception, message);
    }
}
