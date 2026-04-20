using System.Text;
using Dashik.Abstractions;
using Microsoft.Extensions.Logging;
using Dashik.Shared.Infrastructure.Updates;
using Dashik.Shared.Services.Packages;
using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels;

public class AppUpdateViewModel : ObservableObject
{
    private readonly IAppUpdateService _updateService;
    private readonly IPackagesInstaller _packagesInstaller;
    private readonly Func<IPackagesStorage[]> _widgetsStoragesFactory;
    private readonly IAppService _appService;
    private readonly ILogger _logger;

    /// <summary>
    /// Is the new version available.
    /// </summary>
    public bool HasNewVersion => RemoteVersion > LocalVersion || WidgetPackages.Any(wp => wp.HasUpdate);

    public Version LocalVersion => new(Sdk.Application.GetShortVersion());

    public Version? RemoteVersion
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnPropertyChanged(nameof(HasNewVersion));
        }
    }

    public IReadOnlyList<WidgetPackageGroup> WidgetPackages
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnPropertyChanged(nameof(HasNewVersion));
        }
    }
    = [];

    public AppUpdateViewModel(
        IAppUpdateService updateService,
        IPackagesInstaller packagesInstaller,
        Func<IPackagesStorage[]> widgetsStoragesFactory,
        IAppService appService,
        ILogger<AppUpdateViewModel> logger)
    {
        _updateService = updateService;
        _packagesInstaller = packagesInstaller;
        _widgetsStoragesFactory = widgetsStoragesFactory;
        _appService = appService;
        _logger = logger;
    }

    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        await _updateService.UpdateAsync(cancellationToken);

        foreach (var widgetPackageGroup in WidgetPackages)
        {
            if (!widgetPackageGroup.HasUpdate || widgetPackageGroup.Remote == null)
            {
                continue;
            }
            await _packagesInstaller.InstallAsync(_appService.GetMainPackageDirectory(), widgetPackageGroup.Remote, cancellationToken);
        }
    }

    public async Task CheckAppUpdatesAsync(CancellationToken cancellationToken)
    {
        var remoteVersion = await _updateService.CheckUpdatesAsync(cancellationToken);
        if (string.IsNullOrEmpty(remoteVersion))
        {
            _logger.LogWarning("Cannot get remote app version.");
            return;
        }

        RemoteVersion = new Version(remoteVersion);
    }

    public async Task CheckPackagesUpdatesAsync(CancellationToken cancellationToken)
    {
        var storages = _widgetsStoragesFactory.Invoke();
        var remotePackages = await _packagesInstaller.GetRemoteAsync(storages, cancellationToken);
        var localPackages = await _packagesInstaller.GetLocalAsync(_appService.GetPackagesDirectories(), cancellationToken);
        WidgetPackages = WidgetPackageGroup.Combine(_appService.GetFeeds(), localPackages, remotePackages);
    }

    public string Dump()
    {
        var sb = new StringBuilder();
        if (RemoteVersion > LocalVersion)
        {
            sb.AppendLine($"New application version available! New version is {RemoteVersion}");
        }

        var newPackages = WidgetPackages.Where(wp => wp.HasUpdate).ToArray();
        if (newPackages.Length > 0)
        {
            sb.AppendLine("New packages:");
            foreach (var newPackage in newPackages)
            {
                if (newPackage.Local == null)
                {
                    continue;
                }
                sb.AppendLine($"- {newPackage.Current.Name}: {newPackage.Local.Version} -> {newPackage.Current.Version}");
            }
        }
        return sb.ToString();
    }
}
