using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Avalonia;
using Avalonia.Collections;
using ReactiveUI;
using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Shared.Infrastructure;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services;
using Dashik.Shared.Services.Widgets;
using Dashik.Shared.Utils;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Views.Settings;
using Dashik.Sdk;
using Dashik.Sdk.Models;
using Dashik.Sdk.ViewModels;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetsContainerViewModel : ViewModelBase, ICloseableViewModel, IDisposable
{
    private readonly AppSettings _appSettings;
    private readonly SettingsStorage _settingsStorage;
    private readonly IWidgetsFactory _widgetsFactory;
    private readonly IWidgetInstanceProvider _widgetInstanceProvider;
    private readonly IMvvmService _mvvmService;
    private readonly ILogger<WidgetsContainerViewModel> _logger;
    private readonly ParallelDispatcher _dispatcher = new(maxDegreeOfParallelism: 2);

    private readonly IDisposable _widgetsUpdateTimerObservable;
    private readonly IDisposable _saveUiObservable;
    private readonly IDisposable _widgetsSaveObservable;
    private bool _updating;

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    /// <summary>
    /// Widgets collection.
    /// </summary>
    public IEnumerable<WidgetViewModel> Widgets => Spaces.SelectMany(s => s.Widgets);

    private WindowState _windowState;

    public WindowState WindowState
    {
        get => _windowState;
        set => this.RaiseAndSetIfChanged(ref _windowState, value);
    }

    private double _windowHeight = 750;

    public double WindowHeight
    {
        get => _windowHeight;
        set => this.RaiseAndSetIfChanged(ref _windowHeight, value);
    }

    private double _windowWidth = 450;

    public double WindowWidth
    {
        get => _windowWidth;
        set => this.RaiseAndSetIfChanged(ref _windowWidth, value);
    }

    public PixelPoint WindowPosition
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string WindowScreen
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    public bool Topmost
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public AvaloniaList<SpaceViewModel> Spaces { get; }

    public SpaceViewModel? SelectedSpace
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string[] WidgetFilter { get; set; } = [];

    public AppUpdateViewModel UpdateInfo { get; }

    public ReactiveCommand<Unit, Unit> AddWidgetCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseApplicationCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenLogsCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenFontsCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenWebsiteCommand { get; }

    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

    public ReactiveCommand<Unit, Unit> CheckUpdatesCommand { get; }

    public ReactiveCommand<SpaceViewModel, Unit> SwitchSpaceCommand { get; }

    private readonly Subject<string> _widgetSave = new();

    private sealed class WidgetsIdComparer : IComparer<WidgetViewModel>
    {
        private readonly string[] _order;

        public WidgetsIdComparer(string[] order)
        {
            _order = order;
        }

        /// <inheritdoc />
        public int Compare(WidgetViewModel? x, WidgetViewModel? y)
        {
            if (x == null || y == null)
            {
                return 1;
            }

            var posX = Array.IndexOf(_order, x.WidgetId);
            var posY = Array.IndexOf(_order, y.WidgetId);
            return posX > posY ? 0 : -1;
        }
    }

#pragma warning disable CS8618
    internal WidgetsContainerViewModel()
#pragma warning restore CS8618
    {
        Spaces = [];

        _saveUiObservable = this
            .WhenAnyValue(
                p => p.WindowState,
                p => p.WindowHeight,
                p => p.WindowWidth,
                p => p.WindowPosition,
                p => p.Topmost,
                p => p.SelectedSpace)
            .Where(_ => !Loading)
            .DelaySubscription(TimeSpan.FromSeconds(5))
            .Throttle(TimeSpan.FromSeconds(3))
            .Subscribe(_ => RxSchedulers.MainThreadScheduler.ScheduleAsync(SaveUiState));

        _widgetsUpdateTimerObservable = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ => RxSchedulers.MainThreadScheduler.ScheduleAsync(UpdateWidgets));

        _widgetsSaveObservable = _widgetSave
            .GroupBy(widgetId => widgetId)
            .SelectMany(group => group.Throttle(TimeSpan.FromSeconds(2)))
            .SubscribeAsync(async (widgetId, ct) =>
            {
                if (_widgetInstanceProvider != null
                    && TryGetWidgetById(widgetId, out var widget)
                    && widget != null
                    && widget.WidgetInstance != null)
                {
                    await _widgetInstanceProvider.SaveAsync(widget.WidgetInstance, ct);
                }
            });
    }

    public WidgetsContainerViewModel(
        AppSettings appSettings,
        SettingsStorage settingsStorage,
        AppUpdateViewModel updateInfo,
        IWidgetsFactory widgetsFactory,
        IWidgetInstanceProvider widgetInstanceProvider,
        IMvvmService mvvmService,
        ILogger<WidgetsContainerViewModel> logger)
        : this()
    {
        _appSettings = appSettings;
        _settingsStorage = settingsStorage;
        _widgetsFactory = widgetsFactory;
        _widgetInstanceProvider = widgetInstanceProvider;
        _mvvmService = mvvmService;
        _logger = logger;
        UpdateInfo = updateInfo;

        AddWidgetCommand = ReactiveCommand.CreateFromTask(AddWidgetWindow);
        OpenSettingsCommand = ReactiveCommand.CreateFromTask(OpenSettingsWindow);
        CloseApplicationCommand = ReactiveCommand.Create(CloseApplicationWindow);
        OpenAboutCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
        OpenLogsCommand = ReactiveCommand.CreateFromTask(OpenLogsWindow);
        OpenFontsCommand = ReactiveCommand.CreateFromTask(OpenFontsWindow);
        OpenWebsiteCommand = ReactiveCommand.CreateFromTask(OpenWebsite);
        UpdateCommand = ReactiveCommand.CreateFromTask(UpdateInfo.UpdateAsync);
        CheckUpdatesCommand = ReactiveCommand.CreateFromTask(CheckUpdates);
        SwitchSpaceCommand = ReactiveCommand.Create<SpaceViewModel>(SwitchSpace);

        _dispatcher.Start();
    }

    private Task UpdateWidgets(IScheduler scheduler, CancellationToken cancellationToken)
    {
        if (_updating)
        {
            return Task.CompletedTask;
        }

        try
        {
            _updating = true;
            foreach (var widget in Widgets)
            {
                if (widget.WidgetInstance == null
                    || widget.WidgetInstance is TransientWidgetInstance
                    || widget.WidgetInstance.MainSettings.Disabled
                    || widget.Pending)
                {
                    continue;
                }

                if (WidgetFilter.Length > 0
                    && WidgetFilter.All(f => !widget.WidgetInstance.Id.Contains(f, StringComparison.OrdinalIgnoreCase))
                    && WidgetFilter.All(f => !widget.WidgetInstance.Info.Id.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                widget.Pending = true;
                _dispatcher.Queue(async (st, ct) =>
                {
                    var localWidget = (WidgetViewModel)st!;
                    var updateTask = localWidget.UpdateWidgetAsync(cancellationToken: ct);
                    await updateTask.WaitAsync(TimeSpan.FromSeconds(30), ct);
                    localWidget.Pending = false;
                }, widget);
            }
        }
        finally
        {
            _updating = false;
        }

        return Task.CompletedTask;
    }

    #region UI state

    private async Task SaveUiState(IScheduler scheduler, CancellationToken cancellationToken)
    {
        await _settingsStorage.SaveWindowStateAsync(new MainWindowStateSaveModel
        {
            WindowHeight = WindowHeight,
            WindowWidth = WindowWidth,
            WindowPositionX = WindowPosition.X,
            WindowPositionY = WindowPosition.Y,
            WindowScreen = WindowScreen,
            WidgetsOrder = Spaces.ToDictionary(s => s.Id, s => s.Widgets.Select(w => w.WidgetId).ToArray()),
            Topmost = Topmost,
            ActiveSpace = SelectedSpace?.Id ?? string.Empty,
        }, cancellationToken);
    }

    private async Task LoadUiState(CancellationToken cancellationToken)
    {
        var windowState = await _settingsStorage.LoadWindowStateAsync(cancellationToken);
        WindowHeight = windowState.WindowHeight;
        WindowWidth = windowState.WindowWidth;

        if (!string.IsNullOrEmpty(WindowScreen))
        {
            if (windowState.WindowPositions.TryGetValue(WindowScreen, out var windowPosition))
            {
                WindowPosition = new PixelPoint(windowPosition.X, windowPosition.Y);
            }
        }

        foreach (var space in Spaces)
        {
            var order = windowState.WidgetsOrder.TryGetValue(space.Id, out var widgetsOrder)
                ? widgetsOrder
                : space.Widgets.Select(w => w.WidgetId).ToArray();
            space.Widgets = new AvaloniaList<WidgetViewModel>(space.Widgets.Order(new WidgetsIdComparer(order)));
        }
        if (!string.IsNullOrEmpty(windowState.ActiveSpace))
        {
            SelectedSpace = Spaces.FirstOrDefault(s => s.Id == windowState.ActiveSpace) ?? SelectedSpace;
        }

        Topmost = windowState.Topmost;
    }

    #endregion

    private void CloseApplicationWindow()
    {
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    private async Task OpenAboutWindow(CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Sdk.Application.GetProductFullName())
            .AppendLine()
            .AppendLine("Runtime: " + Sdk.Application.GetRuntimeIdentifier())
            .AppendLine("Platform: " + Sdk.Application.GetPlatform())
            .AppendLine("Architecture: " + Sdk.Application.GetArchitecture());

        var messageBoxVm = new MessageBoxViewModel(sb.ToString(), "About");
        await _mvvmService.OpenAsync(messageBoxVm, cancellationToken);
    }

    private async Task OpenLogsWindow(CancellationToken cancellationToken)
    {
        var viewModel = _mvvmService.CreateViewModel<LogsViewModel>();
        await _mvvmService.OpenAsync(viewModel, cancellationToken);
    }

    private async Task OpenFontsWindow(CancellationToken cancellationToken)
    {
        var viewModel = _mvvmService.CreateViewModel<TextWindowViewModel>();
        var sb = new StringBuilder()
            .AppendLine("Available system fonts:")
            .AppendLine(string.Join(Environment.NewLine,
                Avalonia.Media.FontManager.Current.SystemFonts.Select(f => "- " + f.Name)));
        viewModel.Text = sb.ToString();
        await _mvvmService.OpenAsync(viewModel, cancellationToken);
    }

    private async Task OpenWebsite(CancellationToken cancellationToken)
    {
    }

    private async Task CheckUpdates(CancellationToken cancellationToken)
    {
        try
        {
            await UpdateInfo.CheckAppUpdatesAsync(cancellationToken);
            await UpdateInfo.CheckPackagesUpdatesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while checking updates: {Message}", e.Message);
        }
    }

    private void SwitchSpace(SpaceViewModel space)
    {
        SelectedSpace = space;
    }

    private async Task OpenSettingsWindow(CancellationToken cancellationToken)
    {
        var appSettingsViewModel = new AppSettingsViewModel(_appSettings)
        {
            IsTopmost = Topmost
        };
        var viewModel = _mvvmService.CreateViewModel<SettingsViewModel>(appSettingsViewModel);

        viewModel.AddSection(
            SettingsSection.Create<AppMainSectionControl, WidgetMainSectionViewModel>("Main")
        );
        viewModel.AddSection(
            SettingsSection.Create<AppSpacesSectionControl, AppSpacesSectionViewModel>("Spaces")
        );
        viewModel.AddJsonSection();

        if (await _mvvmService.OpenAsync(viewModel, cancellationToken) == DialogResult.OK)
        {
            var newSettings = (AppSettingsViewModel)viewModel.Settings;
            var newAppSettings = newSettings.ToAppSettings();
            using var cloner = new AppCloner();
            cloner.CloneTo(newAppSettings, _appSettings);
            await _settingsStorage.SaveAsync(_appSettings, cancellationToken);
            await LoadInternalAsync(cancellationToken);
            Topmost = newSettings.IsTopmost;
        }
    }

    private async Task AddWidgetWindow(CancellationToken cancellationToken)
    {
        if (SelectedSpace == null)
        {
            return;
        }

        var viewModel = _mvvmService.CreateViewModel<WidgetsManagementViewModel>();
        if (await _mvvmService.OpenAsync(viewModel, cancellationToken) == DialogResult.OK
            && viewModel.ResultValue != null)
        {
            var widgetInfo = viewModel.ResultValue;
            var instance = new WidgetInstance(widgetInfo);
            instance.MainSettings.SpaceId = SelectedSpace.Id;
            try
            {
                var widget = await _widgetsFactory.CreateAsync(
                    widgetInfo.WidgetType,
                    new WidgetInitInfo(instance, instance.WidgetSettings),
                    cancellationToken
                );
                var vm = _mvvmService.CreateViewModel<WidgetViewModel>();
                vm.Widget = widget;
                vm.WidgetInstance = instance;
                PrepareWidgetViewModel(vm);
                SelectedSpace.Widgets.Add(vm);
                await _widgetInstanceProvider.SaveAsync(instance, cancellationToken);
            }
            catch (Exception e)
            {
                var vm = _mvvmService.CreateViewModel<WidgetViewModel>();
                vm.Widget = new StubWidget
                {
                    Header = widgetInfo.Id,
                    Text = e.Message,
                };
                vm.WidgetInstance = instance;
                PrepareWidgetViewModel(vm);
                SelectedSpace.Widgets.Add(vm);
            }
        }
        await SaveUiState(RxSchedulers.MainThreadScheduler, cancellationToken);
    }

    private async Task RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken)
    {
        if (SelectedSpace == null
            || !TryGetWidgetById(widgetId, out var widgetViewModel)
            || widgetViewModel == null
            || widgetViewModel.WidgetInstance == null)
        {
            return;
        }

        var messageBoxVm = new MessageBoxViewModel(Resources.Messages.WidgetsContainer_WidgetRemoveConfirmation, Resources.Messages.Remove)
            .SetYesNoMode();
        if (await _mvvmService.OpenAsync(messageBoxVm, cancellationToken) == DialogResult.Yes)
        {
            SelectedSpace.Widgets.Remove(widgetViewModel);
            await _widgetInstanceProvider.RemoveAsync(widgetViewModel.WidgetInstance, cancellationToken);
            await SaveUiState(DefaultScheduler.Instance, cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        await LoadInternalAsync(cancellationToken: cancellationToken);
        Observable
            .Timer(TimeSpan.FromSeconds(10))
            .Select(_ => Unit.Default)
            .InvokeCommand(CheckUpdatesCommand);
        await base.LoadAsync(cancellationToken);

        stopwatch.Stop();
        _logger.LogTrace($"App state loaded in {stopwatch.ElapsedMilliseconds} ms.");
    }

    private async Task LoadInternalAsync(CancellationToken cancellationToken = default)
    {
        Loading = true;
        var instances = (await _widgetInstanceProvider.LoadAsync(cancellationToken)).ToList();

        Spaces.Clear();
        Spaces.AddRange(_appSettings.Spaces.Select(s => new SpaceViewModel(s)));
        SelectedSpace = Spaces.FirstOrDefault(s => s.Default);
        if (SelectedSpace == null)
        {
            throw new DashikException("No space found. Please add at least one space in settings.");
        }

        // Load instance.
        var vms = new List<WidgetViewModel>(capacity: instances.Count);
        foreach (var instance in instances)
        {
            var widget = await _widgetsFactory.CreateAsync(
                instance.Info.WidgetType,
                new WidgetInitInfo(instance, instance.WidgetSettings),
                cancellationToken);
            var vm = _mvvmService.CreateViewModel<WidgetViewModel>();
            vm.Widget = widget;
            vm.WidgetInstance = instance;
            PrepareWidgetViewModel(vm);
            vms.Add(vm);
        }
        foreach (var space in Spaces)
        {
            var widgets = vms
                .Where(w => w.WidgetInstance != null && w.WidgetInstance.MainSettings.SpaceId == space.Id)
                .ToArray();
            space.Widgets.AddRange(widgets);
            vms.Remove(widgets);
        }
        // Add not assigned to the default space.
        SelectedSpace.Widgets.AddRange(vms);

        await LoadUiState(cancellationToken);

        Loading = false;
    }

    private void PrepareWidgetViewModel(WidgetViewModel widgetViewModel)
    {
        widgetViewModel.RemoveWidgetRequested
            .SelectMany(widgetId => Observable.FromAsync(ct => RemoveWidgetAsync(widgetId, ct)))
            .Subscribe();
        widgetViewModel.SaveWidgetRequested
            .Subscribe(widgetId => _widgetSave.OnNext(widgetId));
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        foreach (var widget in Widgets)
        {
            widget.Sync();
            if (widget.WidgetInstance != null)
            {
                await _widgetInstanceProvider.SaveAsync(widget.WidgetInstance, cancellationToken);
            }
        }
    }

    private bool TryGetWidgetById(string id, out WidgetViewModel? widget)
    {
        widget = Widgets.FirstOrDefault(w => w.WidgetId == id);
        return widget != null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _widgetsUpdateTimerObservable.Dispose();
        _widgetSave.Dispose();
        _widgetsSaveObservable.Dispose();
        _saveUiObservable.Dispose();
        _dispatcher.Dispose();
    }
}
