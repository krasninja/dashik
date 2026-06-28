using Microsoft.Extensions.Logging;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetsBarViewModel : WidgetsBaseViewModel
{
    private readonly ILogger _logger;

    public WidgetsBarViewModel(ILogger<WidgetsBarViewModel> logger)
    {
        _logger = logger;
    }
}
