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
    /// Preview images.
    /// </summary>
    public List<string> PreviewImages { get; } = [];
}
