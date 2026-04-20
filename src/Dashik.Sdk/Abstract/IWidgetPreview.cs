using Dashik.Sdk.Models;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Allows to set up widget for demo mode. It is used in preview window in
/// "Add Widget" dialog.
/// </summary>
public interface IWidgetPreview
{
    /// <summary>
    /// Get all available demo modes.
    /// </summary>
    /// <returns>Demo modes.</returns>
    IReadOnlyList<WidgetPreview> GetPreviewConfigurations();

    /// <summary>
    /// Set widget preview.
    /// </summary>
    /// <param name="widgetPreview">Widget preview.</param>
    void SetPreview(WidgetPreview widgetPreview);
}
