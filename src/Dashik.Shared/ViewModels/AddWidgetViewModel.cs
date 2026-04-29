using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Avalonia.Media;
using Dashik.Abstractions;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;

namespace Dashik.Shared.ViewModels;

public sealed class AddWidgetViewModel : ViewModelBase
{
    private readonly IWidgetsProvider _widgetsProvider;

    #region Types

    public sealed class WidgetCategoryNode
    {
        public string Title => Info.Name;

        public string Description => Info.Description;

        public WidgetCategoryInfo Info { get; }

        public ObservableCollection<WidgetNode> Widgets { get; } = new();

        public WidgetCategoryNode(WidgetCategoryInfo info)
        {
            Info = info;
        }
    }

    public sealed class WidgetNode(WidgetInfo widgetInfo) : ReactiveObject
    {
        public string Id => WidgetInfo.Id;

        public WidgetInfo WidgetInfo { get; } = widgetInfo;

        public string Title => WidgetInfo.Name;

        public IImage Icon => WidgetInfo.Icon;

        public IImage[] PreviewImages => WidgetInfo.PreviewImages;

        public string Description => WidgetInfo.Description;

        public bool Selected
        {
            get => field;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    #endregion

    public ObservableCollection<WidgetCategoryNode> Categories { get; } = new();

    public WidgetNode? SelectedWidgetNode
    {
        get => field;
        set
        {
            foreach (var widget in Categories.SelectMany(c => c.Widgets))
            {
                if (widget != value)
                {
                    widget.Selected = false;
                }
            }
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public IObservable<WidgetInfo?> AddWidgetRequested => AddWidgetCommand.Select(_ => SelectedWidgetNode?.WidgetInfo);

    public ReactiveCommand<WidgetNode, Unit> AddWidgetCommand { get; internal set; }

    public AddWidgetViewModel(IWidgetsProvider widgetsProvider)
    {
        _widgetsProvider = widgetsProvider;

        AddWidgetCommand = ReactiveCommand.Create<WidgetNode>(_ => { });
    }

    /// <inheritdoc />
    public override Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var categories = _widgetsProvider.GetCategories().ToArray();

        var widgets = _widgetsProvider.GetAll();
        foreach (var widgetInfo in widgets)
        {
            var categoryModel = Categories.FirstOrDefault(c => c.Info.Category == widgetInfo.Info.Category);
            if (categoryModel == null)
            {
                var category = categories.FirstOrDefault(c => c.Category == widgetInfo.Info.Category);
                if (category == null)
                {
                    continue;
                }
                categoryModel = new WidgetCategoryNode(category);
                Categories.Add(categoryModel);
            }
            categoryModel.Widgets.Add(new WidgetNode(widgetInfo));
        }

        SelectedWidgetNode = Categories.SelectMany(c => c.Widgets).FirstOrDefault();

        return base.LoadAsync(cancellationToken);
    }
}
