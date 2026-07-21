using System.Runtime.Serialization;
using Avalonia.Collections;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;

namespace Dashik.Host.ViewModels.Settings;

[DataContract]
public class AppSettingsViewModel : ViewModelBase
{
    private readonly ISystemUtils? _systemUtils;

    [DataMember]
    public AvaloniaList<PackageFeedModel> PackagesFeeds { get; set; } = [];

    [DataMember]
    public string InstancesDirectory { get; set; } = string.Empty;

    [DataMember]
    public AvaloniaList<string> LocalPackagesDirectories { get; set; } = [];

    [DataMember]
    public AvaloniaList<SpaceModel> Spaces { get; set; } = [];

    [DataMember]
    public bool OriginalLaunchOnSystemStartup { get; set; }

    [DataMember]
    public bool LaunchOnSystemStartup { get; set; }

    [DataMember]
    public bool ShowSystemTrayIcon { get; set; }

    [DataMember]
    public bool StartMinimized { get; set; }

    [DataMember]
    public bool AutoUpdate { get; set; }

    /// <summary>
    /// Ctor for deserialization.
    /// </summary>
    internal AppSettingsViewModel()
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
        StartMinimized = appSettings.StartMinimized;
        ShowSystemTrayIcon = appSettings.ShowSystemTrayIcon;
        AutoUpdate = appSettings.AutoUpdate;
    }

    public AppSettings ToAppSettings()
    {
        return new AppSettings
        {
            PackagesFeeds = PackagesFeeds.ToList(),
            InstancesDirectory = InstancesDirectory,
            LocalPackagesDirectories = LocalPackagesDirectories.ToList(),
            Spaces = Spaces.ToList(),
            StartMinimized = StartMinimized,
            ShowSystemTrayIcon = ShowSystemTrayIcon,
            AutoUpdate = AutoUpdate,
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
