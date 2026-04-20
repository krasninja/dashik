using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Dashik.Sdk.ViewModels;

namespace Dashik.Sdk.Views;

public partial class MessageBoxWindow : ReactiveWindow<MessageBoxViewModel>
{
    public MessageBoxWindow()
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

        this.AttachedToVisualTree += (s, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                var defaultButton = this.GetLogicalDescendants()
                    .OfType<Button>()
                    .FirstOrDefault(b => b.IsDefault && b.IsVisible);

                if (defaultButton != null)
                {
                    defaultButton.Focus();
                }
            }, DispatcherPriority.Loaded);
        };
    }
}
