using System.Text.Json.Nodes;
using Dashik.Sdk.Abstract;

namespace Dashik.Host.Services.Widgets;

public sealed class StubWidgetContext : IWidgetContext
{
    public static StubWidgetContext Instance { get; } = new();

    /// <inheritdoc />
    public bool PreviewMode => false;

    /// <inheritdoc />
    public HttpClient CreateHttpClient() => new();

    /// <inheritdoc />
    public Task SetStateAsync(object state, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<object?> GetStateAsync(Type stateType, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

    /// <inheritdoc />
    public Task<JsonObject?> SendMessageAsync(string toWidgetId, string messageId, JsonObject payload, CancellationToken cancellationToken = default)
        => Task.FromResult((JsonObject?)null);
}
