using Dashik.Sdk.Models;

namespace Dashik.Sdk.Abstract;

/// <summary>
/// Widget that supports user-settings.
/// </summary>
public interface IWidgetSettings
{
    /// <summary>
    /// Settings object.
    /// </summary>
    object Settings { get; }

    /// <summary>
    /// Settings type.
    /// </summary>
    Type SettingsType { get; }

    /// <summary>
    /// Get settings sections.
    /// </summary>
    IReadOnlyList<SettingsSection> GetSections();
}
