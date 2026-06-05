using System.Runtime.Serialization;
using ReactiveUI;

namespace Dashik.Sdk.Models;

/// <summary>
/// Widgets main settings. It contains common properties for all widgets.
/// </summary>
[DataContract]
public class WidgetMainSettings : ReactiveObject
{
    /// <summary>
    /// Widget update interval.
    /// </summary>
    [DataMember]
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Widget title. If empty, the default title will be used.
    /// </summary>
    [DataMember]
    public string CustomTitle
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    /// <summary>
    /// Use custom title or default one.
    /// </summary>
    [DataMember]
    public bool UseCustomTitle
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Target widget height.
    /// </summary>
    [DataMember]
    public double Height
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Is widget update disables.
    /// </summary>
    [DataMember]
    public bool Disabled
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Is widget content hidden.
    /// </summary>
    [DataMember]
    public bool Hidden
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Widget's place. If empty - the main is used.
    /// </summary>
    [DataMember]
    public string SpaceId
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    /// <summary>
    /// HTTP or SOCKS proxy.
    /// </summary>
    /// <example>socks5://127.0.0.1:1080.</example>
    /// <example>http://10.1.1.1:8888.</example>
    [DataMember]
    public string WebProxy
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;
}
