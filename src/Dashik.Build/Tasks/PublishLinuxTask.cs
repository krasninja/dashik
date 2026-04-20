using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using Dashik.Sdk;

namespace Dashik.Build.Tasks;

[TaskName("Publish-Linux")]
[TaskDescription("Publish Linux project")]
public sealed class PublishLinuxTask : AsyncFrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override Task RunAsync(BuildContext context)
    {
        context.CleanDirectory(context.OutputDirectory);
        context.DotNetPublish(
            context.Directory("./src/Dashik.Desktop/"),
            new BaseDotNetPublishSettings(context)
            {
                Runtime = DotNetConstants.RidLinuxX64,
            });

        var velopackArgs = context.GetVelopackArguments(Application.PlatformLinux);
        velopackArgs = velopackArgs.Concat(["--categories", "Office"]).ToArray();
        context.StartProcess("vpk", string.Join(' ', velopackArgs));

        return Task.CompletedTask;
    }
}
