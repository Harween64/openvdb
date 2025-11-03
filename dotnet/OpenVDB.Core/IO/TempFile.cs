// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// TempFile.cs - C# port of TempFile.h and TempFile.cc
//
// This file provides the TempFile class for creating temporary files.

using System;
using System.IO;

namespace OpenVDB.IO
{
    /// <summary>
    /// Represents an output stream to a unique temporary file.
    /// </summary>
    /// <remarks>
    /// The file is created in the directory specified by the environment variable
    /// OPENVDB_TEMP_DIR, or if that is not set, in the system default temporary directory.
    /// The file is automatically deleted when the object is disposed unless specified otherwise.
    /// </remarks>
    public class TempFile : IDisposable
    {
        private readonly string _filename;
        private FileStream? _fileStream;
        private bool _isOpen;
        private bool _autoDelete;
        private bool _disposed;

        /// <summary>
        /// Creates and opens a unique temporary file.
        /// </summary>
        /// <param name="autoDelete">If true, the file will be automatically deleted when disposed.</param>
        public TempFile(bool autoDelete = true)
        {
            _autoDelete = autoDelete;
            _disposed = false;

            // Determine the temporary directory
            var tempDir = Environment.GetEnvironmentVariable("OPENVDB_TEMP_DIR");
            if (string.IsNullOrEmpty(tempDir) || !Directory.Exists(tempDir))
            {
                tempDir = Path.GetTempPath();
            }

            // Create a unique temporary file name
            _filename = Path.Combine(tempDir, $"openvdb_{Guid.NewGuid():N}.tmp");

            // Create and open the file
            try
            {
                _fileStream = new FileStream(_filename, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                _isOpen = true;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to create temporary file: {_filename}", ex);
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~TempFile()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the path to the temporary file.
        /// </summary>
        public string Filename => _filename;

        /// <summary>
        /// Gets whether the file is open for writing.
        /// </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        /// Gets the underlying file stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the file is not open.</exception>
        public FileStream Stream
        {
            get
            {
                if (!_isOpen || _fileStream == null)
                {
                    throw new InvalidOperationException("Temporary file is not open");
                }
                return _fileStream;
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }
            _isOpen = false;
        }

        /// <summary>
        /// Disposes of resources used by the TempFile.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Close the file
                Close();
            }

            // Delete the file if auto-delete is enabled
            if (_autoDelete && System.IO.File.Exists(_filename))
            {
                try
                {
                    System.IO.File.Delete(_filename);
                }
                catch
                {
                    // Ignore errors when deleting
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Writes a byte array to the temporary file.
        /// </summary>
        /// <param name="buffer">The byte array to write.</param>
        public void Write(byte[] buffer)
        {
            Stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a portion of a byte array to the temporary file.
        /// </summary>
        /// <param name="buffer">The byte array to write.</param>
        /// <param name="offset">The offset in the buffer.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Flushes the stream buffer to disk.
        /// </summary>
        public void Flush()
        {
            Stream.Flush();
        }
    }
}
