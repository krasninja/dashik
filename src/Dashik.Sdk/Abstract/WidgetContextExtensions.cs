namespace Dashik.Sdk.Abstract;

/// <summary>
/// Extensions for <see cref="IWidgetContext" />.
/// </summary>
public static class WidgetContextExtensions
{
    public static async Task<T?> GetStateAsync<T>(
        this IWidgetContext context,
        CancellationToken cancellationToken = default) where T : class
    {
        return (T?)await context.GetStateAsync(typeof(T), cancellationToken);
    }
}
