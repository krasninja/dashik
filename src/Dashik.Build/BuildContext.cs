using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;
using Dashik.Sdk;

namespace Dashik.Build;

public class BuildContext : FrostingContext
{
    public ConvertableDirectoryPath OutputDirectory => this.Directory("./output");

    public ConvertableDirectoryPath ReleasesDirectory => this.Directory("./output/releases");

    public ConvertableDirectoryPath ProjectSdkDirectory => this.Directory("./src/Dashik.Sdk/");

    public string Version { get; }

    /// <inheritdoc />
    public BuildContext(ICakeContext context) : base(context)
    {
        context.EnsureDirectoryExists(OutputDirectory);
        Version = context.Arguments.GetArgument("version") ?? GetGitVersion();
    }

    private string GetGitVersion()
    {
        var gitVersion = this.GitVersion();
        return gitVersion.SemVer;
    }

    internal string[] GetVelopackArguments(string platform, string arch = "x64")
    {
        // https://docs.velopack.io/packaging/cross-compiling.
        // macOS packages use the "bundle" command; other platforms use "pack".
        var command = platform == Application.PlatformMacOS ? "bundle" : "pack";
        var iconPath = platform == Application.PlatformMacOS
            ? "./src/Dashik.Desktop/Assets/Icon.icns"
            : "./src/Dashik.Desktop/Assets/Icon.ico";
        return new[]
        {
            $"[{platform}]", command,
            "--runtime", $"{platform}-{arch}",
            "--packId", "dashik",
            "--packVersion", Version,
            "--packTitle", "Dashik",
            "--packAuthors", "\"Ivan Kozhin\"",
            "--icon", iconPath,
            "--packDir", OutputDirectory,
            "--mainExe", platform.Contains("win", StringComparison.InvariantCultureIgnoreCase) ? "dashik.exe" : "dashik",
            "--outputDir", Path.Combine(ReleasesDirectory, platform),
            "-xy",
        };
    }
}
