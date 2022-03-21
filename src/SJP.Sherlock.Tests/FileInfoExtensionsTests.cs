using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SJP.Sherlock.Tests;

[TestFixture]
internal static class FileInfoExtensionsTests
{
    [Test]
    public static void GetLockingProcesses_WhenGivenNullFileInfo_ThrowsArgNullException()
    {
        FileInfo tmp = null;
        Assert.That(() => tmp.GetLockingProcesses(), Throws.ArgumentNullException);
    }

    [Test]
    public static void IsFileLocked_WhenGivenNullFileInfo_ThrowsArgNullException()
    {
        FileInfo tmp = null;
        Assert.That(() => tmp.IsFileLocked(), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetLockingProcesses_WhenGivenFileWithNoLocks_ReturnsEmptySet()
    {
        using var tmpPath = new TemporaryFile();
        var lockingProcs = tmpPath.FileInfo.GetLockingProcesses();

        Assert.That(lockingProcs, Is.Empty);
    }

    [Test, TestPlatform.Windows]
    public static void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectProcess()
    {
        using var tmpPath = new TemporaryFile();
        using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var lockingProcs = RestartManager.GetLockingProcesses(tmpPath.FileInfo);
        var process = Process.GetCurrentProcess();

        var lockingId = lockingProcs.Single().ProcessId;
        var currentId = process.Id;

        Assert.That(currentId, Is.EqualTo(lockingId));
    }

    [Test, TestPlatform.Windows]
    public static void GetLockingProcesses_WhenLockingOnPath_ReturnsCorrectNumberOfLocks()
    {
        using var tmpPath = new TemporaryFile();
        using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var lockingProcs = RestartManager.GetLockingProcesses(tmpPath.FileInfo);
        Assert.That(lockingProcs, Has.One.Items);
    }

    [Test]
    public static void IsFileLocked_WhenLockingNotOnPath_ReturnsFalse()
    {
        using var tmpPath = new TemporaryFile();

        var isLocked = tmpPath.FileInfo.IsFileLocked();
        Assert.That(isLocked, Is.False);
    }

    [Test, TestPlatform.Windows]
    public static void GetLockingProcesses_WhenLockingOnPath_ReturnsTrue()
    {
        using var tmpPath = new TemporaryFile();
        using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        var isLocked = tmpPath.FileInfo.IsFileLocked();
        Assert.That(isLocked, Is.True);
    }
}
