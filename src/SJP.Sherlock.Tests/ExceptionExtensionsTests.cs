using System;
using System.IO;
using NUnit.Framework;

namespace SJP.Sherlock.Tests;

[TestFixture]
internal static class ExceptionExtensionsTests
{
    [Test]
    public static void IsFileLocked_GivenNullIOException_ThrowsArgNullException()
    {
        IOException ex = null;
        Assert.That(() => ex.IsFileLocked(), Throws.ArgumentNullException);
    }

    [Test]
    public static void IsFileLocked_GivenRegularIOException_ReturnsFalse()
    {
        var ex = new IOException("test");
        Assert.That(ex.IsFileLocked(), Is.False);
    }

    [Test, TestPlatform.Windows]
    public static void IsFileLocked_GivenLockingIOException_ReturnsTrue()
    {
        using var tmpPath = new TemporaryFile();
        try
        {
            using (var file = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var innerFile = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                _ = "won't get here";

            Assert.Fail("The file locking did not throw an exception when it should have.");
        }
        catch (IOException ex)
        {
            Assert.That(ex.IsFileLocked(), Is.True);
        }
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenNullExceptionAndValidDirectory_ThrowsArgNullException()
    {
        Exception ex = null;
        using var tmpDir = new TemporaryDirectory();
        Assert.That(() => ex.RethrowWithLockingInformation(tmpDir.DirectoryInfo), Throws.ArgumentNullException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenValidExceptionAndNullDirectory_ThrowsArgNullException()
    {
        var ex = new Exception();
        DirectoryInfo tmpPath = null;
        Assert.That(() => ex.RethrowWithLockingInformation(tmpPath), Throws.ArgumentNullException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenNullExceptionAndValidFileInfo_ThrowsArgNullException()
    {
        Exception ex = null;
        using var tmpPath = new TemporaryFile();

        Assert.That(() => ex.RethrowWithLockingInformation(tmpPath.FileInfo), Throws.ArgumentNullException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenValidExceptionAndNullFileInfo_ThrowsArgNullException()
    {
        var ex = new Exception();
        FileInfo tmpFile = null;
        Assert.That(() => ex.RethrowWithLockingInformation(tmpFile), Throws.ArgumentException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenNullExceptionAndValidString_ThrowsArgNullException()
    {
        Exception ex = null;
        using var tmpPath = new TemporaryFile();

        Assert.That(() => ex.RethrowWithLockingInformation(tmpPath.FilePath), Throws.ArgumentNullException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenValidExceptionAndNullString_ThrowsArgNullException()
    {
        var ex = new Exception();
        const string tmpFile = null;
        Assert.That(() => ex.RethrowWithLockingInformation(tmpFile), Throws.ArgumentException);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenNonIOException_ReturnsFalse()
    {
        var ex = new Exception("test");
        using var tmpDir = new TemporaryDirectory();

        var result = ex.RethrowWithLockingInformation(tmpDir.DirectoryPath);

        Assert.That(result, Is.False);
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenIOExceptionWithNoLockedFiles_ReturnsFalse()
    {
        using var tmpPath = new TemporaryFile();
        try
        {
            using (var file = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var innerFile = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                _ = "won't get here";

            Assert.Fail("The file locking did not throw an exception when it should have.");
        }
        catch (IOException ex)
        {
            var result = ex.RethrowWithLockingInformation(tmpPath.FileInfo);

            Assert.That(result, Is.False);
        }
    }

    [Test]
    public static void RethrowWithLockingInformation_GivenLockingIOException_ReturnsTrue()
    {
        using var tmpPath = new TemporaryFile();
        try
        {
            using (var file = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            using (var innerFile = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                _ = "won't get here";

            Assert.Fail("The file locking did not throw an exception when it should have.");
        }
        catch (IOException ex)
        {
            try
            {
                using var _ = tmpPath.FileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                ex.RethrowWithLockingInformation(tmpPath.FileInfo);
            }
            catch (IOException innerEx)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(ex.Message, Is.Not.EqualTo(innerEx.Message));
                    Assert.That(ex.HResult, Is.EqualTo(innerEx.HResult));
                    Assert.That(innerEx.InnerException, Is.Not.Null);
                    Assert.That(ex, Is.SameAs(innerEx.InnerException));
                });
            }
        }
    }
}
