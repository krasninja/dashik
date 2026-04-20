using Avalonia.Controls;
using Avalonia.Media;
using Dashik.Shared.Infrastructure.UI;

namespace Dashik.Shared.ViewModels.Settings;

public class SettingsSectionViewModel : ViewModelBase
{
    private readonly Sdk.Models.SettingsSectionModel _controlModel;

    public string Title { get; }

    public IImage? Icon { get; set; }

    public Control Control { get; }

    public Func<object, object?>? SettingsFunc { get; init; }

    /// <inheritdoc />
    public SettingsSectionViewModel(string title, Control control, Sdk.Models.SettingsSectionModel controlModel)
    {
        Title = title;
        Control = control;

        _controlModel = controlModel;
    }

    public void SetSettings(object? settings)
    {
        _controlModel.SyncSetting();

        if (settings == null)
        {
            _controlModel.Settings = null;
            return;
        }
        _controlModel.Settings = SettingsFunc != null ? SettingsFunc.Invoke(settings) : settings;

        if (Control.DataContext == null)
        {
            Control.DataContext = _controlModel;
        }
    }
}
