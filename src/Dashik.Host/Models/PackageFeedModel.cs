namespace Dashik.Host.Models;

public class PackageFeedModel
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URI to the feed.
    /// </summary>
    public Uri Uri { get; set; } = new("http://localhost");

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    public PackageFeedModel()
    {
    }

    public PackageFeedModel(string name, Uri uri)
    {
        Name = name;
        Uri = uri;
    }

    public PackageFeedModel(PackageFeedModel model)
    {
        Name = model.Name;
        Uri = model.Uri;
    }
}
