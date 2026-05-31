using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core.Execution;
using Dashik.Abstractions;
using Dashik.QueryCat.AssemblyLoader;
using Dashik.Sdk.Abstract;

namespace Dashik.QueryCat;

public class DashikPluginsLoader : DotNetAssemblyPluginsLoader
{
    private const string PluginLoadMethodName = "LoadWidget";

    private readonly IExecutionThread _thread;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public DashikPluginsLoader(IExecutionThread thread, IServiceProvider serviceProvider, params string[] directories)
        : base(thread.FunctionsManager, thread, directories)
    {
        _thread = thread;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<DashikPluginsLoader>>();

        PluginKeyword = "widgets";
    }

    /// <inheritdoc />
    protected override async Task OnPluginLoadedAsync(Assembly assembly, Type? registrationClassType, CancellationToken cancellationToken)
    {
        var widgetsProvider = _serviceProvider.GetRequiredService<IWidgetsProvider>();

        if (registrationClassType == null)
        {
            var widgetTypes = assembly.GetExportedTypes()
                .Where(t => t.IsAssignableTo(typeof(IWidget)))
                .ToArray();
            foreach (var widgetType in widgetTypes)
            {
                widgetsProvider.Register(widgetType);
            }
            _logger.LogInformation(
                "Loaded '{Widgets}' from assembly '{Assembly}'.",
                string.Join(',', widgetTypes.Select(w => w.FullName)),
                assembly.FullName
            );
        }

        await base.OnPluginLoadedAsync(assembly, registrationClassType, cancellationToken);
    }
}
