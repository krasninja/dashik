using ReactiveUI.Avalonia;
using Dashik.Host.ViewModels;

namespace Dashik.Host.Views;

public partial class AddPackageControl : ReactiveUserControl<AddPackageViewModel>
{
    public AddPackageControl()
    {
        InitializeComponent();
    }
}
