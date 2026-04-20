using Avalonia.Controls;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Additional widget menu items.
/// </summary>
public interface IWidgetMenu
{
    IReadOnlyList<MenuItem> GetMenuItems();
}
