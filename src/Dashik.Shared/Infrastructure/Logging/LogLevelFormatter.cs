using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

public static class LogLevelFormatter
{
    public static string GetShortName(LogLevel level) => level switch
    {
        LogLevel.Critical => "CRIT",
        LogLevel.Trace => "TRAC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "ERR",
        LogLevel.None => "NONE",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };
}
