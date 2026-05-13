using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// The interface allows for widgets to add custom system tray menu items.
/// </summary>
public interface IWidgetTrayMenu
{
    /// <summary>
    /// List of widget specific menu items.
    /// </summary>
    ObservableCollection<NativeMenuItem> TrayMenuItems { get; }
}
