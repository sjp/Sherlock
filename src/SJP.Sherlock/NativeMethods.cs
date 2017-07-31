using System;
using System.Runtime.InteropServices;

namespace SJP.Sherlock
{
    internal static class NativeMethods
    {
        [DllImport(RestartManagerDll, CharSet = CharSet.Unicode)]
        internal static extern int RmRegisterResources(
            uint pSessionHandle,
            uint nFiles,
            string[] rgsFilenames,
            uint nApplications,
            [In] RM_UNIQUE_PROCESS[] rgApplications,
            uint nServices,
            string[] rgsServiceNames
        );

        [DllImport(RestartManagerDll, CharSet = CharSet.Unicode)]
        internal static extern int RmStartSession(
            out uint pSessionHandle,
            int dwSessionFlags,
            string strSessionKey
        );

        [DllImport(RestartManagerDll)]
        internal static extern int RmEndSession(uint pSessionHandle);

        [DllImport(RestartManagerDll, CharSet = CharSet.Unicode)]
        internal static extern int RmGetList(
            uint dwSessionHandle,
            out uint pnProcInfoNeeded,
            ref uint pnProcInfo,
            [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
            ref uint lpdwRebootReasons
        );

        /// <summary>
        /// Contains a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            /// <summary>
            /// The low-order part of the file time.
            /// </summary>
            public uint dwLowDateTime;

            /// <summary>
            /// The high-order part of the file time.
            /// </summary>
            public uint dwHighDateTime;
        }

        /// <summary>
        /// Uniquely identifies a process by its PID and the time the process began. An array of <see cref="RM_UNIQUE_PROCESS"/> structures can be passed to the <see cref="RmRegisterResources(uint, uint, string[], uint, RM_UNIQUE_PROCESS[], uint, string[])"/> function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct RM_UNIQUE_PROCESS
        {
            /// <summary>
            /// The product identifier (PID).
            /// </summary>
            public uint dwProcessId;

            /// <summary>
            /// The creation time of the process. The time is provided as a <see cref="FILETIME"/> structure that is returned by the lpCreationTime parameter of the GetProcessTimes function.
            /// </summary>
            public FILETIME ProcessStartTime;
        }

        private static readonly int RM_SESSION_KEY_LEN = Guid.Empty.ToByteArray().Length; // 16-byte
        internal static readonly int CCH_RM_SESSION_KEY = RM_SESSION_KEY_LEN * 2;

        private const string RestartManagerDll = "rstrtmgr.dll";

        internal enum WinErrorCode
        {
            /// <summary>
            /// The function completed successfully.
            /// </summary>
            ERROR_SUCCESS = 0,

            /// <summary>
            /// No Restart Manager session exists for the handle supplied.
            /// </summary>
            ERROR_INVALID_HANDLE = 6,

            /// <summary>
            /// A Restart Manager operation could not complete because not enough memory was available.
            /// </summary>
            ERROR_OUTOFMEMORY = 14,

            /// <summary>
            /// An operation was unable to read or write to the registry.
            /// </summary>
            ERROR_WRITE_FAULT = 29,

            /// <summary>
            /// The process cannot access the file because it is being used by another process.
            /// </summary>
            ERROR_SHARING_VIOLATION = 32,

            /// <summary>
            /// The process cannot access the file because another process has locked a portion of the file.
            /// </summary>
            ERROR_LOCK_VIOLATION = 33,

            /// <summary>
            /// A Restart Manager function could not obtain a Registry write mutex in the allotted time. A system restart is recommended because further use of the Restart Manager is likely to fail.
            /// </summary>
            ERROR_SEM_TIMEOUT = 121,

            /// <summary>
            /// One or more arguments are not correct. This error value is returned by the Restart Manager function if a <b>NULL</b> pointer or 0 is passed in a parameter that requires a non-<b>null</b> and non-zero value.
            /// </summary>
            ERROR_BAD_ARGUMENTS = 160,

            /// <summary>
            /// This error value is returned by the <see cref="RmGetList(uint, out uint, ref uint, RM_PROCESS_INFO[], ref uint)"/> function if the <i>rgAffectedApps</i> buffer is too small to hold all application information in the list.
            /// </summary>
            ERROR_MORE_DATA = 234,

            /// <summary>
            /// The maximum number of sessions has been reached.
            /// </summary>
            ERROR_MAX_SESSIONS_REACHED = 353,

            /// <summary>
            /// The current operation is canceled by user.
            /// </summary>
            ERROR_CANCELLED = 1223
        }

        /// <summary>
        /// Specifies the type of application that is described by the <see cref="RM_PROCESS_INFO"/> structure.
        /// </summary>
        internal enum RM_APP_TYPE
        {
            /// <summary>
            /// The application cannot be classified as any other type. An application of this type can only be shut down by a forced shutdown.
            /// </summary>
            RmUnknownApp = 0,

            /// <summary>
            /// A Windows application run as a stand-alone process that displays a top-level window.
            /// </summary>
            RmMainWindow = 1,

            /// <summary>
            /// A Windows application that does not run as a stand-alone process and does not display a top-level window.
            /// </summary>
            RmOtherWindow = 2,

            /// <summary>
            /// The application is a Windows service.
            /// </summary>
            RmService = 3,

            /// <summary>
            /// The application is Windows Explorer.
            /// </summary>
            RmExplorer = 4,

            /// <summary>
            /// The application is a stand-alone console application.
            /// </summary>
            RmConsole = 5,

            /// <summary>
            /// A system restart is required to complete the installation because a process cannot be shut down. The process cannot be shut down because of the following reasons. The process may be a critical process. The current user may not have permission to shut down the process. The process may belong to the primary installer that started the Restart Manager.
            /// </summary>
            RmCritical = 1000
        }

        /// <summary>
        /// Describes the current status of an application that is acted upon by the Restart Manager.
        /// </summary>
        internal enum RM_APP_STATUS
        {
            /// <summary>
            /// The application is in a state that is not described by any other enumerated state.
            /// </summary>
            RmStatusUnknown = 0x0,

            /// <summary>
            /// The application is currently running.
            /// </summary>
            RmStatusRunning = 0x1,

            /// <summary>
            /// The Restart Manager has stopped the application.
            /// </summary>
            RmStatusStopped = 0x2,

            /// <summary>
            /// An action outside the Restart Manager has stopped the application.
            /// </summary>
            RmStatusStoppedOther = 0x4,

            /// <summary>
            /// The Restart Manager has restarted the application.
            /// </summary>
            RmStatusRestarted = 0x8,

            /// <summary>
            /// The Restart Manager encountered an error when stopping the application.
            /// </summary>
            RmStatusErrorOnStop = 0x10,

            /// <summary>
            /// The Restart Manager encountered an error when restarting the application.
            /// </summary>
            RmStatusErrorOnRestart = 0x20,

            /// <summary>
            /// Shutdown is masked by a filter.
            /// </summary>
            RmStatusShutdownMasked = 0x40,

            /// <summary>
            /// Restart is masked by a filter.
            /// </summary>
            RmStatusRestartMasked = 0x80
        }

        /// <summary>
        /// Describes the reasons a restart of the system is needed.
        /// </summary>
        internal enum RM_REBOOT_REASON
        {
            /// <summary>
            /// A system restart is not required.
            /// </summary>
            RmRebootReasonNone = 0x0,

            /// <summary>
            /// The current user does not have sufficient privileges to shut down one or more processes.
            /// </summary>
            RmRebootReasonPermissionDenied = 0x1,

            /// <summary>
            /// One or more processes are running in another Terminal Services session.
            /// </summary>
            RmRebootReasonSessionMismatch = 0x2,

            /// <summary>
            /// A system restart is needed because one or more processes to be shut down are critical processes.
            /// </summary>
            RmRebootReasonCriticalProcess = 0x4,

            /// <summary>
            /// A system restart is needed because one or more services to be shut down are critical services.
            /// </summary>
            RmRebootReasonCriticalService = 0x8,

            /// <summary>
            /// A system restart is needed because the current process must be shut down.
            /// </summary>
            RmRebootReasonDetectedSelf = 0x10
        }

        /// <summary>
        /// Describes an application that is to be registered with the Restart Manager.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RM_PROCESS_INFO
        {
            /// <summary>
            /// Contains an <see cref="RM_UNIQUE_PROCESS"/> structure that uniquely identifies the application by its PID and the time the process began.
            /// </summary>
            public RM_UNIQUE_PROCESS Process;

            /// <summary>
            /// If the process is a service, this parameter returns the long name for the service. If the process is not a service, this parameter returns the user-friendly name for the application. If the process is a critical process, and the installer is run with elevated privileges, this parameter returns the name of the executable file of the critical process. If the process is a critical process, and the installer is run as a service, this parameter returns the long name of the critical process.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;

            /// <summary>
            /// If the process is a service, this is the short name for the service. This member is not used if the process is not a service.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;

            /// <summary>
            /// Contains an <see cref="RM_APP_TYPE"/> enumeration value that specifies the type of application as <see cref="RM_APP_TYPE.RmUnknownApp"/>, <see cref="RM_APP_TYPE.RmMainWindow"/>, <see cref="RM_APP_TYPE.RmOtherWindow"/>, <see cref="RM_APP_TYPE.RmService"/>, <see cref="RM_APP_TYPE.RmExplorer"/> or <see cref="RM_APP_TYPE.RmCritical"/>.
            /// </summary>
            public RM_APP_TYPE ApplicationType;

            /// <summary>
            /// Contains a bit mask that describes the current status of the application. See the <see cref="RM_APP_STATUS"/> enumeration.
            /// </summary>
            public uint AppStatus;

            /// <summary>
            /// Contains the Terminal Services session ID of the process. If the terminal session of the process cannot be determined, the value of this member is set to RM_INVALID_SESSION (-1). This member is not used if the process is a service or a system critical process.
            /// </summary>
            public uint TSSessionId;

            /// <summary>
            /// <b>TRUE</b> if the application can be restarted by the Restart Manager; otherwise, <b>FALSE</b>. This member is always <b>TRUE</b> if the process is a service. This member is always <b>FALSE</b> if the process is a critical system process.
            /// </summary>
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;

            private const int CCH_RM_MAX_APP_NAME = 255;
            private const int CCH_RM_MAX_SVC_NAME = 63;
        }

        [DllImport("kernel32")]
        internal static extern bool GetVersionEx(ref OSVERSIONINFOEX versionInfo);

        /// <summary>
        /// Contains operating system version information. The information includes major and minor version numbers, a build number, a platform identifier, and information about product suites and the latest Service Pack installed on the system. This structure is used with the GetVersionEx and VerifyVersionInfo functions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct OSVERSIONINFOEX
        {
            /// <summary>
            /// The size of this data structure, in bytes. Set this member to sizeof(OSVERSIONINFOEX).
            /// </summary>
            public int dwOSVersionInfoSize;

            /// <summary>
            /// The major version number of the operating system. For more information, see Remarks.
            /// </summary>
            public int dwMajorVersion;

            /// <summary>
            /// The minor version number of the operating system. For more information, see Remarks.
            /// </summary>
            public int dwMinorVersion;

            /// <summary>
            /// The build number of the operating system.
            /// </summary>
            public int dwBuildNumber;

            /// <summary>
            /// The operating system platform. This member can be VER_PLATFORM_WIN32_NT (2).
            /// </summary>
            public int dwPlatformId;

            /// <summary>
            /// A null-terminated string, such as "Service Pack 3", that indicates the latest Service Pack installed on the system. If no Service Pack has been installed, the string is empty.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;

            /// <summary>
            /// The major version number of the latest Service Pack installed on the system. For example, for Service Pack 3, the major version number is 3. If no Service Pack has been installed, the value is zero.
            /// </summary>
            public ushort wServicePackMajor;

            /// <summary>
            /// The minor version number of the latest Service Pack installed on the system. For example, for Service Pack 3, the minor version number is 0.
            /// </summary>
            public ushort wServicePackMinor;

            /// <summary>
            /// A bit mask that identifies the product suites available on the system. This member can be a combination of the following values.
            /// </summary>
            public OSVERIONINFOEX_SUITE_MASK wSuiteMask;

            /// <summary>
            /// Any additional information about the system. This member can be one of the following values.
            /// </summary>
            public OSVERSIONINFOEX_PRODUCT_TYPE wProductType;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public byte wReserved;
        }

        // TODO: If/when this project is updated to .NET Standard 2.0 we can remove this
        //       because it's only necessary as we don't have access to Environment.OSVersion.
        //       We need access to the NT version to determine whether RestartManager is available.
        /// <summary>
        ///
        /// </summary>
        [Flags]
        internal enum OSVERIONINFOEX_SUITE_MASK : ushort
        {
            /// <summary>
            /// Microsoft BackOffice components are installed.
            /// </summary>
            VER_SUITE_BACKOFFICE = 0x4,

            /// <summary>
            /// Windows Server 2003, Web Edition is installed.
            /// </summary>
            VER_SUITE_BLADE = 0x400,

            /// <summary>
            /// Windows Server 2003, Compute Cluster Edition is installed.
            /// </summary>
            VER_SUITE_COMPUTE_SERVER = 0x4000,

            /// <summary>
            /// Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed.
            /// </summary>
            VER_SUITE_DATACENTER = 0x80,

            /// <summary>
            /// Windows Server 2008 Enterprise, Windows Server 2003, Enterprise Edition, or Windows 2000 Advanced Server is installed. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_ENTERPRISE = 0x2,

            /// <summary>
            /// Windows XP Embedded is installed.
            /// </summary>
            VER_SUITE_EMBEDDEDNT = 0x40,

            /// <summary>
            /// Windows Vista Home Premium, Windows Vista Home Basic, or Windows XP Home Edition is installed.
            /// </summary>
            VER_SUITE_PERSONAL = 0x200,

            /// <summary>
            /// Remote Desktop is supported, but only one interactive session is supported. This value is set unless the system is running in application server mode.
            /// </summary>
            VER_SUITE_SINGLEUSERTS = 0x100,

            /// <summary>
            /// Microsoft Small Business Server was once installed on the system, but may have been upgraded to another version of Windows. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_SMALLBUSINESS = 0x1,

            /// <summary>
            /// Microsoft Small Business Server is installed with the restrictive client license in force. Refer to the Remarks section for more information about this bit flag.
            /// </summary>
            VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x20,

            /// <summary>
            /// Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed.
            /// </summary>
            VER_SUITE_STORAGE_SERVER = 0x2000,

            /// <summary>
            /// Terminal Services is installed. This value is always set. If <see cref="VER_SUITE_TERMINAL"/> is set but <see cref="VER_SUITE_SINGLEUSERTS"/> is not set, the system is running in application server mode.
            /// </summary>
            VER_SUITE_TERMINAL = 0x10,

            /// <summary>
            /// Windows Home Server is installed.
            /// </summary>
            VER_SUITE_WH_SERVER = 0x8000
        }

        [Flags]
        internal enum OSVERSIONINFOEX_PRODUCT_TYPE : byte
        {
            /// <summary>
            /// The system is a domain controller and the operating system is Windows Server 2012 , Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server.
            /// </summary>
            VER_NT_DOMAIN_CONTROLLER = 0x2,

            /// <summary>
            /// The operating system is Windows Server 2012, Windows Server 2008 R2, Windows Server 2008, Windows Server 2003, or Windows 2000 Server. Note that a server that is also a domain controller is reported as <see cref="VER_NT_DOMAIN_CONTROLLER"/>, not <see cref="VER_NT_SERVER"/>.
            /// </summary>
            VER_NT_SERVER = 0x3,

            /// <summary>
            /// The operating system is Windows 8, Windows 7, Windows Vista, Windows XP Professional, Windows XP Home Edition, or Windows 2000 Professional.
            /// </summary>
            VER_NT_WORKSTATION = 0x1
        }
    }
}
