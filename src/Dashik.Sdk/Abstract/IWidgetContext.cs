namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget specific custom functionality.
/// </summary>
public interface IWidgetContext
{
    /// <summary>
    /// Creates new <see cref="HttpClient" />.
    /// </summary>
    /// <returns>Instance of <see cref="HttpClient" />.</returns>
    public HttpClient CreateHttpClient();
}
