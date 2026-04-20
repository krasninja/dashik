using Avalonia;
using Avalonia.Controls;

namespace Dashik.Shared.Controls;

/// <summary>
/// Widgets panel arranges child items vertically and moves widgets to the right
/// if space available.
/// </summary>
public sealed class WidgetsPanel : Panel
{
    private const int MaxRebalanceIterations = 5;

    private double[] _heights = [];
    private int[] _columnIndexForWidget = [];

    /// <summary>
    /// Defines the <see cref="ItemWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemWidthProperty
        = AvaloniaProperty.Register<WrapPanel, double>(nameof(ItemWidth), double.NaN);

    /// <summary>
    /// Gets or sets the width of all items in the WidgetsPanel.
    /// </summary>
    public double ItemWidth
    {
        get => this.GetValue(ItemWidthProperty);
        set => this.SetValue(ItemWidthProperty, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = ArrangeInternal(finalSize, dryRun: true);
        var xOffset = size.Width < finalSize.Width ? (finalSize.Width - size.Width) / 2 : 0;
        ArrangeInternal(finalSize, xOffset);
        return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        var size = ArrangeInternal(availableSize, dryRun: true);
        return size;
    }

    /// <summary>
    /// Arrange children.
    /// </summary>
    /// <param name="finalSize">Target size (available space).</param>
    /// <param name="startXOffset">Start X position to arrange.</param>
    /// <param name="dryRun">Dry run, do not make the actual arrange.</param>
    /// <returns>Target all widgets' width.</returns>
    private Size ArrangeInternal(Size finalSize, double startXOffset = double.NaN, bool dryRun = false)
    {
        var controlsCount = GetVisibleChildren().Count();
        if (controlsCount < 1)
        {
            return new Size(0, 0);
        }

        var itemWidth = !double.IsNaN(ItemWidth) ? ItemWidth : Children[0].DesiredSize.Width;
        var availableColumns = Math.Max((int)(finalSize.Width / itemWidth), 1);
        var itemsPerColumn = Math.Max(controlsCount / availableColumns, 1);

        var columnIndexForWidget = _columnIndexForWidget.Length != controlsCount
            ? new int[controlsCount]
            : _columnIndexForWidget;
        _columnIndexForWidget = columnIndexForWidget;

        // Initially place controls equally by columns.
        var currentColumn = 0;
        var currentItemIndexInColumn = 0;
        for (var i = 0; i < controlsCount; i++)
        {
            columnIndexForWidget[i] = currentColumn;
            if (currentItemIndexInColumn >= itemsPerColumn)
            {
                currentItemIndexInColumn = 0;
                currentColumn++;
            }
            else
            {
                currentItemIndexInColumn++;
            }
        }

        // Rebalance.
        var heights = _heights.Length != GetTotalColumns(columnIndexForWidget)
            ? new double[GetTotalColumns(columnIndexForWidget)]
            : _heights;
        _heights = heights;
        for (var i = 0; i < MaxRebalanceIterations; i++)
        {
            if (TryMoveAndRollback(columnIndexForWidget, heights, 1))
            {
                continue;
            }
            if (TryMoveAndRollback(columnIndexForWidget, heights, -1))
            {
                continue;
            }
            break;
        }

        // Arrange widgets.
        if (!dryRun)
        {
            double currentYOffset = 0;
            foreach (var (i, child) in GetVisibleChildren().Index())
            {
                // Place widget.
                if (i > 0 && columnIndexForWidget[i - 1] != columnIndexForWidget[i])
                {
                    currentYOffset = 0;
                }
                var controlSize =
                    new Rect(child.DesiredSize)
                        .Translate(
                            new Vector(columnIndexForWidget[i] * itemWidth + startXOffset, currentYOffset));
                currentYOffset += controlSize.Height;
                child.Arrange(controlSize);
            }
        }

        var highestColumn = CalculateHeights(columnIndexForWidget, heights);
        var size = new Size(
            (columnIndexForWidget[^1] + 1) * itemWidth,
            heights[highestColumn]);
        return size;
    }

    private static int GetTotalColumns(int[] columns) => columns[^1] + 1;

    private bool TryMoveAndRollback(int[] columns, double[] heights, int delta)
    {
        if (heights.Length < 2)
        {
            return false;
        }

        // Get the highest column.
        var maxHeightColumn = CalculateHeights(columns, heights);
        var currentMaxHeight = heights[maxHeightColumn];
        var movedColumn = MoveLastWidgetToColumn(columns, maxHeightColumn, delta);

        // Try to move it.
        if (movedColumn < 0)
        {
            return false;
        }

        // If new layout has more height - rollback. Otherwise, keep it.
        maxHeightColumn = CalculateHeights(columns, heights);
        var newMaxHeight = heights[maxHeightColumn];
        if (newMaxHeight >= currentMaxHeight)
        {
            columns[movedColumn] -= delta;
            return false;
        }

        return true;
    }

    private int CalculateHeights(int[] columns, double[] heights)
    {
        Array.Fill(heights, 0);
        var maxHeightColumnIndex = 0;
        var maxHeight = heights[0];

        // Iterate and sum control's height.
        foreach (var (i, child) in GetVisibleChildren().Index())
        {
            var column = columns[i];
            heights[column] += child.DesiredSize.Height;

            // Calc highest column.
            if (heights[column] > maxHeight)
            {
                maxHeight = heights[column];
                maxHeightColumnIndex = column;
            }
        }

        return maxHeightColumnIndex;
    }

    private int MoveLastWidgetToColumn(int[] columns, int column, int delta)
    {
        var index = delta > 0
            ? Array.LastIndexOf(columns, column)
            : Array.IndexOf(columns, column);
        var newColumn = column + delta;
        if (newColumn < 0 || index >= columns.Length - 1)
        {
            return -1;
        }
        columns[index] = newColumn;
        return index;
    }

    private IEnumerable<Control> GetVisibleChildren()
    {
        foreach (var child in Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }
            yield return child;
        }
    }
}
