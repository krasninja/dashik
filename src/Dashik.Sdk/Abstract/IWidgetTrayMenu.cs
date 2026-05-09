using Avalonia.Controls;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// The interface allows for widgets to add custom system tray menu items.
/// </summary>
public interface IWidgetTrayMenu
{
    /// <summary>
    /// System tray menu icons.
    /// </summary>
    /// <returns>List of widget specific menu items.</returns>
    IReadOnlyList<MenuItem> GetTrayMenuItems();
}
