using System.Reflection;
using System.Runtime.InteropServices;

namespace Dashik.Sdk;

/// <summary>
/// Application related methods and properties.
/// </summary>
public static class Application
{
    public const string PlatformMulti = "multi";
    public const string PlatformLinux = "linux";
    public const string PlatformFreeBSD = "freebsd";
    public const string PlatformWindows = "win";
    public const string PlatformAndroid = "android";
    public const string PlatformMacOS = "osx";
    public const string PlatformIOS = "ios";
    public const string PlatformBrowser = "browser";
    public const string PlatformUnknown = "unknown";

    public const string ArchitectureMsil = "msil";
    public const string ArchitectureArm = "arm";
    public const string ArchitectureArm64 = "arm64";
    public const string ArchitectureX86 = "x86";
    public const string ArchitectureX64 = "x64";
    public const string ArchitectureWasm = "wasm";
    public const string ArchitectureUnknown = "unknown";

    /// <summary>
    /// Product name.
    /// </summary>
    public const string ProductName = "Dashik";

        /// <summary>
    /// Get short version without hash (for example, 0.7.0-alpha.16).
    /// </summary>
    /// <returns>Short version.</returns>
    public static string GetShortVersion()
    {
        var version = GetVersion();
        var hashSeparatorIndex = version.IndexOf('+', StringComparison.Ordinal);
        return hashSeparatorIndex > -1 ? version.Substring(0, hashSeparatorIndex) : version;
    }

    /// <summary>
    /// Get version with hash (0.7.0-alpha.16+1fa9a9d3976d7663a571658bfe720f3565f91a0f).
    /// </summary>
    /// <returns>Full version.</returns>
    public static string GetVersion()
        => typeof(Application).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty;

    /// <summary>
    /// Full product name with version.
    /// </summary>
    /// <returns></returns>
    public static string GetProductFullName() => $"{ProductName} {GetVersion()}";

    /// <summary>
    /// Get current platform identifier.
    /// </summary>
    /// <returns>Platform identifier.</returns>
    public static string GetPlatform()
    {
        if (OperatingSystem.IsLinux())
        {
            return PlatformLinux;
        }
        if (OperatingSystem.IsWindows())
        {
            return PlatformWindows;
        }
        if (OperatingSystem.IsAndroid())
        {
            return PlatformAndroid;
        }
        if (OperatingSystem.IsMacOS())
        {
            return PlatformMacOS;
        }
        if (OperatingSystem.IsFreeBSD())
        {
            return PlatformFreeBSD;
        }
        if (OperatingSystem.IsBrowser())
        {
            return PlatformBrowser;
        }
        if (OperatingSystem.IsIOS())
        {
            return PlatformIOS;
        }
        return PlatformUnknown;
    }

    /// <summary>
    /// Get current platform architecture.
    /// </summary>
    /// <returns>Architecture identifier.</returns>
    public static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm => ArchitectureArm,
            Architecture.Arm64 => ArchitectureArm64,
            Architecture.X86 => ArchitectureX86,
            Architecture.X64 => ArchitectureX64,
            Architecture.Wasm => ArchitectureWasm,
            _ => ArchitectureUnknown,
        };
    }

    /// <summary>
    /// Get RID.
    /// </summary>
    /// <returns>Returns RID.</returns>
    public static string GetRuntimeIdentifier()
    {
        var platform = GetPlatform();
        if (platform == PlatformFreeBSD)
        {
            platform = PlatformLinux;
        }
        return $"{platform}-{GetArchitecture()}";
    }
}
