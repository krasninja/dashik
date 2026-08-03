using Dashik.Sdk;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Parser;
using Application = QueryCat.Backend.Core.Application;

namespace Dashik.Desktop;

internal static class ErrorsHandler
{
    private static readonly Lock _objLock = new();

    internal static int ProcessException(Exception exception)
    {
        var logger = Application.LoggerFactory.CreateLogger(nameof(Program));
        lock (_objLock)
        {
            if (exception is AggregateException aggregateException)
            {
                exception = aggregateException.InnerExceptions[0];
            }

            if (exception is SyntaxException syntaxException)
            {
                logger.LogError(syntaxException.GetErrorLine());
                logger.LogError(new string(' ', syntaxException.Position) + '^');
                logger.LogError("{Line}:{Position}: {Message}", syntaxException.Line, syntaxException.Position,
                    syntaxException.Message);
                return 4;
            }
            else if (exception is QueryCatException domainException)
            {
                logger.LogError(domainException.Message);
                return 2;
            }
            else if (exception is FormatException formatException)
            {
                logger.LogError(formatException.Message);
                return 3;
            }
            else if (exception is DashikException dashikException)
            {
                logger.LogError(dashikException.Message);
                return 2;
            }
            else if (exception is OperationCanceledException || exception is TaskCanceledException)
            {
                return 0;
            }
            else
            {
                logger.LogCritical(logger.IsEnabled(LogLevel.Debug) ? exception : null, exception.Message);
                return 1;
            }
        }
    }
}
