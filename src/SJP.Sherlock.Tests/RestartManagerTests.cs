using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    internal static class RestartManagerTests
    {
        [Test]
        public static void GetLockingProcesses_WhenGivenNullStringArgs_ThrowsArgNullException()
        {
            Assert.That(() => RestartManager.GetLockingProcesses((IEnumerable<string>)null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenNullFileInfoArgs_ThrowsArgNullException()
        {
            Assert.That(() => RestartManager.GetLockingProcesses((IEnumerable<FileInfo>)null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenNullDirectoryInfoArgs_ThrowsArgNullException()
        {
            Assert.That(() => RestartManager.GetLockingProcesses((DirectoryInfo)null), Throws.ArgumentNullException);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPathAndGivenString_ReturnsNonEmptyLockingSet()
        {
            using var tmpPath = new TemporaryFile();
            using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var lockingProcs = RestartManager.GetLockingProcesses(tmpPath.FilePath);
            Assert.That(lockingProcs, Is.Not.Empty);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPathAndGivenString_ReturnsCorrectProcess()
        {
            using var tmpPath = new TemporaryFile();
            using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var lockingProcs = RestartManager.GetLockingProcesses(tmpPath.FilePath);
            var process = Process.GetCurrentProcess();

            var lockingId = lockingProcs.Single().ProcessId;
            var currentId = process.Id;

            Assert.That(currentId, Is.EqualTo(lockingId));
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPathAndGivenDirectory_ReturnsNonEmptyLockingSet()
        {
            using var tmpDir = new TemporaryDirectory();
            var tmpFile = new FileInfo(Path.Combine(tmpDir.DirectoryPath, Path.GetRandomFileName()));

            using var _ = tmpFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var lockingProcs = RestartManager.GetLockingProcesses(tmpDir.DirectoryInfo);
            Assert.That(lockingProcs, Is.Not.Empty);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPathAndGivenDirectory_ReturnsCorrectProcess()
        {
            using var tmpDir = new TemporaryDirectory();
            var tmpFile = new FileInfo(Path.Combine(tmpDir.DirectoryPath, Path.GetRandomFileName()));

            using var _ = tmpFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var lockingProcs = RestartManager.GetLockingProcesses(tmpDir.DirectoryInfo);
            var process = Process.GetCurrentProcess();

            var lockingId = lockingProcs.Single().ProcessId;
            var currentId = process.Id;

            Assert.That(currentId, Is.EqualTo(lockingId));
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenEmptyStringCollection_ReturnsEmptyResult()
        {
            var arg = Array.Empty<string>();
            var result = RestartManager.GetLockingProcesses(arg);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenEmptyFileInfoCollection_ReturnsEmptyResult()
        {
            var arg = Array.Empty<FileInfo>();
            var result = RestartManager.GetLockingProcesses(arg);

            Assert.That(result, Is.Empty);
        }
    }
}
