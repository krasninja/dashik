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

    public bool Topmost { get; set; }

    public string ActiveSpace { get; set; } = string.Empty;

    /// <summary>
    /// Widgets orders (values are ids) per space (key is spaces id).
    /// </summary>
    public Dictionary<string, string[]> WidgetsOrder { get; set; } = new();
}
