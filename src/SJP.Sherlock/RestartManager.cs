using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static SJP.Sherlock.NativeMethods;

namespace SJP.Sherlock;

/// <summary>
/// Provides methods for retrieving file locking information via the Windows Restart Manager API.
/// </summary>
public static class RestartManager
{
    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files within the directory.
    /// </summary>
    /// <param name="directory">A directory to search for locked files.</param>
    /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(DirectoryInfo directory)
    {
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        if (!Platform.SupportsRestartManager)
            return Array.Empty<IProcessInfo>();

        var files = directory.GetFiles();
        var result = new HashSet<IProcessInfo>();

        foreach (var file in files)
        {
            var lockingProcesses = file.GetLockingProcesses();
            result.UnionWith(lockingProcesses);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files within the directory.
    /// </summary>
    /// <param name="directory">A directory to search for locked files.</param>
    /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
    /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(DirectoryInfo directory, string searchPattern)
    {
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        if (!Platform.SupportsRestartManager)
            return Array.Empty<IProcessInfo>();

        var files = directory.GetFiles(searchPattern);
        var result = new HashSet<IProcessInfo>();

        foreach (var file in files)
        {
            var lockingProcesses = file.GetLockingProcesses();
            result.UnionWith(lockingProcesses);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files within the directory.
    /// </summary>
    /// <param name="directory">A directory to search for locked files.</param>
    /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(DirectoryInfo directory, string searchPattern, SearchOption searchOption)
    {
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));

        if (!Platform.SupportsRestartManager)
            return Array.Empty<IProcessInfo>();

        var files = directory.GetFiles(searchPattern, searchOption);
        var result = new HashSet<IProcessInfo>();

        foreach (var file in files)
        {
            var lockingProcesses = file.GetLockingProcesses();
            result.UnionWith(lockingProcesses);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files.
    /// </summary>
    /// <param name="files">A set of files to test for a process holding a lock.</param>
    /// <returns>A set of processes that lock upon one or more files in <paramref name="files"/>.</returns>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(params FileInfo[] files) => GetLockingProcesses(files as IEnumerable<FileInfo>);

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files.
    /// </summary>
    /// <param name="files">A set of files to test for a process holding a lock.</param>
    /// <returns>A set of processes that lock upon one or more files in <paramref name="files"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="files"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="files"/> contains a <c>null</c> value.</exception>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(IEnumerable<FileInfo> files)
    {
        if (files == null)
            throw new ArgumentNullException(nameof(files));
        if (files.Any(f => f == null))
            throw new ArgumentException($"A null {nameof(FileInfo)} was provided.", nameof(files));

        if (!Platform.SupportsRestartManager)
            return Array.Empty<IProcessInfo>();

        var filePaths = files.Select(f => f.FullName).ToList();
        return GetLockingProcesses(filePaths);
    }

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files.
    /// </summary>
    /// <param name="paths">A set of paths to test for a process holding a lock.</param>
    /// <returns>A set of processes that lock upon one or more files in <paramref name="paths"/>.</returns>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(params string[] paths) => GetLockingProcesses(paths as IEnumerable<string>);

    /// <summary>
    /// Retrieves the set of processes which contain locks on one or more files.
    /// </summary>
    /// <param name="paths">A set of file paths to test for a process holding a lock.</param>
    /// <returns>A set of processes that lock upon one or more files in <paramref name="paths"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="paths"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="paths"/> contains a <c>null</c> value.</exception>
    public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(IEnumerable<string> paths)
    {
        if (paths == null)
            throw new ArgumentNullException(nameof(paths));
        if (paths.Any(p => p == null))
            throw new ArgumentException("A null file path was provided.", nameof(paths));

        if (!Platform.SupportsRestartManager)
            return Array.Empty<IProcessInfo>();

        var pathsArray = paths.ToArray();
        if (pathsArray.Length == 0)
            return Array.Empty<IProcessInfo>();

        const int maxRetries = 10;

        // See http://blogs.msdn.com/b/oldnewthing/archive/2012/02/17/10268840.aspx.
        var key = new string('\0', CCH_RM_SESSION_KEY + 1);

        var errorCode = RmStartSession(out var handle, 0, key).ToErrorCode();
        if (errorCode != WinErrorCode.ERROR_SUCCESS)
            throw GetException(errorCode, nameof(NativeMethods.RmStartSession), "Failed to begin restart manager session.");

        try
        {
            var resources = pathsArray;
            errorCode = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null).ToErrorCode();
            if (errorCode != WinErrorCode.ERROR_SUCCESS)
                throw GetException(errorCode, nameof(NativeMethods.RmRegisterResources), "Could not register resources.");

            // Repeated calls to RmGetList will often be required.
            // Keep calling until ERROR_MORE_DATA is no longer returned or max # of retries is reached, whichever is first.
            uint pnProcInfo = 0;
            RM_PROCESS_INFO[]? rgAffectedApps = null;
            var retry = 0;
            do
            {
                var lpdwRebootReasons = (uint)RM_REBOOT_REASON.RmRebootReasonNone;
                errorCode = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, rgAffectedApps, ref lpdwRebootReasons).ToErrorCode();
                if (errorCode == WinErrorCode.ERROR_SUCCESS)
                {
                    if (pnProcInfo == 0 || rgAffectedApps == null)
                        return Array.Empty<IProcessInfo>();

                    var lockInfos = new List<IProcessInfo>((int)pnProcInfo);
                    for (var i = 0; i < pnProcInfo; i++)
                    {
                        var rgAffectedApp = rgAffectedApps[i];
                        var procInfo = CreateFromRmProcessInfo(rgAffectedApp);
                        lockInfos.Add(procInfo);
                    }

                    return new HashSet<IProcessInfo>(lockInfos);
                }

                if (errorCode != WinErrorCode.ERROR_MORE_DATA)
                    throw GetException(errorCode, nameof(NativeMethods.RmGetList), $"Failed to get entries (retry {retry}).");

                pnProcInfo = pnProcInfoNeeded;
                rgAffectedApps = new RM_PROCESS_INFO[pnProcInfo];
            } while ((errorCode == WinErrorCode.ERROR_MORE_DATA) && (retry++ < maxRetries));
        }
        finally
        {
            errorCode = RmEndSession(handle).ToErrorCode();
            if (errorCode != WinErrorCode.ERROR_SUCCESS)
                throw GetException(errorCode, nameof(NativeMethods.RmEndSession), "Failed to end the restart manager session.");
        }

        return Array.Empty<IProcessInfo>();
    }

    private static IProcessInfo CreateFromRmProcessInfo(RM_PROCESS_INFO procInfo)
    {
        var processId = procInfo.Process.dwProcessId;

        // ProcessStartTime is returned as local time, not UTC.
        var highDateTime = (long)procInfo.Process.ProcessStartTime.dwHighDateTime;
        highDateTime <<= 32;
        var lowDateTime = procInfo.Process.ProcessStartTime.dwLowDateTime;
        var fileTime = highDateTime | lowDateTime;

        var startTime = DateTime.FromFileTime(fileTime);
        var applicationName = procInfo.strAppName;
        var serviceShortName = procInfo.strServiceShortName;
        var applicationType = (ApplicationType)procInfo.ApplicationType;
        var applicationStatus = (ApplicationStatus)procInfo.AppStatus;
        var terminalServicesSessionId = procInfo.TSSessionId;
        var restartable = procInfo.bRestartable;

        return new ProcessInfo(
            processId,
            startTime,
            applicationName,
            serviceShortName,
            applicationType,
            applicationStatus,
            terminalServicesSessionId,
            restartable
        );
    }

    private static Exception GetException(WinErrorCode errorCode, string apiName, string message)
    {
        var errorCodeNumber = (int)errorCode;
        var reason = _win32ErrorMessages.TryGetValue(errorCode, out var errorMessage)
            ? errorMessage
            : string.Format("0x{0:x8}", errorCodeNumber);

        return new Win32Exception(errorCodeNumber, $"{message} ({apiName}() error {errorCodeNumber}: {reason})");
    }

    private static readonly IDictionary<WinErrorCode, string> _win32ErrorMessages = new Dictionary<WinErrorCode, string>
    {
        [WinErrorCode.ERROR_ACCESS_DENIED] = "Access is denied.",
        [WinErrorCode.ERROR_SEM_TIMEOUT] = "A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.",
        [WinErrorCode.ERROR_BAD_ARGUMENTS] = "One or more arguments are not correct. This error value is returned by the Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.",
        [WinErrorCode.ERROR_MAX_SESSIONS_REACHED] = "The maximum number of sessions has been reached.",
        [WinErrorCode.ERROR_WRITE_FAULT] = "An operation was unable to read or write to the registry.",
        [WinErrorCode.ERROR_OUTOFMEMORY] = "A Restart Manager operation could not complete because not enough memory was available.",
        [WinErrorCode.ERROR_CANCELLED] = "The current operation is canceled by user.",
        [WinErrorCode.ERROR_MORE_DATA] = "More data is available.",
        [WinErrorCode.ERROR_INVALID_HANDLE] = "No Restart Manager session exists for the handle supplied."
        // ignoring ERROR_SUCCESS because this is the OK case
    };
}