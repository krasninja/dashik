using ReactiveUI;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.ViewModels.Settings;

namespace Dashik.Host.Views.Settings;

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
