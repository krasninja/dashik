using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using DynamicData;
using Dashik.Host.Infrastructure.UI;
using Dashik.Host.Models;
using Dashik.Host.Services;
using Dashik.Host.Services.Packages;
using Dashik.Host.Utils;
using Dashik.Sdk.Models;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Host.ViewModels;

public sealed class AddFeedViewModel : ViewModelBase
{
    private readonly AppSettings _appSettings;
    private readonly IMvvmService _mvvmService;
    private readonly SettingsStorage _settingsStorage;

    public sealed class FeedViewModel : ReactiveObject
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = "New Feed";

        [Required]
        [Uri]
        public string Uri { get; set; } = "https://";

        public bool InEditMode
        {
            get => field;
            set => this.RaiseAndSetIfChanged(ref field, value);
        }

        public FeedViewModel()
        {
        }

        public FeedViewModel(string name, string uri)
        {
            Name = name;
            Uri = uri;
        }
    }

    public ObservableCollection<FeedViewModel> Feeds { get; } = new();

    public string DefaultFeedUri { get; set; }

    public IObservable<Unit> PackageFeedUpdateRequested { get; } = new Subject<Unit>();

    public ReactiveCommand<Unit, Unit> AddFeedCommand { get; set; }

    public ReactiveCommand<FeedViewModel, Unit> EditFeedCommand { get; }

    public ReactiveCommand<FeedViewModel, Unit> ApplyFeedCommand { get; }

    public ReactiveCommand<FeedViewModel, Unit> RemoveFeedCommand { get; }

    public AddFeedViewModel(
        AppSettings appSettings,
        IMvvmService mvvmService,
        SettingsStorage settingsStorage)
    {
        _appSettings = appSettings;
        _mvvmService = mvvmService;
        _settingsStorage = settingsStorage;

        DefaultFeedUri = DefaultPackagesStorage.Instance.Uri;

        AddFeedCommand = ReactiveCommand.CreateFromTask(AddFeedAsync);
        EditFeedCommand = ReactiveCommand.Create<FeedViewModel>(EditFeed);
        ApplyFeedCommand = ReactiveCommand.CreateFromTask<FeedViewModel>(ApplyFeedAsync);
        RemoveFeedCommand = ReactiveCommand.CreateFromTask<FeedViewModel>(RemoveFeedAsync);
    }

    public async Task AddFeedAsync(CancellationToken cancellationToken)
    {
        var feed = new FeedViewModel
        {
            InEditMode = true,
        };
        Feeds.Add(feed);
    }

    public void EditFeed(FeedViewModel feed)
    {
        feed.InEditMode = true;
    }

    public async Task RemoveFeedAsync(FeedViewModel feed, CancellationToken cancellationToken)
    {
        var messageBoxVm = new MessageBoxViewModel("Are you sure you want to remove the feed?", Resources.Messages.Remove)
            .SetYesNoMode();
        if (await _mvvmService.OpenAsync(messageBoxVm, this, cancellationToken) == DialogResult.Yes)
        {
            if (Feeds.Remove(feed))
            {
                await SaveSettings(cancellationToken);
            }
        }
    }

    public async Task ApplyFeedAsync(FeedViewModel feed, CancellationToken cancellationToken)
    {
        var context = new ValidationContext(feed);
        var isValid = Validator.TryValidateObject(feed, context, null, true);
        if (!isValid)
        {
            return;
        }

        try
        {
            var feedModel = await GetFeedAsync(feed.Uri, cancellationToken);
            feed.Name = feedModel.Name;
        }
        catch (Exception e)
        {
            var messageBoxVm = new MessageBoxViewModel($"Cannot verify feed: {e.Message}", "Error")
                .SetErrorMode();
            await _mvvmService.OpenAsync(messageBoxVm, cancellationToken: cancellationToken);
            return;
        }

        await SaveSettings(cancellationToken);
        feed.InEditMode = false;
    }

    private Task<WidgetPackageFeed> GetFeedAsync(string uri, CancellationToken cancellationToken)
    {
        var remotePackagesStorage = new FeedPackagesStorage(uri, "Feed Checker");
        var feed = remotePackagesStorage.GetFeedAsync(cancellationToken);
        return feed;
    }

    private async Task SaveSettings(CancellationToken cancellationToken)
    {
        _appSettings.PackagesFeeds = Feeds
            .Select(f => new PackageFeedModel(f.Name, new Uri(f.Uri)))
            .ToList();

        await _settingsStorage.SaveAsync(_appSettings, cancellationToken);
        PackageFeedUpdateRequested.Next();
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Feeds.Clear();
        Feeds.AddRange(_appSettings.PackagesFeeds.Select(f => new FeedViewModel(f.Name, f.Uri.ToString())));
        await base.LoadAsync(cancellationToken);
    }
}
