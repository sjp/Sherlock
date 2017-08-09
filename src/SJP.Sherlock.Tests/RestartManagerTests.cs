using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    public class RestartManagerTests
    {
        [Test]
        public void GetLockingProcesses_WhenGivenNullStringArgs_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RestartManager.GetLockingProcesses((IEnumerable<string>)null));
        }

        [Test]
        public void GetLockingProcesses_WhenGivenNullFileInfoArgs_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RestartManager.GetLockingProcesses((IEnumerable<FileInfo>)null));
        }

        [Test]
        public void GetLockingProcesses_WhenGivenNullDirectoryInfoArgs_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RestartManager.GetLockingProcesses((DirectoryInfo)null));
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPathAndGivenString_ReturnsNonEmptyLockingSet()
        {
            var tmpPath = Path.GetTempFileName();
            using (var file = File.Open(tmpPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpPath);
                Assert.IsTrue(lockingProcs.Any());
            }

            File.Delete(tmpPath);
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPathAndGivenString_ReturnsCorrectProcess()
        {
            var tmpPath = Path.GetTempFileName();
            using (var file = File.Open(tmpPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpPath);
                var process = Process.GetCurrentProcess();

                var lockingId = lockingProcs.Single().ProcessId;
                var currentId = process.Id;

                Assert.AreEqual(currentId, lockingId);
            }

            File.Delete(tmpPath);
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPathAndGivenDirectory_ReturnsNonEmptyLockingSet()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpDir);
                Assert.IsTrue(lockingProcs.Any());
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void GetLockingProcesses_WhenLockingOnPathAndGivenDirectory_ReturnsCorrectProcess()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            using (var file = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpDir);
                var process = Process.GetCurrentProcess();

                var lockingId = lockingProcs.Single().ProcessId;
                var currentId = process.Id;

                Assert.AreEqual(currentId, lockingId);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public void GetLockingProcesses_WhenGivenEmptyStringCollection_ReturnsEmptyResult()
        {
            var arg = new List<string>();
            var result = RestartManager.GetLockingProcesses(arg);

            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void GetLockingProcesses_WhenGivenEmptyFileInfoCollection_ReturnsEmptyResult()
        {
            var arg = new List<FileInfo>();
            var result = RestartManager.GetLockingProcesses(arg);

            Assert.AreEqual(0, result.Count());
        }
    }
}
