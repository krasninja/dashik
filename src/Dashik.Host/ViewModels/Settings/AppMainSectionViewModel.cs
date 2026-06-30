using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

public class AppMainSectionViewModel : SettingsSectionModel
{
    public AppSettingsViewModel AppSettings => (AppSettingsViewModel)Settings!;
}
