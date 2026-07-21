using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Utils;
using Dashik.Host.Views.Settings;

namespace Dashik.Host.ViewModels.Settings;

/// <summary>
/// Main view model for settings that contains sections and save functionality.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase, ICloseableViewModel, IDialogViewModel<int>
{
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<SettingsSectionViewModel> Sections { get; } = new();

    private SettingsSectionViewModel? _selectedSection;

    public SettingsSectionViewModel? SelectedSection
    {
        get => _selectedSection;
        set
        {
            _selectedSection?.SetSettings(null);
            this.RaiseAndSetIfChanged(ref _selectedSection, value);
            _selectedSection?.SetSettings(Settings);
        }
    }

    /// <summary>
    /// Settings object.
    /// </summary>
    public object Settings
    {
        get => field;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    /// <inheritdoc />
    public int ResultValue { get; } = 0;

    /// <inheritdoc />
    public DialogResult Result { get; private set; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public SettingsViewModel(object settings, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var cloner = new AppCloner();
        Settings = cloner.Clone(settings);

        CancelCommand = ReactiveCommand.Create(Cancel);
        SaveCommand = ReactiveCommand.Create(Save);

        this.WhenAnyValue(x => x.Settings)
            .Subscribe(localSettings =>
            {
                foreach (var section in Sections)
                {
                    section.SetSettings(localSettings);
                }
            });
    }

    public void AddYamlSection()
    {
        var yamlSection = SettingsSection.Create<YamlSectionControl, YamlSectionViewModel>("YAML");
        AddSection(yamlSection);
    }

    public void AddSection(SettingsSection section, Func<object, object?>? settingsFunc = null)
    {
        var control = (Control)_serviceProvider.GetService(section.ControlType)!;
        var viewModel = (SettingsSectionModel)_serviceProvider.GetService(section.ViewModelType)!;
        var sectionTabViewModel = new SettingsSectionViewModel(section.Name, control, viewModel)
        {
            Icon = section.Icon,
            SettingsFunc = settingsFunc,
        };
        sectionTabViewModel.SetSettings(Settings);
        Sections.Add(sectionTabViewModel);
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (Sections.Count > 0)
        {
            SelectedSection = Sections[0];
        }

        foreach (var section in Sections)
        {
            await section.LoadAsync(cancellationToken);
        }
        await base.LoadAsync(cancellationToken);
    }

    private void Cancel()
    {
        Result = DialogResult.Cancel;
        Close();
    }

    private void Save()
    {
        Result = DialogResult.OK;
        Close();
    }

    private void Close()
    {
        foreach (var section in Sections)
        {
            if (section.Control is IDisposable disposableControl)
            {
                disposableControl.Dispose();
            }
        }
        CloseRequest?.Invoke(this, EventArgs.Empty);
    }
}
