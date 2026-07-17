using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;
using Dashik.Host.Services;
using Dashik.Host.Services.Packages;
using Dashik.Host.Services.Widgets;
using Dashik.Sdk.Mvvm;
using Dashik.QueryCat;

namespace Dashik.Host.Infrastructure.Setup;

internal sealed class AppServicesSetup(
    Container container,
    AppArguments appArguments,
    AppSettings appSettings)
{
    public void Setup()
    {
        container.RegisterInstance(appSettings);
        container.RegisterSingleton<IWidgetsProvider, LocalWidgetsProvider>();
        container.Register<IWidgetsFactory, DefaultWidgetsFactory>();
        container.Register<IAppService>(() =>
        {
            var localAppSettings = container.GetRequiredService<AppSettings>();
            return new AppService(
                localAppSettings,
                appArguments.ConfigDirectory,
                appArguments.ApplicationDirectory,
                appArguments.PluginDirectories.ToArray()
            );
        });
        container.Register<IWidgetInstanceProvider, LocalWidgetInstanceProvider>();
        container.Register<IMvvmService, AvaloniaMvvmService>();
        container.Register<IDataTemplate, ViewLocator>();
        container.Register<IPackagesStorage[]>(CreateWidgetsStorages);
        container.Register<Func<IPackagesStorage[]>>(() => CreateWidgetsStorages);
        container.Register<IPackagesInstaller, PackagesInstaller>();
        container.RegisterSingleton(() =>
        {
            var appService = container.GetRequiredService<IAppService>();
            return CreateExecutionThread(appService, appArguments.DebugMode);
        });
        container.Register<IWidgetsStateStorage, FileWidgetsStateStorage>();
        container.Register(() =>
        {
            var thread = container.GetRequiredService<IExecutionThread>();
            return thread.PluginsManager.PluginsLoader;
        });
        container.RegisterSingleton<ISystemUtils>(() =>
        {
            if (OperatingSystem.IsLinux())
            {
                return new LinuxSystemUtils();
            }
            if (OperatingSystem.IsWindows())
            {
                return new WindowsSystemUtils();
            }
            if (OperatingSystem.IsMacOS())
            {
                return new MacSystemUtils();
            }
            return new StubSystemUtils();
        });
    }

    private IExecutionThread CreateExecutionThread(IAppService appService, bool debugMode = false)
    {
        return new ExecutionThreadBootstrapper()
            .WithPluginsLoader(thread =>
            {
                return new DashikPluginsLoader(thread, container, appService.GetPackagesDirectories())
                {
                    PreferDllLoad = debugMode,
                };
            })
            .Create();
    }

    private IPackagesStorage[] CreateWidgetsStorages()
    {
        var storages = new List<IPackagesStorage>();
        storages.Add(DefaultPackagesStorage.Instance);
        var localAppSettings = container.GetRequiredService<AppSettings>();
        foreach (var feed in localAppSettings.PackagesFeeds)
        {
            storages.Add(new FeedPackagesStorage(feed.Uri.ToString(), feed.Name));
        }
        return storages.ToArray();
    }
}
