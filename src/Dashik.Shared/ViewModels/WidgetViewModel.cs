using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Dashik.Abstractions;
using Dashik.Sdk;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Utils;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Views.Settings;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Shared.Services.Widgets;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetViewModel : ViewModelBase, IDisposable
{
    public const double DefaultWidgetWidth = 250;

    private readonly IMvvmService _mvvmService;
    private readonly IServiceProvider _serviceProvider;
    private Control? _control;
    private Control? _errorControl;
    private readonly ILogger _logger;

    public sealed class WidgetAllSettings(WidgetMainSettings mainSettings, object? settings)
    {
        public WidgetMainSettings MainSettings { get; } = mainSettings;

        public object? Settings { get; } = settings;
    }

    public double WidgetWidth
    {
        get => DefaultWidgetWidth;
    }

    public IWidgetInstance? WidgetInstance
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            UpdateTitle();
        }
    }

    public IWidget? Widget
    {
        get;
        set
        {
            this.RaisePropertyChanging(nameof(WidgetControl));
            this.RaiseAndSetIfChanged(ref field, value);
            UpdateTitle();
            UpdateControl();
            this.RaisePropertyChanged(nameof(WidgetControl));
        }
    }

    public string WidgetId => WidgetInstance?.Id ?? string.Empty;

    public Control? WidgetControl => !RequireConfiguration ? _control : _errorControl;

    private StubWidget? ErrorWidget
    {
        get;
        set
        {
            this.RaisePropertyChanging(nameof(WidgetControl));
            this.RaiseAndSetIfChanged(ref field, value);
            _errorControl = value?.CreateControl(WidgetControlTarget.Panel, Size.Infinity);
            this.RaisePropertyChanged(nameof(WidgetControl));
        }
    }

    public bool RequireConfiguration
    {
        get;
        private set
        {
            this.RaisePropertyChanging(nameof(WidgetControl));
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(WidgetControl));
        }
    }

    public string Title
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = "Empty";

    public Exception? LastException
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public DateTime? LastUpdatedUtc
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool Updating
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool Pending
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool Initialized
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CanUpdate => Widget is IWidgetUpdate;

    /// <summary>
    /// Disable widget menu.
    /// </summary>
    public bool ReadOnly
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public AvaloniaList<MenuItem> WidgetMenuItems { get; } = new();

    public ReactiveCommand<WidgetViewModel, Unit> UpdateWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> DisableWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> EnableWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> RemoveWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> ExpandWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> CollapseWidgetCommand { get; }

    public ReactiveCommand<WidgetViewModel, Unit> OpenWidgetSettingsCommand { get; }

    public IObservable<string> RemoveWidgetRequested => RemoveWidgetCommand.Select(_ => WidgetId);

    private readonly Subject<string> _saveWidgetRequested = new();

    public IObservable<string> SaveWidgetRequested => _saveWidgetRequested;

    private readonly Subject<Unit> _reorderWidgetRequested = new();

    public IObservable<Unit> ReorderWidgetRequested => _reorderWidgetRequested;

    public WidgetViewModel(
        IMvvmService mvvmService,
        IServiceProvider serviceProvider,
        ILogger<WidgetViewModel> logger)
    {
        _mvvmService = mvvmService;
        _serviceProvider = serviceProvider;
        _logger = logger;

        RemoveWidgetCommand = ReactiveCommand.Create<WidgetViewModel>(_ => { });
        UpdateWidgetCommand = ReactiveCommand.CreateFromTask<WidgetViewModel>(async vm =>
        {
            await vm.UpdateWidgetAsync(force: true);
        });
        DisableWidgetCommand = ReactiveCommand.Create<WidgetViewModel>(vm =>
        {
            if (vm.WidgetInstance == null)
            {
                return;
            }
            vm.WidgetInstance.MainSettings.Disabled = true;
            _saveWidgetRequested.OnNext(vm.WidgetId);
        });
        EnableWidgetCommand = ReactiveCommand.Create<WidgetViewModel>(vm =>
        {
            if (vm.WidgetInstance == null)
            {
                return;
            }
            vm.WidgetInstance.MainSettings.Disabled = false;
            _saveWidgetRequested.OnNext(vm.WidgetId);
        });
        CollapseWidgetCommand = ReactiveCommand.Create<WidgetViewModel>(vm =>
        {
            if (vm.WidgetInstance == null)
            {
                return;
            }
            vm.WidgetInstance.MainSettings.Hidden = true;
            _saveWidgetRequested.OnNext(vm.WidgetId);
        });
        ExpandWidgetCommand = ReactiveCommand.Create<WidgetViewModel>(vm =>
        {
            if (vm.WidgetInstance == null)
            {
                return;
            }
            vm.WidgetInstance.MainSettings.Hidden = false;
            _saveWidgetRequested.OnNext(vm.WidgetId);
        });
        OpenWidgetSettingsCommand = ReactiveCommand.CreateFromTask<WidgetViewModel>(OpenWidgetSettings);
    }

    private async Task OpenWidgetSettings(WidgetViewModel vm, CancellationToken cancellationToken)
    {
        if (vm.WidgetInstance == null || vm.Widget == null)
        {
            return;
        }

        var widgetSettings = vm.Widget is IWidgetSettings widgetWithSettings ? widgetWithSettings.Settings : null;
        var settingsModel = new WidgetAllSettings(vm.WidgetInstance.MainSettings, widgetSettings);
        var originalSpaceId = settingsModel.MainSettings.SpaceId;
        var viewModel = _mvvmService.CreateViewModel<SettingsViewModel>(settingsModel);
        AddSettingsSection(viewModel, vm.Widget);

        if (await _mvvmService.OpenAsync(viewModel, this, cancellationToken) == DialogResult.OK)
        {
            CopySettings((WidgetAllSettings)viewModel.Settings);
            UpdateTitle();
            _saveWidgetRequested.OnNext(vm.WidgetId);
            await vm.Widget.InitializeAsync(
                new WidgetInitInfo(vm.WidgetInstance, vm.WidgetInstance.MainSettings, vm.WidgetInstance.WidgetSettings),
                cancellationToken);
            Initialized = false;
            await InitializeAsync(cancellationToken);
            await UpdateWidgetAsync(force: true, cancellationToken);
            await LoadAsync(cancellationToken);

            if (originalSpaceId != settingsModel.MainSettings.SpaceId)
            {
                _reorderWidgetRequested.OnNext(Unit.Default);
            }
        }
    }

    private void AddSettingsSection(SettingsViewModel viewModel, IWidget widget)
    {
        viewModel.AddSection(
            SettingsSection.Create<WidgetMainSettingsControl, WidgetMainSectionViewModel>("Main"),
            obj => ((WidgetAllSettings)obj).MainSettings
        );

        if (widget is IWidgetSettings widgetWithSettings)
        {
            foreach (var settingsSection in widgetWithSettings.GetSections())
            {
                viewModel.AddSection(
                    new SettingsSection(settingsSection.Name, settingsSection.ControlType, settingsSection.ViewModelType),
                    obj => ((WidgetAllSettings)obj).Settings
                );
            }
        }

        viewModel.AddJsonSection();
    }

    private void CopySettings(WidgetAllSettings newSettings)
    {
        if (WidgetInstance == null)
        {
            return;
        }

        AppCloner.CloneObjectTo(newSettings.MainSettings, WidgetInstance.MainSettings);
        if (newSettings.Settings != null && Widget is IWidgetSettings widgetWithSettings)
        {
            AppCloner.CloneObjectTo(newSettings.Settings, widgetWithSettings.Settings);
            WidgetInstance.WidgetSettings = JsonSerializer.SerializeToNode(widgetWithSettings.Settings)!.AsObject();
        }
    }

    public void Sync()
    {
        if (Widget is IWidgetSettings widgetSettings && WidgetInstance != null)
        {
            WidgetInstance.WidgetSettings = JsonSerializer.SerializeToNode(widgetSettings.Settings)?.AsObject()
                                            ?? new JsonObject();
        }
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (Widget == null || WidgetInstance == null || Initialized)
        {
            return;
        }

        var initInfo = new WidgetInitInfo(WidgetInstance, WidgetInstance.MainSettings, WidgetInstance.WidgetSettings);

        // Validate settings.
        if (Widget is IWidgetSettings widgetSettings && !initInfo.Context.PreviewMode)
        {
            try
            {
                RequireConfiguration = false;
                var settings = initInfo.GetSettings(widgetSettings.SettingsType);
                ValidateObject(settings);
            }
            catch (WidgetNotConfiguredException e)
            {
                RequireConfiguration = true;
                ErrorWidget = await CreateErrorWidgetAsync(initInfo, e, cancellationToken);
            }
        }

        // Initialize.
        try
        {
            initInfo.IncompleteConfiguration = RequireConfiguration;
            await Widget.InitializeAsync(initInfo, cancellationToken);
        }
        catch (Exception e)
        {
            ErrorWidget = await CreateErrorWidgetAsync(initInfo, e, cancellationToken);
        }

        Initialized = true;
    }

    private static void ValidateObject(object obj)
    {
        var context = new ValidationContext(obj);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(obj, context, results, true);
        if (!isValid && results.Count > 0)
        {
            var error = "Need to setup the widget: " + results[0].ErrorMessage;
            throw new WidgetNotConfiguredException(error);
        }
    }

    private async Task<StubWidget> CreateErrorWidgetAsync(WidgetInitInfo initInfo, Exception e, CancellationToken cancellationToken)
    {
        var widget = (StubWidget)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(StubWidget));
        await widget.InitializeAsync(
            new WidgetInitInfo(
                new TransientWidgetInstance(new WidgetInfo(widget.GetType()))
                {
                    Message = e.Message,
                    Error = true,
                    RequiresSetup = e is WidgetNotConfiguredException,
                },
                initInfo.MainSettings,
                initInfo.Settings
            ),
            cancellationToken
        );

        return widget;
    }

    /// <summary>
    /// Update widget data. The update might be skipped if widget is not
    /// updatable, or it is not time to refresh.
    /// </summary>
    /// <param name="force">Skip any checks and force update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>True</c> if update was run, <c>false</c> otherwise.</returns>
    public async Task<bool> UpdateWidgetAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (WidgetInstance == null
            || Widget == null
            || Updating)
        {
            return false;
        }
        if (Widget is not IWidgetUpdate widgetUpdate)
        {
            return false;
        }
        if (!force
            && DateTime.UtcNow - LastUpdatedUtc < WidgetInstance.MainSettings.UpdateInterval)
        {
            return false;
        }

        try
        {
            Pending = true;
            Updating = true;
            LastException = null;
            await widgetUpdate.UpdateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {WidgetId} ('{Title}').", WidgetId, Title);
            LastException = ex;
        }
        finally
        {
            Updating = false;
            LastUpdatedUtc = DateTime.UtcNow;
            Pending = false;
        }

        return true;
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        UpdateWidgetMenuItems();
        await base.LoadAsync(cancellationToken);
    }

    private void UpdateWidgetMenuItems()
    {
        if (Widget is IWidgetMenu widgetWithMenu)
        {
            WidgetMenuItems.Clear();
            foreach (var menuItem in widgetWithMenu.GetWidgetMenuItems())
            {
                WidgetMenuItems.Add(menuItem);
            }
        }
    }

    private void UpdateTitle()
    {
        if (WidgetInstance != null && WidgetInstance.MainSettings.UseCustomTitle)
        {
            Title = WidgetInstance.MainSettings.CustomTitle;
            return;
        }
        if (Widget != null)
        {
            Title = Widget.Header;
            return;
        }
        Title = "Widget";
    }

    private void UpdateControl()
    {
        if (Widget == null)
        {
            return;
        }
        ReleaseControl();
        _control = Widget.CreateControl(WidgetControlTarget.Panel, Size.Infinity);
    }

    private void ReleaseControl()
    {
        if (_control == null)
        {
            return;
        }
        (_control.DataContext as IDisposable)?.Dispose();
        (_control as IDisposable)?.Dispose();
        _control = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _saveWidgetRequested.Dispose();
        _reorderWidgetRequested.Dispose();
        ReleaseControl();
    }

    /// <inheritdoc />
    public override string ToString() => $"{WidgetId}: {Title}";
}
