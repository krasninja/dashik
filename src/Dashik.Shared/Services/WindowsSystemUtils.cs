using System.Runtime.Versioning;
using Dashik.Abstractions;
using Microsoft.Win32;

namespace Dashik.Shared.Services;

[SupportedOSPlatform("windows")]
internal sealed class WindowsSystemUtils : ISystemUtils
{
    private const string RunRegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Dashik";

    /// <inheritdoc />
    public Task<bool> IsLaunchAtStartupEnabledAsync(CancellationToken cancellationToken = default)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        return Task.FromResult(!string.IsNullOrEmpty(value));
    }

    /// <inheritdoc />
    public Task EnableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        var executablePath = GetExecutablePath();
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new InvalidOperationException("Cannot determine the current executable path.");
        }

        using var key = Registry.CurrentUser.CreateSubKey(RunRegistryKeyPath, writable: true);
        key.SetValue(ValueName, QuoteIfNeeded(executablePath), RegistryValueKind.String);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisableLaunchAtStartupAsync(CancellationToken cancellationToken = default)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKeyPath, writable: true);
        if (key?.GetValue(ValueName) != null)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
        return Task.CompletedTask;
    }

    private static string? GetExecutablePath() => Environment.ProcessPath;

    private static string QuoteIfNeeded(string path)
        => path.Contains(' ') ? $"\"{path}\"" : path;
}
