using System.Text.Json.Nodes;

namespace Dashik.Host.Models;

public sealed class WidgetMessage
{
    public string WidgetId { get; }

    public string MessageId { get; }

    public JsonObject Payload { get; }

    public WidgetMessage(string widgetId, string messageId, JsonObject payload)
    {
        WidgetId = widgetId;
        MessageId = messageId;
        Payload = payload;
    }
}
