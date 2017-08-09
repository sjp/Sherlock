using System;
using EnumsNET;

namespace SJP.Sherlock
{
    internal sealed class ProcessInfo : IProcessInfo, IEquatable<ProcessInfo>
    {
        public ProcessInfo(uint processId, DateTime startTime, string applicationName, string serviceShortName, ApplicationType appType, ApplicationStatus appStatus, uint sessionId, bool restartable)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentNullException(nameof(applicationName));

            if (!appType.IsValid())
                throw new ArgumentException($"The { nameof(ApplicationType) } provided must be a valid enum.", nameof(appType));
            if (!appStatus.IsValid())
                throw new ArgumentException($"The { nameof(ApplicationStatus) } provided must be a valid enum.", nameof(appStatus));

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

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + ProcessId.GetHashCode();
                hash = (hash * 23) + StartTime.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ProcessInfo a, ProcessInfo b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ProcessInfo a, ProcessInfo b)
        {
            if (ReferenceEquals(a, b))
                return false;

            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
                return true;

            return !a.Equals(b);
        }

        public bool Equals(ProcessInfo other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return other.ProcessId == ProcessId && other.StartTime == StartTime;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ProcessInfo);
        }

        public override string ToString() => ProcessId.ToString() + "@" + StartTime.ToString("o");
    }
}
