using System.Net.Http.Json;
using Dashik.Abstractions;
using Dashik.Sdk.Models;

namespace Dashik.Shared.Services.Packages;

public class FeedPackagesStorage : IPackagesStorage
{
    /// <inheritdoc />
    public string Uri { get; }

    /// <inheritdoc />
    public string Name { get; }

    public FeedPackagesStorage(string uri, string name)
    {
        Uri = uri;
        Name = name;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WidgetPackage>> GetAsync(CancellationToken cancellationToken = default)
    {
        // ReSharper disable once ShortLivedHttpClient
        var httpClient = new HttpClient();
        var uri = Uri.EndsWith('/') ? Uri : $"{Uri}/";
        var packages = await httpClient.GetFromJsonAsync<List<WidgetPackage>>(uri + "index.json", cancellationToken);
        if (packages == null)
        {
            return [];
        }
        return packages;
    }
}
