namespace Dashik.Shared.Infrastructure.Updates;

/// <summary>
/// Application update service.
/// </summary>
public interface IAppUpdateService
{
    /// <summary>
    /// Check for updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New version of empty string.</returns>
    Task<string> CheckUpdatesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Update the app.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(CancellationToken cancellationToken);
}
