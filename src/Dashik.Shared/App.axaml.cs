using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using QueryCat.Backend.Core.Utils;
using QueryCat.Backend.Parser;
using Dashik.Shared.Infrastructure.Setup;
using Dashik.Shared.ViewModels;
using Dashik.Shared.Views;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Shared;

/// <summary>
/// Avalonia <see cref="Application" /> class.
/// </summary>
public sealed partial class App : Application, IDisposable
{
    internal AppRoot Root { get; }

    public App() : this(new AppRoot(new AppArguments()))
    {
    }

    internal App(AppRoot appRoot)
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
        AsyncUtils.RunSync(async ct =>
        {
            await Root.InitializeAsync(ct);
        });

        if ((Root.AppArguments.Mode == AppArguments.RunMode.Client
             || Root.AppArguments.Mode == AppArguments.RunMode.Normal)
            && ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var containerViewModel = Root.Container.GetInstance<WidgetsContainerViewModel>();
            containerViewModel.WidgetFilter = Root.AppArguments.WidgetsFilter.ToArray();
            desktop.MainWindow = new WidgetsContainerWindow
            {
                DataContext = containerViewModel,
            };
            desktop.Exit += (_, _) =>
            {
                (desktop.MainWindow?.DataContext as IDisposable)?.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
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
            var mvvmService = Root.Container.GetRequiredService<IMvvmService>();
            await mvvmService.OpenAsync(messageBoxVm);
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Root.Dispose();
    }
}
