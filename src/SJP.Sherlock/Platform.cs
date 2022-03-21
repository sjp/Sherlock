using System;

namespace SJP.Sherlock;

/// <summary>
/// Provides platform-dependent information.
/// </summary>
public static class Platform
{
    /// <summary>
    /// Determines if the Restart Manager API is available on the operating system. The API was introduced in Windows NT v6.0 (i.e. Vista and Server 2008).
    /// </summary>
    public static bool SupportsRestartManager
    {
        get
        {
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var validVersion = Environment.OSVersion.Version >= MinimumRequiredWindowsVersion;

            return isWindows && validVersion;
        }
    }

    // represents NT v6.0, i.e. Windows Vista and Windows Server 2008
    private static Version MinimumRequiredWindowsVersion { get; } = new Version(6, 0);
}