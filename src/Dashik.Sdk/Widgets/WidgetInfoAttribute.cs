using Avalonia.Platform;

namespace Dashik.Sdk.Widgets;

/// <summary>
/// Widget attribute that is required for widget discovery and registration.
/// It contains widget metadata, such as id, name, description, settings type, and info type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class WidgetInfoAttribute : Attribute
{
    /// <summary>
    /// Widget unique identifier (f.e. "org.company.widgets.clock").
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Widget generic name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Widget description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Settings type. Optional.
    /// </summary>
    public Type? SettingsType { get; set; }

    /// <summary>
    /// Information type that provide more widget information. Optional.
    /// Must be inherited from <see cref="WidgetInfo" /> type.
    /// </summary>
    public Type? InfoType { get; set; }

    /// <summary>
    /// Widget category.
    /// </summary>
    public WidgetCategory Category { get; set; } = WidgetCategory.Misc;

    /// <inheritdoc />
    public WidgetInfoAttribute(string id, string name)
    {
        AssetLoader.Open(new Uri("avares://Dashik.Sdk/Assets/GenericWidgetIcon.png"));
        Id = id;
        Name = name;
    }
}
