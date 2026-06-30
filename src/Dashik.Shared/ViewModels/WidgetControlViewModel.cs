using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Dashik.Sdk.Abstract;
using Dashik.Shared.Infrastructure.UI;

namespace Dashik.Shared.ViewModels;

public sealed class WidgetControlViewModel : ViewModelBase, IDisposable
{
    private readonly WidgetViewModel _widgetViewModel;
    private readonly CompositeDisposable _disposables = new();

    private Control? _control;
    private Control? _errorControl;

    public Control? Control
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public WidgetViewModel WidgetViewModel => _widgetViewModel;

    /// <summary>
    /// Target view, panel by default.
    /// </summary>
    public WidgetControlTarget ControlTarget { get; set; } = WidgetControlTarget.Panel;

    /// <summary>
    /// Widgets render target size.
    /// </summary>
    public Size ControlTargetSize { get; set; } = Size.Infinity;

    public WidgetControlViewModel(WidgetViewModel widgetViewModel)
    {
        _widgetViewModel = widgetViewModel;

        _disposables.Add(
            widgetViewModel.WhenAnyValue(p => p.ErrorWidget).Subscribe(_ => { UpdateErrorControl(); })
        );
        _disposables.Add(
            widgetViewModel.WhenAnyValue(p => p.Widget).Subscribe(_ => { UpdateWidgetControl(); })
        );

        UpdateErrorControl();
        UpdateWidgetControl();
    }

    private void UpdateErrorControl()
    {
        if (WidgetViewModel.ErrorWidget != null)
        {
            _errorControl = WidgetViewModel.ErrorWidget.CreateControl(WidgetControlTarget.Panel, Size.Infinity);
        }
        else
        {
            _errorControl = null;
        }

        Control = WidgetViewModel.RequireConfiguration ? _errorControl : _control;
    }

    private void UpdateWidgetControl()
    {
        ReleaseControl();
        if (_widgetViewModel.Widget != null)
        {
            _control = _widgetViewModel.Widget.CreateControl(ControlTarget, ControlTargetSize);
        }
        else
        {
            _control = null;
        }

        Control = WidgetViewModel.RequireConfiguration ? _errorControl : _control;
    }

    private void ReleaseControl()
    {
        if (_control == null)
        {
            return;
        }
        (_control as IDisposable)?.Dispose();
        _control = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposables.Dispose();
    }
}
