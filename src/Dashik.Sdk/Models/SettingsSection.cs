using Avalonia.Controls;
using Avalonia.Media;

namespace Dashik.Sdk.Models;

public sealed class SettingsSection
{
    public string Name { get; }

    public IImage? Icon { get; set; }

    public Type ControlType { get; }

    public Type ViewModelType { get; }

    public SettingsSection(string name, Type controlType, Type viewModelType)
    {
        Name = name;
        ControlType = controlType;
        ViewModelType = viewModelType;
    }

    public static SettingsSection Create<TControl, TViewModel>(string name)
        where TControl : Control
        where TViewModel : SettingsSectionModel
    {
        return new SettingsSection(name, typeof(TControl), typeof(TViewModel));
    }

    public static SettingsSection Create<TControl>(string name)
        where TControl : Control
            => Create<TControl, SettingsSectionModel>(name);
}
