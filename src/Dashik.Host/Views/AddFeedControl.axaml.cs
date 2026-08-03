using ReactiveUI.Avalonia;
using Dashik.Host.ViewModels;

namespace Dashik.Host.Views;

public partial class AddFeedControl : ReactiveUserControl<AddFeedViewModel>
{
    public AddFeedControl()
    {
        InitializeComponent();
    }
}
