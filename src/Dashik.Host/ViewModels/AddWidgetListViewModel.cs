using System.Reactive;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Dashik.Abstractions;
using Dashik.Sdk.Mvvm;

namespace Dashik.Host.ViewModels;

public sealed class AddWidgetListViewModel : AddWidgetViewModel
{
    public ReactiveCommand<AddWidgetDetailsViewModel?, Unit> SelectWidgetCommand { get; }

    /// <inheritdoc />
    public AddWidgetListViewModel(
        IWidgetsProvider widgetsProvider,
        IWidgetsFactory widgetsFactory,
        IWidgetsStateStorage stateStorage,
        IServiceProvider serviceProvider,
        Func<IPackagesStorage[]> packagesStoragesFactory,
        IMvvmService mvvmService,
        ILoggerFactory loggerFactory,
        ILogger<AddWidgetListViewModel> logger)
        : base(
            widgetsProvider,
            widgetsFactory,
            stateStorage,
            serviceProvider,
            packagesStoragesFactory,
            mvvmService,
            loggerFactory,
            logger)
    {
        SelectWidgetCommand = ReactiveCommand.Create<AddWidgetDetailsViewModel?>(w =>
        {
            SelectedWidgetNode = w;
        });
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await base.LoadAsync(cancellationToken);
        SelectedWidgetNode = null;
    }
}
