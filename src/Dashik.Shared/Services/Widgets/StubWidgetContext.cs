using Dashik.Sdk.Abstract;

namespace Dashik.Shared.Services.Widgets;

public sealed class StubWidgetContext : IWidgetContext
{
    public static StubWidgetContext Instance { get; } = new();

    /// <inheritdoc />
    public bool PreviewMode => false;

    /// <inheritdoc />
    public HttpClient CreateHttpClient() => new();
}
