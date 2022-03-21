using System;

namespace SJP.Sherlock;

/// <summary>
/// Provides information describing Windows processes.
/// </summary>
public interface IProcessInfo
{
    /// <summary>
    /// The process ID.
    /// </summary>
    uint ProcessId { get; }

    /// <summary>
    /// The time at which the process was started.
    /// </summary>
    DateTime StartTime { get; }

    /// <summary>
    /// If the process is a service, this parameter returns the long name for the service. If the process is not a service, this parameter returns the user-friendly name for the application. If the process is a critical process, and the installer is run with elevated privileges, this parameter returns the name of the executable file of the critical process. If the process is a critical process, and the installer is run as a service, this parameter returns the long name of the critical process.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// If the process is a service, this is the short name for the service. This member is not used if the process is not a service.
    /// </summary>
    string ServiceShortName { get; }

    /// <summary>
    /// Describes the type of the application. e.g. service, console application, etc.
    /// </summary>
    ApplicationType ApplicationType { get; }

    /// <summary>
    /// Describes the current status of the application
    /// </summary>
    ApplicationStatus ApplicationStatus { get; }

    /// <summary>
    /// Contains the Terminal Services session ID of the process. If the terminal session of the process cannot be determined, the value of this member is set to -1. This member is not used if the process is a service or a system critical process.
    /// </summary>
    uint TerminalServicesSessionId { get; }

    /// <summary>
    /// <c>true</c> if the application can be restarted by the Restart Manager; otherwise, <c>false</c>. This member is always <c>true</c> if the process is a service. This member is always <c>false</c> if the process is a critical system process.
    /// </summary>
    bool Restartable { get; }
}
