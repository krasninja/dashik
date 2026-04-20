using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core.Diagnostics;
using Cake.Core.IO.Arguments;
using Cake.Frosting;
using Cake.Git;
using Verbosity = Cake.Core.Diagnostics.Verbosity;

namespace Dashik.Build.Tasks;

[TaskName("Publish-Sdk")]
[TaskDescription("Build NuGet package")]
public sealed class PublishSdkTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        var currentSha = new string('0', 64);
        try
        {
            currentSha = context.GitLogTip("../../").Sha;
        }
        catch (Exception e)
        {
            context.Log.Write(Verbosity.Normal, LogLevel.Warning, $"Failed to get current git commit hash: {e.Message}");
        }

        context.DotNetPack(context.ProjectSdkDirectory, new DotNetPackSettings
        {
            NoLogo = true,
            OutputDirectory = context.OutputDirectory,
            Configuration = DotNetConstants.ConfigurationRelease,
            // https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#packing-using-a-nuspec
            ArgumentCustomization = pag =>
            {
                pag.Append(new TextArgument("-p:NuspecFile=Dashik.Sdk.nuspec"));
                pag.Append(new TextArgument($"-p:NuspecProperties=\"version={context.Version};CommitHash={currentSha}\""));
                return pag;
            },
        });

        return base.RunAsync(context);
    }
}
