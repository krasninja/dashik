using ReactiveUI;

namespace Dashik.Widgets.Motd;

public class MotdWidgetViewModel : ReactiveObject
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
