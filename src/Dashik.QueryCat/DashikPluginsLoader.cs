using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
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

    public DashikPluginsLoader(IExecutionThread thread, IServiceProvider serviceProvider, params string[] directories)
        : base(thread.FunctionsManager, thread, directories)
    {
        _thread = thread;
        _serviceProvider = serviceProvider;

        PluginKeyword = "widgets";
    }

    /// <inheritdoc />
    protected override async Task OnPluginLoadedAsync(Assembly assembly, Type? registrationClassType, CancellationToken cancellationToken)
    {
        var widgetsProvider = _serviceProvider.GetRequiredService<IWidgetsProvider>();

        if (registrationClassType == null)
        {
            var widgetTypes = assembly.GetExportedTypes().Where(t => t.IsAssignableTo(typeof(IWidget)));
            foreach (var widgetType in widgetTypes)
            {
                widgetsProvider.Register(widgetType);
            }
        }

        await base.OnPluginLoadedAsync(assembly, registrationClassType, cancellationToken);
    }
}
