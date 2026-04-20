using ReactiveUI.Avalonia;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Views;

public partial class WidgetControl : ReactiveUserControl<WidgetViewModel>
{
    public WidgetControl()
    {
        InitializeComponent();
    }
}
