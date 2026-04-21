using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Utils;
using Dashik.Shared.Views.Settings;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;

namespace Dashik.Shared.ViewModels.Settings;

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

    public void AddJsonSection()
    {
        var jsonSection = SettingsSection.Create<JsonSectionControl, JsonSectionViewModel>("JSON");
        AddSection(jsonSection);
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
    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (Sections.Count > 0)
        {
            SelectedSection = Sections[0];
        }
        return base.LoadAsync(cancellationToken);
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
