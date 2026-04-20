using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dashik.Sdk.ViewModels;
using Dashik.Sdk.Views;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.ViewModels;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Views;
using Dashik.Shared.Views.Settings;

namespace Dashik.Shared;

public sealed class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public Control Build(object? data)
    {
        return data switch
        {
            WidgetsContainerViewModel vm => new WidgetsContainerWindow { DataContext = vm },
            WidgetsManagementViewModel vm => new WidgetsManagementWindow { DataContext = vm },
            SettingsViewModel vm => new SettingsWindow { DataContext = vm },
            LogsViewModel vm => new LogsWindow { DataContext = vm },
            TextWindowViewModel vm => new TextWindow { DataContext = vm },
            MessageBoxViewModel vm => new MessageBoxWindow { DataContext = vm },
            _ => new TextBlock
            {
                Text = $"View not found for {data?.GetType().Name}",
            }
        };
    }

    /// <inheritdoc />
    public bool Match(object? data) => data is ViewModelBase;
}
