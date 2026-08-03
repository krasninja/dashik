using System.Collections.ObjectModel;
using Avalonia.Controls;
using Dashik.Abstractions;
using ReactiveUI;
using ReactiveUI.Primitives;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Utils;
using Dashik.Host.Views.Settings;

namespace Dashik.Host.ViewModels.Settings;

/// <summary>
/// Main view model for settings that contains sections and save functionality.
/// It is the container for all the settings.
/// </summary>
public sealed class SettingsViewModel : ViewModelBase, ICloseableViewModel, IDialogViewModel<int>
{
    /**
     * SettingsViewModel:
     * - **GeneralSettings** (object)
     * - SelectedSection
     * - Sections[] (SettingsSectionViewModel):
     *   - Title
     *   - Control (Control)
     *   - SettingsConverter (ISectionSettingsConverter)
     *   - ControlModel (Sdk.Models.SettingsSectionModel):
     *     - **Settings** (object)
     *
     */

    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<SettingsSectionViewModel> Sections { get; } = new();

    private SettingsSectionViewModel? _selectedSection;

    public SettingsSectionViewModel? SelectedSection
    {
        get => _selectedSection;
        set
        {
            var originalSelectedSection = _selectedSection;
            this.RaiseAndSetIfChanged(ref _selectedSection, value);

            // Get updated settings from previous selected section.
            if (originalSelectedSection != null && originalSelectedSection.ControlModelSettings != null)
            {
                GeneralSettings = originalSelectedSection.SettingsConverter
                    .ConvertBack(GeneralSettings, originalSelectedSection.ControlModelSettings);
            }

            // Set settings to the new section.
            if (_selectedSection != null)
            {
                _selectedSection.SetSettings(GeneralSettings);
            }
        }
    }

    /// <summary>
    /// Settings object.
    /// </summary>
    public object GeneralSettings
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <inheritdoc />
    public event EventHandler? CloseRequest;

    /// <inheritdoc />
    public int ResultValue { get; } = 0;

    /// <inheritdoc />
    public DialogResult Result { get; private set; }

    public ReactiveCommand<RxVoid, RxVoid> CancelCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> SaveCommand { get; }

    public SettingsViewModel(object settings, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var cloner = new AppCloner();
        GeneralSettings = cloner.Clone(settings);

        CancelCommand = ReactiveCommand.Create(Cancel);
        SaveCommand = ReactiveCommand.Create(Save);
    }

    public void AddYamlSection()
    {
        var yamlSection = SettingsSection.Create<YamlSectionControl, YamlSectionViewModel>("YAML");
        AddSection(yamlSection);
    }

    public void AddSection(
        SettingsSection section,
        ISectionSettingsConverter? settingsConverter = null)
    {
        var control = (Control)_serviceProvider.GetService(section.ControlType)!;
        var viewModel = (SettingsSectionModel)_serviceProvider.GetService(section.ViewModelType)!;
        var sectionTabViewModel = new SettingsSectionViewModel(section.Name, control, viewModel, settingsConverter)
        {
            Icon = section.Icon,
        };
        sectionTabViewModel.SetSettings(GeneralSettings);
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
