using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using Dashik.Sdk;

namespace Dashik.Build.Tasks;

[TaskName("Publish-Mac")]
[TaskDescription("Publish Mac project")]
public sealed class PublishMacTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        context.CleanDirectory(context.OutputDirectory);
        context.DotNetPublish(
            context.Directory("./src/Dashik.Desktop/"),
            new BaseDotNetPublishSettings(context)
            {
                Runtime = DotNetConstants.RidMacOSX64,
            });

        var velopackArgs = context.GetVelopackArguments(Application.PlatformMacOS);
        context.StartProcess("vpk", string.Join(' ', velopackArgs));

        return Task.CompletedTask;
    }
}
