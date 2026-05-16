using Dashik.Sdk.Models;
using Dashik.Shared.Models;

namespace Dashik.Shared.ViewModels.Settings;

/// <summary>
/// Main widget settings (title, update interval, etc.).
/// </summary>
public class WidgetMainSectionViewModel : SettingsSectionModel
{
    public List<SpaceViewModel> Spaces { get; } = new();

    public WidgetMainSettings? MainSettings => Settings as WidgetMainSettings;

    public WidgetMainSectionViewModel(AppSettings appSettings)
    {
        Spaces.AddRange(appSettings.Spaces.Select(s => new SpaceViewModel(s)));
    }
}
