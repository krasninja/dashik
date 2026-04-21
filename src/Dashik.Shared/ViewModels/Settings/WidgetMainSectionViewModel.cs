using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

/// <summary>
/// Main widget settings (title, update interval, etc.).
/// </summary>
public class WidgetMainSectionViewModel : SettingsSectionModel
{
    public WidgetMainSettings? MainSettings => Settings as WidgetMainSettings;
}
