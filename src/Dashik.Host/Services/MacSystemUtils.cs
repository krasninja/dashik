using System.Security;
using Dashik.Abstractions;

namespace Dashik.Host.Services;

internal sealed class MacSystemUtils : ISystemUtils
{
    private const string LaunchAgentLabel = "com.dashik.app";
    private const string LaunchAgentFileName = LaunchAgentLabel + ".plist";

    /// <inheritdoc />
    public Task<bool> IsLaunchAtStartupEnabledAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(File.Exists(GetLaunchAgentFilePath()));

    /// <inheritdoc />
    public async Task EnableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        var executablePath = GetExecutablePath();
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new InvalidOperationException("Cannot determine the current executable path.");
        }

        var launchAgentFilePath = GetLaunchAgentFilePath();
        var launchAgentsDirectory = Path.GetDirectoryName(launchAgentFilePath)!;
        Directory.CreateDirectory(launchAgentsDirectory);

        var contents =
            $"""
             <?xml version="1.0" encoding="UTF-8"?>
             <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
             <plist version="1.0">
             <dict>
                 <key>Label</key>
                 <string>{LaunchAgentLabel}</string>
                 <key>ProgramArguments</key>
                 <array>
                     <string>{SecurityElement.Escape(executablePath)}</string>
                 </array>
                 <key>RunAtLoad</key>
                 <true/>
                 <key>ProcessType</key>
                 <string>Interactive</string>
             </dict>
             </plist>
             """;
        await File.WriteAllTextAsync(launchAgentFilePath, contents, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        var launchAgentFilePath = GetLaunchAgentFilePath();
        if (File.Exists(launchAgentFilePath))
        {
            File.Delete(launchAgentFilePath);
        }
        return Task.CompletedTask;
    }

    private static string GetLaunchAgentFilePath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "LaunchAgents",
            LaunchAgentFileName);

    private static string? GetExecutablePath() => Environment.ProcessPath;
}
