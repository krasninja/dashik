namespace Dashik.Abstractions;

/// <summary>
/// Package feed. Feed is the source to get packages.
/// </summary>
public class PackageFeed
{
    /// <summary>
    /// Feed name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Feed URI.
    /// </summary>
    public Uri Uri { get; }

    public PackageFeed(string name, Uri uri)
    {
        Name = name;
        Uri = uri;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Name}: {Uri}";
}
