using ReactiveUI.Avalonia;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Views;

public partial class AddPackageControl : ReactiveUserControl<AddPackageViewModel>
{
    public AddPackageControl()
    {
        InitializeComponent();
    }
}
