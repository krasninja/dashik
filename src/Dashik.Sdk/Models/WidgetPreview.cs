namespace Dashik.Sdk.Models;

/// <summary>
/// Widget preview.
/// </summary>
public class WidgetPreview
{
    public string Name { get; }

    public string WidgetTitle { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public object? Settings { get; init; }

    public object Data { get; }

    public WidgetPreview(string name, object data)
    {
        Name = name;
        Data = data;
    }
}
