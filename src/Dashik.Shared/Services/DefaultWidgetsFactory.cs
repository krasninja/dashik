using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Dashik.Abstractions;
using Dashik.Sdk;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Services.Widgets;

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
        if (widget is IWidgetSettings widgetSettings && !initInfo.Context.PreviewMode)
        {
            var settings = initInfo.GetSettings(widgetSettings.SettingsType);

            // Attempt to set setting by property.
            var settingsProperty = widgetType.GetProperty(nameof(IWidgetSettings.Settings));
            if (settingsProperty != null && settingsProperty.CanWrite)
            {
                settingsProperty.SetValue(widget, settings);
            }

            // Validate settings.
            try
            {
                ValidateObject(settings);
            }
            catch (WidgetNotConfiguredException e)
            {
                return await CreateErrorWidgetAsync(initInfo, e, cancellationToken);
            }
        }

        // Initialize.
        try
        {
            await widget.InitializeAsync(initInfo, cancellationToken);
        }
        catch (Exception e)
        {
            return await CreateErrorWidgetAsync(initInfo, e, cancellationToken);
        }

        return widget;
    }

    private static void ValidateObject(object obj)
    {
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(obj, context, results, true);
        if (!isValid && results.Count > 0)
        {
            var error = "Need to setup the widget: " + results[0].ErrorMessage;
            if (!string.IsNullOrEmpty(error))
            {
                throw new WidgetNotConfiguredException(error);
            }
        }
    }

    private async Task<StubWidget> CreateErrorWidgetAsync(WidgetInitInfo initInfo, Exception e, CancellationToken cancellationToken)
    {
        var widget = (StubWidget)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(StubWidget));
        await widget.InitializeAsync(
            new WidgetInitInfo(
                new TransientWidgetInstance(new WidgetInfo(widget.GetType()))
                {
                    Message = e.Message,
                    Error = true,
                    RequiresSetup = e is WidgetNotConfiguredException,
                },
                initInfo.MainSettings,
                initInfo.Settings
            ),
            cancellationToken
        );

        return widget;
    }
}
