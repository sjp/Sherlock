using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    internal static class PlatformTests
    {
        [Test]
        public static void SupportsRestartManager_WhenRunningWindowsAndAtLeastVista_ReturnsTrue()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // this will always be true on Windows for unit tests because .NET Core is not supported on XP/Vista/Server 2008
            Assert.AreEqual(isWindows, Platform.SupportsRestartManager);
        }
    }
}
