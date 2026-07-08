using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

namespace Dashik.Sdk.Models;

/// <summary>
/// Preview image.
/// </summary>
public class WidgetMetadataPreview
{
    /// <summary>
    /// Preview file name.
    /// </summary>
    public string PreviewName { get; set; } = string.Empty;

    /// <summary>
    /// Icon image.
    /// </summary>
    [JsonIgnore]
    public virtual Task<Bitmap?> PreviewImage { get; } = Task.FromResult((Bitmap?)null);

    /// <summary>
    /// Preview name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Preview description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Constructor.
    /// </summary>
    public WidgetMetadataPreview()
    {
    }

    /// <summary>
    /// Cloning constructor.
    /// </summary>
    /// <param name="preview">Preview.</param>
    public WidgetMetadataPreview(WidgetMetadataPreview preview)
    {
        PreviewName = preview.PreviewName;
        Name = preview.Name;
        Description = preview.Description;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}
