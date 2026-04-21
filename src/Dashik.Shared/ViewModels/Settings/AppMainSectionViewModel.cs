using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class AppMainSectionViewModel : SettingsSectionModel
{
    public AppSettingsViewModel AppSettings => (AppSettingsViewModel)Settings!;
}
