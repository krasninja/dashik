using ReactiveUI;
using ReactiveUI.Avalonia;
using Dashik.Sdk.ViewModels;

namespace Dashik.Sdk.Views;

public partial class TextWindow : ReactiveWindow<TextWindowViewModel>
{
    public TextWindow()
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
        });
    }
}
