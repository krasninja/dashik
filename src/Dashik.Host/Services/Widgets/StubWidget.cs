using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.ViewModels;
using ReactiveUI;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.Services.Widgets;

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

    /// <summary>
    /// Stub text.
    /// </summary>
    public string Text
    {
        get => _viewModel.Text;
        set { _viewModel.Text = value; }
    }

    /// <summary>
    /// Do we render the widget because of error?
    /// </summary>
    public bool Error
    {
        get => _viewModel.Error;
        set { _viewModel.Error = value; }
    }

    private readonly StubWidgetViewModel _viewModel = new();

    public WidgetViewModel? OriginalWidgetViewModel
    {
        get => _viewModel.OriginalWidgetViewModel;
        set => _viewModel.OriginalWidgetViewModel = value;
    }

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

        public bool RequiresSetup
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        public WidgetViewModel? OriginalWidgetViewModel
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
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
            Path = nameof(StubWidgetViewModel.Text),
            Mode = BindingMode.TwoWay,
        };
        textBlock.Bind(TextBlock.TextProperty, textBinding);
        var textColorBinding = new Binding
        {
            Path = nameof(StubWidgetViewModel.TextColor),
            Mode = BindingMode.TwoWay,
        };
        textBlock.Bind(TextBlock.ForegroundProperty, textColorBinding);

        var requiresSetupButton = new Button
        {
            Content = "Setup",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        requiresSetupButton.Click += (obj, _) =>
        {
            if (rootPanel.DataContext is WidgetViewModel widgetViewModel)
            {
                widgetViewModel.OpenWidgetSettingsCommand.Execute(widgetViewModel)
                    .Subscribe();
            }
            if (_viewModel.OriginalWidgetViewModel != null)
            {
                _viewModel.OriginalWidgetViewModel.OpenWidgetSettingsCommand.Execute(_viewModel.OriginalWidgetViewModel)
                    .Subscribe();
            }
        };
        var requiresSetupButtonBinding = new Binding
        {
            Path = nameof(StubWidgetViewModel.RequiresSetup),
            Mode = BindingMode.TwoWay,
        };
        requiresSetupButton.Bind(Visual.IsVisibleProperty, requiresSetupButtonBinding);

        rootPanel.Children.Add(textBlock);
        rootPanel.Children.Add(requiresSetupButton);
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

            _viewModel.RequiresSetup = transientInstance.RequiresSetup;
        }
        return Task.CompletedTask;
    }
}
