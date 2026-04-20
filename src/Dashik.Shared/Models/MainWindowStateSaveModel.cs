namespace Dashik.Shared.Models;

/// <summary>
/// Save windows request.
/// </summary>
public class MainWindowStateSaveModel
{
    public double WindowHeight { get; set; } = 750;

    public double WindowWidth { get; set; } = 450;

    public int? WindowPositionX { get; set; }

    public int? WindowPositionY { get; set; }

    public string WindowScreen { get; set; } = string.Empty;

    public bool Topmost { get; set; }

    public string ActiveSpace { get; set; } = string.Empty;

    /// <summary>
    /// Widgets order (by id).
    /// </summary>
    public IReadOnlyDictionary<string, string[]> WidgetsOrder { get; set; } = new Dictionary<string, string[]>();
}
