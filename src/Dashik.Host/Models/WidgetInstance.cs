using System.Text.Json.Nodes;
using Dashik.Abstractions;
using Dashik.Sdk;
using Dashik.Sdk.Models;
using Dashik.Sdk.Utils;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.Models;

/// <summary>
/// Widget instance represents the copy of widget with its own options and state.
/// </summary>
public class WidgetInstance : IWidgetInstance, IDisposable
{
    private readonly IWidgetsStateStorage _stateStorage;

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public JsonObject WidgetSettings { get; set; } = new();

    /// <inheritdoc />
    public WidgetMainSettings MainSettings { get; set; } = new();

    /// <inheritdoc />
    public WidgetInfo Info { get; }

    /// <inheritdoc />
    public bool PreviewMode => false;

    internal Func<WidgetMessage, CancellationToken, Task<JsonObject?>> OnMessageSend { get; set; }

    public WidgetInstance(string id, WidgetInfo info, IWidgetsStateStorage stateStorage)
    {
        Id = id;
        Info = info;
        MainSettings.UpdateInterval = Info.DefaultUpdateInterval;
        OnMessageSend = (_, _) => Task.FromResult((JsonObject?)null);

        _stateStorage = stateStorage;
    }

    public WidgetInstance(WidgetInfo info, IWidgetsStateStorage widgetsStateStorage)
        : this(IdGenerator.Generate(length: 8), info, widgetsStateStorage)
    {
    }

    #region Context

    private HttpClientHandler? _clientHandler;

    /// <inheritdoc />
    public HttpClient CreateHttpClient()
    {
        if (_clientHandler == null)
        {
            if (!string.IsNullOrEmpty(MainSettings.WebProxy))
            {
                var proxy = new System.Net.WebProxy(MainSettings.WebProxy);
                proxy.BypassProxyOnLocal = true;
                _clientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true,
                };
            }
            else
            {
                _clientHandler = new HttpClientHandler();
            }
        }

        var client = new HttpClient(_clientHandler, disposeHandler: false);
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            $"{Application.ProductName}/{Application.GetVersion()} ({Application.GetPlatform()} {Application.GetArchitecture()})"
        );
        return client;
    }

    /// <inheritdoc />
    public async Task SetStateAsync(object state, CancellationToken cancellationToken = default)
    {
        await _stateStorage.SetStateAsync(state, GetStateId(), cancellationToken);
    }

    /// <inheritdoc />
    public Task<object?> GetStateAsync(Type stateType, CancellationToken cancellationToken = default)
    {
        return _stateStorage.GetStateAsync(stateType, GetStateId(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JsonObject?> SendMessageAsync(string toWidgetId, string messageId, JsonObject payload, CancellationToken cancellationToken = default)
    {
        var result = await OnMessageSend.Invoke(
            new WidgetMessage(toWidgetId, messageId, payload),
            cancellationToken
        );
        return result;
    }

    private string GetStateId() => $"{Info.InfoAttribute.Id}-{Id}";

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clientHandler?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString() => $"{Info.Name}: {Id}";
}
