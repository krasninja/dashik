using ReactiveUI;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.ViewModels.Settings;

namespace Dashik.Shared.Views.Settings;

public partial class SettingsWindow : BaseReactiveWindow<SettingsViewModel>
{
    public SettingsWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel == null)
            {
                return;
            }
            ViewModel.WhenAnyValue(x => x.SelectedSection).Subscribe((section) =>
            {
                if (section == null || ViewModel.SelectedSection == null)
                {
                    return;
                }
                ViewModel.SelectedSection.SetSettings(ViewModel.Settings);
            });
        });
    }
}
