using System;
using System.IO;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    internal static class ExceptionExtensionsTests
    {
        [Test]
        public static void IsFileLocked_GivenNullIOException_ThrowsArgNullException()
        {
            IOException ex = null;
            Assert.Throws<ArgumentNullException>(() => ex.IsFileLocked());
        }

        [Test]
        public static void IsFileLocked_GivenRegularIOException_ReturnsFalse()
        {
            var ex = new IOException("test");
            Assert.IsFalse(ex.IsFileLocked());
        }

        [Test, TestPlatform.Windows]
        public static void IsFileLocked_GivenLockingIOException_ReturnsTrue()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            try
            {
                using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var innerFile = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    _ = "won't get here";

                Assert.Fail("The file locking did not throw an exception when it should have.");
            }
            catch (IOException ex)
            {
                Assert.IsTrue(ex.IsFileLocked());
            }

            tmpPath.Delete();
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenNullExceptionAndValidDirectory_ThrowsArgNullException()
        {
            Exception ex = null;
            var tmpPath = new DirectoryInfo(Path.GetTempPath());
            Assert.Throws<ArgumentNullException>(() => ex.RethrowWithLockingInformation(tmpPath));
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenValidExceptionAndNullDirectory_ThrowsArgNullException()
        {
            var ex = new Exception();
            DirectoryInfo tmpPath = null;
            Assert.Throws<ArgumentNullException>(() => ex.RethrowWithLockingInformation(tmpPath));
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenNullExceptionAndValidFileInfo_ThrowsArgNullException()
        {
            Exception ex = null;
            var tmpFile = new FileInfo(Path.GetTempFileName());

            Assert.Throws<ArgumentNullException>(() => ex.RethrowWithLockingInformation(tmpFile));

            tmpFile.Delete();
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenValidExceptionAndNullFileInfo_ThrowsArgNullException()
        {
            var ex = new Exception();
            FileInfo tmpFile = null;
            Assert.Throws<ArgumentException>(() => ex.RethrowWithLockingInformation(tmpFile));
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenNullExceptionAndValidString_ThrowsArgNullException()
        {
            Exception ex = null;
            var tmpFile = Path.GetTempFileName();

            Assert.Throws<ArgumentNullException>(() => ex.RethrowWithLockingInformation(tmpFile));

            File.Delete(tmpFile);
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenValidExceptionAndNullString_ThrowsArgNullException()
        {
            var ex = new Exception();
            const string tmpFile = null;
            Assert.Throws<ArgumentException>(() => ex.RethrowWithLockingInformation(tmpFile));
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenNonIOException_ReturnsFalse()
        {
            var ex = new Exception("test");
            var tmpPath = new DirectoryInfo(Path.GetTempPath());

            var result = ex.RethrowWithLockingInformation(tmpPath);

            Assert.IsFalse(result);
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenIOExceptionWithNoLockedFiles_ReturnsFalse()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            try
            {
                using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var innerFile = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    _ = "won't get here";

                Assert.Fail("The file locking did not throw an exception when it should have.");
            }
            catch (IOException ex)
            {
                var result = ex.RethrowWithLockingInformation(tmpPath);

                Assert.IsFalse(result);
            }

            tmpPath.Delete();
        }

        [Test]
        public static void RethrowWithLockingInformation_GivenLockingIOException_ReturnsTrue()
        {
            var tmpPath = new FileInfo(Path.GetTempFileName());
            try
            {
                using (var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                using (var innerFile = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    _ = "won't get here";

                Assert.Fail("The file locking did not throw an exception when it should have.");
            }
            catch (IOException ex)
            {
                try
                {
                    using var file = tmpPath.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    ex.RethrowWithLockingInformation(tmpPath);
                }
                catch (IOException innerEx)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreNotEqual(ex.Message, innerEx.Message);
                        Assert.AreEqual(ex.HResult, innerEx.HResult);
                        Assert.IsNotNull(innerEx.InnerException);
                        Assert.AreSame(ex, innerEx.InnerException);
                    });
                }
            }

            tmpPath.Delete();
        }
    }
}
