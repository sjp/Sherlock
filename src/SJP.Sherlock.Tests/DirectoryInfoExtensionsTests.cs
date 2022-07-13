using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests;

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

    [Test, TestPlatform.Windows]
    public static void GetLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
    {
        using var tmpDir = new TemporaryDirectory();
        var tmpDirFile = Path.Combine(tmpDir.DirectoryPath, Path.GetRandomFileName());

        using var _ = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var lockedFiles = tmpDir.DirectoryInfo.GetLockedFiles();
        Assert.That(lockedFiles, Is.Not.Empty);
    }

    [Test]
    public static void GetLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
    {
        using var tmpDir = new TemporaryDirectory();

        var lockedFiles = tmpDir.DirectoryInfo.GetLockedFiles();
        Assert.That(lockedFiles, Is.Empty);
    }

    [Test, TestPlatform.Windows]
    public static void EnumerateLockedFiles_WhenLockingOnPathInDirectory_ReturnsListOfLockedFiles()
    {
        using var tmpDir = new TemporaryDirectory();
        var tmpDirFile = Path.Combine(tmpDir.DirectoryPath, Path.GetRandomFileName());

        using var _ = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var lockedFiles = tmpDir.DirectoryInfo.EnumerateLockedFiles();

        Assert.Multiple(() =>
        {
            Assert.That(lockedFiles, Is.Not.Empty);
            Assert.That(lockedFiles.Select(f => f.FullName), Contains.Item(tmpDirFile));
        });
    }

    [Test]
    public static void EnumerateLockedFiles_WhenNotLockingOnPathInDirectory_ReturnsEmptyList()
    {
        using var tmpDir = new TemporaryDirectory();

        var lockedFiles = tmpDir.DirectoryInfo.EnumerateLockedFiles();
        Assert.That(lockedFiles, Is.Empty);
    }

    [Test, TestPlatform.Windows]
    public static void GetLockingProcesses_WhenLockingOnPathInDirectory_ReturnsCorrectProcess()
    {
        using var tmpDir = new TemporaryDirectory();
        var tmpDirFile = Path.Combine(tmpDir.DirectoryPath, Path.GetRandomFileName());

        using var _ = File.Open(tmpDirFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var lockedProcesses = tmpDir.DirectoryInfo.GetLockingProcesses();
        var process = Process.GetCurrentProcess();

        var lockingId = lockedProcesses.Single().ProcessId;
        var currentId = process.Id;

        Assert.That(currentId, Is.EqualTo(lockingId));
    }

    [Test]
    public static void GetLockingProcesses_WhenNotLockingOnPathInDirectory_ReturnsEmptySet()
    {
        using var tmpDir = new TemporaryDirectory();

        var lockedProceses = tmpDir.DirectoryInfo.GetLockingProcesses();
        Assert.That(lockedProceses, Is.Empty);
    }
}