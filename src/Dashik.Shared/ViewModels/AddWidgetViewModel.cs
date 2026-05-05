using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using ReactiveUI;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Dashik.Abstractions;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services.Widgets;

namespace Dashik.Shared.ViewModels;

public sealed class AddWidgetViewModel : ViewModelBase
{
    private readonly IWidgetsProvider _widgetsProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWidgetsFactory _widgetsFactory;

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

    public sealed class WidgetNodePreviewInfo
    {
        public WidgetViewModel WidgetViewModel { get; }

        public WidgetPreview PreviewConfiguration { get; }

        public WidgetNodePreviewInfo(WidgetViewModel widgetViewModel, WidgetPreview previewConfiguration)
        {
            WidgetViewModel = widgetViewModel;
            PreviewConfiguration = previewConfiguration;
        }
    }

    public sealed class WidgetNode(WidgetInfo widgetInfo, WidgetNodePreviewInfo[] widgetPreviews) : ReactiveObject
    {
        public string Id => WidgetInfo.Id;

        public WidgetInfo WidgetInfo { get; } = widgetInfo;

        public WidgetNodePreviewInfo[] WidgetPreviewViewModels => widgetPreviews;

        public bool HasPreviewItems => WidgetPreviewViewModels.Length > 0;

        public string Title => WidgetInfo.Name;

        public IImage Icon => WidgetInfo.Icon;

        public int SelectedPreviewIndex
        {
            get;
            set
            {
                this.RaiseAndSetIfChanged(ref field, value);
            }
        }

        public string Description => WidgetInfo.Description;

        public bool Selected
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    #endregion

    public ObservableCollection<WidgetCategoryNode> Categories { get; } = new();

    public WidgetNode? SelectedWidgetNode
    {
        get;
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

    public ReactiveCommand<Unit, Unit> NextPreviewCommand { get; }

    public ReactiveCommand<Unit, Unit> PreviousPreviewCommand { get; }

    public AddWidgetViewModel(
        IWidgetsProvider widgetsProvider,
        IWidgetsFactory widgetsFactory,
        IServiceProvider serviceProvider)
    {
        _widgetsProvider = widgetsProvider;
        _widgetsFactory = widgetsFactory;
        _serviceProvider = serviceProvider;

        AddWidgetCommand = ReactiveCommand.Create<WidgetNode>(_ => { });
        NextPreviewCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedWidgetNode == null)
            {
                return;
            }

            if (SelectedWidgetNode.SelectedPreviewIndex < SelectedWidgetNode.WidgetPreviewViewModels.Length - 1)
            {
                SelectedWidgetNode.SelectedPreviewIndex++;
            }
            else
            {
                SelectedWidgetNode.SelectedPreviewIndex = 0;
            }
        });
        PreviousPreviewCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedWidgetNode == null)
            {
                return;
            }

            if (SelectedWidgetNode.SelectedPreviewIndex > 0)
            {
                SelectedWidgetNode.SelectedPreviewIndex--;
            }
            else
            {
                SelectedWidgetNode.SelectedPreviewIndex = SelectedWidgetNode.WidgetPreviewViewModels.Length - 1;
            }
        });
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
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

            var previewViewModels = new List<WidgetNodePreviewInfo>();
            if (widgetInfo.WidgetType.IsAssignableTo(typeof(IWidgetPreview)))
            {
                var widgetPreview = (IWidgetPreview)await _widgetsFactory.CreateAsync(
                    widgetInfo.WidgetType,
                    new WidgetInitInfo(PreviewWidgetContext.Instance, new JsonObject()),
                    cancellationToken
                );
                var previewConfigurations = widgetPreview.GetPreviewConfigurations();
                foreach (var previewConfiguration in previewConfigurations)
                {
                    widgetPreview = (IWidgetPreview)await _widgetsFactory.CreateAsync(
                        widgetInfo.WidgetType,
                        new WidgetInitInfo(PreviewWidgetContext.Instance, new JsonObject()),
                        cancellationToken
                    );
                    var widgetPreviewViewModel = _serviceProvider.GetRequiredService<WidgetViewModel>();
                    widgetPreviewViewModel.Widget = (IWidget)widgetPreview;
                    widgetPreviewViewModel.WidgetInstance = new WidgetInstance(widgetInfo);
                    widgetPreviewViewModel.ReadOnly = true;

                    widgetPreview.SetPreview(previewConfiguration);
                    previewViewModels.Add(new WidgetNodePreviewInfo(widgetPreviewViewModel, previewConfiguration));
                }
            }

            categoryModel.Widgets.Add(new WidgetNode(widgetInfo, previewViewModels.ToArray()));
        }

        SelectedWidgetNode = Categories.SelectMany(c => c.Widgets).FirstOrDefault();

        await base.LoadAsync(cancellationToken);
    }
}
