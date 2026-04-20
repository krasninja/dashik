using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Shared.Infrastructure.UI;

/// <summary>
/// Avalonia main window implementation.
/// </summary>
public sealed class AvaloniaMvvmService : IMvvmService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataTemplate _dataTemplate;

    public AvaloniaMvvmService(IServiceProvider serviceProvider, IDataTemplate dataTemplate)
    {
        _serviceProvider = serviceProvider;
        _dataTemplate = dataTemplate;
    }

    /// <inheritdoc />
    public Control? FindControlByViewModel(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var dataTemplatesHost = (IDataTemplateHost)Application.Current!;
        if (!dataTemplatesHost.IsDataTemplatesInitialized)
        {
            return null;
        }
        foreach (var dataTemplate in dataTemplatesHost.DataTemplates)
        {
            if (dataTemplate.Match(viewModel))
            {
                return dataTemplate.Build(viewModel);
            }
        }
        return null;
    }

    /// <inheritdoc />
    public Window? GetMainWindow()
    {
        if (Application.Current == null)
        {
            return null;
        }
        var mainWindow = Application.Current.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime classic => classic.MainWindow,
            _ => throw new InvalidOperationException("Unsupported application lifetime.")
        };
        if (mainWindow == null)
        {
            return null;
        }
        // We cannot create dialog for non-visible window,
        // so we wait for it.
        while (!mainWindow.IsVisible)
        {
            Thread.Sleep(90);
        }
        return mainWindow;
    }

    /// <inheritdoc />
    public object CreateViewModel(Type type, params object[] parameters)
        => ActivatorUtilities.CreateInstance(_serviceProvider, type, parameters);

    /// <inheritdoc />
    public async Task OpenAsync(object viewModel, CancellationToken cancellationToken = default)
    {
        var control = _dataTemplate.Build(viewModel) as Window;
        if (control == null)
        {
            throw new InvalidOperationException($"The data template '{viewModel.GetType().FullName}' was not found.");
        }
        var mainWindow = GetMainWindow();

        if (viewModel is ViewModelBase viewModelBase)
        {
            await viewModelBase.LoadAsync(cancellationToken);
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            if (mainWindow != null)
            {
                control.Show(mainWindow);
            }
            else
            {
                control.Show();
            }
        });
    }

    /// <inheritdoc />
    public async Task<DialogResult> OpenAsync<TDialogResult>(IDialogViewModel<TDialogResult> viewModel, CancellationToken cancellationToken = default)
    {
        var control = _dataTemplate.Build(viewModel) as Window;
        if (control == null)
        {
            throw new InvalidOperationException($"The data template '{viewModel.GetType().FullName}' was not found.");
        }
        var mainWindow = GetMainWindow();

        try
        {
            if (viewModel is ViewModelBase viewModelBase)
            {
                await viewModelBase.LoadAsync(cancellationToken);
            }
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (mainWindow != null)
                {
                    await control.ShowDialog<TDialogResult>(mainWindow);
                }
                else
                {
                    control.Show();
                }
            });
        }
        catch (Exception e)
        {
            var messageBoxVm = new MessageBoxViewModel(e.Message, "Error").SetErrorMode();
            await OpenAsync(messageBoxVm, cancellationToken);
            return DialogResult.Abort;
        }

        return viewModel.Result;
    }
}
