using ReactiveUI;

namespace Dashik.Shared.Infrastructure.UI;

/// <summary>
/// Base view model with loading state feature.
/// </summary>
public class ViewModelBase : ReactiveObject
{
    /// <summary>
    /// Is in loading state.
    /// </summary>
    public bool Loading
    {
        get => field;
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
