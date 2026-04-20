namespace Dashik.Shared.Models;

/// <summary>
/// Application settings model.
/// </summary>
public class AppSettings
{
    public List<PackageFeedModel> PackagesFeeds { get; set; } = [];

    public string InstancesDirectory { get; set; } = "instances";

    public List<string> LocalPackagesDirectories { get; set; } = [];

    /// <summary>
    /// List of space.
    /// </summary>
    public List<SpaceModel> Spaces { get; set; } = [];

    /// <summary>
    /// Get main space. It is used to place all widgets by default.
    /// </summary>
    /// <returns>Instance of <see cref="SpaceModel" />.</returns>
    public SpaceModel? GetDefaultSpace() => Spaces.Find(s => s.Default);
}
