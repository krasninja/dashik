namespace Dashik.Shared.Models;

public class MainWindowStateModel
{
    public sealed class WindowPosition
    {
        public int X { get; set; }

        public int Y { get; set; }
    }

    public double WindowHeight { get; set; } = 750;

    public double WindowWidth { get; set; } = 450;

    public Dictionary<string, WindowPosition> WindowPositions { get; set; } = new();

    /// <summary>
    /// Show over all other windows. Always on top.
    /// </summary>
    public bool Topmost { get; set; }

    /// <summary>
    /// Show system tray icon.
    /// </summary>
    public bool ShowSystemTrayIcon { get; set; }

    /// <summary>
    /// Current selected user's space.
    /// </summary>
    public string ActiveSpace { get; set; } = string.Empty;

    /// <summary>
    /// Widgets orders (values are ids) per space (key is spaces id).
    /// </summary>
    public Dictionary<string, string[]> WidgetsOrder { get; set; } = new();
}
