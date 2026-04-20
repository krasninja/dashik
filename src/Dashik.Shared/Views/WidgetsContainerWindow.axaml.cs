using Avalonia;
using Avalonia.Controls;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Views;

public partial class WidgetsContainerWindow : ReactiveWindow<WidgetsContainerViewModel>
{
    private readonly ILogger _logger = global::QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(nameof(WidgetsContainerWindow));

    public WidgetsContainerWindow()
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
            });
    }

    /// <inheritdoc />
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        try
        {
            await ViewModel.SaveAsync();
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, exception.Message);
        }

        base.OnClosing(e);
    }

    /// <inheritdoc />
    protected override async void OnOpened(EventArgs e)
    {
        if (ViewModel == null)
        {
            return;
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
            });
        ViewModel.WhenValueChanged(p => p.Topmost)
            .Subscribe(isTopmost =>
            {
                this.Topmost = isTopmost;
            });

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
}
