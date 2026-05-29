using Dashik.Abstractions;

namespace Dashik.Shared.Services;

internal sealed class LinuxSystemUtils : ISystemUtils
{
    private const string DesktopFileName = "dashik.desktop";
    private const string ApplicationName = "Dashik";

    /// <inheritdoc />
    public Task<bool> IsLaunchAtStartupEnabledAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(File.Exists(GetDesktopFilePath()));

    /// <inheritdoc />
    public async Task EnableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        var executablePath = GetExecutablePath();
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new InvalidOperationException("Cannot determine the current executable path.");
        }

        var desktopFilePath = GetDesktopFilePath();
        var autostartDirectory = Path.GetDirectoryName(desktopFilePath)!;
        Directory.CreateDirectory(autostartDirectory);

        var contents =
            $"""
             [Desktop Entry]
             Type=Application
             Name={ApplicationName}
             Exec={EscapeExec(executablePath)}
             Terminal=false
             Categories=Utility
             StartupNotify=false
             StartupWMClass=dashik
             X-GNOME-Autostart-enabled=true
             X-GNOME-Autostart-Delay=2
             X-KDE-autostart-after=panel
             X-LXQt-Need-Tray=true
             """;
        await File.WriteAllTextAsync(desktopFilePath, contents, cancellationToken);
    }

    /// <inheritdoc />
    public Task DisableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        var desktopFilePath = GetDesktopFilePath();
        if (File.Exists(desktopFilePath))
        {
            File.Delete(desktopFilePath);
        }
        return Task.CompletedTask;
    }

    private static string GetDesktopFilePath()
    {
        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(configHome))
        {
            configHome = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config");
        }
        return Path.Combine(configHome, "autostart", DesktopFileName);
    }

    private static string? GetExecutablePath()
    {
        // When running as an AppImage, the APPIMAGE env variable points to the actual .AppImage file.
        var appImagePath = Environment.GetEnvironmentVariable("APPIMAGE");
        if (!string.IsNullOrEmpty(appImagePath) && File.Exists(appImagePath))
        {
            return appImagePath;
        }
        return Environment.ProcessPath;
    }

    private static string EscapeExec(string path)
        => path.Contains(' ') ? $"\"{path}\"" : path;
}
