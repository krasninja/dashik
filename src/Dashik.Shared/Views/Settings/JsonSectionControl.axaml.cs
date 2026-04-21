using System.Reactive.Linq;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using ReactiveUI.Avalonia;
using TextMateSharp.Grammars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dashik.Shared.ViewModels.Settings;
using Dashik.Shared.Utils;

namespace Dashik.Shared.Views.Settings;

public sealed partial class JsonSectionControl : ReactiveUserControl<JsonSectionViewModel>, IDisposable
{
    private readonly IDisposable _textChangedSubscription;
    private bool _suppressTextChanged;
    private bool _pendingChanges;

    public JsonSectionControl()
    {
        InitializeComponent();

        var registryOptions = new RegistryOptions(ThemeName.LightPlus);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".json").Id));

        _textChangedSubscription = Observable.FromEventPattern(
                h => Editor.TextChanged += h,
                h => Editor.TextChanged -= h)
            .Where(_ => !_suppressTextChanged)
            .Do(_ => _pendingChanges = true)
            .Select(_ => Editor.Text)
            .Throttle(TimeSpan.FromSeconds(2))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(OnTextChange);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        ViewModel.WhenAnyValue(x => x.JsonSettings)
            .Subscribe(JsonSettingsUpdate);

        ViewModel.Sync += ViewModelSync;
    }

    private void ViewModelSync(object? sender, EventArgs e)
    {
        OnTextChange(Editor.Text);
    }

    private void OnTextChange(string obj)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            if (ViewModel.Settings != null)
            {
                var settings = JsonConvert.DeserializeObject(obj, ViewModel.Settings.GetType(), ViewModel.JsonSerializerOptions);
                AppCloner.CloneObjectTo(settings, ViewModel.Settings);
            }
            _pendingChanges = false;
            ViewModel.JsonError = string.Empty;
        }
        catch (JsonException e)
        {
            ViewModel.JsonError = e.Message;
        }
    }

    private void JsonSettingsUpdate(object? obj)
    {
        if (obj == null)
        {
            return;
        }

        var jsonDocument = (JObject)obj;
        _suppressTextChanged = true;
        Editor.Text = jsonDocument.ToString(Formatting.Indented);
        _suppressTextChanged = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _textChangedSubscription.Dispose();
        if (_pendingChanges)
        {
            OnTextChange(Editor.Text);
        }
    }
}
