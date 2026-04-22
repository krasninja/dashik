using Dashik.Sdk.Utils;
using Dashik.Sdk.Widgets;

namespace Dashik.Widgets.Motd;

public sealed class MotdWidgetInfo : WidgetInfo
{
    /// <inheritdoc />
    public MotdWidgetInfo(WidgetInfoAttribute infoAttribute, Type widgetType) : base(infoAttribute, widgetType)
    {
        Icon = EmbeddedResourceUtils.GetAsBitmap("Dashik.Widgets.Motd.Assets.Icon.png", typeof(MotdWidgetInfo).Assembly);
    }
}
