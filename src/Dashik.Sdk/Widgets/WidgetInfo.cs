using System.Reflection;
using Avalonia.Media;
using Dashik.Sdk.Abstract;

namespace Dashik.Sdk.Widgets;

/// <summary>
/// Contains the widget metadata information: id, type, name, icons, previews, etc.
/// </summary>
public class WidgetInfo
{
    private readonly WidgetInfoAttribute _infoAttributeAttribute;

    /// <summary>
    /// Widget type identifier.
    /// </summary>
    public string Id => _infoAttributeAttribute.Id;

    /// <summary>
    /// Widget type. Should inherit from <see cref="IWidget" />.
    /// </summary>
    public Type WidgetType { get; }

    /// <summary>
    /// Widget setting type.
    /// </summary>
    public Type? SettingsType => _infoAttributeAttribute.SettingsType;

    /// <summary>
    /// Widget name. It is used in header.
    /// </summary>
    public string Name => _infoAttributeAttribute.Name;

    /// <summary>
    /// Widget general description.
    /// </summary>
    public string Description => _infoAttributeAttribute.Description;

    /// <summary>
    /// Instance of <see cref="WidgetInfoAttribute" />.
    /// </summary>
    public WidgetInfoAttribute InfoAttribute => _infoAttributeAttribute;

    /// <summary>
    /// Widget icon.
    /// </summary>
    public IImage Icon { get; protected set; } = Assets.GenericWidgetIcon;

    /// <summary>
    /// Default update interval.
    /// </summary>
    public TimeSpan DefaultUpdateInterval { get; protected set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Related widget package identifier.
    /// </summary>
    public string PackageId { get; private set; }

    public WidgetInfo(WidgetInfoAttribute infoAttributeAttribute, Type widgetType)
    {
        _infoAttributeAttribute = infoAttributeAttribute;
        WidgetType = widgetType;
        PackageId = widgetType.Assembly.GetName().Name ?? string.Empty;
    }

    public WidgetInfo(Type widgetType)
    {
        var infoAttribute = widgetType.GetCustomAttribute<WidgetInfoAttribute>();
        if (infoAttribute == null)
        {
            throw new InvalidOperationException($"Widget type must have '{nameof(WidgetInfoAttribute)}' attribute.");
        }
        _infoAttributeAttribute = infoAttribute;
        WidgetType = widgetType;
        PackageId = widgetType.Assembly.GetName().Name ?? string.Empty;
    }

    /// <inheritdoc />
    public override string ToString() => $"Id = {Id}, WidgetType = {WidgetType}";
}
