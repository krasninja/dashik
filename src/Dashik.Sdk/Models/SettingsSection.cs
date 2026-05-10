using Avalonia.Controls;
using Avalonia.Media;

namespace Dashik.Sdk.Models;

/// <summary>
/// Section to be added to widget settings dialog.
/// </summary>
public sealed class SettingsSection
{
    /// <summary>
    /// Section name.
    /// </summary>
    public string Name { get; }

    public IImage? Icon { get; set; }

    /// <summary>
    /// Avalonia control to render.
    /// </summary>
    public Type ControlType { get; }

    /// <summary>
    /// Control view model to be attached to control.
    /// Should inherit <see cref="SettingsSectionModel" />.
    /// </summary>
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
