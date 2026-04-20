using System.CommandLine;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using Velopack;
using Dashik.Shared;
using Dashik.Desktop.Infrastructure.Updates;
using Dashik.Shared.Infrastructure.Setup;
using Dashik.Shared.Infrastructure.Updates;

namespace Dashik.Desktop.Commands;

internal sealed class ApplicationRootCommand : RootCommand
{
    public ApplicationRootCommand() : base("Dashik")
    {
        var pluginDirectoriesOption = new Option<string[]>("--plugin-dir")
        {
            AllowMultipleArgumentsPerToken = true,
            Description = "Plugin directories.",
        };
        var instancesDirectoryNameOption = new Option<string>("--instances-dir-name")
        {
            Description = "Instances directory name.",
        };
        var widgetFilterOption = new Option<string[]>("--widgets-filter")
        {
            AllowMultipleArgumentsPerToken = true,
            Description = "Widgets filter.",
        };
        var configurationOption = new Option<string>("--config")
        {
            Description = "Configuration file."
        };
        var debugOption = new Option<bool>("--debug")
        {
            Description = "Debug mode.",
        };
        var modeOption = new Option<AppArguments.RunMode>("--mode")
        {
            Description = "Run mode.",
        };
        var logLevelOption = new Option<LogLevel>("--log-level")
        {
            DefaultValueFactory = _ => LogLevel.Information,
            Description = "Log level.",
        };
        var logFileOption = new Option<string>("--log-file")
        {
            Description = "Log file.",
        };

        // Bootstrap the app.
        this.TreatUnmatchedTokensAsErrors = false;
        this.Add(pluginDirectoriesOption);
        this.Add(instancesDirectoryNameOption);
        this.Add(widgetFilterOption);
        this.Add(configurationOption);
        this.Add(debugOption);
        this.Add(modeOption);
        this.Add(logLevelOption);
        this.SetAction(async (parseResult, cancellationToken) =>
        {
            parseResult.InvocationConfiguration.EnableDefaultExceptionHandler = false;

            var appArguments = new AppArguments();
            appArguments.PluginDirectories.AddRange(parseResult.GetValue(pluginDirectoriesOption) ?? []);
            appArguments.InstancesDirectoryName = parseResult.GetValue(instancesDirectoryNameOption) ?? string.Empty;
            appArguments.WidgetsFilter.AddRange(parseResult.GetValue(widgetFilterOption) ?? []);
            appArguments.ConfigurationFile = parseResult.GetValue(configurationOption) ?? string.Empty;
            appArguments.DebugMode = parseResult.GetValue(debugOption);
            appArguments.Mode = parseResult.GetValue(modeOption);
            appArguments.MinLogLevel = parseResult.GetValue(logLevelOption);
            appArguments.LogFile = parseResult.GetValue(logFileOption) ?? string.Empty;

            appArguments.PluginDirectories = appArguments.PluginDirectories
                .Where(Directory.Exists)
                .ToList();

            var appRoot = new AppRoot(appArguments);
            await appRoot.SetupServicesAsync(container =>
            {
                container.RegisterSingleton<IAppUpdateService>(
                    () => new VelopackAppUpdateService(AppServicesSetup.ReleaseUri, container.GetRequiredService<ILogger<VelopackAppUpdateService>>())
                );
            }, cancellationToken);
            VelopackApp.Build()
                .SetLogger(
                    new MicrosoftExtensionsVelopackLogger(
                        appRoot.Container.GetRequiredService<ILogger<VelopackApp>>())
                )
                .Run();

            var avaloniaApp = AppBuilder
                .Configure(() => new App(appRoot))
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI(builder =>
                {
                })
                .LogToTrace();
            var args = parseResult.Tokens.Select(t => t.Value).ToArray();
            avaloniaApp.StartWithClassicDesktopLifetime(args);
        });
    }
}
