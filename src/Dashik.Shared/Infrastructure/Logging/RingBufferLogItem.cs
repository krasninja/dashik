using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Logging;

public sealed record LogItem(
    LogLevel LogLevel,
    string CategoryName,
    string Message,
    DateTime Time,
    Exception? Exception);
