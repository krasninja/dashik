using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using ReactiveUI;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.ViewModels;

namespace Dashik.Shared.Services.Widgets;

/// <summary>
/// The widget is to provide system, error notifications. When, for example, the widget
/// cannot be loaded.
/// </summary>
[WidgetInfo(
    id: "com.dashik.widgets.stub",
    name: "Stub",
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
        get => _viewModel.Text;
        set { _viewModel.Text = value; }
    }

    public bool Error
    {
        get => _viewModel.Error;
        set { _viewModel.Error = value; }
    }

    private readonly StubWidgetViewModel _viewModel = new();
    private readonly StackPanel _rootPanel;

    /// <inheritdoc />
    public Control Control => _rootPanel;

    private sealed class StubWidgetViewModel : ViewModelBase
    {
        public string Text
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }
        = string.Empty;

        public IBrush TextColor
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }
        = Brushes.Black;

        public bool Error
        {
            get;
            set
            {
                this.RaiseAndSetIfChanged(ref field, value);
                TextColor = value ? Brushes.Red : Brushes.Black;
            }
        }
    }

    /// <inheritdoc />
    public Control? CreateControl(WidgetControlTarget target, Size targetSize)
    {
        var rootPanel = new StackPanel();
        var textBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.WrapWithOverflow,
            Padding = new Thickness(2),
        };

        var textBinding = new Binding
        {
            Source = textBlock,
            Path = nameof(StubWidgetViewModel.Text),
            Mode = BindingMode.TwoWay,
        };
        textBlock.Bind(TextBlock.TextProperty, textBinding);

        var textColorBinding = new Binding
        {
            Source = textBlock,
            Path = nameof(StubWidgetViewModel.TextColor),
            Mode = BindingMode.TwoWay,
        };
        textBlock.Bind(TextBlock.ForegroundProperty, textColorBinding);

        rootPanel.Children.Add(textBlock);
        rootPanel.DataContext = _viewModel;
        return rootPanel;
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
