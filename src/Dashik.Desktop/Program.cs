using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Parser;
using Application = QueryCat.Backend.Core.Application;
using Dashik.Desktop.Commands;
using Dashik.Sdk;

namespace Dashik.Desktop;

/// <summary>
/// Program entry point.
/// </summary>
internal sealed class Program
{
    private static readonly Lock _objLock = new();

    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">Startup arguments.</param>
    /// <returns>Exit code.</returns>
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new ApplicationRootCommand();
        int returnCode;
        try
        {
            returnCode = await rootCommand.Parse(args).InvokeAsync();
        }
        catch (Exception e)
        {
            returnCode = ProcessException(e);
            if (returnCode > 0)
            {
                Console.Error.WriteLine(e);
            }
        }
        return returnCode;
    }

    private static int ProcessException(Exception exception)
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
