using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Dashik.Sdk.Utils;

/// <summary>
/// Avalonia images helpers.
/// </summary>
/// <remarks>
/// Source: https://docs.avaloniaui.net/docs/guides/data-binding/how-to-bind-image-files.
/// </remarks>
public static class ImageUtils
{
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Load image from project resources.
    /// </summary>
    /// <param name="resourceUri">Resource URI.</param>
    /// <returns>Instance of <see cref="Bitmap" />.</returns>
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    /// <summary>
    /// Load from web URL.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <returns>Instance of <see cref="Bitmap" />.</returns>
    public static async Task<Bitmap?> LoadFromWeb(string? url)
    {
        return !string.IsNullOrEmpty(url) ? await LoadFromWeb(new Uri(url)) : null;
    }

    /// <summary>
    /// Load form web URL.
    /// </summary>
    /// <param name="url">URL.</param>
    /// <returns>Instance of <see cref="Bitmap" />.</returns>
    public static async Task<Bitmap?> LoadFromWeb(Uri url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return new Bitmap(await response.Content.ReadAsStreamAsync());
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
