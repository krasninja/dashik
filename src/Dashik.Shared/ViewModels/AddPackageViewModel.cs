using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using Microsoft.Extensions.Logging;
using Dashik.Abstractions;
using Dashik.Shared.Infrastructure.UI;
using Dashik.Shared.Services.Packages;
using Dashik.Sdk.Mvvm;
using Dashik.Sdk.ViewModels;

namespace Dashik.Shared.ViewModels;

public sealed class AddPackageViewModel : ViewModelBase
{
    private readonly IAppService _appService;
    private readonly IMvvmService _mvvmService;
    private readonly PackagesInstaller _packagesInstaller;
    private readonly Func<IPackagesStorage[]> _widgetsStoragesFactory;
    private readonly ILogger<AddPackageViewModel> _logger;

    public sealed class PackageNode
    {
        public WidgetPackageGroup PackageGroup { get; }

        public string Id => PackageGroup.Current.Id;

        public string Title => PackageGroup.Current.Name;

        public string Description => PackageGroup.Current.Description;

        public WidgetPackage Current => PackageGroup.Current;

        public PackageNode(WidgetPackageGroup packageGroup)
        {
            PackageGroup = packageGroup;
        }
    }

    public ObservableCollection<PackageNode> Packages { get; } = new();

    public PackageNode? SelectedPackageNode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public IObservable<string> InstallPackageRequested => InstallPackageCommand.Select(_ => SelectedPackageNode?.Id ?? string.Empty);

    public ReactiveCommand<PackageNode, Unit> InstallPackageCommand { get; internal set; }

    public ReactiveCommand<PackageNode, Unit> RemovePackageCommand { get; internal set; }

    public AddPackageViewModel(
        IAppService appService,
        IMvvmService mvvmService,
        PackagesInstaller packagesInstaller,
        Func<IPackagesStorage[]> widgetsStoragesFactory,
        ILogger<AddPackageViewModel> logger)
    {
        _appService = appService;
        _mvvmService = mvvmService;
        _packagesInstaller = packagesInstaller;
        _widgetsStoragesFactory = widgetsStoragesFactory;
        _logger = logger;

        InstallPackageCommand = ReactiveCommand.CreateFromTask<PackageNode>(InstallPackage);
        RemovePackageCommand = ReactiveCommand.CreateFromTask<PackageNode>(RemovePackage);
    }

    private async Task InstallPackage(PackageNode node, CancellationToken cancellationToken)
    {
        if (node.PackageGroup.Remote == null)
        {
            return;
        }
        var package = await _packagesInstaller.InstallAsync(_appService.GetMainPackageDirectory(),
            node.PackageGroup.Remote, cancellationToken);
        node.PackageGroup.Local = package;
    }

    private async Task RemovePackage(PackageNode node, CancellationToken cancellationToken)
    {
        if (node.PackageGroup.Local == null)
        {
            return;
        }

        var messageBoxVm = new MessageBoxViewModel("Are you sure you want to remove the package?", Resources.Messages.Remove)
            .SetYesNoMode();
        if (await _mvvmService.OpenAsync(messageBoxVm, cancellationToken) == DialogResult.Yes)
        {
            var removed = await _packagesInstaller.RemoveAsync(node.PackageGroup.Local, cancellationToken);
            if (removed)
            {
                node.PackageGroup.Local = null;
            }
        }
    }

    /// <inheritdoc />
    public override async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var storages = _widgetsStoragesFactory.Invoke();

        try
        {
            var remotePackages = await _packagesInstaller.GetRemoteAsync(storages, cancellationToken);
            var localPackages = await _packagesInstaller.GetLocalAsync(_appService.GetPackagesDirectories(), cancellationToken);

            // Group.
            var groups = WidgetPackageGroup.Combine(_appService.GetFeeds(), localPackages, remotePackages);
            foreach (var group in groups)
            {
                Packages.Add(new PackageNode(group));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            var messageBoxVm = new MessageBoxViewModel(e.Message, "Error").SetErrorMode();
            await _mvvmService.OpenAsync(messageBoxVm, cancellationToken);
        }

        SelectedPackageNode = Packages.FirstOrDefault();
        await base.LoadAsync(cancellationToken);
    }
}
