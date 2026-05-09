using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Services.Widgets;

/// <summary>
/// The widget is to provide system, error notifications. When, for example, the widget
/// cannot be loaded.
/// </summary>
[WidgetInfo(
    id: "com.dashik.widgets.stub",
    name: "Not Found",
    Description = "The stub widget for widgets which packages were not found or any other error."
)]
internal sealed class StubWidget : ReactiveObject, IWidget
{
    /// <inheritdoc />
    public string Header
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
        = "ERROR";

    public string Text
    {
        get => _textBlock.Text ?? string.Empty;
        set => _textBlock.Text = value;
    }

    public bool Error
    {
        get;
        set
        {
            if (this.RaiseAndSetIfChanged(ref field, value))
            {
                _textBlock.Foreground = value ? Brushes.Red : Brushes.Black;
            }
        }
    }

    private readonly TextBlock _textBlock;
    private readonly StackPanel _rootPanel;

    /// <inheritdoc />
    public Control Control => _rootPanel;

    public StubWidget()
    {
        _rootPanel = new StackPanel();
        _textBlock = new TextBlock
        {
            Foreground = Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Text = "Error",
            TextWrapping = TextWrapping.WrapWithOverflow,
            Padding = new Thickness(2),
        };
        _rootPanel.Children.Add(_textBlock);
    }

    /// <inheritdoc />
    public Task InitializeAsync(WidgetInitInfo initInfo, CancellationToken cancellationToken = default)
    {
        if (initInfo.Context is TransientWidgetInstance transientInstance)
        {
            if (!string.IsNullOrEmpty(transientInstance.Message))
            {
                Text = transientInstance.Message;
            }
            if (!string.IsNullOrEmpty(transientInstance.Title))
            {
                Header = transientInstance.Title;
            }
            Error = transientInstance.Error;

            if (transientInstance.RequiresSetup)
            {
                var button = new Button
                {
                    Content = "Setup",
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                button.Click += (_, _) =>
                {
                    if (Control.DataContext is WidgetViewModel widgetViewModel)
                    {
                        widgetViewModel.OpenWidgetSettingsCommand.Execute(widgetViewModel)
                            .Subscribe();
                    }
                };
                _rootPanel.Children.Add(button);
            }
        }
        return Task.CompletedTask;
    }
}
