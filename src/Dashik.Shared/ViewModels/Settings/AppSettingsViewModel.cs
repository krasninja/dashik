using Avalonia.Collections;
using Dashik.Shared.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class AppSettingsViewModel
{
    public AvaloniaList<PackageFeedModel> PackagesFeeds { get; set; } = [];

    public string InstancesDirectory { get; set; } = string.Empty;

    public AvaloniaList<string> LocalPackagesDirectories { get; set; } = [];

    public AvaloniaList<SpaceModel> Spaces { get; set; } = [];

    public AppSettingsViewModel()
    {
    }

    public AppSettingsViewModel(AppSettings appSettings)
    {
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
}
