using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;

namespace Dashik.Build;

public class BuildContext : FrostingContext
{
    public ConvertableDirectoryPath OutputDirectory => this.Directory("./output");

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
        return new[]
        {
            $"[{platform}] pack",
            "--runtime", $"{platform}-{arch}",
            "--packId", "dashik",
            "--packVersion", Version,
            "--packTitle", "Dashik",
            "--packAuthors", "\"Ivan Kozhin\"",
            "--icon", "./src/Dashik.Desktop/Assets/Icon.ico",
            "--packDir", OutputDirectory,
            "--exclude", "releases",
            "--mainExe", platform.Contains("win", StringComparison.InvariantCultureIgnoreCase) ? "dashik.exe" : "dashik",
            "--outputDir", Path.Combine(OutputDirectory, "releases", platform),
            "-xy",
        };
    }
}
