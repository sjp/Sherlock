using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.RestartManager;

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
            return [];

        return directory.GetFiles()
            .SelectMany(f => f.GetLockingProcesses())
            .ToHashSet();
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
            return [];

        return directory.GetFiles(searchPattern)
            .SelectMany(f => f.GetLockingProcesses())
            .ToHashSet();
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
            return [];

        return directory.GetFiles(searchPattern, searchOption)
            .SelectMany(f => f.GetLockingProcesses())
            .ToHashSet();
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
            return [];

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
            return [];

        var pathsArray = paths.ToArray();
        if (pathsArray.Length == 0)
            return [];

        const int maxRetries = 10;

        // See https://devblogs.microsoft.com/oldnewthing/20120217-00/?p=8283
        var sessionKey = "sherlock-" + Guid.NewGuid().ToString();

        uint sessionHandle;
        unsafe
        {
            fixed (char* strSessionKey = sessionKey)
            {
                var sessionKeySpan = new Span<char>(strSessionKey, sessionKey.Length);
                var errorCode = PInvoke.RmStartSession(out sessionHandle, sessionKeySpan);
                if (errorCode != WIN32_ERROR.ERROR_SUCCESS)
                    throw GetException(errorCode, nameof(PInvoke.RmStartSession), "Failed to begin restart manager session.");
            }
        }

        var lockInfos = new List<IProcessInfo>();

        try
        {
            unsafe
            {
                fixed (PCWSTR* rgsFileNames = new PCWSTR[pathsArray.Length])
                {
                    var intPtrs = new IntPtr[pathsArray.Length];

                    try
                    {
                        for (var i = 0; i < pathsArray.Length; i++)
                        {
                            var intPtr = Marshal.StringToHGlobalUni(pathsArray[i]);
                            intPtrs[i] = intPtr;
                            rgsFileNames[i] = new PCWSTR((char*)intPtr.ToPointer());
                        }

                        var registerErrorCode = PInvoke.RmRegisterResources(sessionHandle, (uint)pathsArray.Length, rgsFileNames, 0, null, 0, null);
                        if (registerErrorCode != WIN32_ERROR.ERROR_SUCCESS)
                            throw GetException(registerErrorCode, nameof(PInvoke.RmRegisterResources), "Could not register resources.");
                    }
                    finally
                    {
                        foreach (var intPtr in intPtrs)
                        {
                            Marshal.FreeHGlobal(intPtr);
                        }
                    }
                }
            }

            // Repeated calls to RmGetList will often be required.
            // Keep calling until ERROR_MORE_DATA is no longer returned or max # of retries is reached, whichever is first.
            unsafe
            {
                uint pnProcInfo = 0;
                var retry = 0;
                var affectedApps = 0;
                WIN32_ERROR errorCode;
                do
                {
                    fixed (RM_PROCESS_INFO* rgAffectedApps = new RM_PROCESS_INFO[affectedApps])
                    {
                        var rgAffectedAppsSpan = new Span<RM_PROCESS_INFO>(rgAffectedApps, affectedApps);
                        errorCode = PInvoke.RmGetList(sessionHandle, out var pnProcInfoNeeded, ref pnProcInfo, rgAffectedAppsSpan, out _);
                        if (errorCode != WIN32_ERROR.ERROR_MORE_DATA && errorCode != WIN32_ERROR.ERROR_SUCCESS)
                            throw GetException(errorCode, nameof(PInvoke.RmGetList), $"Failed to get entries (retry {retry}).");

                        for (var i = 0; i < pnProcInfo; i++)
                        {
                            var rgAffectedApp = rgAffectedApps[i];
                            var procInfo = CreateFromRmProcessInfo(rgAffectedApp);
                            lockInfos.Add(procInfo);
                        }

                        if (errorCode == WIN32_ERROR.ERROR_SUCCESS)
                            break;

                        pnProcInfo = pnProcInfoNeeded;
                        affectedApps = (int)pnProcInfo;
                    }
                } while ((errorCode == WIN32_ERROR.ERROR_MORE_DATA) && (retry++ < maxRetries));
            }
        }
        finally
        {
            var errorCode = PInvoke.RmEndSession(sessionHandle);
            if (errorCode != WIN32_ERROR.ERROR_SUCCESS)
                throw GetException(errorCode, nameof(PInvoke.RmEndSession), "Failed to end the restart manager session.");
        }

        return lockInfos.ToHashSet();
    }

    private static IProcessInfo CreateFromRmProcessInfo(RM_PROCESS_INFO procInfo)
    {
        var processId = procInfo.Process.dwProcessId;

        // ProcessStartTime is returned as local time, not UTC.
        long fileTime;
        unchecked
        {
            var highDateTime = ((long)procInfo.Process.ProcessStartTime.dwHighDateTime << 32);
            var lowDateTime = (uint)procInfo.Process.ProcessStartTime.dwLowDateTime;
            fileTime = highDateTime | lowDateTime;
        }

        var startTime = DateTime.FromFileTime(fileTime);
        var applicationName = procInfo.strAppName.ToString();
        var serviceShortName = procInfo.strServiceShortName.ToString();
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

    private static Exception GetException(WIN32_ERROR errorCode, string apiName, string message)
    {
        var errorCodeNumber = (int)errorCode;
        var reason = Win32ErrorMessages.TryGetValue(errorCode, out var errorMessage)
            ? errorMessage
            : string.Format("0x{0:x8}", errorCodeNumber);

        return new Win32Exception(errorCodeNumber, $"{message} ({apiName}() error {errorCodeNumber}: {reason})");
    }

    private static readonly IReadOnlyDictionary<WIN32_ERROR, string> Win32ErrorMessages = new Dictionary<WIN32_ERROR, string>
    {
        [WIN32_ERROR.ERROR_ACCESS_DENIED] = "Access is denied.",
        [WIN32_ERROR.ERROR_SEM_TIMEOUT] = "A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.",
        [WIN32_ERROR.ERROR_BAD_ARGUMENTS] = "One or more arguments are not correct. This error value is returned by the Restart Manager function if a NULL pointer or 0 is passed in a parameter that requires a non-null and non-zero value.",
        [WIN32_ERROR.ERROR_MAX_SESSIONS_REACHED] = "The maximum number of sessions has been reached.",
        [WIN32_ERROR.ERROR_WRITE_FAULT] = "An operation was unable to read or write to the registry.",
        [WIN32_ERROR.ERROR_OUTOFMEMORY] = "A Restart Manager operation could not complete because not enough memory was available.",
        [WIN32_ERROR.ERROR_CANCELLED] = "The current operation is canceled by user.",
        [WIN32_ERROR.ERROR_MORE_DATA] = "More data is available.",
        [WIN32_ERROR.ERROR_INVALID_HANDLE] = "No Restart Manager session exists for the handle supplied."
        // ignoring ERROR_SUCCESS because this is the OK case
    };
}