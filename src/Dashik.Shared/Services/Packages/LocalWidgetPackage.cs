using Avalonia.Media.Imaging;
using NuGet.Packaging;
using Dashik.Abstractions;

namespace Dashik.Shared.Services.Packages;

public class LocalWidgetPackage : WidgetPackage
{
    /// <inheritdoc />
    public override Task<Bitmap?> IconFileImage => LoadIconFileImage();

    public string NugetPackageFile { get; set; }

    /// <inheritdoc />
    public LocalWidgetPackage(string nugetPackageFile)
    {
        NugetPackageFile = nugetPackageFile;
    }

    public LocalWidgetPackage(string nugetPackageFile, WidgetPackage package) : base(package)
    {
        NugetPackageFile = nugetPackageFile;
    }

    private async Task<Bitmap?> LoadIconFileImage(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(IconFileName))
        {
            return null;
        }

        await using FileStream inputStream = new FileStream(NugetPackageFile, FileMode.Open, FileAccess.Read);
        using PackageArchiveReader reader = new PackageArchiveReader(inputStream);
        if (!string.IsNullOrEmpty(IconFileName))
        {
            var entry = reader.GetEntry(IconFileName);
            await using var iconStream = await reader.GetEntry(IconFileName).OpenAsync(cancellationToken);
            var ms = new MemoryStream(capacity: (int)entry.Length);
            await iconStream.CopyToAsync(ms, cancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            return new Bitmap(ms);
        }

        return null;
    }
}
