using Dashik.Abstractions;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.Services.Widgets;

internal sealed class RemoteWidgetsProvider : IWidgetsProvider
{
    /// <inheritdoc />
    public void Register(Type widgetType)
    {
    }

    /// <inheritdoc />
    public IEnumerable<WidgetInfo> GetAll()
    {
        yield break;
    }

    /// <inheritdoc />
    public WidgetInfo? GetById(string widgetId) => null;

    /// <inheritdoc />
    public WidgetInfo? GetByTypeId(string widgetTypeId) => null;

    /// <inheritdoc />
    public IEnumerable<WidgetCategoryInfo> GetCategories()
    {
        yield break;
    }
}
