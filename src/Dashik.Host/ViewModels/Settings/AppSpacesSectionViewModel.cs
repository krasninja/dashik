using ReactiveUI;
using ReactiveUI.Primitives;
using Dashik.Host.Models;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Host.ViewModels.Settings;

public class AppSpacesSectionViewModel : SettingsSectionModel
{
    private readonly IMvvmService _mvvmService;

    public AppSettingsObjectViewModel AppSettings => (AppSettingsObjectViewModel)Settings!;

    public SpaceModel? SelectedSpace
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<RxVoid, RxVoid> AddSpaceCommand
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<RxVoid, RxVoid> RemoveSpaceCommand
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <inheritdoc />
    public AppSpacesSectionViewModel(IMvvmService mvvmService)
    {
        _mvvmService = mvvmService;
        AddSpaceCommand = ReactiveCommand.Create(() =>
        {
            var space = new SpaceModel();
            AppSettings.Spaces.Add(space);
            SelectedSpace = space;
        });

        RemoveSpaceCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (SelectedSpace == null || SelectedSpace.Default)
            {
                return;
            }
            var messageBoxVm = new MessageBoxViewModel("Are you sure you want to remove the space?", Resources.Messages.Remove)
                .SetYesNoMode();
            if (await _mvvmService.OpenAsync(messageBoxVm, this) == DialogResult.Yes)
            {
                AppSettings.Spaces.Remove(SelectedSpace);
            }
        });
    }
}
