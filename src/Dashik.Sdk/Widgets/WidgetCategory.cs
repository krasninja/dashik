using Dashik.Sdk.Utils;

namespace Dashik.Sdk.Widgets;

/// <summary>
/// Available widget categories.
/// </summary>
public enum WidgetCategory
{
    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Misc_Description))]
    Misc,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Accessibility_Description))]
    Accessibility,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.ApplicationLaunchers_Description))]
    ApplicationLaunchers,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Clipboard_Description))]
    Clipboard,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.DateTime_Description))]
    DateTime,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.EnvironmentWeather_Description))]
    EnvironmentWeather,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.FileSystem_Description))]
    FileSystem,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.FunGames_Description))]
    FunGames,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Graphics_Description))]
    Graphics,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Language_Description))]
    Language,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Multimedia_Description))]
    Multimedia,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.OnlineServices_Description))]
    OnlineServices,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.SystemInformation_Description))]
    SystemInformation,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Productivity_Description))]
    Productivity,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.SoftwareDevelopment_Description))]
    SoftwareDevelopment,

    [ResourceDescription(typeof(Resources.Categories), nameof(Resources.Categories.Utilities_Description))]
    Utilities = 100,
}
