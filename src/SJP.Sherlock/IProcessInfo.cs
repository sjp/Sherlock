using System;

namespace SJP.Sherlock
{
    public interface IProcessInfo
    {
        uint ProcessId { get; }

        DateTime StartTime { get; }

        string ApplicationName { get; }

        string ServiceShortName { get; }

        ApplicationType ApplicationType { get; }

        ApplicationStatus ApplicationStatus { get; }

        uint TerminalServicesSessionId { get; }

        bool Restartable { get; }
    }
}
