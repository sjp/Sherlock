using System;
using System.IO;

namespace SJP.Sherlock.Tests
{
    /// <summary>
    /// A temporary file that will be deleted once disposed.
    /// </summary>
    /// <seealso cref="IDisposable" />
    internal sealed class TemporaryFile : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryFile"/> class.
        /// </summary>
        public TemporaryFile()
        {
            FilePath = GetTempFilePath();
        }

        /// <summary>
        /// The full path of the temporary file, always a random location.
        /// </summary>
        /// <value>The directory path.</value>
        public string FilePath { get; }

        /// <summary>
        /// Gets the file information for the temporary file.
        /// </summary>
        /// <value>The file information.</value>
        public FileInfo FileInfo => new FileInfo(FilePath);

        private static string GetTempFilePath()
        {
            return Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName()
            );
        }

        /// <summary>
        /// Deletes the temporary file.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            if (File.Exists(FilePath))
                File.Delete(FilePath);

            _disposed = true;
        }

        private bool _disposed;
    }
}
