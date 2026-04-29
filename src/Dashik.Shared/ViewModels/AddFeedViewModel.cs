using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using ReactiveUI;
using DynamicData;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Models;
using Dashik.Shared.Services;
using Dashik.Shared.Services.Packages;
using Dashik.Shared.Utils;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Shared.ViewModels;

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

        AddFeedCommand = ReactiveCommand.Create(AddFeed);
        EditFeedCommand = ReactiveCommand.Create<FeedViewModel>(EditFeed);
        ApplyFeedCommand = ReactiveCommand.CreateFromTask<FeedViewModel>(ApplyFeed);
        RemoveFeedCommand = ReactiveCommand.CreateFromTask<FeedViewModel>(RemoveFeed);
    }

    public void AddFeed()
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

    public async Task RemoveFeed(FeedViewModel feed, CancellationToken cancellationToken)
    {
        var messageBoxVm = new MessageBoxViewModel("Are you sure you want to remove the feed?", Resources.Messages.Remove)
            .SetYesNoMode();
        if (await _mvvmService.OpenAsync(messageBoxVm, cancellationToken) == DialogResult.Yes)
        {
            if (Feeds.Remove(feed))
            {
                await SaveSettings(cancellationToken);
            }
        }
    }

    public async Task ApplyFeed(FeedViewModel feed, CancellationToken cancellationToken)
    {
        var context = new ValidationContext(feed);
        var isValid = Validator.TryValidateObject(feed, context, null, true);
        if (!isValid)
        {
            return;
        }

        await SaveSettings(cancellationToken);
        feed.InEditMode = false;
    }

    private async Task SaveSettings(CancellationToken cancellationToken)
    {
        _appSettings.PackagesFeeds = Feeds
            .Select(f => new PackageFeedModel(f.Name, new Uri(f.Uri)))
            .ToList();

        await _settingsStorage.SaveAsync(_appSettings, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Feeds.Clear();
        Feeds.AddRange(_appSettings.PackagesFeeds.Select(f => new FeedViewModel(f.Name, f.Uri.ToString())));
        await base.LoadAsync(cancellationToken);
    }
}
