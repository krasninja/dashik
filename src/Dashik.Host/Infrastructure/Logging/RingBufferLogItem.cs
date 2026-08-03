using Microsoft.Extensions.Logging;

namespace Dashik.Host.Infrastructure.Logging;

public sealed record LogItem(
    LogLevel LogLevel,
    string CategoryName,
    string Message,
    DateTime Time,
    Exception? Exception);
