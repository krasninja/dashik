using Dashik.Sdk.Utils;
using Dashik.Sdk.Widgets;

namespace Dashik.Widgets.Motd;

public sealed class MotdWidgetInfo : WidgetInfo
{
    /// <inheritdoc />
    public MotdWidgetInfo(WidgetInfoAttribute infoAttributeAttribute, Type widgetType) : base(infoAttributeAttribute, widgetType)
    {
        Icon = EmbeddedResourceUtils.GetAsBitmap("Dashik.Widgets.Motd.Assets.Icon.png", typeof(MotdWidgetInfo).Assembly);
    }
}
