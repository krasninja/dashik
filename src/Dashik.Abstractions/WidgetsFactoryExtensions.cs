using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;

namespace Dashik.Abstractions;

/// <summary>
/// Extensions for <see cref="IWidgetsFactory" />.
/// </summary>
public static class WidgetsFactoryExtensions
{
    public static async Task<TWidget> CreateAsync<TWidget>(
        this IWidgetsFactory widgetsFactory,
        WidgetInitInfo initInfo,
        CancellationToken cancellationToken = default) where TWidget : IWidget
    {
        return (TWidget)await widgetsFactory.CreateAsync(typeof(TWidget), initInfo, cancellationToken);
    }
}
