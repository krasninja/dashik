using System.ComponentModel;
using System.Reflection;
using Avalonia.Media;
using Dashik.Sdk.Widgets;

namespace Dashik.Abstractions;

/// <summary>
/// Widget category information, such as name, description, and icon. Used for widget discovery and registration.
/// </summary>
public sealed class WidgetCategoryInfo
{
    public string Name { get; }

    public string Description { get; }

    public IImage Icon { get; }

    public WidgetCategory Category { get; }

    public WidgetCategoryInfo(WidgetCategory category, IImage icon)
    {
        this.Category = category;
        this.Name = category.ToString();
        this.Description = GetDescription(category);
        this.Icon = icon;
    }

    /// <summary>
    /// Get enum description.
    /// </summary>
    /// <param name="value">Enum instance.</param>
    /// <returns>Description.</returns>
    public static string GetDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString();
    }
}
