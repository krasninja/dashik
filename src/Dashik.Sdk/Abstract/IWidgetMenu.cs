using Avalonia.Controls;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// The interface allows for widgets to add custom widget menu items. In widget menu.
/// </summary>
public interface IWidgetMenu
{
    /// <summary>
    /// Get widget specific menu items.
    /// </summary>
    /// <returns>List of widget specific menu items.</returns>
    IReadOnlyList<MenuItem> GetWidgetMenuItems();
}
