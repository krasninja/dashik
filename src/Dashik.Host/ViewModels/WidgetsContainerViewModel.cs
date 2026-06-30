using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Avalonia;
using Avalonia.Collections;
using ReactiveUI;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;
using Dashik.Host.Utils;
using Dashik.Host.Models;
using Dashik.Host.Services;

namespace Dashik.Host.ViewModels;

public sealed class WidgetsContainerViewModel : WidgetsBaseViewModel, ICloseableViewModel
{
    private readonly AppSettings _appSettings;
    private readonly SettingsStorage _settingsStorage;
    private readonly IMvvmService _mvvmService;
    private readonly ILogger<WidgetsContainerViewModel> _logger;
    private bool _savingUiState;

    private readonly CompositeDisposable _disposables = new();

    internal AppSettings ApplicationSettings => _appSettings;

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    public WindowState WindowState
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double WindowHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = 750;

    public double WindowWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = 450;

    public PixelPoint WindowPosition
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string WindowScreen
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = string.Empty;

    public bool Topmost
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenLogsCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenFontsCommand { get; }

    public ReactiveCommand<Unit, Unit> OpenWebsiteCommand { get; }

    public ReactiveCommand<SpaceViewModel, Unit> SwitchSpaceCommand { get; }

    public ReactiveCommand<Unit, Unit> ShowTrayIconCommand { get; }

    public ReactiveCommand<Unit, Unit> FitCommand { get; }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

#pragma warning disable CS8618
    internal WidgetsContainerViewModel()
#pragma warning restore CS8618
    {
        _disposables.Add(this
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
            .SubscribeAsync((_, ct) => SaveUiStateAsync(ct))
        );

        _disposables.Add(this
            .WhenAnyValue(p => p.WidgetsViewModel)
            .Subscribe(vm =>
            {
                if (vm == null)
                {
                    return;
                }
                vm.SpaceCollectionUpdate.Subscribe(_ =>
                {
                    SetSpace(SelectedSpace?.Id);
                });
            })
        );
    }

    public WidgetsContainerViewModel(
        AppSettings appSettings,
        SettingsStorage settingsStorage,
        IMvvmService mvvmService,
        ILogger<WidgetsContainerViewModel> logger)
        : this()
    {
        _appSettings = appSettings;
        _settingsStorage = settingsStorage;
        _mvvmService = mvvmService;
        _logger = logger;

        OpenAboutCommand = ReactiveCommand.CreateFromTask(OpenAboutWindow);
        OpenLogsCommand = ReactiveCommand.CreateFromTask(OpenLogsWindow);
        OpenFontsCommand = ReactiveCommand.CreateFromTask(OpenFontsWindow);
        OpenWebsiteCommand = ReactiveCommand.CreateFromTask(OpenWebsite);
        SwitchSpaceCommand = ReactiveCommand.Create<SpaceViewModel>(SwitchSpace);
        ShowTrayIconCommand = ReactiveCommand.Create(ShowTrayIcon);
        FitCommand = ReactiveCommand.Create(Fit);
        CloseCommand = ReactiveCommand.Create(Close);
    }

    #region UI state

    /// <inheritdoc />
    public override async Task SaveUiStateAsync(CancellationToken cancellationToken)
    {
        if (WidgetsViewModel == null || _savingUiState)
        {
            return;
        }
        _savingUiState = true;

        try
        {
            await _settingsStorage.SaveWindowStateAsync(Id, new WindowStateSaveModel
            {
                WindowHeight = WindowHeight,
                WindowWidth = WindowWidth,
                WindowPositionX = WindowPosition.X,
                WindowPositionY = WindowPosition.Y,
                WindowScreen = WindowScreen,
                WidgetsOrder = WidgetsViewModel.Spaces
                    .ToDictionary(s => s.Id, s => s.Widgets.Select(w => w.WidgetId).ToArray()),
                Topmost = Topmost,
                ActiveSpace = SelectedSpace?.Id ?? string.Empty,
            }, cancellationToken);
        }
        finally
        {
            _savingUiState = false;
        }
    }

    /// <inheritdoc />
    public override async Task LoadUiStateAsync(WindowStateModel state, CancellationToken cancellationToken = default)
    {
        await base.LoadUiStateAsync(state, cancellationToken);

        WindowHeight = state.WindowHeight;
        WindowWidth = state.WindowWidth;
        Topmost = state.Topmost;

        if (!string.IsNullOrEmpty(WindowScreen))
        {
            if (state.WindowPositions.TryGetValue(WindowScreen, out var windowPosition))
            {
                WindowPosition = new PixelPoint(windowPosition.X, windowPosition.Y);
            }
        }

        if (WidgetsViewModel != null)
        {
            foreach (var space in WidgetsViewModel.Spaces)
            {
                var order = state.WidgetsOrder.TryGetValue(space.Id, out var widgetsOrder)
                    ? widgetsOrder
                    : space.Widgets.Select(w => w.WidgetId).ToArray();
                space.Widgets = new AvaloniaList<WidgetViewModel>(space.Widgets.Order(new WidgetsIdComparer(order)));
            }
            SetSpace(state.ActiveSpace);
        }
    }

    private void SetSpace(string? spaceId)
    {
        if (!string.IsNullOrEmpty(spaceId) && WidgetsViewModel != null)
        {
            SelectedSpace = WidgetsViewModel.Spaces.FirstOrDefault(s => s.Id == spaceId) ?? SelectedSpace;
        }
    }

    #endregion

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
        cancellationToken.ThrowIfCancellationRequested();

        var mainWindow = _mvvmService.GetMainWindow();
        if (mainWindow == null)
        {
            return;
        }
        await mainWindow.Launcher.LaunchUriAsync(new Uri(MainWebsite));
    }

    private void SwitchSpace(SpaceViewModel space)
    {
        SelectedSpace = space;
    }

    private void ShowTrayIcon()
    {
    }

    private void Fit()
    {
        const int margin = 8;
        var columnsCount = (int)(WindowWidth / WidgetViewModel.DefaultWidgetWidth);
        WindowWidth = columnsCount * WidgetViewModel.DefaultWidgetWidth + margin * 2;
    }

    private void Close()
    {
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _disposables.Dispose();
        base.Dispose(disposing);
    }
}
