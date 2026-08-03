using System.Text.Json.Nodes;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget that support receiving incoming messages from another widgets.
/// </summary>
public interface IWidgetBus
{
    /// <summary>
    /// Receive message.
    /// </summary>
    /// <param name="fromWidgetId">From widget identifier.</param>
    /// <param name="messageId">Command or query name.</param>
    /// <param name="payload">Incoming payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response or null if the message cannot be processed.</returns>
    Task<JsonObject?> ReceiveMessageAsync(string fromWidgetId, string messageId, JsonObject payload, CancellationToken cancellationToken = default);
}
