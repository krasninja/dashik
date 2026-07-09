using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using ReactiveUI;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;
using Dashik.Host.Services.Packages;
using Dashik.Host.Services.Widgets;
using Dashik.Sdk.Abstract;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.ViewModels;

public class AddWidgetViewModel : ViewModelBase
{
    private readonly IWidgetsProvider _widgetsProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPackagesStorage[] _packagesStorages;
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

        public bool TryAddWidgetNode(WidgetNode widgetNode)
        {
            if (Widgets.Any(w => w.Id == widgetNode.Id))
            {
                return false;
            }
            Widgets.Add(widgetNode);
            return true;
        }
    }

    public interface IWidgetNodePreviewInfo
    {
        string Name { get; }

        string Description { get; }

        bool IsLocal { get; }
    }

    public sealed class LocalWidgetNodePreviewInfo : IWidgetNodePreviewInfo
    {
        public WidgetViewModel WidgetViewModel { get; }

        public WidgetControlViewModel WidgetControlViewModel { get; }

        public WidgetPreview PreviewConfiguration { get; }

        /// <inheritdoc />
        public string Name => PreviewConfiguration.Name;

        /// <inheritdoc />
        public string Description => PreviewConfiguration.Description;

        /// <inheritdoc />
        public bool IsLocal => true;

        public LocalWidgetNodePreviewInfo(WidgetViewModel widgetViewModel, WidgetPreview previewConfiguration)
        {
            WidgetViewModel = widgetViewModel;
            PreviewConfiguration = previewConfiguration;
            WidgetControlViewModel = new WidgetControlViewModel(widgetViewModel);
        }
    }

    public sealed class RemoteWidgetNodePreviewInfo : IWidgetNodePreviewInfo
    {
        private readonly RemoteWidgetMetadataPreview _preview;

        public Task<Bitmap?> Image => _preview.PreviewImage;

        public string Name => _preview.Name;

        public string Description => _preview.Description;

        public bool IsLocal => false;

        public RemoteWidgetNodePreviewInfo(RemoteWidgetMetadataPreview preview)
        {
            _preview = preview;
        }
    }

    public sealed class WidgetNode : ReactiveObject
    {
        public string Id { get; }

        public object[] WidgetPreviewViewModels { get; }

        public bool HasPreviewItems => WidgetPreviewViewModels.Length > 0;

        public string Title { get; }

        public string Description { get; }

        private readonly Func<Task<IImage?>> _icon;

        public Task<IImage?> Icon => _icon.Invoke();

        public int SelectedPreviewIndex
        {
            get;
            set
            {
                this.RaiseAndSetIfChanged(ref field, value);
            }
        }

        public bool Selected
        {
            get;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        public bool IsLocal => RemoteWidgetPackage == null;

        public RemoteWidgetPackage? RemoteWidgetPackage { get; }

        public WidgetNode(WidgetInfo widgetInfo, object[] widgetPreviews)
        {
            Id = widgetInfo.Id;
            Title = widgetInfo.Name;
            Description = widgetInfo.Description;
            var icon = widgetInfo.Icon;
            _icon = () => Task.FromResult(icon)!;
            WidgetPreviewViewModels = widgetPreviews;
        }

        public WidgetNode(WidgetMetadata widgetMetadata, RemoteWidgetPackage remoteWidgetPackage, object[] widgetPreviews)
        {
            Id = widgetMetadata.Id;
            Title = widgetMetadata.Name;
            Description = widgetMetadata.Description;
            var iconFileImage = widgetMetadata.IconFileImage;
            _icon = async () => await iconFileImage;
            WidgetPreviewViewModels = widgetPreviews;
            RemoteWidgetPackage = remoteWidgetPackage;
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

    public bool LoadLocalWidgets { get; set; } = true;

    public bool LoadRemoteWidgets { get; set; } = true;

    public IObservable<WidgetNode?> AddWidgetRequested => AddWidgetCommand.Select(_ => SelectedWidgetNode);

    public ReactiveCommand<WidgetNode, Unit> AddWidgetCommand { get; internal set; }

    public ReactiveCommand<Unit, Unit> NextPreviewCommand { get; }

    public ReactiveCommand<Unit, Unit> PreviousPreviewCommand { get; }

    public AddWidgetViewModel(
        IWidgetsProvider widgetsProvider,
        IWidgetsFactory widgetsFactory,
        IWidgetsStateStorage stateStorage,
        IServiceProvider serviceProvider,
        IPackagesStorage[] packagesStorages,
        ILogger<AddWidgetViewModel> logger)
    {
        _widgetsProvider = widgetsProvider;
        _widgetsFactory = widgetsFactory;
        _stateStorage = stateStorage;
        _serviceProvider = serviceProvider;
        _packagesStorages = packagesStorages;
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

    private WidgetCategoryNode? GetOrCreateCategoryNode(
        IEnumerable<WidgetCategoryInfo> allCategories,
        WidgetCategory category)
    {
        var categoryModel = Categories.FirstOrDefault(c => c.Info.Category == category);
        if (categoryModel == null)
        {
            var categoryInfo = allCategories.FirstOrDefault(c => c.Category == category);
            if (categoryInfo == null)
            {
                return null;
            }
            categoryModel = new WidgetCategoryNode(categoryInfo);
            Categories.Add(categoryModel);
        }
        return categoryModel;
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Categories.Clear();
        SelectedWidgetNode = null;

        if (LoadLocalWidgets)
        {
            await LoadLocalWidgetsAsync(cancellationToken);
        }
        if (LoadRemoteWidgets)
        {
            await LoadRemoteWidgetsAsync(cancellationToken);
        }

        SelectedWidgetNode = Categories.SelectMany(c => c.Widgets).FirstOrDefault();

        await base.LoadAsync(cancellationToken);
    }

    private async Task LoadLocalWidgetsAsync(CancellationToken cancellationToken = default)
    {
        var categories = _widgetsProvider.GetCategories().ToArray();
        var widgets = _widgetsProvider.GetAll();
        foreach (var widgetInfo in widgets)
        {
            var categoryModel = GetOrCreateCategoryNode(categories, widgetInfo.InfoAttribute.Category);
            if (categoryModel == null)
            {
                continue;
            }

            var previewViewModels = new List<LocalWidgetNodePreviewInfo>();
            if (widgetInfo.WidgetType.IsAssignableTo(typeof(IWidgetPreview)))
            {
                try
                {
                    var preview = await CreateWidgetPreviewAsync(widgetInfo, cancellationToken);
                    previewViewModels.AddRange(preview);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to create preview for widget {WidgetId}.", widgetInfo.Id);
                }
            }

            categoryModel.TryAddWidgetNode(
                new WidgetNode(widgetInfo, previewViewModels.Cast<object>().ToArray())
            );
        }
    }

    private async Task LoadRemoteWidgetsAsync(CancellationToken cancellationToken = default)
    {
        var categories = _widgetsProvider.GetCategories().ToArray();
        foreach (var packagesStorage in _packagesStorages)
        {
            var remotePackages = await packagesStorage.GetAsync(cancellationToken);
            foreach (var remotePackage in remotePackages)
            {
                foreach (var remotePackageWidget in remotePackage.Widgets)
                {
                    var categoryModel = GetOrCreateCategoryNode(categories, remotePackageWidget.Category);
                    if (categoryModel == null)
                    {
                        continue;
                    }
                    var remoteModel = new RemoteWidgetMetadata(packagesStorage.Uri, remotePackageWidget);
                    var previews = remoteModel.PreviewItems
                        .Select(pi => new RemoteWidgetMetadataPreview(packagesStorage.Uri, pi))
                        .Select(pr => new RemoteWidgetNodePreviewInfo(pr));
                    categoryModel.TryAddWidgetNode(
                        new WidgetNode(
                            remoteModel,
                            new RemoteWidgetPackage(packagesStorage.Uri, remotePackage),
                            previews.Cast<object>().ToArray()
                        )
                    );
                }
            }
        }
    }

    private async Task<IReadOnlyList<LocalWidgetNodePreviewInfo>> CreateWidgetPreviewAsync(WidgetInfo widgetInfo, CancellationToken cancellationToken)
    {
        var widgetPreview = (IWidgetPreview)await _widgetsFactory.CreateAsync(
            widgetInfo.WidgetType,
            new WidgetInitInfo(PreviewWidgetContext.Instance, _defaultPreviewSettings, new JsonObject()),
            cancellationToken
        );
        var previewConfigurations = widgetPreview.GetPreviewConfigurations();

        var previewViewModels = new List<LocalWidgetNodePreviewInfo>(capacity: previewConfigurations.Count);
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
            previewViewModels.Add(new LocalWidgetNodePreviewInfo(widgetPreviewViewModel, previewConfiguration));
        }

        return previewViewModels;
    }
}
