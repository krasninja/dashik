using NuGet.Packaging;
using Dashik.Abstractions;

namespace Dashik.Shared.Services.Packages;

public sealed class PackagesInstaller : IPackagesInstaller
{
    private const string NupkgExtensionPattern = "*.nupkg";

    private static readonly HttpClient _httpClient = new();

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalWidgetPackage>> GetLocalAsync(string dir, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(dir))
        {
            return [];
        }

        var widgetEntries = new List<LocalWidgetPackage>();
        foreach (var file in Directory.GetFiles(dir, NupkgExtensionPattern, SearchOption.TopDirectoryOnly))
        {
            await using FileStream inputStream = new FileStream(file, FileMode.Open);
            using PackageArchiveReader reader = new PackageArchiveReader(inputStream);
            var nuspec = await reader.GetNuspecReaderAsync(cancellationToken);
            var fileName = nuspec.GetId() + "." + nuspec.GetVersion() + NupkgExtensionPattern;
            var widgetEntry = new LocalWidgetPackage(file)
            {
                Id = nuspec.GetId(),
                FileName = fileName,
                Name = nuspec.GetTitle(),
                Description = nuspec.GetDescription(),
                Summary = nuspec.GetSummary(),
                Version = nuspec.GetVersion().ToString(),
                Authors = nuspec.GetAuthors(),
                Tags = nuspec.GetTags().Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Copyright = nuspec.GetCopyright(),
                Owner = nuspec.GetOwners(),
                IconFileName = nuspec.GetIcon(),
                FileSize = new FileInfo(file).Length,
            };
            widgetEntries.Add(widgetEntry);
        }
        return widgetEntries;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RemoteWidgetPackage>> GetRemoteAsync(IPackagesStorage storage, CancellationToken cancellationToken = default)
    {
        var packages = new List<RemoteWidgetPackage>();
        var storagePackages = await storage.GetAsync(cancellationToken);
        foreach (var storagePackage in storagePackages)
        {
            var package = new RemoteWidgetPackage(storage.Uri, storagePackage);
            packages.Add(package);
        }
        return packages;
    }

    /// <inheritdoc />
    public async Task<LocalWidgetPackage> InstallAsync(string path, RemoteWidgetPackage package, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(path);

        var stream = await _httpClient.GetStreamAsync(package.RemoteFileUri, cancellationToken);
        var filePath = Path.Combine(path, package.FileName);
        await using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var file = Path.Combine(path, package.FileName);
        return new LocalWidgetPackage(file, package);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(LocalWidgetPackage package, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(package.NugetPackageFile))
        {
            return false;
        }
        if (File.Exists(package.NugetPackageFile))
        {
            File.Delete(package.NugetPackageFile);
            package.NugetPackageFile = string.Empty;
            return true;
        }
        return false;
    }
}
