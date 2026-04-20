using Dashik.Abstractions;

namespace Dashik.Shared.Services.Packages;

/// <summary>
/// Get, install, remove, update widgets' packages.
/// </summary>
public interface IPackagesInstaller
{
    /// <summary>
    /// Get local installed packages.
    /// </summary>
    /// <param name="dir">Directory to scan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of installed packages.</returns>
    Task<IReadOnlyList<LocalWidgetPackage>> GetLocalAsync(string dir, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remote packages.
    /// </summary>
    /// <param name="storage">Widgets storage.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Remote widgets packages.</returns>
    Task<IReadOnlyList<RemoteWidgetPackage>> GetRemoteAsync(IPackagesStorage storage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Install package.
    /// </summary>
    /// <param name="path">Path to install.</param>
    /// <param name="package">Remote package to install.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Local widget package.</returns>
    Task<LocalWidgetPackage> InstallAsync(string path, RemoteWidgetPackage package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove package.
    /// </summary>
    /// <param name="package">Local package to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>True</c> if removed, <c>false</c> otherwise.</returns>
    Task<bool> RemoveAsync(LocalWidgetPackage package, CancellationToken cancellationToken = default);
}
