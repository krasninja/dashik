namespace Dashik.Abstractions;

/// <summary>
/// Application launch at system startup.
/// </summary>
public interface ISystemUtils
{
    #region Autostart

    /// <summary>
    /// Is auto-launch enabled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>True</c> if enabled, <c>false</c> otherwise.</returns>
    Task<bool> IsLaunchAtStartupEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable application auto-launch on system startup. The current exe location is used.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task EnableLaunchAtStartupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable application auto-launch on system startup.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task DisableLaunchAtStartupAsync(CancellationToken cancellationToken = default);

    #endregion
}
