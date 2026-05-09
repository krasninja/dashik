using System.Collections.ObjectModel;

namespace Dashik.Widgets.Motd;

/// <summary>
/// MOTD widget settings.
/// </summary>
public class MotdWidgetSettings
{
    /// <summary>
    /// "Message of the day" messages.
    /// </summary>
    public ObservableCollection<Motd> Messages { get; set; } = [];
}
