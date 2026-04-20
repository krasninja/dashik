using Dashik.Abstractions;

namespace Dashik.Shared.Services.Packages;

public static class PackagesInstallerExtensions
{
    public static async Task<IReadOnlyList<RemoteWidgetPackage>> GetRemoteAsync(
        this IPackagesInstaller installer,
        IPackagesStorage[] storages,
        CancellationToken cancellationToken = default)
    {
        var remotePackages = new List<RemoteWidgetPackage>();
        foreach (var storage in storages)
        {
            remotePackages.AddRange(await installer.GetRemoteAsync(storage, cancellationToken));
        }
        return remotePackages;
    }

    public static async Task<IReadOnlyList<LocalWidgetPackage>> GetLocalAsync(
        this IPackagesInstaller installer,
        string[] dirs,
        CancellationToken cancellationToken = default)
    {
        var localPackages = new List<LocalWidgetPackage>();
        foreach (var dir in dirs)
        {
            localPackages.AddRange(await installer.GetLocalAsync(dir, cancellationToken));
        }
        return localPackages;
    }
}
