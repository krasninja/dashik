using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using ReactiveUI;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;
using Dashik.Host.Services.Widgets;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.ViewModels;

public sealed class AddWidgetViewModel : ViewModelBase
{
    private readonly IWidgetsProvider _widgetsProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IWidgetsFactory _widgetsFactory;
    private readonly IWidgetsStateStorage _stateStorage;

    private static readonly WidgetMainSettings _defaultPreviewSettings = new()
    {
        UpdateInterval = TimeSpan.FromSeconds(30),
        UseCustomTitle = false,
        Height = 300,
        Disabled = false,
        Hidden = false,
    };

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

        public WidgetControlViewModel WidgetControlViewModel { get; }

        public WidgetPreview PreviewConfiguration { get; }

        public WidgetNodePreviewInfo(WidgetViewModel widgetViewModel, WidgetPreview previewConfiguration)
        {
            WidgetViewModel = widgetViewModel;
            PreviewConfiguration = previewConfiguration;
            WidgetControlViewModel = new WidgetControlViewModel(widgetViewModel);
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

    public int WidgetsCount => Categories.SelectMany(c => c.Widgets).Count();

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
        IWidgetsStateStorage stateStorage,
        IServiceProvider serviceProvider,
        ILogger<AddWidgetViewModel> logger)
    {
        _widgetsProvider = widgetsProvider;
        _widgetsFactory = widgetsFactory;
        _stateStorage = stateStorage;
        _serviceProvider = serviceProvider;
        _logger = logger;

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
        Categories.Clear();
        SelectedWidgetNode = null;

        var categories = _widgetsProvider.GetCategories().ToArray();

        var widgets = _widgetsProvider.GetAll();
        foreach (var widgetInfo in widgets)
        {
            var categoryModel = Categories.FirstOrDefault(c => c.Info.Category == widgetInfo.InfoAttribute.Category);
            if (categoryModel == null)
            {
                var category = categories.FirstOrDefault(c => c.Category == widgetInfo.InfoAttribute.Category);
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
                try
                {
                    var preview = await CreateWidgetPreviewAsync(widgetInfo, cancellationToken);
                    previewViewModels.AddRange(preview);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to create preview for widget {WidgetId}", widgetInfo.Id);
                }
            }

            categoryModel.Widgets.Add(new WidgetNode(widgetInfo, previewViewModels.ToArray()));
        }

        SelectedWidgetNode = Categories.SelectMany(c => c.Widgets).FirstOrDefault();

        await base.LoadAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<WidgetNodePreviewInfo>> CreateWidgetPreviewAsync(WidgetInfo widgetInfo, CancellationToken cancellationToken)
    {
        var widgetPreview = (IWidgetPreview)await _widgetsFactory.CreateAsync(
            widgetInfo.WidgetType,
            new WidgetInitInfo(PreviewWidgetContext.Instance, _defaultPreviewSettings, new JsonObject()),
            cancellationToken
        );
        var previewConfigurations = widgetPreview.GetPreviewConfigurations();

        var previewViewModels = new List<WidgetNodePreviewInfo>(capacity: previewConfigurations.Count);
        foreach (var previewConfiguration in previewConfigurations)
        {
            widgetPreview = (IWidgetPreview)await _widgetsFactory.CreateAsync(
                widgetInfo.WidgetType,
                new WidgetInitInfo(PreviewWidgetContext.Instance, _defaultPreviewSettings, new JsonObject()),
                cancellationToken
            );
            var widgetPreviewViewModel = _serviceProvider.GetRequiredService<WidgetViewModel>();
            widgetPreviewViewModel.Widget = (IWidget)widgetPreview;
            widgetPreviewViewModel.WidgetInstance = new WidgetInstance(widgetInfo, _stateStorage);
            widgetPreviewViewModel.ReadOnly = true;

            widgetPreview.SetPreview(previewConfiguration);
            previewViewModels.Add(new WidgetNodePreviewInfo(widgetPreviewViewModel, previewConfiguration));
        }

        return previewViewModels;
    }
}
