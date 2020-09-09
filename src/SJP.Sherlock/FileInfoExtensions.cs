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
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <c>null</c>.</exception>
        public static IReadOnlyCollection<IProcessInfo> GetLockingProcesses(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            return RestartManager.GetLockingProcesses(fileInfo.FullName);
        }

        /// <summary>
        /// Determines whether the file is locked by any process.
        /// </summary>
        /// <param name="fileInfo">A file to test.</param>
        /// <returns><c>true</c> if any processes hold a lock on the file, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <c>null</c>.</exception>
        public static bool IsFileLocked(this FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            if (!Platform.SupportsRestartManager)
                return IsSimpleFileLocked(fileInfo);

            return RestartManager.GetLockingProcesses(fileInfo.FullName).Count > 0;
        }

        private static bool IsSimpleFileLocked(FileInfo file)
        {
            try
            {
                using (_ = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    return false;
            }
            catch (IOException ex)
            {
                return ex.IsFileLocked();
            }
            catch
            {
                // wasn't due to file locking that an exception was thrown
                return false;
            }
        }
    }
}
