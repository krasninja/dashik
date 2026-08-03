namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget that supports plain text output.
/// </summary>
public interface IWidgetText
{
    /// <summary>
    /// Get widget content as plain text.
    /// </summary>
    /// <param name="mode">Output mode.</param>
    /// <returns>Widget content.</returns>
    string GetText(WidgetTextMode mode);
}
