using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace Dashik.Sdk.Abstract;

public interface IWidgetTrayMenu
{
    /// <summary>
    /// System tray menu icons.
    /// </summary>
    ObservableCollection<MenuItem> MenuItems { get; }
}
