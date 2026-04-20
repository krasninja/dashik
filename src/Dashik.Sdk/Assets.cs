using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Dashik.Sdk;

/// <summary>
/// General assets.
/// </summary>
internal static class Assets
{
    /// <summary>
    /// The default icon used for widget.
    /// </summary>
    public static readonly IImage GenericWidgetIcon
        = new Bitmap(AssetLoader.Open(new Uri(@"avares://Dashik.Sdk/Assets/GenericWidgetIcon.png")));
}
