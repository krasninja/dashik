using System.Text.Json;
using Dashik.Abstractions;

namespace Dashik.Shared.Services;

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
    public async Task SetStateAsync(object state, string instanceId, CancellationToken cancellationToken = default)
    {
        var stateDirectory = _appService.GetStateDirectory();
        if (!Directory.Exists(stateDirectory))
        {
            Directory.CreateDirectory(stateDirectory);
        }

        await using var settingsFile = new FileStream(
            Path.Combine(stateDirectory, instanceId + JsonExtension),
            FileMode.Create,
            FileAccess.Write,
            FileShare.Inheritable);
        await JsonSerializer.SerializeAsync(settingsFile, state, cancellationToken: cancellationToken);
        settingsFile.Close();
    }

    /// <inheritdoc />
    public async Task<object?> GetStateAsync(Type stateType, string instanceId, CancellationToken cancellationToken = default)
    {
        var stateDirectory = _appService.GetStateDirectory();
        if (!Directory.Exists(stateDirectory))
        {
            return null;
        }

        await using var fileStream = File.OpenRead(
            Path.Combine(stateDirectory, instanceId + JsonExtension)
        );
        var model = await JsonSerializer.DeserializeAsync(
            fileStream,
            stateType,
            new JsonSerializerOptions(JsonSerializerDefaults.General),
            cancellationToken);
        return model!;
    }
}
