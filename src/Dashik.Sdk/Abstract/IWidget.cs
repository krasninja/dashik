using Avalonia;
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
    /// Create the control associated with the widget.
    /// </summary>
    /// <param name="target">The target where the control will be placed.</param>
    /// <param name="targetSize">The size of the target.</param>
    /// <returns>Instance of <see cref="Control" />.</returns>
    Control? CreateControl(WidgetControlTarget target, Size targetSize);

    /// <summary>
    /// Initialize the widget.
    /// </summary>
    /// <param name="initInfo">Initialization data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default);
}
