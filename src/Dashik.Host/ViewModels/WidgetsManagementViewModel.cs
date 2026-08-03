using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Extensions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Sdk.Mvvm;

namespace Dashik.Host.ViewModels;

public sealed class WidgetsManagementViewModel : ViewModelBase, ICloseableViewModel, IDialogViewModel<string[]>, IDisposable
{
    private readonly MultipleDisposable _disposables = new();

    public AddWidgetListViewModel AddWidgetViewModel { get; }

    public AddPackageViewModel AddPackageViewModel { get; }

    public AddFeedViewModel AddFeedViewModel { get; }

    public int SelectedTabIndex
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    #region ICloseableViewModel

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    #endregion

    #region IDialogViewModel

    /// <inheritdoc />
    public string[] ResultValue { get; private set; }

    /// <inheritdoc />
    public DialogResult Result { get; private set; } = DialogResult.Cancel;

    #endregion

#pragma warning disable CS8618
    internal WidgetsManagementViewModel()
#pragma warning restore CS8618
    {
    }

    public WidgetsManagementViewModel(
        AddWidgetListViewModel addWidgetViewModel,
        AddPackageViewModel addPackageViewModel,
        AddFeedViewModel addFeedViewModel) : this()
    {
        AddWidgetViewModel = addWidgetViewModel;
        AddPackageViewModel = addPackageViewModel;
        AddFeedViewModel = addFeedViewModel;

        AddWidgetViewModel.AddWidgetRequested
            .SubscribeAsync(AddWidgetAsync)
            .DisposeWith(_disposables);

        AddPackageViewModel.PackagesLoaded
            .SubscribeAsync(async _ =>
            {
                // Reload widgets after packages are loaded.
                await AddWidgetViewModel.LoadAsync();
            })
            .DisposeWith(_disposables);
        AddFeedViewModel.PackageFeedUpdateRequested
            .SubscribeAsync(async _ =>
            {
                // Reload widgets and packages after feeds are loaded.
                await AddWidgetViewModel.LoadAsync();
                await AddPackageViewModel.LoadAsync();
            })
            .DisposeWith(_disposables);
    }

    private async ValueTask AddWidgetAsync(AddWidgetDetailsViewModel[] widgetNodes)
    {
        foreach (var package in widgetNodes.Select(wn => wn.RemoteWidgetPackage).Distinct())
        {
            if (package != null)
            {
                await AddPackageViewModel.InstallPackageAsync(package, CancellationToken.None);
            }
        }

        ResultValue = widgetNodes.Select(w => w.Id).ToArray();
        Result = DialogResult.OK;
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Loading = true;

        try
        {
            await AddWidgetViewModel.LoadAsync(cancellationToken);
            await AddPackageViewModel.LoadAsync(cancellationToken);
            await AddFeedViewModel.LoadAsync(cancellationToken);
            await base.LoadAsync(cancellationToken);

            // Select "packages" tab if no widgets installed.
            if (AddWidgetViewModel.WidgetsCount < 1)
            {
                SelectedTabIndex = 1;
            }
        }
        finally
        {
            Loading = false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
