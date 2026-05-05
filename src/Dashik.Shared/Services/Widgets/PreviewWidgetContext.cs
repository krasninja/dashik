using Dashik.Sdk.Abstract;

namespace Dashik.Shared.Services.Widgets;

public sealed class PreviewWidgetContext : IWidgetContext
{
    public static PreviewWidgetContext Instance { get; } = new();

    /// <inheritdoc />
    public HttpClient CreateHttpClient() => new();
}
