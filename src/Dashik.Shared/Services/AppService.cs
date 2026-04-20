using Dashik.Abstractions;
using Dashik.Shared.Models;
using Dashik.Shared.Services.Packages;

namespace Dashik.Shared.Services;

public sealed class AppService : IAppService
{
    internal const string WidgetsDirectory = "widgets";

    private readonly AppSettings _appSettings;
    private readonly string _dataDirectory;
    private readonly string[] _additionalPackagesDirectories;

    public AppService(AppSettings appSettings, string dataDirectory, string[] additionalPackagesDirectories)
    {
        _appSettings = appSettings;
        _dataDirectory = dataDirectory;
        _additionalPackagesDirectories = additionalPackagesDirectories;
    }

    /// <inheritdoc />
    public string GetDataDirectory() => _dataDirectory;

    /// <inheritdoc />
    public string GetInstancesDirectory() => Path.Combine(_dataDirectory, _appSettings.InstancesDirectory);

    /// <inheritdoc />
    public string[] GetPackagesDirectories()
    {
        string[] pluginsDirs =
        [
            Path.Combine(GetDataDirectory(), WidgetsDirectory),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WidgetsDirectory)
        ];
        return _additionalPackagesDirectories.Concat(pluginsDirs).ToArray();
    }

    /// <inheritdoc />
    public string GetMainPackageDirectory() => Path.Combine(GetDataDirectory(), WidgetsDirectory);

    /// <inheritdoc />
    public PackageFeed[] GetFeeds()
    {
        return Array.Empty<PackageFeed>()
            .Concat([new PackageFeed("Default", new Uri(DefaultPackagesStorage.Instance.Uri))])
            .Concat(_appSettings.PackagesFeeds.Select(f => new PackageFeed(f.Name, f.Uri)))
            .ToArray();
    }
}
