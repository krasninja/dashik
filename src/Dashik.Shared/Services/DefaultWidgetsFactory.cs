using Microsoft.Extensions.DependencyInjection;
using Dashik.Abstractions;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;

namespace Dashik.Shared.Services;

public sealed class DefaultWidgetsFactory : IWidgetsFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Decorator to provide widget-related services.
    /// </summary>
    private sealed class WidgetServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly WidgetInitInfo _initInfo;

        public WidgetServiceProvider(IServiceProvider serviceProvider, WidgetInitInfo initInfo)
        {
            _serviceProvider = serviceProvider;
            _initInfo = initInfo;
        }

        /// <inheritdoc />
        public object? GetService(Type serviceType)
        {
            if (typeof(IWidgetContext).IsAssignableFrom(serviceType))
            {
                return _initInfo.Context;
            }
            return _serviceProvider.GetService(serviceType);
        }
    }

    public DefaultWidgetsFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<IWidget> CreateAsync(Type widgetType, WidgetInitInfo initInfo, CancellationToken cancellationToken = default)
    {
        var serviceProvider = new WidgetServiceProvider(_serviceProvider, initInfo);
        var widget = (IWidget)ActivatorUtilities.CreateInstance(serviceProvider, widgetType);

        // Try to set settings.
        if (widget is IWidgetSettings widgetSettings)
        {
            var settings = initInfo.GetSettings(widgetSettings.SettingsType);
            var settingsProperty = widgetType.GetProperty(nameof(IWidgetSettings.Settings));
            if (settingsProperty != null && settingsProperty.CanWrite)
            {
                settingsProperty.SetValue(widget, settings);
            }
        }

        await widget.InitializeAsync(initInfo, cancellationToken);
        return widget;
    }
}
