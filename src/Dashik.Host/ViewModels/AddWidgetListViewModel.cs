using Microsoft.Extensions.Logging;
using Dashik.Abstractions;

namespace Dashik.Host.ViewModels;

public sealed class AddWidgetListViewModel : AddWidgetViewModel
{
    /// <inheritdoc />
    public AddWidgetListViewModel(
        IWidgetsProvider widgetsProvider,
        IWidgetsFactory widgetsFactory,
        IWidgetsStateStorage stateStorage,
        IServiceProvider serviceProvider,
        IPackagesStorage[] packagesStorages,
        ILogger<AddWidgetViewModel> logger)
        : base(
            widgetsProvider,
            widgetsFactory,
            stateStorage,
            serviceProvider,
            packagesStorages,
            logger)
    {
    }
}
