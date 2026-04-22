using Avalonia.Media.Imaging;
using Dashik.Sdk.Models;
using Dashik.Sdk.Utils;

namespace Dashik.Shared.Services.Packages;

public class RemoteWidgetPackage : WidgetPackage
{
    public string StorageUri { get; }

    public string RemoteFileUri => FormatAbsoluteUri(FileName);

    public string IconFileUri => FormatAbsoluteUri(IconFileName);

    /// <summary>
    /// Icon image.
    /// </summary>
    public override Task<Bitmap?> IconFileImage => ImageUtils.LoadFromWeb(IconFileUri);

    public RemoteWidgetPackage(string storageUri, WidgetPackage package) : base(package)
    {
        StorageUri = storageUri;
    }

    private string FormatAbsoluteUri(string uri)
    {
        return StorageUri.EndsWith('/') ? StorageUri + uri : StorageUri + '/' + uri;
    }
}
