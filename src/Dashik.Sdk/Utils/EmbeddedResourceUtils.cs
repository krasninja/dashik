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
    /// <param name="assembly">Assembly with the resource.</param>
    /// <param name="uri">URI.</param>
    /// <returns>Avalonia bitmap.</returns>
    public static Bitmap GetAsBitmap(Assembly assembly, string uri)
    {
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

    /// <summary>
    /// Get internal assembly resource as Avalonia bitmap.
    /// </summary>
    /// <param name="fullUri">URI. Should be in full format like "(assembly name), (resource path)".</param>
    /// <returns>Bitmap.</returns>
    public static Bitmap GetAsBitmap(string fullUri)
    {
        return _bitmapCache.GetOrAdd(fullUri, localFullUri =>
        {
            var uriParts = localFullUri.Split(',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (uriParts.Length != 2)
            {
                throw new ArgumentException("Invalid URI.", nameof(localFullUri));
            }

            var assemblyPart = uriParts[0];
            var resourcePart = uriParts[1];
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName != null
                    && !a.FullName.StartsWith("System")
                    && !a.FullName.StartsWith("Microsoft"))
                .FirstOrDefault(a => GetShortAssemblyName(a.FullName).Equals(assemblyPart));
            if (assembly == null)
            {
                throw new InvalidOperationException($"Cannot find assembly {assemblyPart}.");
            }
            using var stream = assembly.GetManifestResourceStream(resourcePart);
            if (stream == null)
            {
                throw new InvalidOperationException($"Cannot find resource URI {resourcePart}.");
            }
            return new Bitmap(stream);
        });
    }

    private static string GetShortAssemblyName(string? fullAssemblyName)
    {
        if (string.IsNullOrEmpty(fullAssemblyName))
        {
            return string.Empty;
        }
        var commaIndex = fullAssemblyName.IndexOf(',');
        if (commaIndex > -1)
        {
            return fullAssemblyName.Substring(0, commaIndex);
        }
        return string.Empty;
    }
}
