using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;

namespace Dashik.Abstractions;

/// <summary>
/// Widgets factory.
/// </summary>
public interface IWidgetsFactory
{
    /// <summary>
    /// Create widget by the specific type.
    /// </summary>
    /// <param name="widgetType">Widget type.</param>
    /// <param name="initInfo">Initialization model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Instance of <see cref="IWidget" />.</returns>
    Task<IWidget> CreateAsync(Type widgetType, WidgetInitInfo initInfo, CancellationToken cancellationToken = default);
}
