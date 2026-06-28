using System.Reactive.Linq;
using Avalonia.Collections;
using ReactiveUI;
using Dashik.Sdk.Utils;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;

namespace Dashik.Shared.ViewModels;

public abstract class WidgetsBaseViewModel : ViewModelBase
{
    public string Id { get; protected set; } = IdGenerator.Generate();

    public WidgetsCollectionViewModel? WidgetsViewModel
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public AvaloniaList<WidgetViewModel> Widgets { get; set; } = [];

    public SpaceViewModel? SelectedSpace
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    protected sealed class WidgetsIdComparer : IComparer<WidgetViewModel>
    {
        private readonly string[] _order;

        public WidgetsIdComparer(string[] order)
        {
            _order = order;
        }

        /// <inheritdoc />
        public int Compare(WidgetViewModel? x, WidgetViewModel? y)
        {
            if (x == null || y == null)
            {
                return 1;
            }

            var posX = Array.IndexOf(_order, x.WidgetId);
            var posY = Array.IndexOf(_order, y.WidgetId);

            if (posX < posY)
            {
                return -1;
            }
            return posX > posY ? 1 : 0;
        }
    }

    protected WidgetsBaseViewModel()
    {
        this.WhenAnyValue(p => p.SelectedSpace)
            .Do(space =>
            {
                if (space == null)
                {
                    return;
                }
                Widgets.Clear();
                Widgets.AddRange(space.Widgets);
            });
    }

    /// <summary>
    /// Load UI state.
    /// </summary>
    /// <param name="state">UI state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public virtual Task LoadUiStateAsync(
        WindowStateModel state,
        CancellationToken cancellationToken = default)
    {
        Id = state.Id;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Save UI state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public virtual Task SaveUiStateAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
