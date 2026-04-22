using System.Collections.Concurrent;
using System.Reflection;
using Avalonia.Media.Imaging;

namespace Dashik.Sdk.Utils;

/// <summary>
/// The helper methods to work with embedded resources.
/// </summary>
public static class EmbeddedResourceUtils
{
    private static readonly ConcurrentDictionary<string, Bitmap> _bitmapCache = new();

    /// <summary>
    /// Get internal assembly resource as Avalonia bitmap.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <param name="assembly">Assembly with the resource.</param>
    /// <returns>Avalonia bitmap.</returns>
    public static Bitmap GetAsBitmap(string uri, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        var fullUri = $"{assembly.GetName().Name}, {uri}";
        return _bitmapCache.GetOrAdd(fullUri, localFullUri =>
        {
            using var stream = assembly.GetManifestResourceStream(uri);
            if (stream == null)
            {
#if DEBUG
                var allNames = assembly.GetManifestResourceNames();
#endif
                throw new InvalidOperationException($"Cannot find resource URI {localFullUri}.");
            }
            return new Bitmap(stream);
        });
    }

    /// <summary>
    /// Get internal assembly resource as string.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <param name="assembly">Assembly with the resource.</param>
    /// <returns>Text.</returns>
    public static string GetAsText(string uri, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        using var stream = assembly.GetManifestResourceStream(uri);
        if (stream == null)
        {
            throw new InvalidOperationException($"Cannot find resource URI {uri}.");
        }
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
}
