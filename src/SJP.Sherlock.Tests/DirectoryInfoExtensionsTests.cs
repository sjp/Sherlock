using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    internal static class DirectoryInfoExtensionsTests
    {
        [Test]
        public static void GetLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.That(() => tmp.GetLockedFiles(), Throws.ArgumentNullException);
        }

        [Test]
        public static void EnumerateLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.That(() => tmp.EnumerateLockedFiles(), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetLockingProcesses_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.That(() => tmp.GetLockingProcesses(), Throws.ArgumentNullException);
        }

        [Test]
        public static void ContainsLockedFiles_WhenGivenNullDirectoryInfo_ThrowsArgNullException()
        {
            DirectoryInfo tmp = null;
            Assert.That(() => tmp.ContainsLockedFiles(), Throws.ArgumentNullException);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
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
                var lockedFiles = tmpDir.GetLockedFiles();
                Assert.That(lockedFiles, Is.Not.Empty);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public static void GetLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedFiles = tmpDir.GetLockedFiles();
            Assert.That(lockedFiles, Is.Empty);

            tmpDir.Delete(true);
        }

        [Test, TestPlatform.Windows]
        public static void EnumerateLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
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
                var lockedFiles = tmpDir.EnumerateLockedFiles();
                Assert.That(lockedFiles, Is.Not.Empty);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public static void EnumerateLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedFiles = tmpDir.EnumerateLockedFiles();
            Assert.That(lockedFiles, Is.Empty);

            tmpDir.Delete(true);
        }

        [Test, TestPlatform.Windows]
        public static void GetLockingProcesses_WhenLockingOnPathInDirectory_ReturnsCorrectProcess()
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
                var lockedProcesses = tmpDir.GetLockingProcesses();
                var process = Process.GetCurrentProcess();

                var lockingId = lockedProcesses.Single().ProcessId;
                var currentId = process.Id;

                Assert.That(currentId, Is.EqualTo(lockingId));
            }

            tmpDir.Delete(true);
        }

        [Test]
        public static void GetLockingProcesses_WhenNotLockingOnPathInDirectory_ReturnsEmptySet()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var lockedProceses = tmpDir.GetLockingProcesses();
            Assert.That(lockedProceses, Is.Empty);

            tmpDir.Delete(true);
        }

        [Test, TestPlatform.Windows]
        public static void ContainsLockedFiles_WhenLockingOnPathInDirectory_ReturnsTrue()
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
                var containsLockedFiles = tmpDir.ContainsLockedFiles();
                Assert.That(containsLockedFiles, Is.True);
            }

            tmpDir.Delete(true);
        }

        [Test]
        public static void ContainsLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsFalse()
        {
            var tmpFilePath = Path.GetTempFileName();
            var tmpDirPath = Path.GetDirectoryName(tmpFilePath);
            tmpDirPath = Path.Combine(tmpDirPath, Guid.NewGuid().ToString());

            var tmpDir = new DirectoryInfo(tmpDirPath);
            tmpDir.Create();
            var tmpDirFile = Path.Combine(tmpDirPath, Path.GetFileName(tmpFilePath));
            File.Move(tmpFilePath, tmpDirFile);

            var containsLockedFiles = tmpDir.ContainsLockedFiles();
            Assert.That(containsLockedFiles, Is.False);

            tmpDir.Delete(true);
        }
    }
}
