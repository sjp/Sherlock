using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static SJP.Sherlock.NativeMethods;

namespace SJP.Sherlock
{
    public static class RestartManager
    {
        public static ISet<IProcessInfo> GetLockingProcesses(params FileInfo[] files) => GetLockingProcesses(files as IEnumerable<FileInfo>);

        public static ISet<IProcessInfo> GetLockingProcesses(IEnumerable<FileInfo> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            var filePaths = files.Select(f => f.FullName).ToList();
            return GetLockingProcesses(filePaths);
        }

        public static ISet<IProcessInfo> GetLockingProcesses(params string[] paths) => GetLockingProcesses(paths as IEnumerable<string>);

        public static ISet<IProcessInfo> GetLockingProcesses(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            if (!Platform.SupportsRestartManager)
                throw new PlatformNotSupportedException("The Restart Manager API is available on this operating system. It was introduced in Windows NT v6.0 (i.e. Vista and Server 2008).");

            var pathsArray = paths.ToArray();
            if (pathsArray.Length == 0)
                return _emptySet;

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

                //
                // Obtain the list of affected applications/services.
                //
                // NOTE: Restart Manager returns the results into the buffer allocated by the caller. The first call to
                // RmGetList() will return the size of the buffer (i.e. nProcInfoNeeded) the caller needs to allocate.
                // The caller then needs to allocate the buffer (i.e. rgAffectedApps) and make another RmGetList()
                // call to ask Restart Manager to write the results into the buffer. However, since Restart Manager
                // refreshes the list every time RmGetList()is called, it is possible that the size returned by the first
                // RmGetList()call is not sufficient to hold the results discovered by the second RmGetList() call. Therefore,
                // it is recommended that the caller follows the following practice to handle this race condition:
                //
                //    Use a loop to call RmGetList() in case the buffer allocated according to the size returned in previous
                //    call is not enough.
                //
                uint pnProcInfo = 0;
                RM_PROCESS_INFO[] rgAffectedApps = null;
                int retry = 0;
                do
                {
                    var lpdwRebootReasons = (uint)RM_REBOOT_REASON.RmRebootReasonNone;
                    errorCode = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, rgAffectedApps, ref lpdwRebootReasons).ToErrorCode();
                    if (errorCode == WinErrorCode.ERROR_SUCCESS)
                    {
                        if (pnProcInfo == 0)
                            return _emptySet;

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
                        throw GetException(errorCode, nameof(NativeMethods.RmGetList), $"Failed to get entries (retry { retry.ToString() }).");

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

            return _emptySet;
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
            var reason = _win32ErrorMessages.ContainsKey(errorCode)
                ? _win32ErrorMessages[errorCode]
                : string.Format("0x{0:x8}", errorCode);

            var errorCodeNumber = (int)errorCode;
            throw new Win32Exception(errorCodeNumber, $"{ message } ({ apiName }() error { errorCodeNumber.ToString() }: { reason })");
        }

        private static readonly IDictionary<WinErrorCode, string> _win32ErrorMessages = new Dictionary<WinErrorCode, string>
        {
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

        private readonly static ISet<IProcessInfo> _emptySet = new HashSet<IProcessInfo>();
    }
}
