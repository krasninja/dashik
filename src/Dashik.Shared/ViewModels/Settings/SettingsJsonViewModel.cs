using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class SettingsJsonViewModel : SettingsSectionModel
{
    private JObject _jsonSettings;
    private readonly JsonSerializer _serializer;

    public JObject JsonSettings
    {
        get => _jsonSettings;
        set => this.RaiseAndSetIfChanged(ref _jsonSettings, value);
    }

    public string JsonError
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public event EventHandler? Sync;

    internal JsonSerializerSettings JsonSerializerOptions { get; } = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
    };

    /// <inheritdoc />
    public SettingsJsonViewModel()
    {
        _jsonSettings = JObject.Parse("{}");
        _serializer = JsonSerializer.Create(JsonSerializerOptions);
        JsonError = string.Empty;

        this.WhenAnyValue(x => x.Settings)
            .Subscribe(SettingsUpdate);
    }

    /// <inheritdoc />
    public override void SyncSetting()
    {
        Sync?.Invoke(this, EventArgs.Empty);
        base.SyncSetting();
    }

    private void SettingsUpdate(object? obj)
    {
        JsonSettings = obj == null ? JObject.Parse("{}") : JObject.FromObject(obj, _serializer);
    }
}
