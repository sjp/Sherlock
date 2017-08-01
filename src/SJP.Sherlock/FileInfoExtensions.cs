using System;
using System.Collections.Generic;
using System.IO;

namespace SJP.Sherlock
{
    /// <summary>
    /// Extension methods for getting locking information on files.
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Retrieves the set of processes that are currently locking the file (if any).
        /// </summary>
        /// <param name="fileInfo">A file to test.</param>
        /// <returns>A set of processes that hold a lock on <paramref name="fileInfo"/>.</returns>
        /// <exception cref="PlatformNotSupportedException">The Restart Manager API is not supported on the current platform.</exception>
        public static ISet<IProcessInfo> GetLockingProcesses(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return RestartManager.GetLockingProcesses(fileInfo.FullName);
        }

        /// <summary>
        /// Determines whether the file is locked by any process.
        /// </summary>
        /// <param name="fileInfo">A file to test.</param>
        /// <returns><b>True</b> if any processes hold a lock on the file, otherwise <b>false</b>.</returns>
        /// <exception cref="PlatformNotSupportedException">The Restart Manager API is not supported on the current platform.</exception>
        public static bool IsFileLocked(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return RestartManager.GetLockingProcesses(fileInfo.FullName).Count > 0;
        }
    }
}
