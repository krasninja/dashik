using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Dashik.Sdk.Models;

/// <summary>
/// Package.
/// </summary>
public class WidgetPackage : ReactiveObject
{
    /// <summary>
    /// Package id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Summary.
    /// </summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>
    /// Version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Version.
    /// </summary>
    public Version SemVerVersion => new(Version);

    /// <summary>
    /// Authors.
    /// </summary>
    public string Authors { get; init; } = string.Empty;

    /// <summary>
    /// Tags.
    /// </summary>
    public string[] Tags { get; init; } = [];

    /// <summary>
    /// Copyright.
    /// </summary>
    public string Copyright { get; init; } = string.Empty;

    /// <summary>
    /// Owner.
    /// </summary>
    public string Owner { get; init; } = string.Empty;

    /// <summary>
    /// Icon file name.
    /// </summary>
    public string IconFileName { get; set; } = string.Empty;

    /// <summary>
    /// Icon image.
    /// </summary>
    public virtual Task<Bitmap?> IconFileImage { get; } = Task.FromResult((Bitmap?)null);

    /// <summary>
    /// File size.
    /// </summary>
    public long FileSize { get; set; }

    public WidgetPackage()
    {
    }

    public WidgetPackage(WidgetPackage package)
    {
        this.Id = package.Id;
        this.FileName = package.FileName;
        this.Name = package.Name;
        this.Description = package.Description;
        this.Summary = package.Summary;
        this.Version = package.Version;
        this.Authors = package.Authors;
        this.Tags = package.Tags.ToArray();
        this.Copyright = package.Copyright;
        this.Owner = package.Owner;
        this.IconFileName = package.IconFileName;
        this.FileSize = package.FileSize;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Id}, {Version}";
}
