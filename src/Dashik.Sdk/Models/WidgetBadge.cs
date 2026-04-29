using Avalonia.Media;
using ReactiveUI;

namespace Dashik.Sdk.Models;

/// <summary>
/// Widget badge.
/// </summary>
public class WidgetBadge : ReactiveObject
{
    public string Name
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    public int Value
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Color Color
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = Colors.DarkGray;

    public WidgetBadge()
    {
    }

    public WidgetBadge(string name, int value)
    {
        Name = name;
        Value = value;
    }
}
