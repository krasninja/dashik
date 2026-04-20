namespace Dashik.Shared.Services.Packages;

public sealed class DefaultPackagesStorage : FeedPackagesStorage
{
    private static readonly Uri _defaultFeedUri = new("https://dashik.anti-soft.ru/downloads/widgets/");

    public static DefaultPackagesStorage Instance { get; } = new();

    /// <inheritdoc />
    public DefaultPackagesStorage() : base(_defaultFeedUri.ToString(), "Default")
    {
    }
}
