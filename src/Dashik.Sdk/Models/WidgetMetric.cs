using Avalonia.Media;
using QueryCat.Backend.Core.Types;

namespace Dashik.Sdk.Models;

/// <summary>
/// Widget metric metadata.
/// </summary>
public class WidgetMetric
{
    /// <summary>
    /// Unique metric text identifier withing widget.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Metric value type.
    /// </summary>
    public DataType Type { get; }

    /// <summary>
    /// Metric name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Metric description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Custom metric color.
    /// </summary>
    public Color Color { get; }

    public WidgetMetric(string id, DataType type, string name, string? description = null, Color? color = null)
    {
        Id = id;
        Type = type;
        Name = name;
        Description = description ?? string.Empty;
        Color = color ?? Colors.DarkGray;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Id}: {Name}";
}
