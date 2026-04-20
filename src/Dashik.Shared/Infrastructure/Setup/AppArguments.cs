using Microsoft.Extensions.Logging;

namespace Dashik.Shared.Infrastructure.Setup;

public class AppArguments
{
    public enum RunMode
    {
        Normal,

        Console,

        Server,

        Client,
    }

    public List<string> PluginDirectories { get; set; } = new();

    public string InstancesDirectoryName { get; set; } = string.Empty;

    public List<string> WidgetsFilter { get; set; } = new();

    public string ConfigurationFile { get; set; } = string.Empty;

    public bool DebugMode { get; set; }

    public RunMode Mode { get; set; } = RunMode.Normal;

    public string ClientUri { get; set; } = string.Empty;

    public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

    public string LogFile { get; set; } = string.Empty;
}
