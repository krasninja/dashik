using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core.IO.Arguments;
using Cake.Frosting;

namespace Dashik.Build.Tasks;

[TaskName("Publish-Packages")]
[TaskDescription("Publish packages.")]
public sealed class PublishPackagesTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        context.EnsureDirectoryExists(context.OutputDirectory);

        context.DotNetPack("./src/Dashik.Widgets.Motd", new DotNetPackSettings
        {
            OutputDirectory = context.OutputDirectory,
            Configuration = DotNetConstants.ConfigurationRelease,
            NoLogo = true,
            ArgumentCustomization = pag =>
            {
                pag.Append(new TextArgument("-p:UseAssemblyName=false"));
                pag.Append(new TextArgument("-p:OutputType=Library"));
                return pag;
            },
        });

        return Task.CompletedTask;
    }
}
