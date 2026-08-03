using System.Text.Json.Nodes;

namespace Dashik.Host.Models;

/// <summary>
/// Widget message bus item.
/// </summary>
public sealed class WidgetMessage
{
    public const string UpdateMessageId = "__update";

    /// <summary>
    /// Message destination widget identifier. Can be empty for system messages.
    /// </summary>
    public string WidgetId { get; }

    /// <summary>
    /// Message identifier.
    /// </summary>
    public string MessageId { get; }

    /// <summary>
    /// JSON message payload.
    /// </summary>
    public JsonObject Payload { get; }

    public WidgetMessage(string widgetId, string messageId, JsonObject? payload = null)
    {
        WidgetId = widgetId;
        MessageId = messageId;
        Payload = payload ?? new JsonObject();
    }
}
