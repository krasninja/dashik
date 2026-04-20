using Microsoft.Extensions.Logging;
using Velopack;
using Dashik.Shared.Infrastructure.Updates;

namespace Dashik.Desktop.Infrastructure.Updates;

/// <summary>
/// Application update service.
/// </summary>
internal sealed class VelopackAppUpdateService : IAppUpdateService
{
    private readonly string _updateUrl;
    private readonly ILogger<VelopackAppUpdateService> _logger;

    private UpdateManager UpdateManager => field ??= new UpdateManager(_updateUrl);

    public VelopackAppUpdateService(string updateUrl, ILogger<VelopackAppUpdateService> logger)
    {
        _updateUrl = updateUrl;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> CheckUpdatesAsync(CancellationToken cancellationToken)
    {
        if (!UpdateManager.IsInstalled)
        {
            return string.Empty;
        }

        var newVersion = await UpdateManager.CheckForUpdatesAsync();
        if (newVersion == null)
        {
            return string.Empty;
        }
        return newVersion.TargetFullRelease.Version.ToString();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(CancellationToken cancellationToken)
    {
        UpdateInfo? newVersion;
        try
        {
            newVersion = await UpdateManager.CheckForUpdatesAsync();
            if (newVersion == null)
            {
                return;
            }
        }
        catch (Velopack.Exceptions.NotInstalledException e)
        {
            _logger.LogWarning(e, "Application is not installed. Skipping update.");
            return;
        }

        // Download new version.
        await UpdateManager.DownloadUpdatesAsync(newVersion, cancelToken: cancellationToken);

        // Install new version and restart application.
        UpdateManager.ApplyUpdatesAndRestart(newVersion);
    }
}
