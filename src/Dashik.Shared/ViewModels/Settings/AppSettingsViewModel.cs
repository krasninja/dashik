using Avalonia.Collections;
using Dashik.Abstractions;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class AppSettingsViewModel : ViewModelBase
{
    private readonly ISystemUtils? _systemUtils;

    public AvaloniaList<PackageFeedModel> PackagesFeeds { get; set; } = [];

    public string InstancesDirectory { get; set; } = string.Empty;

    public AvaloniaList<string> LocalPackagesDirectories { get; set; } = [];

    public AvaloniaList<SpaceModel> Spaces { get; set; } = [];

    public bool OriginalLaunchOnSystemStartup { get; set; }

    public bool LaunchOnSystemStartup { get; set; }

    public bool IsTopmost { get; set; }

    public bool ShowSystemTrayIcon { get; set; }

    /// <summary>
    /// Ctor for deserialization.
    /// </summary>
    private AppSettingsViewModel()
    {
    }

    public AppSettingsViewModel(
        AppSettings appSettings,
        ISystemUtils systemUtils)
    {
        _systemUtils = systemUtils;
        PackagesFeeds = new AvaloniaList<PackageFeedModel>(appSettings.PackagesFeeds);
        InstancesDirectory = appSettings.InstancesDirectory;
        LocalPackagesDirectories = new AvaloniaList<string>(appSettings.LocalPackagesDirectories);
        Spaces = new AvaloniaList<SpaceModel>(appSettings.Spaces);
    }

    public AppSettings ToAppSettings()
    {
        return new AppSettings
        {
            PackagesFeeds = PackagesFeeds.ToList(),
            InstancesDirectory = InstancesDirectory,
            LocalPackagesDirectories = LocalPackagesDirectories.ToList(),
            Spaces = Spaces.ToList()
        };
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_systemUtils != null)
        {
            LaunchOnSystemStartup = await _systemUtils.IsLaunchAtStartupEnabledAsync(cancellationToken);
            OriginalLaunchOnSystemStartup = LaunchOnSystemStartup;
        }
        await base.LoadAsync(cancellationToken);
    }

    public async Task SetApplicationStartupAsync(CancellationToken cancellationToken = default)
    {
        if (OriginalLaunchOnSystemStartup == LaunchOnSystemStartup || _systemUtils == null)
        {
            return;
        }

        if (LaunchOnSystemStartup)
        {
            await _systemUtils.EnableLaunchAtStartupAsync(cancellationToken);
        }
        else
        {
            await _systemUtils.DisableLaunchAtStartupAsync(cancellationToken);
        }
    }
}
