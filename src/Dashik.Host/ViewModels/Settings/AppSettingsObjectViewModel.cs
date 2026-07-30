using Avalonia.Collections;
using YamlDotNet.Serialization;
using Dashik.Host.Models;

namespace Dashik.Host.ViewModels.Settings;

public class AppSettingsObjectViewModel
{
    public AvaloniaList<PackageFeedModel> PackagesFeeds { get; set; } = [];

    public string InstancesDirectory { get; set; } = string.Empty;

    public AvaloniaList<string> LocalPackagesDirectories { get; set; } = [];

    public AvaloniaList<SpaceModel> Spaces { get; set; } = [];

    [YamlIgnore]
    public bool OriginalLaunchOnSystemStartup { get; set; }

    public bool LaunchOnSystemStartup { get; set; }

    public bool ShowSystemTrayIcon { get; set; }

    public bool StartMinimized { get; set; }

    public bool AutoUpdate { get; set; }

    public bool IsLaunchOnSystemStartupChanged => OriginalLaunchOnSystemStartup != LaunchOnSystemStartup;

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
}
