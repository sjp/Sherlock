using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SJP.Sherlock
{
    public static class DirectoryInfoExtensions
    {
        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles();
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern);
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        public static IEnumerable<FileInfo> GetLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            var files = directory.GetFiles(searchPattern, searchOption);
            return files.Where(f => f.IsFileLocked()).ToList();
        }

        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles()
                .Where(f => f.IsFileLocked());
        }

        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern)
                .Where(f => f.IsFileLocked());
        }

        public static IEnumerable<FileInfo> EnumerateLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory
                .EnumerateFiles(searchPattern, searchOption)
                .Where(f => f.IsFileLocked());
        }

        public static ISet<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory)
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

        public static ISet<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory, string searchPattern)
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

        public static ISet<IProcessInfo> GetLockingProcesses(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
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

        public static bool ContainsLockedFiles(this DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory.GetLockingProcesses().Count > 0;
        }

        public static bool ContainsLockedFiles(this DirectoryInfo directory, string searchPattern)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory.GetLockingProcesses(searchPattern).Count > 0;
        }

        public static bool ContainsLockedFiles(this DirectoryInfo directory, string searchPattern, SearchOption searchOption)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

            return directory.GetLockingProcesses(searchPattern, searchOption).Count > 0;
        }
    }
}
