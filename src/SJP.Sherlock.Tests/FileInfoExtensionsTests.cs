using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    public class FileInfoExtensionsTests
    {
        [Test]
        public void GetLockingProcesses_WhenGivenNullFileInfo_ThrowsArgNullException()
        {
            FileInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.GetLockingProcesses());
        }

        [Test]
        public void IsFileLocked_WhenGivenNullFileInfo_ThrowsArgNullException()
        {
            FileInfo tmp = null;
            Assert.Throws<ArgumentNullException>(() => tmp.IsFileLocked());
        }

        [Test]
        public void GetLockingProcesses_WhenGivenFileWithNoLocks_ReturnsEmptySet()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            var lockingProcs = tmpPath.GetLockingProcesses();

            Assert.IsTrue(lockingProcs.Count == 0);
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectProcess()
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

        [Test]
        public void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectNumberOfLocks()
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
        public void IsFileLocked_WhenLockingNotOnPath_ReturnsFalse()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());

            var isLocked = tmpPath.IsFileLocked();
            Assert.IsFalse(isLocked);

            tmpPath.Delete();
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPath_ReturnsTrue()
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
