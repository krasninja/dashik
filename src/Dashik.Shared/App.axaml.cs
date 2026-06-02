using System.Collections.Specialized;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
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

    private WidgetsContainerViewModel? ViewModel => (WidgetsContainerViewModel?)DataContext;

    private int _totalWidgetMenuItems;

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
        // UI thread exceptions.
        Dispatcher.UIThread.UnhandledException += (s, e) =>
        {
            e.Handled = true;
        };

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
            containerViewModel.WidgetMenuItems.CollectionChanged += WidgetMenuItemsOnCollectionChanged;
            DataContext = containerViewModel;
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

    private void WidgetMenuItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Current == null)
        {
            return;
        }

        var appTrayIcon = TrayIcon.GetIcons(Current)?.FirstOrDefault();
        if (appTrayIcon == null || appTrayIcon.Menu == null)
        {
            return;
        }

        // For reference: https://github.com/AvaloniaUI/Avalonia/issues/8076.

        // Remove old items.
        foreach (var oldItem in e.OldItems ?? Array.Empty<string>())
        {
            var itemToRemove = appTrayIcon.Menu.Items.FirstOrDefault(i => i == oldItem);
            if (itemToRemove != null)
            {
                if (appTrayIcon.Menu.Items.Remove(itemToRemove))
                {
                    _totalWidgetMenuItems--;
                }
            }
        }

        // Add new items.
        var firstSeparatorBeforeWidgetItems = appTrayIcon.Menu.Items.OfType<NativeMenuItemSeparator>()
            .FirstOrDefault();
        if (firstSeparatorBeforeWidgetItems != null)
        {
            var indexOfFirstSeparatorBeforeWidgetItems = appTrayIcon.Menu.Items.IndexOf(firstSeparatorBeforeWidgetItems);
            if (indexOfFirstSeparatorBeforeWidgetItems > -1)
            {
                foreach (var newItem in e.NewItems ?? Array.Empty<NativeMenuItem>())
                {
                    appTrayIcon.Menu.Items.Insert(
                        _totalWidgetMenuItems + indexOfFirstSeparatorBeforeWidgetItems + 1,
                        (NativeMenuItem)newItem);
                    _totalWidgetMenuItems++;
                }

                // Add separator after all widget menu items.
                if (_totalWidgetMenuItems + indexOfFirstSeparatorBeforeWidgetItems + 1 < appTrayIcon.Menu.Items.Count
                    && appTrayIcon.Menu.Items[_totalWidgetMenuItems + indexOfFirstSeparatorBeforeWidgetItems + 1] is not NativeMenuItemSeparator)
                {
                    appTrayIcon.Menu.Items.Insert(
                        _totalWidgetMenuItems + indexOfFirstSeparatorBeforeWidgetItems + 1,
                        new NativeMenuItemSeparator());
                }
            }
        }

        // Clean up NativeMenuItemSeparator.
        for (var i = 0; i < appTrayIcon.Menu.Items.Count - 1; i++)
        {
            if (appTrayIcon.Menu.Items[i] is NativeMenuItemSeparator
                && appTrayIcon.Menu.Items[i + 1] is NativeMenuItemSeparator)
            {
                appTrayIcon.Menu.Items.RemoveAt(i);
            }
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
            var mvvmService = Root.Container.GetRequiredService<IMvvmService>();
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
        var containerViewModel = Root.Container.GetInstance<WidgetsContainerViewModel>();
        containerViewModel.WidgetMenuItems.CollectionChanged -= WidgetMenuItemsOnCollectionChanged;
        Root.Dispose();
    }
}
