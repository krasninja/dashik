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

    /// <summary>
    /// Setting section icon to be rendered in UI.
    /// </summary>
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

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Section name.</param>
    /// <param name="controlType">Avalonia control type to render.</param>
    /// <param name="viewModelType">Control view model type to be attached to control.</param>
    public SettingsSection(string name, Type controlType, Type viewModelType)
    {
        Name = name;
        ControlType = controlType;
        ViewModelType = viewModelType;
    }

    /// <summary>
    /// Create a new instance of <see cref="SettingsSection" />.
    /// </summary>
    /// <param name="name">Section name.</param>
    /// <typeparam name="TControl">Avalonia control type to render.</typeparam>
    /// <typeparam name="TViewModel">Control view model type to be attached to control.</typeparam>
    /// <returns>A new instance of <see cref="SettingsSection" />.</returns>
    public static SettingsSection Create<TControl, TViewModel>(string name)
        where TControl : Control
        where TViewModel : SettingsSectionModel
    {
        return new SettingsSection(name, typeof(TControl), typeof(TViewModel));
    }

    /// <summary>
    /// Create a new instance of <see cref="SettingsSection" />.
    /// </summary>
    /// <param name="name">Section name.</param>
    /// <typeparam name="TControl">Avalonia control type to render.</typeparam>
    /// <returns>A new instance of <see cref="SettingsSection" />.</returns>
    public static SettingsSection Create<TControl>(string name)
        where TControl : Control
            => Create<TControl, SettingsSectionModel>(name);
}
