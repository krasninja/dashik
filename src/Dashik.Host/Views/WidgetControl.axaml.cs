using ReactiveUI.Avalonia;
using Dashik.Host.ViewModels;

namespace Dashik.Host.Views;

public partial class WidgetControl : ReactiveUserControl<WidgetViewModel>
{
    public WidgetControl()
    {
        InitializeComponent();
    }
}
