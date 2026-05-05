using System.Reflection;
using Avalonia.Media;

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

    public Type WidgetType { get; }

    public Type? SettingsType => _infoAttribute.SettingsType;

    public string Name => _infoAttribute.Name;

    /// <summary>
    /// Widget general description.
    /// </summary>
    public string Description => _infoAttribute.Description;

    public WidgetInfoAttribute Info => _infoAttribute;

    public IImage Icon { get; protected set; } = Assets.GenericWidgetIcon;

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
