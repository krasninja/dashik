using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Primitives;
using Dashik.Host.Services.Packages;
using Dashik.Sdk.Models;
using Dashik.Sdk.Widgets;

namespace Dashik.Host.ViewModels;

public class AddWidgetDetailsViewModel : ReactiveObject
{
    public string Id
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    public object[] WidgetPreviewViewModels { get; } = [];

    public bool HasPreviewItems => WidgetPreviewViewModels.Length > 0;

    public string Title
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    public WidgetCategory Category
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = WidgetCategory.Misc;

    public string Description
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    = string.Empty;

    private readonly Func<Task<IImage?>> _icon = () => Task.FromResult<IImage?>(null);

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

    public ReactiveCommand<RxVoid, RxVoid> NextPreviewCommand { get; }

    public ReactiveCommand<RxVoid, RxVoid> PreviousPreviewCommand { get; }

    public AddWidgetDetailsViewModel()
    {
        NextPreviewCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedPreviewIndex < WidgetPreviewViewModels.Length - 1)
            {
                SelectedPreviewIndex++;
            }
            else
            {
                SelectedPreviewIndex = 0;
            }
        });
        PreviousPreviewCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedPreviewIndex > 0)
            {
                SelectedPreviewIndex--;
            }
            else
            {
                SelectedPreviewIndex = WidgetPreviewViewModels.Length - 1;
            }
        });
    }

    public AddWidgetDetailsViewModel(WidgetInfo widgetInfo, object[] widgetPreviews) : this()
    {
        Id = widgetInfo.Id;
        Title = widgetInfo.Name;
        Category = widgetInfo.InfoAttribute.Category;
        Description = widgetInfo.Description;
        var icon = widgetInfo.Icon;
        _icon = () => Task.FromResult(icon)!;
        WidgetPreviewViewModels = widgetPreviews;
    }

    public AddWidgetDetailsViewModel(WidgetMetadata widgetMetadata, RemoteWidgetPackage remoteWidgetPackage,
        object[] widgetPreviews) : this()
    {
        Id = widgetMetadata.Id;
        Title = widgetMetadata.Name;
        Category = widgetMetadata.Category;
        Description = widgetMetadata.Description;
        var iconFileImage = widgetMetadata.IconFileImage;
        _icon = async () => await iconFileImage;
        WidgetPreviewViewModels = widgetPreviews;
        RemoteWidgetPackage = remoteWidgetPackage;
    }
}
