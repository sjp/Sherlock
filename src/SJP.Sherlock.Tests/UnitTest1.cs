using System;
using System.IO;
using NUnit.Framework;

namespace SJP.Sherlock.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void CreatesFileAndLocksIt()
        {
            var tmpPath = Path.GetTempFileName();
            using (var file = File.Open(tmpPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                var lockingProcs = RestartManager.GetLockingProcesses(tmpPath);
                Assert.IsTrue(lockingProcs.Count > 0);
            }

            File.Delete(tmpPath);
        }
    }
}
