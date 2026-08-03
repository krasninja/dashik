using ReactiveUI;

namespace Dashik.Sdk.Models;

/// <summary>
/// Settings section.
/// </summary>
public class SettingsSectionModel : ReactiveObject
{
    /// <summary>
    /// Section settings object.
    /// </summary>
    public virtual object? Settings
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Load the view model state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public virtual Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
