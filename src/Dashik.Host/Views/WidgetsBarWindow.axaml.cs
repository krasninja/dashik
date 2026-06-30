using ReactiveUI.Avalonia;
using Dashik.Host.ViewModels;

namespace Dashik.Host.Views;

public partial class WidgetsBarWindow : ReactiveWindow<WidgetsBarViewModel>
{
    public WidgetsBarWindow()
    {
        InitializeComponent();
    }
}
