using System.Reflection;
using SimpleInjector;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using QueryCat.Backend.Core.Utils;
using QueryCat.Backend.Parser;
using Dashik.Host.Infrastructure.Setup;
using Dashik.Host.ViewModels;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Host;

/// <summary>
/// Avalonia <see cref="Application" /> class.
/// </summary>
public sealed partial class App : Application, IDisposable
{
    internal AppRoot Root { get; }

    public Container Container => Root.Container;

    private MainViewModel? ViewModel => (MainViewModel?)DataContext;

    public App() : this(new AppRoot(new AppArguments()))
    {
    }

    public App(AppRoot appRoot)
    {
        Root = appRoot;
        Dispatcher.UIThread.UnhandledException += UIThreadOnUnhandledException;
        Name = "Dashik Application";
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // UI thread exceptions.
        Dispatcher.UIThread.UnhandledException += (s, e) =>
        {
            e.Handled = true;
        };

        AsyncUtils.RunSync(async ct =>
        {
            await OnFrameworkInitializationCompleteInternalAsync(ct);
        });
    }

    private async Task OnFrameworkInitializationCompleteInternalAsync(CancellationToken cancellationToken = default)
    {
        await Root.InitializeAsync(cancellationToken);

        var mainWindowViewModel = Container.GetInstance<MainViewModel>();
        mainWindowViewModel.WidgetsCollectionViewModel.WidgetFilter = Root.AppArguments.WidgetsFilter.ToArray();
        if (!Root.AppArguments.HeadlessMode)
        {
            await mainWindowViewModel.LoadAsync(cancellationToken);
        }

        // Create fake main window but do not assign it to the app.
        // There might be several windows opened and they are managed thru this fake MainWindow.
        DataContext = mainWindowViewModel;

        if ((Root.AppArguments.Mode == AppArguments.RunMode.Client
             || Root.AppArguments.Mode == AppArguments.RunMode.Normal)
            && ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.Exit += (_, _) =>
            {
                if (ViewModel == null)
                {
                    return;
                }
                ViewModel.Hide();
                ViewModel.Dispose();
            };
        }
    }

    /// <summary>
    /// Show exception in message box.
    /// </summary>
    private void UIThreadOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ex = e.Exception;
        if (ex is TargetInvocationException { InnerException: { } } targetInvocationException
            && targetInvocationException.InnerException != null)
        {
            ex = targetInvocationException.InnerException;
        }

        string message;
        if (ex is SyntaxException syntaxException)
        {
            message = syntaxException.GetErrorLine();
            message += string.Format("{0}:{1}: {2}", syntaxException.Line, syntaxException.Position,
                syntaxException.Message);
        }
        else
        {
            message = ex.Message;
        }

        Console.Error.WriteLine(message);
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var messageBoxVm = new MessageBoxViewModel(message, "Error").SetErrorMode();
            var mvvmService = Container.GetRequiredService<IMvvmService>();
            await mvvmService.OpenAsync(messageBoxVm);
        });
    }

    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            var mainWindow = desktop.MainWindow;
            mainWindow.Show();
            mainWindow.Activate();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispatcher.UIThread.UnhandledException -= UIThreadOnUnhandledException;
        ViewModel?.Dispose();
        Root.Dispose();
    }
}
