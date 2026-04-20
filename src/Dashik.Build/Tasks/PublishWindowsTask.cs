using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using Dashik.Sdk;

namespace Dashik.Build.Tasks;

[TaskName("Publish-Windows")]
[TaskDescription("Publish Windows project")]
public sealed class PublishWindowsTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        context.CleanDirectory(context.OutputDirectory);
        context.DotNetPublish(
            context.Directory("./src/Dashik.Desktop/"),
            new BaseDotNetPublishSettings(context)
            {
                Runtime = DotNetConstants.RidWindowsX64,
            });

        var velopackArgs = context.GetVelopackArguments(Application.PlatformWindows);
        context.StartProcess("vpk", string.Join(' ', velopackArgs));

        return Task.CompletedTask;
    }
}
