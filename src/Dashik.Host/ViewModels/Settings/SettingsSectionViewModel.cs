using Avalonia.Controls;
using Avalonia.Media;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;

namespace Dashik.Host.ViewModels.Settings;

/// <summary>
/// Settings section.
/// </summary>
public class SettingsSectionViewModel : ViewModelBase
{
    /// <summary>
    /// View-model for Control.
    /// </summary>
    private readonly Sdk.Models.SettingsSectionModel _controlModel;

    public string Title { get; }

    public IImage? Icon { get; set; }

    public Control Control { get; }

    public ISectionSettingsConverter SettingsConverter { get; } = EmptySectionSettingsConverter.Instance;

    public object? ControlModelSettings => _controlModel.Settings;

    public sealed class EmptySectionSettingsConverter : ISectionSettingsConverter
    {
        public static EmptySectionSettingsConverter Instance { get; } = new();

        /// <inheritdoc />
        public object Convert(object generalSettings) => generalSettings;

        /// <inheritdoc />
        public object ConvertBack(object generalSettings, object sectionSettings) => sectionSettings;
    }

    /// <inheritdoc />
    public SettingsSectionViewModel(
        string title,
        Control control,
        Sdk.Models.SettingsSectionModel controlModel,
        ISectionSettingsConverter? settingsConverter = null)
    {
        Title = title;
        Control = control;
        _controlModel = controlModel;
        if (settingsConverter != null)
        {
            SettingsConverter = settingsConverter;
        }
    }

    /// <summary>
    /// Set new settings to the section.
    /// </summary>
    /// <param name="settings">Settings object.</param>
    public void SetSettings(object? settings)
    {
        var resolvedSettings = settings != null
            ? SettingsConverter.Convert(settings)
            : settings;
        if (resolvedSettings != null)
        {
            _controlModel.Settings = null;
            _controlModel.Settings = resolvedSettings;
        }

        if (Control.DataContext == null)
        {
            Control.DataContext = _controlModel;
        }
    }
}
