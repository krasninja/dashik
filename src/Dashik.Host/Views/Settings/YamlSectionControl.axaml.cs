using System.Reactive.Linq;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using ReactiveUI.Avalonia;
using TextMateSharp.Grammars;
using YamlDotNet.Core;
using Dashik.Host.Utils;
using Dashik.Host.ViewModels.Settings;

namespace Dashik.Host.Views.Settings;

public sealed partial class YamlSectionControl : ReactiveUserControl<YamlSectionViewModel>, IDisposable
{
    private readonly IDisposable _textChangedSubscription;
    private bool _suppressTextChanged;
    private bool _pendingChanges;

    public YamlSectionControl()
    {
        InitializeComponent();

        var registryOptions = new RegistryOptions(ThemeName.LightPlus);
        var textMateInstallation = Editor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".yaml").Id));

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

        ViewModel.WhenAnyValue(x => x.Settings)
            .Subscribe(YamlSettingsUpdate);

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
                var settings = ViewModel.Deserializer.Deserialize(obj, ViewModel.Settings.GetType());
                AppCloner.CloneObjectTo(settings, ViewModel.Settings);
            }
            _pendingChanges = false;
            ViewModel.YamlError = string.Empty;
        }
        catch (YamlException e)
        {
            var message = e.InnerException != null ? e.InnerException.Message : e.Message;
            ViewModel.YamlError = $"(line {e.Start.Line}, col {e.Start.Column}): {message}";
        }
    }

    private void YamlSettingsUpdate(object? obj)
    {
        if (obj == null)
        {
            return;
        }

        _suppressTextChanged = true;
        Editor.Text = ViewModel!.Serializer.Serialize(obj);
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
