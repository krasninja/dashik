using System.Net.Http.Json;
using Dashik.Abstractions;
using Dashik.Sdk.Models;

namespace Dashik.Host.Services.Packages;

public class FeedPackagesStorage : IPackagesStorage
{
    private const string IndexJson = "index.json";

    /// <inheritdoc />
    public string Uri { get; }

    /// <inheritdoc />
    public string Name { get; }

    public FeedPackagesStorage(string uri, string name)
    {
        Uri = uri.Trim();
        Name = name;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WidgetPackage>> GetAsync(CancellationToken cancellationToken = default)
    {
        var feed = await GetFeedAsync(cancellationToken);
        return feed.Packages;
    }

    internal async Task<WidgetPackageFeed> GetFeedAsync(CancellationToken cancellationToken = default)
    {
        // ReSharper disable once ShortLivedHttpClient
        using var httpClient = new HttpClient();
        var uri = Uri;
        if (!uri.EndsWith(IndexJson))
        {
            uri = uri.EndsWith('/') ? uri : $"{uri}/";
            uri += IndexJson;
        }
        var feedPackages = await httpClient.GetFromJsonAsync<WidgetPackageFeed>(uri, cancellationToken);
        if (feedPackages == null)
        {
            return WidgetPackageFeed.Empty;
        }
        return feedPackages;
    }
}
