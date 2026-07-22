namespace Dashik.Sdk.Models;

public class WidgetPackageFeed
{
    public static WidgetPackageFeed Empty => new()
    {
        Name = "Empty",
    };

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<WidgetPackage> Packages { get; set; } = new();
}
