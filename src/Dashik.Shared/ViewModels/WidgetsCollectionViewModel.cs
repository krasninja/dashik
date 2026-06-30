using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Nodes;
using Avalonia.Collections;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Dashik.Abstractions;
using Dashik.Sdk;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services;
using Dashik.Shared.Services.Widgets;
using Dashik.Shared.Utils;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Views.Settings;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetsCollectionViewModel : ViewModelBase, IDisposable
{
    private readonly AppSettings _appSettings;
    private readonly SettingsStorage _settingsStorage;
    private readonly IMvvmService _mvvmService;
    private readonly IWidgetsFactory _widgetsFactory;
    private readonly IWidgetInstanceProvider _widgetInstanceProvider;
    private readonly IWidgetsStateStorage _stateStorage;
    private readonly ILogger _logger;

    private readonly IDisposable _widgetsUpdateTimerObservable;
    private readonly IDisposable _widgetsSaveObservable;
    private bool _disposed;

    private readonly CompositeDisposable _disposables = new();
    private volatile bool _updating;

    public AvaloniaList<SpaceViewModel> Spaces { get; }

    public AppUpdateViewModel UpdateInfo { get; }

    public string[] WidgetFilter { get; set; } = [];

    internal IEnumerable<WidgetViewModel> ActiveWidgets => Spaces
        .SelectMany(s => s.Widgets)
        .Where(w => w.WidgetInstance != null
                    && w.WidgetInstance is not TransientWidgetInstance
                    && !w.WidgetInstance.MainSettings.Disabled);

    /// <summary>
    /// Widgets collection.
    /// </summary>
    public IEnumerable<WidgetViewModel> Widgets => Spaces.SelectMany(s => s.Widgets);

    private readonly ParallelDispatcher _dispatcher = new(maxDegreeOfParallelism: 2);

    private MainViewModel CurrentApplication =>
        (MainViewModel?)Avalonia.Application.Current?.DataContext
            ?? throw new InvalidOperationException("App needs to be initialized.");

    public ReactiveCommand<SpaceViewModel, Unit> AddWidgetCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }

    public ReactiveCommand<Unit, Unit> CheckUpdatesCommand { get; }

    public Subject<SpaceViewModel> WidgetsCollectionUpdate { get; } = new();

    public Subject<string> WidgetSave { get; } = new();

    public Subject<Unit> SpaceCollectionUpdate { get; } = new();

#pragma warning disable CS8618
    internal WidgetsCollectionViewModel()
#pragma warning restore CS8618
    {
        Spaces = [];

        _widgetsUpdateTimerObservable = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ => RxSchedulers.MainThreadScheduler.ScheduleAsync(UpdateWidgets));

        _widgetsSaveObservable = WidgetSave
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

        _dispatcher.WaitPendingTasksOnDispose = false;
        _dispatcher.Start();
    }

    public WidgetsCollectionViewModel(
        AppSettings appSettings,
        SettingsStorage settingsStorage,
        AppUpdateViewModel updateInfo,
        IMvvmService mvvmService,
        IWidgetsFactory widgetsFactory,
        IWidgetInstanceProvider widgetInstanceProvider,
        IWidgetsStateStorage stateStorage,
        ILogger<WidgetsCollectionViewModel> logger) : this()
    {
        _appSettings = appSettings;
        _settingsStorage = settingsStorage;
        UpdateInfo = updateInfo;
        _mvvmService = mvvmService;
        _widgetsFactory = widgetsFactory;
        _widgetInstanceProvider = widgetInstanceProvider;
        _stateStorage = stateStorage;
        _logger = logger;

        AddWidgetCommand = ReactiveCommand.CreateFromTask<SpaceViewModel>(AddWidgetWindow);
        OpenSettingsCommand = ReactiveCommand.CreateFromTask(OpenSettingsWindow);
        UpdateCommand = ReactiveCommand.CreateFromTask(UpdateInfo.UpdateAsync);
        CheckUpdatesCommand = ReactiveCommand.CreateFromTask(CheckUpdatesAsync);
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _disposables.Add(
            Observable
                .Timer(TimeSpan.FromSeconds(5))
                .SubscribeAsync(async (_, ct) =>
                {
                    await CheckUpdatesAsync(ct);
                    if (_appSettings.AutoUpdate && UpdateInfo.HasNewVersion)
                    {
                        await UpdateInfo.UpdateAsync(ct);
                    }
                })
        );
        await LoadInternalAsync(cancellationToken: cancellationToken);
        await base.LoadAsync(cancellationToken);
    }

    private async Task LoadInternalAsync(CancellationToken cancellationToken = default)
    {
        Loading = true;

        try
        {
            // Load spaces.
            Spaces.Clear();
            Spaces.AddRange(_appSettings.Spaces.Select(s => new SpaceViewModel(s)));
            var defaultSpace = Spaces.FirstOrDefault(s => s.Default);
            if (defaultSpace == null)
            {
                throw new DashikException("No space found. Please add at least one space in settings.");
            }

            // Load instances.
            var widgetViewModels = await LoadInstancesAsync(cancellationToken);
            ArrangeWidgetsBySpaces(widgetViewModels);
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task<IReadOnlyList<WidgetViewModel>> LoadInstancesAsync(CancellationToken cancellationToken = default)
    {
        var instances = (await _widgetInstanceProvider.LoadAsync(cancellationToken)).ToList();
        var vms = new List<WidgetViewModel>(capacity: instances.Count);
        foreach (var instance in instances)
        {
            var vm = await CreateAndPrepareWidgetViewModelAsync(instance, cancellationToken);
            vms.Add(vm);
        }
        return vms;
    }

    private void ArrangeWidgetsBySpaces(IEnumerable<WidgetViewModel> widgetsViewModels)
    {
        var vms = new List<WidgetViewModel>(widgetsViewModels);
        foreach (var space in Spaces)
        {
            space.Widgets.Clear();
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
        var defaultSpace = Spaces.FirstOrDefault(s => s.Default) ?? Spaces.FirstOrDefault();
        if (defaultSpace != null)
        {
            defaultSpace.Widgets.AddRange(vms);
        }
    }

    private bool TryGetWidgetById(string id, out WidgetViewModel? widget)
    {
        widget = Widgets.FirstOrDefault(w => w.WidgetId == id);
        return widget != null;
    }

    private async Task<WidgetViewModel> CreateAndPrepareWidgetViewModelAsync(IWidgetInstance instance, CancellationToken cancellationToken)
    {
        if (instance is WidgetInstance widgetInstance)
        {
            widgetInstance.OnMessageSend = ProcessMessageAsync;
        }
        WidgetViewModel widgetViewModel;
        try
        {
            var widget = await _widgetsFactory.CreateAsync(
                instance.Info.WidgetType,
                new WidgetInitInfo(instance, instance.MainSettings, instance.WidgetSettings),
                cancellationToken
            );
            widgetViewModel = _mvvmService.CreateViewModel<WidgetViewModel>();
            widgetViewModel.Widget = widget;
            widgetViewModel.WidgetInstance = instance;
        }
        catch (Exception e)
        {
            widgetViewModel = _mvvmService.CreateViewModel<WidgetViewModel>();
            var stubWidget = new StubWidget
            {
                Header = instance.Info.Id,
                Text = e.Message,
            };
            widgetViewModel.Widget = stubWidget;
            widgetViewModel.WidgetInstance = instance;
            Spaces[0].Widgets.Add(widgetViewModel);
            _logger.LogError(e, "Failed to create widget {Id}", instance.Info.Id);
        }

        var subscription = widgetViewModel.RemoveWidgetRequested
            .SelectMany(widgetId => Observable.FromAsync(ct => RemoveWidgetAsync(widgetId, ct)))
            .Subscribe();
        _disposables.Add(subscription);

        subscription = widgetViewModel.SaveWidgetRequested
            .Subscribe(widgetId => WidgetSave.OnNext(widgetId));
        _disposables.Add(subscription);

        subscription = widgetViewModel.ReorderWidgetRequested
            .Subscribe(_ =>
            {
                ArrangeWidgetsBySpaces(Widgets);
            });
        _disposables.Add(subscription);

        await widgetViewModel.InitializeAsync(cancellationToken);
        await widgetViewModel.LoadAsync(cancellationToken);
        return widgetViewModel;
    }

    private async Task AddWidgetWindow(SpaceViewModel spaceViewModel, CancellationToken cancellationToken)
    {
        var viewModel = _mvvmService.CreateViewModel<WidgetsManagementViewModel>();
        viewModel.AddPackageViewModel.PackagesLoaded
            .SubscribeAsync(async (_, ct) =>
            {
                await LoadInternalAsync(ct);
            });
        if (await _mvvmService.OpenAsync(viewModel, this, cancellationToken) == DialogResult.OK
            && viewModel.ResultValue != null)
        {
            var widgetInfo = viewModel.ResultValue;
            var instance = new WidgetInstance(widgetInfo, _stateStorage);
            var widgetViewModel = await CreateAndPrepareWidgetViewModelAsync(instance, cancellationToken);
            if (widgetViewModel.WidgetInstance != null)
            {
                widgetViewModel.WidgetInstance.MainSettings.SpaceId = spaceViewModel.Id;
            }
            spaceViewModel.Widgets.Add(widgetViewModel);
            WidgetsCollectionUpdate.OnNext(spaceViewModel);
            await _widgetInstanceProvider.SaveAsync(instance, cancellationToken);
        }
    }

    private async Task<JsonObject?> ProcessMessageAsync(WidgetMessage message, CancellationToken cancellationToken)
    {
        foreach (var widget in ActiveWidgets)
        {
            if (widget.Widget is not IWidgetBus widgetBus)
            {
                continue;
            }

            if (message.WidgetId == widget.WidgetId || message.WidgetId == "*"
                                                    || string.IsNullOrEmpty(message.WidgetId))
            {
                var response = await widgetBus.ReceiveMessageAsync(message.WidgetId, message.MessageId, message.Payload, cancellationToken);
                if (response != null)
                {
                    return response;
                }
            }
        }
        return null;
    }

    private async Task CheckUpdatesAsync(CancellationToken cancellationToken)
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

    private Task UpdateWidgets(IScheduler scheduler, CancellationToken cancellationToken)
    {
        if (_updating)
        {
            return Task.CompletedTask;
        }

        try
        {
            _updating = true;
            foreach (var widget in ActiveWidgets)
            {
                if (widget.WidgetInstance == null || widget.Pending)
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

    private async Task RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken)
    {
        var space = Spaces.FirstOrDefault(s => s.Widgets.Any(w => w.WidgetId == widgetId));

        if (space == null
            || !TryGetWidgetById(widgetId, out var widgetViewModel)
            || widgetViewModel == null
            || widgetViewModel.WidgetInstance == null)
        {
            return;
        }

        var messageBoxVm = new MessageBoxViewModel(Resources.Messages.WidgetsContainer_WidgetRemoveConfirmation, Resources.Messages.Remove)
            .SetYesNoMode();
        if (await _mvvmService.OpenAsync(messageBoxVm, this, cancellationToken) == DialogResult.Yes)
        {
            space.Widgets.Remove(widgetViewModel);
            await _widgetInstanceProvider.RemoveAsync(widgetViewModel.WidgetInstance, cancellationToken);
            WidgetsCollectionUpdate.OnNext(space);
        }
    }

    private async Task OpenSettingsWindow(CancellationToken cancellationToken)
    {
        var appSettingsViewModel = _mvvmService.CreateViewModel<AppSettingsViewModel>();
        await appSettingsViewModel.LoadAsync(cancellationToken);
        var viewModel = _mvvmService.CreateViewModel<SettingsViewModel>(appSettingsViewModel);

        viewModel.AddSection(
            SettingsSection.Create<AppMainSectionControl, WidgetMainSectionViewModel>("Main")
        );
        viewModel.AddSection(
            SettingsSection.Create<AppSpacesSectionControl, AppSpacesSectionViewModel>("Spaces")
        );
        viewModel.AddJsonSection();

        if (await _mvvmService.OpenAsync(viewModel, this, cancellationToken) == DialogResult.OK)
        {
            var newAppSettingsViewModel = (AppSettingsViewModel)viewModel.Settings;
            var newAppSettings = newAppSettingsViewModel.ToAppSettings();
            using var cloner = new AppCloner();
            cloner.CloneTo(newAppSettings, _appSettings);
            await _settingsStorage.SaveAsync(_appSettings, cancellationToken);
            await newAppSettingsViewModel.SetApplicationStartupAsync(cancellationToken);
            await LoadInternalAsync(cancellationToken);
            CurrentApplication.ShowSystemTrayIcon = newAppSettingsViewModel.ShowSystemTrayIcon;
            SpaceCollectionUpdate.OnNext(Unit.Default);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        _disposables.Dispose();
        _widgetsUpdateTimerObservable.Dispose();
        _widgetsSaveObservable.Dispose();

        _dispatcher.Stop();
        _dispatcher.Dispose();
    }
}
