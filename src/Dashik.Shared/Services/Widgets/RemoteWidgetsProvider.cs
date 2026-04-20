using Dashik.Abstractions;
using Dashik.Sdk.Widgets;

namespace Dashik.Shared.Services.Widgets;

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
    public WidgetInfo? GetByTypeId(string widgetTypeId) => null;

    /// <inheritdoc />
    public IEnumerable<WidgetCategoryInfo> GetCategories()
    {
        yield break;
    }
}
