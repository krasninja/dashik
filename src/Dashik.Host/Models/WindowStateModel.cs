using System.Runtime.Serialization;
using Dashik.Sdk.Utils;

namespace Dashik.Host.Models;

[DataContract]
public class WindowStateModel
{
    [DataContract]
    public sealed class WindowPosition
    {
        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }
    }

    [DataMember]
    public string Id { get; set; } = IdGenerator.Generate();

    [DataMember]
    public double WindowHeight { get; set; } = 750;

    [DataMember]
    public double WindowWidth { get; set; } = 450;

    /// <summary>
    /// Position of the window on the screen. Key - screen (monitor) name.
    /// </summary>
    [DataMember]
    public Dictionary<string, WindowPosition> WindowPositions { get; set; } = new();

    /// <summary>
    /// Show over all other windows. Always on top.
    /// </summary>
    [DataMember]
    public bool Topmost { get; set; }

    /// <summary>
    /// Current selected user's space.
    /// </summary>
    [DataMember]
    public string ActiveSpace { get; set; } = string.Empty;

    /// <summary>
    /// Widgets orders (values are ids) per space (key is spaces id).
    /// </summary>
    [DataMember]
    public Dictionary<string, string[]> WidgetsOrder { get; set; } = new();

    /// <summary>
    /// Window type: window or bar.
    /// </summary>
    [DataMember]
    public WidgetsWindowType Type { get; set; } = WidgetsWindowType.Window;
}
