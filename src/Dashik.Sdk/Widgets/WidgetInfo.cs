using System.Reflection;
using Avalonia.Media;
using Dashik.Sdk.Abstract;

namespace Dashik.Sdk.Widgets;

/// <summary>
/// Contains the widget metadata information: id, type, name, icons, previews, etc.
/// </summary>
public class WidgetInfo
{
    private readonly WidgetInfoAttribute _infoAttribute;

    /// <summary>
    /// Widget type identifier.
    /// </summary>
    public string Id => _infoAttribute.Id;

    /// <summary>
    /// Widget type. Should inherit from <see cref="IWidget" />.
    /// </summary>
    public Type WidgetType { get; }

    /// <summary>
    /// Widget setting type.
    /// </summary>
    public Type? SettingsType => _infoAttribute.SettingsType;

    /// <summary>
    /// Widget name. It is used in header.
    /// </summary>
    public string Name => _infoAttribute.Name;

    /// <summary>
    /// Widget general description.
    /// </summary>
    public string Description => _infoAttribute.Description;

    public WidgetInfoAttribute Info => _infoAttribute;

    /// <summary>
    /// Widget icon.
    /// </summary>
    public IImage Icon { get; protected set; } = Assets.GenericWidgetIcon;

    /// <summary>
    /// Default update interval.
    /// </summary>
    public TimeSpan DefaultUpdateInterval { get; protected set; } = TimeSpan.FromMinutes(5);

    public WidgetInfo(WidgetInfoAttribute infoAttribute, Type widgetType)
    {
        _infoAttribute = infoAttribute;
        WidgetType = widgetType;
    }

    public WidgetInfo(Type widgetType)
    {
        var infoAttribute = widgetType.GetCustomAttribute<WidgetInfoAttribute>();
        if (infoAttribute == null)
        {
            throw new InvalidOperationException($"Widget type must have '{nameof(WidgetInfoAttribute)}' attribute.");
        }
        _infoAttribute = infoAttribute;
        WidgetType = widgetType;
    }

    /// <inheritdoc />
    public override string ToString() => $"Id = {Id}, WidgetType = {WidgetType}";
}
