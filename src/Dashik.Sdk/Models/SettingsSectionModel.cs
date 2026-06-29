using ReactiveUI;

namespace Dashik.Sdk.Models;

/// <summary>
/// Settings section.
/// </summary>
public class SettingsSectionModel : ReactiveObject
{
    /// <summary>
    /// Settings object.
    /// </summary>
    public object? Settings
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// To be called when update settings are set.
    /// </summary>
    public virtual void SyncSetting()
    {
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
