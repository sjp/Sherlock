using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJP.Sherlock
{
    /// <summary>
    /// Extension methods for getting locking information on directories.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Retrieves any files that are locked within the specified directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles();
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        /// <summary>
        /// Retrieves any files that are locked matching a search pattern within the specified directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern);
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        /// <summary>
        /// Retrieves any files that are locked matching a search pattern within the specified directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern, searchOption);
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        /// <summary>
        /// Retrieves any files that are locked within the specified directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles()
                .Where(f => f.IsFileLocked());
        }

        /// <summary>
        /// Retrieves any files that are locked matching a search pattern within the specified directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern)
                .Where(f => f.IsFileLocked());
        }

        /// <summary>
        /// Retrieves any files that are locked matching a search pattern within the specified directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>A collection of locked files, which may be empty (i.e. no locked files found).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern, searchOption)
                .Where(f => f.IsFileLocked());
        }

        /// <summary>
        /// Retrieves the set of processes which contain locks on one or more files within the directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles();
            var result = new HashSet<IProcessInfo>();

            foreach (var file in files)
            {
                var lockingProcesses = file.GetLockingProcesses();
                result.UnionWith(lockingProcesses);
            }

            return result;
        }

        /// <summary>
        /// Retrieves the set of processes which contain locks on one or more files within the directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern);
            var result = new HashSet<IProcessInfo>();

            foreach (var file in files)
            {
                var lockingProcesses = file.GetLockingProcesses();
                result.UnionWith(lockingProcesses);
            }

            return result;
        }

        /// <summary>
        /// Retrieves the set of processes which contain locks on one or more files within the directory.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns>A set of processes that lock upon one or more files in the <paramref name="directory"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static IEnumerable<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern, searchOption);
            var result = new HashSet<IProcessInfo>();

            foreach (var file in files)
            {
                var lockingProcesses = file.GetLockingProcesses();
                result.UnionWith(lockingProcesses);
            }

            return result;
        }

        /// <summary>
        /// Determines whether any files in a directory are locked by a process.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <returns><c>true</c> if any of the files in <paramref name="directory"/> are locked by a process, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static bool ContainsLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles()
                .Any(fi => fi.IsFileLocked());
        }

        /// <summary>
        /// Determines whether any files in a directory are locked by a process.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <returns><c>true</c> if any of the files in <paramref name="directory"/> are locked by a process, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static bool ContainsLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern)
                .Any(fi => fi.IsFileLocked());
        }

        /// <summary>
        /// Determines whether any files in a directory are locked by a process.
        /// </summary>
        /// <param name="directory">A directory to search for locked files.</param>
        /// <param name="searchPattern">The search string to match against the names of files in the directory.</param>
        /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
        /// <returns><c>true</c> if any of the files in <paramref name="directory"/> are locked by a process, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="directory"/> is <c>null</c>.</exception>
        public static bool ContainsLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern, searchOption)
                .Any(fi => fi.IsFileLocked());
        }
    }
}
