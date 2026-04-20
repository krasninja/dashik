using System.Collections.Frozen;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Dashik.Abstractions;
using Dashik.Sdk.Widgets;

namespace Dashik.Shared.Services.Widgets;

internal sealed class LocalWidgetsProvider : IWidgetsProvider
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<string, WidgetInfo> _widgetsTypes = new();

    private static readonly ResourceInclude _resourceInclude = new(new Uri("avares://Dashik.Shared/Resources/FontAwesome.axaml"))
    {
        Source = new Uri("avares://Dashik.Shared/Resources/FontAwesome.axaml")
    };

    private static IDictionary<WidgetCategory, string> WidgetCategoryIcons { get; } = new Dictionary<WidgetCategory, string>
    {
        [WidgetCategory.Misc] = "FontAwesomeSolidCube",
        [WidgetCategory.Accessibility] = "FontAwesomeBrandsAccessibleIcon",
        [WidgetCategory.ApplicationLaunchers] = "FontAwesomeSolidRocket",
        [WidgetCategory.Clipboard] = "FontAwesomeSolidClipboard",
        [WidgetCategory.DateTime] = "FontAwesomeRegularClock",
        [WidgetCategory.EnvironmentWeather] = "FontAwesomeSolidCloudMoonRain",
        [WidgetCategory.FileSystem] = "FontAwesomeHardDrive",
        [WidgetCategory.FunGames] = "FontAwesomeSolidVolleyball",
        [WidgetCategory.Graphics] = "FontAwesomeSolidPaintbrush",
        [WidgetCategory.Language] = "FontAwesomeSolidLanguage",
        [WidgetCategory.Multimedia] = "FontAwesomeSolidDrum",
        [WidgetCategory.OnlineServices] = "FontAwesomeSolidCloud",
        [WidgetCategory.SystemInformation] = "FontAwesomeSolidComputer",
        [WidgetCategory.Productivity] = "FontAwesomeSolidCalendarCheck",
        [WidgetCategory.SoftwareDevelopment] = "FontAwesomeSolidCode",
        [WidgetCategory.Utilities] = "FontAwesomeBrandsSuperpowers",
    }.ToFrozenDictionary();

    public LocalWidgetsProvider(
        ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public void Register(Type widgetType)
    {
        var infoAttribute = widgetType.GetCustomAttribute<WidgetInfoAttribute>()
                            ?? throw new InvalidOperationException($"Widget type must have '{nameof(WidgetInfoAttribute)}' attribute.");
        WidgetInfo widgetInfo;
        if (infoAttribute.InfoType != null)
        {
            widgetInfo = (WidgetInfo)Activator.CreateInstance(infoAttribute.InfoType, args: [infoAttribute, widgetType])!;
        }
        else
        {
            widgetInfo = new WidgetInfo(infoAttribute, widgetType);
        }

        if (!_widgetsTypes.TryAdd(widgetInfo.Id, widgetInfo))
        {
            var logger = _loggerFactory.CreateLogger<LocalWidgetsProvider>();
            logger.LogWarning("The widget with id '{WidgetId}' has been already registered. Skipping...", widgetInfo.Id);
        }
    }

    /// <inheritdoc />
    public IEnumerable<WidgetInfo> GetAll() => _widgetsTypes.Values;

    /// <inheritdoc />
    public WidgetInfo? GetByTypeId(string widgetTypeId) => _widgetsTypes.GetValueOrDefault(widgetTypeId);

    /// <inheritdoc />
    public IEnumerable<WidgetCategoryInfo> GetCategories()
    {
        foreach (var category in Enum.GetValues<WidgetCategory>())
        {
            if (WidgetCategoryIcons.TryGetValue(category, out var iconId)
                && _resourceInclude.TryGetResource(iconId, null, out var resource) && resource is StreamGeometry geometry)
            {
                yield return new WidgetCategoryInfo(category, ConvertToBitmap(geometry, Brushes.Gray));
            }
            else
            {
                throw new InvalidOperationException($"{category} is not a supported widget category.");
            }
        }
    }

    private static Bitmap ConvertToBitmap(StreamGeometry geometry, IBrush brush)
    {
        var pixelSize = new PixelSize((int)geometry.Bounds.Width, (int)geometry.Bounds.Height);
        var bitmap = new RenderTargetBitmap(pixelSize);
        using var context = bitmap.CreateDrawingContext();
        context.DrawGeometry(brush, null, geometry);
        return bitmap;
    }
}
