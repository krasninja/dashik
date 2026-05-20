using Dashik.Sdk.Widgets;
using Dashik.Shared.Models;

namespace Dashik.Shared.Services.Widgets;

internal sealed class TransientWidgetInstance : WidgetInstance
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Is it the instance that indicates about widget error.
    /// </summary>
    public bool Error { get; set; }

    /// <summary>
    /// Show "setup" widget button.
    /// </summary>
    public bool RequiresSetup { get; set; }

    /// <inheritdoc />
    public TransientWidgetInstance(string id, WidgetInfo widgetInfo) : base(id, widgetInfo, StubWidgetStateStorage.Instance)
    {
    }

    /// <inheritdoc />
    public TransientWidgetInstance(WidgetInfo widgetInfo) : base(widgetInfo, StubWidgetStateStorage.Instance)
    {
    }
}
