using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.ViewModels;
using Dashik.Host.ViewModels.Settings;
using Dashik.Host.Views;
using Dashik.Host.Views.Settings;
using Dashik.Sdk.ViewModels;
using Dashik.Sdk.Views;

namespace Dashik.Host;

public sealed class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? data)
    {
        return data switch
        {
            WidgetsContainerViewModel vm => new WidgetsContainerWindow { DataContext = vm },
            WidgetsBarViewModel vm => new WidgetsBarWindow { DataContext = vm },
            WidgetsManagementViewModel vm => new WidgetsManagementWindow { DataContext = vm },
            SettingsViewModel vm => new SettingsWindow { DataContext = vm },
            LogsViewModel vm => new LogsWindow { DataContext = vm },
            TextWindowViewModel vm => new TextWindow { DataContext = vm },
            MessageBoxViewModel vm => new MessageBoxWindow { DataContext = vm },
            MainViewModel vm => new MainWindow { DataContext = vm },
            _ => new TextBlock
            {
                Text = $"View not found for {data?.GetType().Name}.",
                Foreground = Brushes.Red,
            }
        };
    }

    /// <inheritdoc />
    public bool Match(object? data) => data is ViewModelBase;
}
