using Avalonia.Media.Imaging;
using Dashik.Sdk.Models;
using Dashik.Sdk.Utils;

namespace Dashik.Host.Services.Packages;

public sealed class RemoteWidgetMetadataPreview : WidgetMetadataPreview
{
    public string StorageUri { get; }

    public override Task<Bitmap?> PreviewImage => ImageUtils.LoadFromWeb(FormatAbsoluteUri(PreviewName));

    public RemoteWidgetMetadataPreview(string storageUri, WidgetMetadataPreview widgetMetadata)
        : base(widgetMetadata)
    {
        StorageUri = storageUri;
    }

    private string FormatAbsoluteUri(string uri)
    {
        return StorageUri.EndsWith('/') ? StorageUri + uri : StorageUri + '/' + uri;
    }
}
