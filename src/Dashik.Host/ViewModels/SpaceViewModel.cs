using Avalonia.Collections;
using ReactiveUI;
using Dashik.Host.Models;

namespace Dashik.Host.ViewModels;

public sealed class SpaceViewModel : ReactiveObject
{
    private readonly SpaceModel _model;

    public string Id { get; init; }

    public string Name { get; init; }

    public bool Default => _model.Default;

    public AvaloniaList<WidgetViewModel> Widgets
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public SpaceViewModel(SpaceModel model)
    {
        _model = model;
        Id = model.Id;
        Name = model.Name;
        Widgets = [];
    }

    public bool GetWidgetById(string widgetId)
    {
        foreach (var widget in Widgets)
        {
            if (widget.WidgetId.Equals(widgetId))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}
