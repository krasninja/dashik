using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dashik.Abstractions;
using ReactiveUI;
using Microsoft.Extensions.Logging;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Utils;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Views.Settings;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetViewModel : ViewModelBase, IDisposable
{
    private readonly IMvvmService _mvvmService;
    private readonly ILogger _logger;

    public sealed class WidgetAllSettings(WidgetMainSettings mainSettings, object? settings)
    {
        public WidgetMainSettings MainSettings { get; } = mainSettings;

        public object? Settings { get; } = settings;
    }

    public IWidgetInstance? WidgetInstance
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            UpdateTitle();
        }
    }

    public IWidget? Widget
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            UpdateTitle();
        }
    }

    public string WidgetId => WidgetInstance?.Id ?? string.Empty;

    public string Title
    {
        get => field;
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

    public WidgetViewModel(IMvvmService mvvmService, ILogger<WidgetViewModel> logger)
    {
        _mvvmService = mvvmService;
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
        var viewModel = _mvvmService.CreateViewModel<SettingsViewModel>(settingsModel);
        AddSettingsSection(viewModel, vm.Widget);

        if (await _mvvmService.OpenAsync(viewModel, cancellationToken) == DialogResult.OK)
        {
            CopySettings((WidgetAllSettings)viewModel.Settings);
            UpdateTitle();
            await vm.Widget.InitializeAsync(new WidgetInitInfo(vm.WidgetInstance, vm.WidgetInstance.WidgetSettings), cancellationToken);
            await LoadAsync(cancellationToken);
            _saveWidgetRequested.OnNext(vm.WidgetId);
        }
    }

    private void AddSettingsSection(SettingsViewModel viewModel, IWidget widget)
    {
        viewModel.AddSection(
            SettingsSection.Create<WidgetMainSettingsControl, MainSettingsSectionViewModel>("Main"),
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

    public async Task UpdateWidgetAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (WidgetInstance == null
            || Widget == null
            || Updating)
        {
            return;
        }
        if (Widget is not IWidgetUpdate widgetUpdate)
        {
            return;
        }
        if (!force
            && DateTime.UtcNow - LastUpdatedUtc < WidgetInstance.MainSettings.UpdateInterval)
        {
            return;
        }

        try
        {
            Pending = true;
            Updating = true;
            LastException = null;
            await widgetUpdate.UpdateAsync(cancellationToken);
            LastUpdatedUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {WidgetId}.", WidgetId);
            LastException = ex;
        }
        finally
        {
            Updating = false;
            LastUpdatedUtc = DateTime.UtcNow;
            Pending = false;
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

    /// <inheritdoc />
    public void Dispose()
    {
        _saveWidgetRequested.Dispose();
    }

    /// <inheritdoc />
    public override string ToString() => $"{WidgetId}: {Title}";
}
