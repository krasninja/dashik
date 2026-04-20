namespace Dashik.Abstractions;

/// <summary>
/// The class is to load widget instances.
/// </summary>
public interface IWidgetInstanceProvider
{
    /// <summary>
    /// Load widgets instances.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of widget instances.</returns>
    Task<IEnumerable<IWidgetInstance>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save widget instance.
    /// </summary>
    /// <param name="instance">Widget instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task SaveAsync(IWidgetInstance instance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove widget instance.
    /// </summary>
    /// <param name="instance">Instance to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task RemoveAsync(IWidgetInstance instance, CancellationToken cancellationToken = default);
}
