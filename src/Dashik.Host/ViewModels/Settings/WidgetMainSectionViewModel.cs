using Dashik.Host.Models;
using Dashik.Sdk.Models;

namespace Dashik.Host.ViewModels.Settings;

/// <summary>
/// Main widget settings (title, update interval, etc.).
/// </summary>
public class WidgetMainSectionViewModel : SettingsSectionModel
{
    public List<SpaceViewModel> Spaces { get; } = new();

    public WidgetMainSectionViewModel(AppSettings appSettings)
    {
        Spaces.AddRange(appSettings.Spaces.Select(s => new SpaceViewModel(s)));
    }
}
