using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

public class AppMainSectionViewModel : SettingsSectionModel
{
    public AppSettingsObjectViewModel AppSettings => (AppSettingsObjectViewModel)Settings!;
}
