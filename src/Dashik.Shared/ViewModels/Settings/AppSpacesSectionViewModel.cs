using System.Reactive;
using ReactiveUI;
using Dashik.Shared.Models;
using Dashik.Sdk.Models;

namespace Dashik.Shared.ViewModels.Settings;

public class AppSpacesSectionViewModel : SettingsSectionModel
{
    public AppSettingsViewModel AppSettings => (AppSettingsViewModel)Settings!;

    public SpaceModel? SelectedSpace
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> AddSpaceCommand
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit> RemoveSpaceCommand
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <inheritdoc />
    public AppSpacesSectionViewModel()
    {
        AddSpaceCommand = ReactiveCommand.Create(() =>
        {
            var space = new SpaceModel();
            AppSettings.Spaces.Add(space);
            SelectedSpace = space;
        });

        RemoveSpaceCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedSpace == null || SelectedSpace.Default)
            {
                return;
            }
            AppSettings.Spaces.Remove(SelectedSpace);
        });
    }
}
