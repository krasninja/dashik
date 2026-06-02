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
    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">Startup arguments.</param>
    /// <returns>Exit code.</returns>
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                ErrorsHandler.ProcessException(exception);
            }
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            var returnCode = ErrorsHandler.ProcessException(e.Exception);
            if (returnCode > 0)
            {
                e.SetObserved();
            }
        };

        var rootCommand = new ApplicationRootCommand();
        var returnCode = await rootCommand.Parse(args).InvokeAsync();
        return returnCode;
    }
}
