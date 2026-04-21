using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class AppFeedsSectionModel : SettingsSectionModel
{
    public AppSettingsViewModel AppSettings => (AppSettingsViewModel)Settings!;
}
