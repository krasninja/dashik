using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using Dashik.Sdk.Widgets;

namespace Dashik.Sdk.Models;

/// <summary>
/// Widget metadata.
/// </summary>
public class WidgetMetadata
{
    /// <summary>
    /// Widget identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Widget name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Widget description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon file name.
    /// </summary>
    public string IconFileName { get; set; } = string.Empty;

    /// <summary>
    /// Icon image.
    /// </summary>
    [JsonIgnore]
    public virtual Task<Bitmap?> IconFileImage { get; } = Task.FromResult((Bitmap?)null);

    /// <summary>
    /// Category.
    /// </summary>
    public WidgetCategory Category { get; set; } = WidgetCategory.Misc;

    /// <summary>
    /// Preview data.
    /// </summary>
    public List<WidgetMetadataPreview> PreviewItems { get; set; } = [];

    /// <summary>
    /// Constructor.
    /// </summary>
    public WidgetMetadata()
    {
    }

    /// <summary>
    /// Clone constructor.
    /// </summary>
    /// <param name="widgetMetadata">Widget to clone.</param>
    public WidgetMetadata(WidgetMetadata widgetMetadata)
    {
        Id = widgetMetadata.Id;
        Name = widgetMetadata.Name;
        Description = widgetMetadata.Description;
        IconFileName = widgetMetadata.IconFileName;
        Category = widgetMetadata.Category;
        PreviewItems = new List<WidgetMetadataPreview>(widgetMetadata.PreviewItems);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Id}, {Name}";
}
