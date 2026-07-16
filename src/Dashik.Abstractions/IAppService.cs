namespace Dashik.Abstractions;

/// <summary>
/// Generic application service.
/// </summary>
public interface IAppService
{
    /// <summary>
    /// Get application data directory.
    /// </summary>
    /// <returns>Data directory.</returns>
    string GetDataDirectory();

    /// <summary>
    /// Get application configuration directory.
    /// </summary>
    /// <returns>Configuration directory.</returns>
    string GetConfigDirectory();

    /// <summary>
    /// Get application widgets instances directory.
    /// </summary>
    /// <returns>Instances directory.</returns>
    string GetInstancesDirectory();

    /// <summary>
    /// Get widgets state directory.
    /// </summary>
    /// <returns>State directory.</returns>
    string GetStateDirectory();

    /// <summary>
    /// Get application directories with NuGet packages. The first directory is the default one.
    /// </summary>
    /// <returns>Packages directories.</returns>
    string[] GetPackagesDirectories();

    /// <summary>
    /// Get main package directory to install.
    /// </summary>
    /// <returns>Main package.</returns>
    string GetMainPackageDirectory();

    /// <summary>
    /// Get directory to store windows settings.
    /// </summary>
    /// <returns>Windows directory.</returns>
    string GetWindowsDirectory();

    /// <summary>
    /// Get feeds.
    /// </summary>
    /// <returns>Feeds.</returns>
    PackageFeed[] GetFeeds();
}
