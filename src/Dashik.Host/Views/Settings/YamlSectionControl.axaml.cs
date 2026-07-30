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
    private bool _inSync;

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

        ViewModel.PullSettings += ViewModelPullSettings;
        ViewModel.PushSettings += ViewModelPushSettings;
    }

    private void ViewModelPullSettings(object? sender, EventArgs e)
    {
        if (ViewModel == null || _inSync)
        {
            return;
        }
        _inSync = true;

        try
        {
            OnTextChange(Editor.Text);
        }
        finally
        {
            _inSync = false;
        }
    }

    private void ViewModelPushSettings(object? sender, EventArgs e)
    {
        if (ViewModel == null || _inSync)
        {
            return;
        }
        _inSync = true;

        try
        {
            YamlSettingsUpdate(ViewModel.Settings);
        }
        finally
        {
            _inSync = false;
        }
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
        var text = string.Empty;
        if (obj != null && ViewModel != null)
        {
            text = ViewModel.Serializer.Serialize(obj);
        }

        _suppressTextChanged = true;
        Editor.Text = text;
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

        if (ViewModel != null)
        {
            ViewModel.PullSettings -= ViewModelPullSettings;
            ViewModel.PushSettings -= ViewModelPushSettings;
        }
    }
}
