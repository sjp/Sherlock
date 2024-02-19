using System;
using EnumsNET;

namespace SJP.Sherlock;

internal sealed record ProcessInfo : IProcessInfo
{
    public ProcessInfo(uint processId, DateTime startTime, string applicationName, string serviceShortName, ApplicationType appType, ApplicationStatus appStatus, uint sessionId, bool restartable)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
            throw new ArgumentNullException(nameof(applicationName));

        if (!appType.IsValid())
            throw new ArgumentException($"The {nameof(ApplicationType)} provided must be a valid enum.", nameof(appType));
        if (!appStatus.IsValid())
            throw new ArgumentException($"The {nameof(ApplicationStatus)} provided must be a valid enum.", nameof(appStatus));

        ProcessId = processId;
        StartTime = startTime;
        ApplicationName = applicationName;
        ServiceShortName = serviceShortName;
        ApplicationType = appType;
        ApplicationStatus = appStatus;
        TerminalServicesSessionId = sessionId;
        Restartable = restartable;
    }

    public uint ProcessId { get; }

    public DateTime StartTime { get; }

    public string ApplicationName { get; }

    public string ServiceShortName { get; }

    public ApplicationType ApplicationType { get; }

    public ApplicationStatus ApplicationStatus { get; }

    public uint TerminalServicesSessionId { get; }

    public bool Restartable { get; }

    public override string ToString() => ProcessId.ToString() + "@" + StartTime.ToString("o");
}