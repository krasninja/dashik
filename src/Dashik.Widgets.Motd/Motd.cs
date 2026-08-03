using System.Runtime.Serialization;
using ReactiveUI;

namespace Dashik.Widgets.Motd;

/// <summary>
/// MOTD object.
/// </summary>
[DataContract]
public sealed class Motd : ReactiveObject
{
    /// <summary>
    /// Message text.
    /// </summary>
    [DataMember]
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
