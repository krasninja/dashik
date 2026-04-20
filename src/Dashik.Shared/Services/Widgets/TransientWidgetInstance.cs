using Dashik.Sdk.Widgets;
using Dashik.Shared.Models;

namespace Dashik.Shared.Services.Widgets;

internal sealed class TransientWidgetInstance : WidgetInstance
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool Error { get; set; }

    /// <inheritdoc />
    public TransientWidgetInstance(string id, WidgetInfo widgetInfo) : base(id, widgetInfo)
    {
    }

    /// <inheritdoc />
    public TransientWidgetInstance(WidgetInfo widgetInfo) : base(widgetInfo)
    {
    }
}
