using System.Collections.ObjectModel;

namespace Dashik.Widgets.Motd;

public class MotdWidgetSettings
{
    /// <summary>
    /// "Message of the day" messages.
    /// </summary>
    public ObservableCollection<Motd> Messages { get; set; } = [];
}
