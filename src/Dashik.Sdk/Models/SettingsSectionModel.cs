using ReactiveUI;

namespace Dashik.Sdk.Models;

public class SettingsSectionModel : ReactiveObject
{
    public object? Settings
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

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
