namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget control target type. The place where it will be placed.
/// </summary>
public enum WidgetControlTarget
{
    /// <summary>
    /// Panel with all content within it.
    /// </summary>
    Panel,

    /// <summary>
    /// The horizontal bar target. The control horizontal size might be larger than vertical.
    /// </summary>
    HorizontalBar,

    /// <summary>
    /// The vertical bar target. The control vertical size might be larger than horizontal.
    /// </summary>
    VerticalBar,

    /// <summary>
    /// The control has equal height and width.
    /// </summary>
    Box,
}
