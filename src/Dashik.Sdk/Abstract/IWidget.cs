using Avalonia.Controls;
using Dashik.Sdk.Widgets;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// The widget represents the object to render in dashboard.
/// It contains control, headers name, settings, and other properties.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Custom header.
    /// </summary>
    string Header { get; }

    /// <summary>
    /// Widget Avalonia control content.
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Initialize the widget.
    /// </summary>
    /// <param name="initInfo">Initialization data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default);
}
