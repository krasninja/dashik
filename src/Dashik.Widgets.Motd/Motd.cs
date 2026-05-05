using ReactiveUI;

namespace Dashik.Widgets.Motd;

/// <summary>
/// MOTD object.
/// </summary>
public sealed class Motd : ReactiveObject
{
    /// <summary>
    /// Message text.
    /// </summary>
    public string Text
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    public Motd()
    {
    }

    public Motd(string text)
    {
        Text = text;
    }

    /// <inheritdoc />
    public override string ToString() => Text;
}
