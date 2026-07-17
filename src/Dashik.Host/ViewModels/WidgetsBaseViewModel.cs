using System.Reactive.Disposables;
using Avalonia.Collections;
using ReactiveUI;
using Dashik.Sdk.Utils;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;

namespace Dashik.Host.ViewModels;

public abstract class WidgetsBaseViewModel : ViewModelBase, IDisposable
{
    /// <summary>
    /// Window identifier.
    /// </summary>
    public string Id { get; protected set; } = IdGenerator.Generate();

    public WidgetsCollectionViewModel? WidgetsViewModel
    {
        get;
        set
        {
            _widgetUpdateSubscription?.Dispose();
            this.RaiseAndSetIfChanged(ref field, value);
            if (value != null)
            {
                _widgetUpdateSubscription = value.WidgetsCollectionUpdate.Subscribe(_ => { SyncWidgets(); });
            }
        }
    }

    public AvaloniaList<WidgetControlViewModel> Widgets { get; set; } = [];

    public SpaceViewModel? SelectedSpace
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private readonly CompositeDisposable _disposable = new();

    private IDisposable? _widgetUpdateSubscription;

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
        _disposable.Add(
            this.WhenAnyValue(p => p.SelectedSpace)
                .Subscribe(_ => { SyncWidgets(); })
        );
    }

    private void SyncWidgets()
    {
        if (SelectedSpace == null)
        {
            return;
        }
        if (Widgets.Count < 1 && SelectedSpace.Widgets.Count > 0)
        {
            Widgets.AddRange(SelectedSpace.Widgets.Select(w => new WidgetControlViewModel(w)));
            return;
        }

        var spaceWidgets = SelectedSpace.Widgets.ToHashSet();

        var toRemove = Widgets.Where(w => !spaceWidgets.Contains(w.WidgetViewModel)).ToList();
        foreach (var widget in toRemove)
        {
            Widgets.Remove(widget);
            widget.Dispose();
        }

        var existing = Widgets.Select(w => w.WidgetViewModel).ToHashSet();
        foreach (var w in SelectedSpace.Widgets)
        {
            if (!existing.Contains(w))
            {
                Widgets.Add(new WidgetControlViewModel(w));
            }
        }
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

    private void RemoveWidgets()
    {
        foreach (var widget in Widgets)
        {
            widget.Dispose();
        }
        Widgets.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposable.Dispose();
            _widgetUpdateSubscription?.Dispose();
            RemoveWidgets();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
