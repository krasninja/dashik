namespace Dashik.Shared.Models;

/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
{
    public List<PackageFeedModel> PackagesFeeds { get; set; } = [];

    public string InstancesDirectory { get; set; } = "instances";

    public string InstanceStateDirectory { get; set; } = "instances-state";

    public List<string> LocalPackagesDirectories { get; set; } = [];

    /// <summary>
    /// List of space.
    /// </summary>
    public List<SpaceModel> Spaces { get; set; } = [];

    /// <summary>
    /// List of windows.
    /// </summary>
    public List<WidgetsWindowModel> Windows { get; set; } = [];

    /// <summary>
    /// Get main space. It is used to place all widgets by default.
    /// </summary>
    /// <returns>Instance of <see cref="SpaceModel" />.</returns>
    public SpaceModel? GetDefaultSpace() => Spaces.Find(s => s.Default);

    /// <summary>
    /// Minimize app on startup.
    /// </summary>
    public bool StartMinimized { get; set; }

    /// <summary>
    /// Show system tray icon.
    /// </summary>
    public bool ShowSystemTrayIcon { get; set; }

    /// <summary>
    /// Application and widgets auto-update.
    /// </summary>
    public bool AutoUpdate { get; set; } = true;
}
