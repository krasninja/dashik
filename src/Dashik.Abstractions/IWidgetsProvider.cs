using Dashik.Sdk.Widgets;

namespace Dashik.Abstractions;

/// <summary>
/// Widgets provider.
/// </summary>
public interface IWidgetsProvider
{
    /// <summary>
    /// Register new widget type.
    /// </summary>
    /// <param name="widgetType">Widget type.</param>
    void Register(Type widgetType);

    /// <summary>
    /// Get all widgets.
    /// </summary>
    /// <returns>List of widgets.</returns>
    IEnumerable<WidgetInfo> GetAll();

    /// <summary>
    /// Get widget by the specific type id.
    /// </summary>
    /// <param name="widgetTypeId">Widget type identifier.</param>
    /// <returns>Instance of <see cref="WidgetInfo" /> or null if not found.</returns>
    WidgetInfo? GetByTypeId(string widgetTypeId);

    /// <summary>
    /// Get widget categories.
    /// </summary>
    /// <returns>Categories.</returns>
    IEnumerable<WidgetCategoryInfo> GetCategories();
}
