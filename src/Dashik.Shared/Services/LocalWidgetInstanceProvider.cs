using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Shared.Services.Widgets;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Models;

namespace Dashik.Shared.Services;

public sealed class LocalWidgetInstanceProvider : IWidgetInstanceProvider
{
    private const string MainSettingsPropertyName = "mainSettings";
    private const string WidgetSettingsPropertyName = "widgetSettings";

    private readonly IAppService _service;
    private readonly IWidgetsProvider _widgetsProvider;
    private readonly ILogger<LocalWidgetInstanceProvider> _logger;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public LocalWidgetInstanceProvider(
        IAppService service,
        IWidgetsProvider widgetsProvider,
        ILogger<LocalWidgetInstanceProvider> logger)
    {
        _service = service;
        _widgetsProvider = widgetsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IWidgetInstance>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var instances = new List<WidgetInstance>();
        var widgets = _widgetsProvider.GetAll().ToDictionary(w => w.Id, w => w);

        var path = _service.GetInstancesDirectory();
        if (!Directory.Exists(path))
        {
            return [];
        }
        var settingsFiles = Directory.EnumerateFiles(path, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var settingsFile in settingsFiles)
        {
            var instanceFileName = Path.GetFileNameWithoutExtension(settingsFile);
            var dashIndex = instanceFileName.LastIndexOf('-');
            if (dashIndex < 0)
            {
                continue;
            }

            // Open settings file.
            await using var settingsFileStream = File.OpenRead(settingsFile);

            // Find related widget info.
            var widgetTypeId = instanceFileName[..dashIndex];
            var widgetId = instanceFileName[(dashIndex + 1)..];
            var instance = (WidgetInstance?)null;
            if (!widgets.TryGetValue(widgetTypeId, out var info))
            {
                _logger.LogWarning("Cannot find widget '{WidgetTypeId}' for file '{File}'.",
                    widgetTypeId, Path.GetFileName(settingsFile));
                info = new WidgetInfo(new WidgetInfoAttribute(widgetTypeId, "NOTFOUND"), typeof(StubWidget));
                instance = new TransientWidgetInstance(id: widgetId, widgetInfo: info)
                {
                    Title = $"Cannot Load {widgetId}",
                    Message = $"Cannot find package for widget '{widgetId}' of type '{widgetTypeId}'.",
                    Error = true,
                };
            }

            // Create widget instance and init it.
            instance ??= new WidgetInstance(id: widgetId, widgetInfo: info);
            try
            {
                var settingsJson = await JsonSerializer.DeserializeAsync<JsonObject>(
                    settingsFileStream, cancellationToken: cancellationToken);
                if (settingsJson == null)
                {
                    continue;
                }

                var mainSettings = settingsJson[MainSettingsPropertyName].Deserialize<WidgetMainSettings>(_jsonSerializerOptions)
                                   ?? new WidgetMainSettings();
                var widgetSettings = settingsJson[WidgetSettingsPropertyName]?.AsObject()
                                     ?? new JsonObject();
                instance.WidgetSettings = widgetSettings;
                instance.MainSettings = mainSettings;
            }
            catch (JsonException e)
            {
                _logger.LogWarning("Cannot deserialize widget instance settings file '{SettingsFile}': {ErrorMessage}",
                    settingsFile, e.Message);
            }

            instances.Add(instance);
        }

        return instances;
    }

    /// <inheritdoc />
    public async Task SaveAsync(IWidgetInstance instance, CancellationToken cancellationToken = default)
    {
        var path = _service.GetInstancesDirectory();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var instanceFileName = FormatInstanceFileName(path, instance);
        await using var settingsFileStream = new FileStream(instanceFileName, FileMode.Create, FileAccess.Write, FileShare.Inheritable);
        var jsonObject = new JsonObject
        {
            [MainSettingsPropertyName] = JsonSerializer.SerializeToNode(instance.MainSettings, _jsonSerializerOptions),
            [WidgetSettingsPropertyName] = instance.WidgetSettings.DeepClone(),
        };
        await JsonSerializer.SerializeAsync(settingsFileStream, jsonObject, _jsonSerializerOptions, cancellationToken: cancellationToken);
        settingsFileStream.Close();
    }

    /// <inheritdoc />
    public Task RemoveAsync(IWidgetInstance instance, CancellationToken cancellationToken = default)
    {
        var path = _service.GetInstancesDirectory();
        if (!Directory.Exists(path))
        {
            return Task.CompletedTask;
        }
        var instanceFileName = FormatInstanceFileName(path, instance);
        File.Delete(instanceFileName);
        return Task.CompletedTask;
    }

    private static string FormatInstanceFileName(string path, IWidgetInstance instance)
    {
        return Path.Combine(path, string.Join('-', instance.Info.Id, instance.Id) + ".json");
    }
}
