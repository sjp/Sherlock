using System;
using System.Collections.Generic;
using System.IO;

namespace SJP.Sherlock
{
    public static class FileInfoExtensions
    {
        public static ISet<IProcessInfo> GetLockingProcesses(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return RestartManager.GetLockingProcesses(fileInfo.FullName);
        }

        public static bool IsFileLocked(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return RestartManager.GetLockingProcesses(fileInfo.FullName).Count > 0;
        }
    }
}
