using Dashik.Sdk.Mvvm;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Dashik.Shared.Infrastructure.UI;

public class BaseReactiveWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public BaseReactiveWindow()
    {
        this.WhenActivated(disposables =>
        {
            if (ViewModel is ICloseableViewModel closeableViewModel)
            {
                closeableViewModel.CloseRequest += (sender, args) =>
                {
                    disposables.Dispose();
                    this.Close();
                };
            }
        });
    }
}
