using Microsoft.Extensions.Logging;

namespace Dashik.Host.ViewModels;

public sealed class WidgetsBarViewModel : WidgetsBaseViewModel
{
    private readonly ILogger _logger;

    public WidgetsBarViewModel(ILogger<WidgetsBarViewModel> logger)
    {
        _logger = logger;
    }
}
