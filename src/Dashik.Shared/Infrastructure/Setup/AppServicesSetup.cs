using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend;
using Dashik.Abstractions;
using Dashik.Sdk.Mvvm;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services;
using Dashik.Shared.Services.Packages;
using Dashik.Shared.Services.Widgets;
using Dashik.QueryCat;

namespace Dashik.Shared.Infrastructure.Setup;

internal sealed class AppServicesSetup(Container container, AppArguments appArguments, AppSettings appSettings, string dataDirectory)
{
    internal const string SettingsFileName = "settings.json";
    internal const string ApplicationDirectory = "dashik";
    internal const string ReleaseUri = @"https://dashik.anti-soft.ru/downloads/releases/";

    public void Setup()
    {
        container.RegisterInstance(appSettings);
        container.RegisterSingleton<IWidgetsProvider, LocalWidgetsProvider>();
        container.Register<IWidgetsFactory, DefaultWidgetsFactory>();
        container.Register<IAppService>(() =>
        {
            var localAppSettings = container.GetRequiredService<AppSettings>();
            return new AppService(localAppSettings, dataDirectory, appArguments.PluginDirectories.ToArray());
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
            return CreateExecutionThread(appService);
        });
        container.Register(() =>
        {
            var thread = container.GetRequiredService<IExecutionThread>();
            return thread.PluginsManager.PluginsLoader;
        });
    }

    private IExecutionThread CreateExecutionThread(IAppService appService)
    {
        return new ExecutionThreadBootstrapper()
            .WithPluginsLoader((thread) => new DashikPluginsLoader(thread, container, appService.GetPackagesDirectories()))
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
