using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.ViewModels;

namespace Dashik.Host.Views;

public sealed partial class WidgetsContainerWindow : BaseReactiveWindow<WidgetsContainerViewModel>, IDisposable
{
    private readonly MultipleDisposable _disposables = new();

    public WidgetsContainerWindow()
    {
        InitializeComponent();

        this.WhenAnyValue(p => p.DataContext)
            .Subscribe(_ =>
            {
                if (ViewModel == null)
                {
                    return;
                }
                var screen = Screens.Primary;
                ViewModel.WindowScreen = screen != null && !string.IsNullOrEmpty(screen.DisplayName)
                    ? screen.DisplayName : string.Empty;
            })
            .DisposeWith(_disposables);
    }

    /// <inheritdoc />
    protected override async void OnOpened(EventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        if (ViewModel.ApplicationSettings.StartMinimized)
        {
            WindowState = WindowState.Minimized;
        }

        await ViewModel.LoadAsync(CancellationToken.None);
        ViewModel.WhenAnyValue(p => p.WindowPosition)
            .Subscribe(pos =>
            {
                // For some reason sometimes we get negative or zero X and Y.
                if (pos.X > 0 && pos.Y > 0)
                {
                    Position = new PixelPoint(pos.X, pos.Y);
                }
            })
            .DisposeWith(_disposables);
        ViewModel.WhenAnyValue(p => p.Topmost)
            .Subscribe(isTopmost =>
            {
                this.Topmost = isTopmost;
            })
            .DisposeWith(_disposables);

        base.OnOpened(e);
    }

    private void WindowBase_OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }
        ViewModel.WindowPosition = Position;
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        Focus();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
