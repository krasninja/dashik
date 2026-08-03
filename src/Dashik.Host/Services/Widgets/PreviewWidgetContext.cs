using System.Text.Json.Nodes;
using Dashik.Sdk.Abstract;

namespace Dashik.Host.Services.Widgets;

public sealed class PreviewWidgetContext : IWidgetContext
{
    private object? _state;

    public static PreviewWidgetContext Instance { get; } = new();

    /// <inheritdoc />
    public bool PreviewMode => true;

    /// <inheritdoc />
    public HttpClient CreateHttpClient() => new();

    /// <inheritdoc />
    public Task SetStateAsync(object state, CancellationToken cancellationToken = default)
    {
        _state = state;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<object?> GetStateAsync(Type stateType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state);
    }

    /// <inheritdoc />
    public void QueueWidgetUpdate()
    {
    }

    /// <inheritdoc />
    public Task<JsonObject?> SendMessageAsync(string toWidgetId, string messageId, JsonObject payload, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((JsonObject?)null);
    }
}
