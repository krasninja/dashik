using Avalonia.Controls;
using Avalonia.Interactivity;
using Dashik.Sdk.Models;

namespace Dashik.Widgets.Motd;

public partial class MotdSettingsSectionControl : UserControl
{
    public MotdWidgetSettings Settings => (MotdWidgetSettings)((SettingsSectionModel)DataContext!).Settings!;

    public MotdSettingsSectionControl()
    {
        InitializeComponent();
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender == null)
        {
            return;
        }
        var msg = (Motd)((Button)sender).Tag!;
        Settings.Messages.Remove(msg);
    }

    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Settings.Messages.Add(new Motd());
    }
}
