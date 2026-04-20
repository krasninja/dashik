using System.Text.Json;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Shared.Infrastructure.Setup;
using Dashik.Shared.Models;

namespace Dashik.Shared.Services;

public sealed class SettingsStorage
{
    private const string WindowStateFileName = "window-state.json";

    private readonly IAppService _appService;
    private readonly ILogger<SettingsStorage> _logger;

    public SettingsStorage(IAppService appService, ILogger<SettingsStorage> logger)
    {
        _appService = appService;
        _logger = logger;
    }

    /// <summary>
    /// Save settings.
    /// </summary>
    /// <param name="settings">App settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        var appDirectory = _appService.GetDataDirectory();
        await using var settingsFile = new FileStream(
            Path.Combine(appDirectory, AppServicesSetup.SettingsFileName),
            FileMode.Create,
            FileAccess.Write,
            FileShare.Inheritable);
        await JsonSerializer.SerializeAsync(settingsFile, settings, SourceGenerationContext.Default.AppSettings,
            cancellationToken: cancellationToken);
        settingsFile.Close();
    }

    /// <summary>
    /// Get window sate model.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>State model.</returns>
    public async Task<MainWindowStateModel> LoadWindowStateAsync(CancellationToken cancellationToken = default)
    {
        var appDirectory = _appService.GetDataDirectory();
        var file = Path.Combine(appDirectory, WindowStateFileName);
        if (!File.Exists(file))
        {
            return new MainWindowStateModel();
        }

        await using var fileStream = File.OpenRead(file);
        try
        {
            var model = await JsonSerializer.DeserializeAsync<MainWindowStateModel>(
                fileStream,
                SourceGenerationContext.Default.MainWindowStateModel,
                cancellationToken);
            return model!;
        }
        catch (JsonException e)
        {
            _logger.LogWarning(e, e.Message);
        }

        return new MainWindowStateModel();
    }

    /// <summary>
    /// Save window state model.
    /// </summary>
    /// <param name="mainWindowState">Window state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task SaveWindowStateAsync(MainWindowStateSaveModel mainWindowState, CancellationToken cancellationToken = default)
    {
        var appDirectory = _appService.GetDataDirectory();
        var file = Path.Combine(appDirectory, WindowStateFileName);

        var model = await LoadWindowStateAsync(cancellationToken);
        model.WindowHeight = mainWindowState.WindowHeight;
        model.WindowWidth = mainWindowState.WindowWidth;
        if (!string.IsNullOrEmpty(mainWindowState.WindowScreen)
            && mainWindowState.WindowPositionX.HasValue
            && mainWindowState.WindowPositionY.HasValue)
        {
            model.WindowPositions[mainWindowState.WindowScreen] = new MainWindowStateModel.WindowPosition()
            {
                X = mainWindowState.WindowPositionX.Value,
                Y = mainWindowState.WindowPositionY.Value,
            };
        }
        model.ActiveSpace = mainWindowState.ActiveSpace;
        model.Topmost = mainWindowState.Topmost;

        // Save spaces.
        foreach (var widgetsOrder in mainWindowState.WidgetsOrder)
        {
            model.WidgetsOrder[widgetsOrder.Key] = widgetsOrder.Value;
        }

        await using var fileStream = File.Open(file, FileMode.Create);
        await JsonSerializer.SerializeAsync(fileStream, model,
            SourceGenerationContext.Default.MainWindowStateModel, cancellationToken)!;
    }
}
