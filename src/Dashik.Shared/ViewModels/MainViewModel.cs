using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;
using Dashik.Shared.Infrastructure.Setup;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services;
using Dashik.Shared.Utils;

namespace Dashik.Shared.ViewModels;

public sealed class MainViewModel : ViewModelBase, IDisposable
{
    public WidgetsCollectionViewModel WidgetsCollectionViewModel { get; }

    public ReactiveCommand<Unit, Unit> ShowTrayIconCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseApplicationCommand { get; }

    public AvaloniaList<NativeMenuItem> WidgetMenuItems { get; } = new();

    public Dictionary<WidgetsBaseViewModel, Window> WidgetsWindows { get; } = new();

    public bool ShowSystemTrayIcon
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private readonly IMvvmService _mvvmService;
    private readonly AppSettings _appSettings;
    private readonly SettingsStorage _settingsStorage;
    private readonly ILogger _logger;
    private int _totalWidgetMenuItems;

    public MainViewModel(
        IMvvmService mvvmService,
        AppArguments appArguments,
        AppSettings appSettings,
        SettingsStorage settingsStorage,
        WidgetsCollectionViewModel widgetsCollectionViewModel,
        ILogger<MainViewModel> logger)
    {
        _mvvmService = mvvmService;
        _appSettings = appSettings;
        _settingsStorage = settingsStorage;
        _logger = logger;

        WidgetsCollectionViewModel = widgetsCollectionViewModel;
        WidgetsCollectionViewModel.WidgetFilter = appArguments.WidgetsFilter.ToArray();
        WidgetMenuItems.CollectionChanged += WidgetMenuItemsOnCollectionChanged;

        ShowTrayIconCommand = ReactiveCommand.Create(ShowTrayIcon);
        ShowSystemTrayIcon = _appSettings.ShowSystemTrayIcon;
        CloseApplicationCommand = ReactiveCommand.Create(CloseApplicationWindow);
    }

    private void ShowTrayIcon()
    {
    }

    private void CloseApplicationWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            desktopApp.Shutdown();
        }
    }

    #region Loading

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        await base.LoadAsync(cancellationToken);
        await WidgetsCollectionViewModel.LoadAsync(cancellationToken);
        await LoadWindowsAsync(cancellationToken);
        LoadSystemTrayMenuItems();

        stopwatch.Stop();
        _logger.LogTrace("App state loaded in {ElapsedMilliseconds} ms.", stopwatch.ElapsedMilliseconds);
    }

    private async Task LoadWindowsAsync(CancellationToken cancellationToken = default)
    {
        var windows = await _settingsStorage.LoadWindowsStatesAsync(cancellationToken);

        foreach (var window in windows)
        {
            try
            {
                var vm = CreateWidgetsWindow(window);
                await vm.LoadUiStateAsync(window, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical("Cannot load window view model: {Message}.", exception.Message);
                var messageBoxVm = new MessageBoxViewModel(exception.Message, "Error").SetErrorMode();
                await _mvvmService.OpenAsync(messageBoxVm, cancellationToken);
            }
        }

        // If no windows at all - create main one.
        if (WidgetsWindows.Count < 1)
        {
            var window = new WindowStateModel();
            if (WidgetsCollectionViewModel.Spaces.Count > 0)
            {
                window.ActiveSpace = WidgetsCollectionViewModel.Spaces[0].Id;
            }
            var vm = CreateWidgetsWindow(window);
            await vm.LoadUiStateAsync(window, cancellationToken);
            await vm.SaveUiStateAsync(cancellationToken);
        }
    }

    private WidgetsBaseViewModel CreateWidgetsWindow(WindowStateModel windowModel)
    {
        WidgetsBaseViewModel? vm = null;

        if (windowModel.Type == WidgetsWindowType.Bar)
        {
            vm = _mvvmService.CreateViewModel<WidgetsBarViewModel>();
        }
        else if (windowModel.Type == WidgetsWindowType.Window)
        {
            vm = _mvvmService.CreateViewModel<WidgetsContainerViewModel>();
        }

        if (vm == null)
        {
            throw new InvalidOperationException($"Cannot create view model of type '{windowModel.Type}'.");
        }
        vm.WidgetsViewModel = WidgetsCollectionViewModel;

        var control = _mvvmService.FindControlByViewModel(vm);
        if (control == null)
        {
            throw new InvalidOperationException($"Cannot create window of type '{windowModel.Type}'.");
        }

        control.DataContext = vm;
        if (control is not Window window)
        {
            throw new InvalidOperationException($"Cannot create window of type '{windowModel.Type}'.");
        }
        window.Closed += WindowOnClosed;
        WidgetsWindows[vm] = window;
        window.Show();
        return vm;
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window
            || window.DataContext is not WidgetsBaseViewModel vm)
        {
            return;
        }

        WidgetsWindows.Remove(vm);
        (vm as IDisposable)?.Dispose();

        var closeApp = true;
        foreach (var widgetsWindow in WidgetsWindows)
        {
            if (widgetsWindow.Value.IsVisible)
            {
                closeApp = false;
                break;
            }
        }
        if (closeApp)
        {
            CloseApplicationWindow();
        }
    }

    private void LoadSystemTrayMenuItems()
    {
        WidgetMenuItems.Clear();
        foreach (var widgetViewModel in WidgetsCollectionViewModel.ActiveWidgets)
        {
            if (widgetViewModel.Widget != null && widgetViewModel.Widget is IWidgetTrayMenu widgetTrayMenu)
            {
                var rootMenuItem = new NativeMenuItem
                {
                    Header = widgetViewModel.Title,
                };
                var rootSubMenu = new NativeMenu();
                if (widgetViewModel.WidgetInstance?.Info.Icon is Bitmap bitmap)
                {
                    rootMenuItem.Icon = bitmap;
                }

                rootSubMenu.Items.AddRange(widgetTrayMenu.TrayMenuItems);
                widgetTrayMenu.TrayMenuItems.CollectionChanged -= TrayMenuItemsOnCollectionChanged;
                widgetTrayMenu.TrayMenuItems.CollectionChanged += TrayMenuItemsOnCollectionChanged;

                void TrayMenuItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                {
                    CollectionUtils.SyncFromChangedEventArg(e, rootSubMenu.Items);
                }

                rootMenuItem.Menu = rootSubMenu;
                WidgetMenuItems.Add(rootMenuItem);
            }
        }
    }

    #endregion

    private void WidgetMenuItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (Application.Current == null)
        {
            return;
        }

        var appTrayIcon = TrayIcon.GetIcons(Application.Current)?.FirstOrDefault();
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

    public void Hide()
    {
        foreach (var widgetsWindow in WidgetsWindows.Values)
        {
            widgetsWindow.Hide();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        WidgetMenuItems.CollectionChanged -= WidgetMenuItemsOnCollectionChanged;
        Hide();
        foreach (var widgetsWindow in WidgetsWindows)
        {
            widgetsWindow.Value.Closed -= WindowOnClosed;
            widgetsWindow.Value.Close();
            (widgetsWindow.Key as IDisposable)?.Dispose();
        }
        WidgetsWindows.Clear();
    }
}
