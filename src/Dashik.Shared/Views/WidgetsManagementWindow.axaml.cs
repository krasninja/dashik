using ReactiveUI;
using ReactiveUI.Avalonia;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Views;

public partial class WidgetsManagementWindow : ReactiveWindow<WidgetsManagementViewModel>
{
    public WidgetsManagementWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel == null)
            {
                return;
            }
            ViewModel.CloseRequest += (sender, args) =>
            {
                disposables.Dispose();
                this.Close();
            };

            // Select "packages" tab if no widgets installed.
            if (ViewModel.AddWidgetViewModel.WidgetsCount < 1)
            {
                MainTab.SelectedIndex = 1;
            }
        });
    }
}
