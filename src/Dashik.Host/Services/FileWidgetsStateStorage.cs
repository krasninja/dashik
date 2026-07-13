using System.Text.Json;
using Dashik.Abstractions;

namespace Dashik.Host.Services;

/// <summary>
/// Saves widgets state as JSON file.
/// </summary>
public sealed class FileWidgetsStateStorage : IWidgetsStateStorage
{
    private const string JsonExtension = ".json";

    private readonly IAppService _appService;

    public FileWidgetsStateStorage(IAppService appService)
    {
        _appService = appService;
    }

    /// <inheritdoc />
    public async Task SetStateAsync(object? state, string instanceId, CancellationToken cancellationToken = default)
    {
        var stateDirectory = _appService.GetStateDirectory();
        if (!Directory.Exists(stateDirectory))
        {
            Directory.CreateDirectory(stateDirectory);
        }

        var file = Path.Combine(stateDirectory, instanceId + JsonExtension);
        if (state == null)
        {
            File.Delete(file);
            return;
        }

        await using var settingsFileStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Inheritable);
        await JsonSerializer.SerializeAsync(settingsFileStream, state, cancellationToken: cancellationToken);
        settingsFileStream.Close();
    }

    /// <inheritdoc />
    public async Task<object?> GetStateAsync(Type stateType, string instanceId, CancellationToken cancellationToken = default)
    {
        var stateDirectory = _appService.GetStateDirectory();
        if (!Directory.Exists(stateDirectory))
        {
            return null;
        }

        var file = Path.Combine(stateDirectory, instanceId + JsonExtension);
        if (!File.Exists(file))
        {
            return null;
        }
        await using var fileStream = File.OpenRead(file);
        var model = await JsonSerializer.DeserializeAsync(
            fileStream,
            stateType,
            new JsonSerializerOptions(JsonSerializerDefaults.General),
            cancellationToken);
        return model;
    }
}
