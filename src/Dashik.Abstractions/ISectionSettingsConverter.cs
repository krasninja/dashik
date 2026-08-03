namespace Dashik.Abstractions;

/// <summary>
/// Settings converter. In some cases, we can have a settings object, but we want to expose
/// only part of it for a specific section.
/// </summary>
public interface ISectionSettingsConverter
{
    /// <summary>
    /// Get section settings by general settings.
    /// </summary>
    /// <param name="generalSettings">General settings object.</param>
    /// <returns>Section settings object.</returns>
    object Convert(object generalSettings);

    /// <summary>
    /// Apply sections settings to general settings and return it.
    /// </summary>
    /// <param name="generalSettings">General settings.</param>
    /// <param name="sectionSettings">Section settings.</param>
    /// <returns>General settings with applied section settings.</returns>
    object ConvertBack(object generalSettings, object sectionSettings);
}
