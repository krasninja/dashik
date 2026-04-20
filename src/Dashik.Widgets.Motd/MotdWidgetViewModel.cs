using Dashik.Sdk.Models;

namespace Dashik.Widgets.Motd;

public class MotdWidgetViewModel : ObservableObject
{
    /// <summary>
    /// Current date MOTD.
    /// </summary>
    public string Motd
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;
}
