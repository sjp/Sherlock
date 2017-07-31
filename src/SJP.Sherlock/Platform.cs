using System;
using System.Runtime.InteropServices;
using static SJP.Sherlock.NativeMethods;

namespace SJP.Sherlock
{
    public static class Platform
    {
        /// <summary>
        /// Determines Restart Manager API is available on the operating system. The API was introduced in Windows NT v6.0 (i.e. Vista and Server 2008.
        /// </summary>
        public static bool SupportsRestartManager => IsWindows && OsVersion >= MinimumRequiredWindowsVersion;

        private static bool IsWindows => _isWindows.Value;

        private readonly static Lazy<bool> _isWindows = new Lazy<bool>(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        private static Version OsVersion => _osVersion.Value;

        private readonly static Lazy<Version> _osVersion = new Lazy<Version>(GetOsVersion);

        private static Version GetOsVersion()
        {
            var versionInfo = new OSVERSIONINFOEX { dwOSVersionInfoSize = Marshal.SizeOf<OSVERSIONINFOEX>() };
            GetVersionEx(ref versionInfo);

            return new Version(versionInfo.dwMajorVersion, versionInfo.dwMinorVersion, versionInfo.dwBuildNumber);
        }

        // represents NT v6.0, i.e. Windows Vista and Windows Server 2008
        private static Version MinimumRequiredWindowsVersion { get; } = new Version(6, 0);
    }
}
