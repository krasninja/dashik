using System.Text.Json.Nodes;
using Dashik.Abstractions;
using Dashik.Sdk;
using Dashik.Sdk.Models;
using Dashik.Sdk.Utils;
using Dashik.Sdk.Widgets;

namespace Dashik.Shared.Models;

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

    public WidgetInstance(string id, WidgetInfo info, IWidgetsStateStorage stateStorage)
    {
        Id = id;
        Info = info;
        MainSettings.UpdateInterval = Info.DefaultUpdateInterval;

        _stateStorage = stateStorage;
    }

    public WidgetInstance(WidgetInfo info, IWidgetsStateStorage widgetsStateStorage)
        : this(IdGenerator.Generate(length: 8), info, widgetsStateStorage)
    {
    }

    #region Context

    private HttpClient? _httpClient = new();

    /// <inheritdoc />
    public HttpClient CreateHttpClient()
    {
        if (_httpClient == null)
        {
            HttpClientHandler clientHandler;
            if (!string.IsNullOrEmpty(MainSettings.WebProxy))
            {
                var proxy = new System.Net.WebProxy(MainSettings.WebProxy);
                clientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true,
                };
            }
            else
            {
                clientHandler = new HttpClientHandler();
            }
            _httpClient = new(clientHandler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", Application.GetProductFullName());
        }
        return _httpClient;
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

    private string GetStateId() => $"{Info.InfoAttribute.Id}-{Id}";

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
