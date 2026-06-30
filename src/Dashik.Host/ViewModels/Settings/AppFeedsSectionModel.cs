using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

public class AppFeedsSectionModel : SettingsSectionModel
{
    public AppSettingsViewModel AppSettings => (AppSettingsViewModel)Settings!;
}
