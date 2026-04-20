namespace Dashik.Sdk.Models;

/// <summary>
/// Widgets main settings. It contains common properties for all widgets.
/// </summary>
public class WidgetMainSettings : ObservableObject
{
    /// <summary>
    /// Widget update interval.
    /// </summary>
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Widget title. If empty, the default title will be used.
    /// </summary>
    public string CustomTitle
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    /// <summary>
    /// Use custom title or default one.
    /// </summary>
    public bool UseCustomTitle
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Target widget height.
    /// </summary>
    public double Height
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Is widget update disables.
    /// </summary>
    public bool Disabled
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Is widget content hidden.
    /// </summary>
    public bool Hidden
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Widget's place. If empty - the main is used.
    /// </summary>
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
    public string WebProxy
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;
}
