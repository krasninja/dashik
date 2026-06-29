using Avalonia;
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
public sealed class MotdWidget : IWidget, IWidgetSettings, IWidgetPreview, IWidgetUpdate, IWidgetMenu
{
    private readonly IWidgetContext _context;

    /// <inheritdoc />
    public string Header => "MOTD";

    /// <inheritdoc />
    public Control Control { get; }

    private MotdWidgetViewModel ViewModel { get; } = new();

    /// <inheritdoc />
    public object Settings { get; set; } = new MotdWidgetSettings();

    /// <inheritdoc />
    public Type SettingsType => typeof(MotdWidgetSettings);

    public MotdWidget(IWidgetContext context)
    {
        _context = context;
        Control = new MotdWidgetControl
        {
            DataContext = ViewModel,
        };
    }

    /// <inheritdoc />
    public Control? CreateControl(WidgetControlTarget target, Size targetSize)
    {
        if (target == WidgetControlTarget.Panel)
        {
            return new MotdWidgetControl
            {
                DataContext = ViewModel,
            };
        }
        return null;
    }

    /// <inheritdoc />
    public Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default)
    {
        SetMotd();
        return Task.CompletedTask;
    }

    private void SetMotd()
    {
        var settings = (MotdWidgetSettings)Settings;
        if (settings.Messages.Count < 1)
        {
            ViewModel.Motd = "(No messages configured)";
            return;
        }
        var message = settings.Messages[Random.Shared.Next(0, settings.Messages.Count)];
        ViewModel.Motd = message.Text;
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

    /// <inheritdoc />
    public IReadOnlyList<WidgetPreview> GetPreviewConfigurations()
    {
        return
        [
            new WidgetPreview("MOTD message")
            {
                Settings = new MotdWidgetSettings
                {
                    Messages =
                    [
                        new Motd("Welcome to Dashik!"),
                    ],
                }
            },
            new WidgetPreview("Another MOTD message")
            {
                Settings = new MotdWidgetSettings
                {
                    Messages =
                    [
                        new Motd("Hello, world!"),
                        new Motd("Have a nice day!"),
                    ],
                }
            }
        ];
    }

    /// <inheritdoc />
    public void SetPreview(WidgetPreview widgetPreview)
    {
        var preview = (MotdWidgetSettings)widgetPreview.Settings!;
        ViewModel.Motd = preview.Messages[0].Text;
    }

    /// <inheritdoc />
    public IReadOnlyList<MenuItem> GetWidgetMenuItems()
    {
        var mi = new MenuItem
        {
            Header = "New MOTD",
        };
        mi.Click += (sender, args) =>
        {
            SetMotd();
        };
        return
        [
            mi,
        ];
    }
}
