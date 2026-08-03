using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;
using ReactiveUI.Primitives.Disposables;
using Dashik.Sdk.ViewModels;

namespace Dashik.Sdk.Views;

public sealed partial class MessageBoxWindow : ReactiveWindow<MessageBoxViewModel>, IDisposable
{
    private const int TopBottomPaddings = 80;

    private readonly MultipleDisposable _disposables = new();

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
            ViewModel.WhenAnyValue(p => p.Message)
                .Subscribe(new DelegateWitness<string>(message =>
                {
                    Height = MessageTextBlock.Height + TopBottomPaddings;
                }))
                .DisposeWith(_disposables);
        }).DisposeWith(_disposables);

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

    /// <inheritdoc />
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
