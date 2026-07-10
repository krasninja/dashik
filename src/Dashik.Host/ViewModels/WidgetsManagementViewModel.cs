using ReactiveUI;
using Dashik.Host.Infrastructure.UI;
using Dashik.Sdk.Mvvm;
using Dashik.Host.Utils;

namespace Dashik.Host.ViewModels;

public sealed class WidgetsManagementViewModel : ViewModelBase, ICloseableViewModel, IDialogViewModel<string[]>
{
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
            .SubscribeAsync(AddWidgetAsync);

        AddPackageViewModel.PackagesLoaded
            .SubscribeAsync(async _ =>
            {
                // Reload widgets after packages are loaded.
                await AddWidgetViewModel.LoadAsync()
                    .ConfigureAwait(false);
            });
    }

    private async Task AddWidgetAsync(AddWidgetDetailsViewModel[] widgetNodes, CancellationToken cancellationToken)
    {
        foreach (var package in widgetNodes.Select(wn => wn.RemoteWidgetPackage).Distinct())
        {
            if (package != null)
            {
                await AddPackageViewModel.InstallPackageAsync(package, cancellationToken);
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

        await AddWidgetViewModel.LoadAsync(cancellationToken);
        await AddPackageViewModel.LoadAsync(cancellationToken);
        await AddFeedViewModel.LoadAsync(cancellationToken);
        await base.LoadAsync(cancellationToken);

        // Select "packages" tab if no widgets installed.
        if (AddWidgetViewModel.WidgetsCount < 1)
        {
            SelectedTabIndex = 1;
        }

        Loading = false;
    }
}
