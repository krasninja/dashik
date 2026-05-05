namespace Dashik.Sdk.Models;

/// <summary>
/// Widget preview.
/// </summary>
public class WidgetPreview
{
    /// <summary>
    /// Preview name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Preview description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Settings to be applied to widget for preview.
    /// </summary>
    public object? Settings { get; init; }

    /// <summary>
    /// Custom data that could be needed to render preview.
    /// It can be model or view model applied to control/widget.
    /// </summary>
    public object? Data { get; }

    public WidgetPreview(string name, object? data = null)
    {
        Name = name;
        Data = data;
    }
}
