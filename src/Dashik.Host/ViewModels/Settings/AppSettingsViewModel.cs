using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;

namespace Dashik.Host.ViewModels.Settings;

public class AppSettingsViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly ISystemUtils _systemUtils;

    public AppSettingsObjectViewModel Model { get; } = new();

    public AppSettingsViewModel(
        AppSettings appSettings,
        ISystemUtils systemUtils)
    {
        _appSettings = appSettings;
        _systemUtils = systemUtils;
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Model.LaunchOnSystemStartup = await _systemUtils.IsLaunchAtStartupEnabledAsync(cancellationToken);
        Model.OriginalLaunchOnSystemStartup = Model.LaunchOnSystemStartup;

        Model.PackagesFeeds.Clear();
        Model.PackagesFeeds.AddRange(_appSettings.PackagesFeeds.Select(pf => new PackageFeedModel(pf)));
        Model.InstancesDirectory = _appSettings.InstancesDirectory;
        Model.LocalPackagesDirectories.Clear();
        Model.LocalPackagesDirectories.AddRange(_appSettings.LocalPackagesDirectories);
        Model.Spaces.Clear();
        Model.Spaces.AddRange(_appSettings.Spaces.Select(s => new SpaceModel(s)));
        Model.StartMinimized = _appSettings.StartMinimized;
        Model.ShowSystemTrayIcon = _appSettings.ShowSystemTrayIcon;
        Model.AutoUpdate = _appSettings.AutoUpdate;

        await base.LoadAsync(cancellationToken);
    }

    public async Task SetApplicationStartupAsync(bool launch, CancellationToken cancellationToken = default)
    {
        if (launch)
        {
            await _systemUtils.EnableLaunchAtStartupAsync(cancellationToken);
        }
        else
        {
            await _systemUtils.DisableLaunchAtStartupAsync(cancellationToken);
        }
    }
}
