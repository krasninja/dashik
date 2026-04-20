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
    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public JsonObject WidgetSettings { get; set; } = new();

    /// <inheritdoc />
    public WidgetMainSettings MainSettings { get; set; } = new();

    /// <inheritdoc />
    public WidgetInfo Info { get; }

    public WidgetInstance(string id, WidgetInfo widgetInfo)
    {
        Id = id;
        Info = widgetInfo;
        MainSettings.UpdateInterval = Info.DefaultUpdateInterval;
    }

    public WidgetInstance(WidgetInfo widgetInfo)
        : this(IdGenerator.Generate(length: 8), widgetInfo)
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
