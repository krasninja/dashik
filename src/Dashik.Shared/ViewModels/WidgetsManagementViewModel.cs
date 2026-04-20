using System.Reactive.Linq;
using Dashik.Sdk.Mvvm;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Sdk.Widgets;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetsManagementViewModel : ViewModelBase, ICloseableViewModel, IDialogViewModel<WidgetInfo?>
{
    public AddWidgetViewModel AddWidgetViewModel { get; }

    public AddPackageViewModel AddPackageViewModel { get; }

    public AddFeedViewModel AddFeedViewModel { get; }

    #region ICloseableViewModel

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    #endregion

    #region IDialogViewModel

    /// <inheritdoc />
    public WidgetInfo? ResultValue { get; private set; }

    /// <inheritdoc />
    public DialogResult Result { get; private set; } = DialogResult.Cancel;

    #endregion

#pragma warning disable CS8618
    internal WidgetsManagementViewModel()
#pragma warning restore CS8618
    {
    }

    public WidgetsManagementViewModel(
        AddWidgetViewModel addWidgetViewModel,
        AddPackageViewModel addPackageViewModel,
        AddFeedViewModel addFeedViewModel) : this()
    {
        AddWidgetViewModel = addWidgetViewModel;
        AddPackageViewModel = addPackageViewModel;
        AddFeedViewModel = addFeedViewModel;

        AddWidgetViewModel.AddWidgetRequested
            .Do(AddWidget)
            .Subscribe();
    }

    private void AddWidget(WidgetInfo? widgetInfo)
    {
        if (widgetInfo == null)
        {
            return;
        }

        ResultValue = widgetInfo;
        Result = DialogResult.OK;
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await AddWidgetViewModel.LoadAsync(cancellationToken);
        await AddPackageViewModel.LoadAsync(cancellationToken);
        await AddFeedViewModel.LoadAsync(cancellationToken);
        await base.LoadAsync(cancellationToken);
    }
}
