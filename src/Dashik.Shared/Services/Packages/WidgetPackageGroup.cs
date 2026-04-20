using Dashik.Abstractions;
using Dashik.Sdk.Models;

namespace Dashik.Shared.Services.Packages;

/// <summary>
/// The group contains info about package feed, it's local copy and remote.
/// </summary>
public sealed class WidgetPackageGroup : ObservableObject
{
    public PackageFeed? Feed { get; set; }

    public LocalWidgetPackage? Local
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            OnPropertyChanged(nameof(Current));
            OnPropertyChanged(nameof(UpToDate));
            OnPropertyChanged(nameof(Installed));
        }
    }

    public RemoteWidgetPackage? Remote { get; set; }

    public WidgetPackage Current
    {
        get
        {
            if (Local != null && Remote != null)
            {
                return Local.SemVerVersion > Remote.SemVerVersion ? Local : Remote;
            }
            return Remote
                   ?? (WidgetPackage?)Local
                   ?? throw new InvalidOperationException("Current package is not set.");
        }
    }

    public bool UpToDate => Local != null && Remote != null && Local.SemVerVersion >= Remote.SemVerVersion;

    public bool HasUpdate => Local != null && Remote != null && Local.SemVerVersion < Remote.SemVerVersion;

    public bool Installed => Local != null;

    public static IReadOnlyList<WidgetPackageGroup> Combine(
        PackageFeed[] feeds,
        IEnumerable<LocalWidgetPackage> local,
        IEnumerable<RemoteWidgetPackage> remote)
    {
        var groups = new Dictionary<string, WidgetPackageGroup>();

        // Fill from local.
        foreach (var package in local)
        {
            if (!groups.TryGetValue(package.Id, out _))
            {
                groups[package.Id] = new WidgetPackageGroup
                {
                    Local = package,
                };
            }
        }

        // Fill from remote.
        foreach (var package in remote)
        {
            var feed = Array.Find(feeds, f => f.Uri == new Uri(package.StorageUri));
            if (!groups.TryGetValue(package.Id, out var group))
            {
                groups[package.Id] = new WidgetPackageGroup
                {
                    Feed = feed,
                    Remote = package,
                };
            }
            else if (group.Remote == null || group.Feed == null ||
                     (group.Remote != null && group.Remote.SemVerVersion < package.SemVerVersion))
            {
                group.Remote = package;
                group.Feed = feed;
            }
        }

        return groups.Values.OrderBy(p => p.Current.Name).ToList();
    }
}
