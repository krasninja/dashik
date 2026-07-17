using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QueryCat.Backend.Core.Plugins;
using SimpleInjector;
using Dashik.Host.Infrastructure.Setup;
using Dashik.Host.Infrastructure.Updates;
using Dashik.Host.Models;

namespace Dashik.Host;

/// <summary>
/// Application root class.
/// </summary>
public sealed class AppRoot : IDisposable, IAsyncDisposable
{
    internal const string SettingsFileName = "settings.json";
    internal const string ApplicationDirectory = "dashik";

    public AppArguments AppArguments { get; }

    public Container Container { get; }

    private ILogger _logger = NullLoggerFactory.Instance.CreateLogger<AppRoot>();

    public AppRoot(AppArguments appArguments)
    {
        Container = new Container
        {
            Options =
            {
                EnableAutoVerification = false,
                ResolveUnregisteredConcreteTypes = true,
            }
        };
        AppArguments = appArguments;
    }

    /// <summary>
    /// Set up application services.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task SetupServicesAsync(CancellationToken cancellationToken = default)
        => await SetupServicesAsync(_ => { }, cancellationToken);

    /// <summary>
    /// Set up application services.
    /// </summary>
    /// <param name="builder">Builder for container.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    public async Task SetupServicesAsync(Action<Container> builder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(AppArguments.ConfigDirectory))
        {
            AppArguments.ConfigDirectory = GetConfigDirectory();
        }
        if (string.IsNullOrEmpty(AppArguments.ApplicationDirectory))
        {
            AppArguments.ApplicationDirectory = GetApplicationDataDirectory();
        }

        var appSettings = await LoadSettingsAsync(AppArguments.ConfigDirectory, cancellationToken);

        new LoggingSetup(Container, AppArguments).Setup();
        new AppServicesSetup(Container, AppArguments, appSettings).Setup();
        builder.Invoke(Container);
        SetupMissedDependencies();

        Container.RegisterInstance<IServiceProvider>(Container);
#if DEBUG
        Container.Verify();
#endif

        _logger = Container.GetInstance<ILogger<AppRoot>>();
    }

    private void SetupMissedDependencies()
    {
        if (!IsRegistered<IAppUpdateService>())
        {
            Container.RegisterInstance<IAppUpdateService>(new StubAppUpdateService());
        }
    }

    private bool IsRegistered<T>() => IsRegistered(typeof(T));

    private bool IsRegistered(Type type)
    {
        return Container.GetRegistration(type) != null;
    }

    #region Settings load

    private async Task<AppSettings> LoadSettingsAsync(string configDirectory, CancellationToken cancellationToken)
    {
        // Load JSON.
        var settingsFileName = Path.Combine(configDirectory, SettingsFileName);
        var settings = new AppSettings();
        if (File.Exists(settingsFileName))
        {
            await using var settingsFile = File.OpenRead(settingsFileName);
            try
            {
                settings = await JsonSerializer.DeserializeAsync(settingsFile,
                    SourceGenerationContext.Default.AppSettings, cancellationToken: cancellationToken);
            }
            catch (JsonException e)
            {
                Console.Error.WriteLine(e);
            }
            settings ??= new AppSettings();
        }

        // Update from command line arguments.
        if (!string.IsNullOrEmpty(AppArguments.InstancesDirectoryName)
            && Directory.Exists(AppArguments.InstancesDirectoryName))
        {
            settings.InstancesDirectory = AppArguments.InstancesDirectoryName;
        }

        // Add default space.
        if (settings.GetDefaultSpace() == null)
        {
            settings.Spaces.Add(SpaceModel.DefaultInstance);
        }

        // Filter not existing local packages dirs.
        settings.LocalPackagesDirectories = settings.LocalPackagesDirectories
            .Where(Directory.Exists)
            .ToList();

        return settings;
    }

    private static string GetApplicationDataDirectory()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationDirectory);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return directory;
    }

    private static string GetConfigDirectory()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationDirectory);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return directory;
    }

    #endregion

    /// <summary>
    /// Initialize and load logger, widgets and other services.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var loader = Container.GetInstance<IPluginsLoader>();

        // Load QueryCat plugins.
        var loadedWidgetsCount = await loader.LoadAsync(new PluginsLoadingOptions
        {
            SkipDuplicates = true,
        }, cancellationToken);

        // Register internal widgets.
        _logger.LogInformation("Loaded {WidgetsCount} widgets.", loadedWidgetsCount);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Container.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
