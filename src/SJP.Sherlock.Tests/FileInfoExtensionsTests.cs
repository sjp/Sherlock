using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    internal static class FileInfoExtensionsTests
    {
        [Test]
        public static void GetLockingProcesses_WhenGivenNullFileInfo_ThrowsArgNullException()
        {
            FileInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.GetLockingProcesses());
        }

        [Test]
        public static void IsFileLocked_WhenGivenNullFileInfo_ThrowsArgNullException()
        {
            FileInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.IsFileLocked());
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenFileWithNoLocks_ReturnsEmptySet()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            var lockingProcs = tmpPath.GetLockingProcesses();

            Assert.IsTrue(lockingProcs.Count == 0);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectProcess()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpPath);
                var process = Process.GetCurrentProcess();

                var lockingId = lockingProcs.Single().ProcessId;
                var currentId = process.Id;

                Assert.AreEqual(currentId, lockingId);
            }

            tmpPath.Delete();
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectNumberOfLocks()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpPath);
                Assert.AreEqual(1, lockingProcs.Count);
            }

            tmpPath.Delete();
        }

        [Test]
        public static void IsFileLocked_WhenLockingNotOnPath_ReturnsFalse()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());

            var isLocked = tmpPath.IsFileLocked();
            Assert.IsFalse(isLocked);

            tmpPath.Delete();
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPath_ReturnsTrue()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var isLocked = tmpPath.IsFileLocked();
                Assert.IsTrue(isLocked);
            }

            tmpPath.Delete();
        }
    }
}
