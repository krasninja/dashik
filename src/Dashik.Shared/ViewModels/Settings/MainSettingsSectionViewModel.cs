using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

/// <summary>
/// Main widget settings (title, update interval, etc.).
/// </summary>
public class MainSettingsSectionViewModel : SettingsSectionModel
{
    public WidgetMainSettings? MainSettings => Settings as WidgetMainSettings;
}
