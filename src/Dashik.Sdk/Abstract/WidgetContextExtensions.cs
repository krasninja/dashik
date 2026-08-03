namespace Dashik.Sdk.Abstract;

/// <summary>
/// Extensions for <see cref="IWidgetContext" />.
/// </summary>
public static class WidgetContextExtensions
{
    /// <summary>
    /// Get state.
    /// </summary>
    /// <param name="context">Widget context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>State object.</returns>
    public static async Task<T?> GetStateAsync<T>(
        this IWidgetContext context,
        CancellationToken cancellationToken = default) where T : class
    {
        return (T?)await context.GetStateAsync(typeof(T), cancellationToken);
    }
}
