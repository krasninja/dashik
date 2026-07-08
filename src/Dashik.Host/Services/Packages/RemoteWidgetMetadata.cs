using Avalonia.Media.Imaging;
using Dashik.Sdk.Models;
using Dashik.Sdk.Utils;

namespace Dashik.Host.Services.Packages;

public sealed class RemoteWidgetMetadata : WidgetMetadata
{
    public string StorageUri { get; }

    public string IconFileUri => FormatAbsoluteUri(IconFileName);

    /// <summary>
    /// Icon image.
    /// </summary>
    public override Task<Bitmap?> IconFileImage => ImageUtils.LoadFromWeb(IconFileUri);

    public RemoteWidgetMetadata(string storageUri, WidgetMetadata widgetMetadata) : base(widgetMetadata)
    {
        StorageUri = storageUri;
    }

    private string FormatAbsoluteUri(string uri)
    {
        return StorageUri.EndsWith('/') ? StorageUri + uri : StorageUri + '/' + uri;
    }
}
