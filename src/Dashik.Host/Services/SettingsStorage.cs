using System.Text.Json;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.Setup;
using Dashik.Host.Models;

namespace Dashik.Host.Services;

public sealed class SettingsStorage
{
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
        var appDirectory = _appService.GetConfigDirectory();
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
    /// Get all windows ids.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of window ids.</returns>
    public Task<string[]> GetWindowsIdsAsync(CancellationToken cancellationToken = default)
    {
        var windowsDirectory = _appService.GetWindowsDirectory();
        if (!Directory.Exists(windowsDirectory))
        {
            return Task.FromResult(Array.Empty<string>());
        }

        var windowsFiles = Directory.EnumerateFiles(windowsDirectory, "*.json", SearchOption.TopDirectoryOnly);
        var ids = windowsFiles.Select(id => Path.GetFileNameWithoutExtension(id)).ToArray();
        return Task.FromResult(ids);
    }

    /// <summary>
    /// Load all windows.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all windows states.</returns>
    public async Task<IReadOnlyCollection<WindowStateModel>> LoadWindowsStatesAsync(CancellationToken cancellationToken = default)
    {
        var ids = await GetWindowsIdsAsync(cancellationToken);

        var list = new List<WindowStateModel>();
        foreach (var id in ids)
        {
            var windowState = await LoadWindowStateAsync(id, cancellationToken);
            list.Add(windowState);
        }
        return list;
    }

    /// <summary>
    /// Get window sate model.
    /// </summary>
    /// <param name="id">Window identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>State model.</returns>
    public async Task<WindowStateModel> LoadWindowStateAsync(string id, CancellationToken cancellationToken = default)
    {
        var windowsDirectory = _appService.GetWindowsDirectory();
        var file = Path.Combine(windowsDirectory, id + ".json");
        if (!File.Exists(file))
        {
            return new WindowStateModel();
        }

        await using var fileStream = File.OpenRead(file);
        try
        {
            var model = await JsonSerializer.DeserializeAsync<WindowStateModel>(
                fileStream,
                SourceGenerationContext.Default.WindowStateModel,
                cancellationToken);
            return model!;
        }
        catch (JsonException e)
        {
            _logger.LogWarning(e, e.Message);
        }

        return new WindowStateModel();
    }

    /// <summary>
    /// Save window state model.
    /// </summary>
    /// <param name="id">Window identifier.</param>
    /// <param name="windowState">Window state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task SaveWindowStateAsync(string id, WindowStateSaveModel windowState, CancellationToken cancellationToken = default)
    {
        var windowsDirectory = _appService.GetWindowsDirectory();
        var file = Path.Combine(windowsDirectory, id + ".json");
        if (!Directory.Exists(windowsDirectory))
        {
            Directory.CreateDirectory(windowsDirectory);
        }

        var model = await LoadWindowStateAsync(id, cancellationToken);
        model.Id = id;
        model.WindowHeight = windowState.WindowHeight;
        model.WindowWidth = windowState.WindowWidth;
        if (!string.IsNullOrEmpty(windowState.WindowScreen)
            && windowState.WindowPositionX.HasValue
            && windowState.WindowPositionY.HasValue)
        {
            model.WindowPositions[windowState.WindowScreen] = new WindowStateModel.WindowPosition
            {
                X = windowState.WindowPositionX.Value,
                Y = windowState.WindowPositionY.Value,
            };
        }
        model.ActiveSpace = windowState.ActiveSpace;
        model.Topmost = windowState.Topmost;

        // Save spaces.
        foreach (var widgetsOrder in windowState.WidgetsOrder)
        {
            model.WidgetsOrder[widgetsOrder.Key] = widgetsOrder.Value;
        }

        await using var fileStream = File.Open(file, FileMode.Create);
        await JsonSerializer.SerializeAsync(fileStream, model,
            SourceGenerationContext.Default.WindowStateModel, cancellationToken)!;
    }
}
