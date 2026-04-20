using Avalonia.Controls;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;

namespace Dashik.Widgets.Motd;

[WidgetInfo(
    id: "com.antisoft.widgets.motd",
    name: "Motd",
    Description = "Displays the message of the day.",
    Category = WidgetCategory.Misc,
    InfoType = typeof(MotdWidgetInfo)
)]
public sealed class MotdWidget : IWidget, IWidgetSettings, IWidgetUpdate
{
    private readonly IWidgetContext _context;

    /// <inheritdoc />
    public string Header => "MOTD";

    /// <inheritdoc />
    public Control Control { get; }

    private MotdWidgetViewModel ViewModel => (MotdWidgetViewModel)Control.DataContext!;

    /// <inheritdoc />
    public object Settings { get; set; } = new MotdWidgetSettings();

    /// <inheritdoc />
    public Type SettingsType => typeof(MotdWidgetSettings);

    public MotdWidget(IWidgetContext context)
    {
        _context = context;
        Control = new MotdWidgetControl
        {
            DataContext = new MotdWidgetViewModel(),
        };
    }

    /// <inheritdoc />
    public Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default)
    {
        var settings = (MotdWidgetSettings)Settings;
        if (settings.Messages.Count < 1)
        {
            ViewModel.Motd = "(No messages configured)";
            return Task.CompletedTask;
        }
        var message = settings.Messages[Random.Shared.Next(0, settings.Messages.Count - 1)];
        ViewModel.Motd = message.Text;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IReadOnlyList<SettingsSection> GetSections() =>
    [
        SettingsSection.Create<MotdSettingsSectionControl>("MOTD"),
    ];
}
