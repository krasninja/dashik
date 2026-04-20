namespace Dashik.Shared.Models;

public class PackageFeedModel
{
    public string Name { get; set; }

    public Uri Uri { get; set; }

    public PackageFeedModel(string name, Uri uri)
    {
        Name = name;
        Uri = uri;
    }
}
