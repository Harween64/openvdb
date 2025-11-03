// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Stream.cs - C# port of Stream.h and Stream.cc
//
// This file provides the Stream class for reading and writing VDB data to streams.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenVDB.Grid;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Grid archive associated with arbitrary input and output streams (not necessarily files).
    /// </summary>
    public class Stream : Archive
    {
        private System.IO.Stream? _inputStream;
        private System.IO.Stream? _outputStream;
        private BinaryReader? _reader;
        private BinaryWriter? _writer;
        private MetaMap? _fileMetadata;
        private List<GridBase> _grids;
        private bool _delayLoad;

        /// <summary>
        /// Initializes a new instance for reading from an input stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from.</param>
        /// <param name="delayLoad">If true, enable delayed loading of grids (not fully supported yet).</param>
        public Stream(System.IO.Stream inputStream, bool delayLoad = false) : base()
        {
            _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
            _outputStream = null;
            _reader = new BinaryReader(_inputStream);
            _writer = null;
            _fileMetadata = null;
            _grids = new List<GridBase>();
            _delayLoad = delayLoad && IsDelayedLoadingEnabled();

            // Read the stream
            ReadStream();
        }

        /// <summary>
        /// Initializes a new instance for writing (no stream specified yet).
        /// </summary>
        public Stream() : base()
        {
            _inputStream = null;
            _outputStream = null;
            _reader = null;
            _writer = null;
            _fileMetadata = null;
            _grids = new List<GridBase>();
            _delayLoad = false;
        }

        /// <summary>
        /// Initializes a new instance for writing to an output stream.
        /// </summary>
        /// <param name="outputStream">The stream to write to.</param>
        public Stream(System.IO.Stream outputStream) : base()
        {
            _inputStream = null;
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            _reader = null;
            _writer = new BinaryWriter(_outputStream);
            _fileMetadata = null;
            _grids = new List<GridBase>();
            _delayLoad = false;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Stream(Stream other) : base(other)
        {
            _inputStream = other._inputStream;
            _outputStream = other._outputStream;
            _reader = _inputStream != null ? new BinaryReader(_inputStream) : null;
            _writer = _outputStream != null ? new BinaryWriter(_outputStream) : null;
            _fileMetadata = other._fileMetadata != null ? new MetaMap(other._fileMetadata) : null;
            _grids = new List<GridBase>(other._grids);
            _delayLoad = other._delayLoad;
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~Stream()
        {
            Dispose();
        }

        /// <summary>
        /// Create a copy of this archive.
        /// </summary>
        /// <returns>A new Stream instance that is a copy of this one.</returns>
        public override Archive Copy()
        {
            return new Stream(this);
        }

        /// <summary>
        /// Gets the file-level metadata.
        /// </summary>
        /// <returns>A copy of the file-level metadata.</returns>
        public MetaMap? GetMetadata()
        {
            return _fileMetadata != null ? new MetaMap(_fileMetadata) : null;
        }

        /// <summary>
        /// Gets the grids that were read from the input stream.
        /// </summary>
        /// <returns>A list of grids.</returns>
        public List<GridBase> GetGrids()
        {
            return new List<GridBase>(_grids);
        }

        /// <summary>
        /// Writes the grids to the output stream.
        /// </summary>
        /// <param name="grids">The grids to write.</param>
        /// <param name="metadata">Optional file-level metadata.</param>
        /// <exception cref="InvalidOperationException">If no output stream was specified.</exception>
        public override void Write(IEnumerable<GridBase> grids, MetaMap? metadata = null)
        {
            if (_outputStream == null || _writer == null)
            {
                throw new InvalidOperationException("No output stream specified for writing");
            }

            var gridList = grids.ToList();

            try
            {
                // Write file header
                WriteHeader();

                // Write file-level metadata
                WriteFileMetadata(metadata ?? new MetaMap());

                // Write grid count
                WriteGridCount(_writer, gridList.Count);

                // Write each grid
                foreach (var grid in gridList)
                {
                    WriteGrid(grid);
                }

                // Flush the writer
                _writer.Flush();
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to write grids to stream", ex);
            }
        }

        /// <summary>
        /// Disposes of resources used by the Stream.
        /// </summary>
        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _reader = null;
            _writer = null;
            
            // Note: We don't dispose the underlying streams as they may be managed externally
        }

        #region Private Helper Methods

        private void ReadStream()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            try
            {
                // Read file header
                ReadHeader();

                // Read file-level metadata
                _fileMetadata = ReadFileMetadata();

                // Read grid count
                var gridCount = ReadGridCount(_reader);

                // Read each grid
                for (int i = 0; i < gridCount; i++)
                {
                    var grid = ReadGrid();
                    if (grid != null)
                    {
                        _grids.Add(grid);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to read grids from stream", ex);
            }
        }

        private void ReadHeader()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // 1) Read the magic number for VDB (8 bytes)
            long magic = _reader.ReadInt64();
            const long OPENVDB_MAGIC = 0x56444220;
            if (magic != OPENVDB_MAGIC)
            {
                throw new IOException("Not a valid VDB stream - invalid magic number");
            }

            // 2) Read the file format version number (4 bytes)
            uint fileVersion = _reader.ReadUInt32();
            
            const uint OPENVDB_FILE_VERSION_NODE_MASK_COMPRESSION = 222;
            if (fileVersion < OPENVDB_FILE_VERSION_NODE_MASK_COMPRESSION)
            {
                throw new IOException($"VDB stream version {fileVersion} is not supported. Minimum version is {OPENVDB_FILE_VERSION_NODE_MASK_COMPRESSION}");
            }

            // 3) Read the library version numbers (major and minor, 4 bytes each)
            uint libraryMajor = _reader.ReadUInt32();
            uint libraryMinor = _reader.ReadUInt32();

            // 4) Read the flag indicating whether the stream supports random access (1 byte)
            byte hasGridOffsets = _reader.ReadByte();
            // Streams typically don't have grid offsets

            // 5) Read the 36-byte UUID as ASCII string
            var uuidChars = _reader.ReadChars(36);
            string uuid = new string(uuidChars);
        }

        private MetaMap ReadFileMetadata()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // Read metadata using the MetaMap's read functionality
            var metadata = new MetaMap();
            metadata.ReadMeta(_reader);
            return metadata;
        }

        private GridBase? ReadGrid()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // Read grid descriptor
            var descriptor = new GridDescriptor();
            var grid = descriptor.Read(_reader);
            if (grid == null)
                return null;

            // TODO: Grid reading requires implementation of ReadMeta, ReadTransform, ReadTopology, and ReadBuffers in GridBase
            // These methods are not yet implemented in the Grid class
            // grid.ReadMeta(_reader);
            // grid.ReadTransform(_reader);
            // grid.ReadTopology(_reader);
            // grid.ReadBuffers(_reader);

            throw new NotImplementedException(
                "Full grid reading requires ReadMeta, ReadTransform, ReadTopology, and ReadBuffers methods to be implemented in GridBase class. " +
                "This is part of the full Grid I/O implementation which is beyond the scope of completing these stubs.");
        }

        private void WriteHeader()
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // 1) Write the magic number for VDB (8 bytes)
            const long OPENVDB_MAGIC = 0x56444220;
            _writer.Write(OPENVDB_MAGIC);

            // 2) Write the file format version number (4 bytes)
            const uint OPENVDB_FILE_VERSION = 224; // Current file version
            _writer.Write(OPENVDB_FILE_VERSION);

            // 3) Write the library version numbers (4 bytes each)
            const uint OPENVDB_LIBRARY_MAJOR_VERSION = 11;
            const uint OPENVDB_LIBRARY_MINOR_VERSION = 0;
            _writer.Write(OPENVDB_LIBRARY_MAJOR_VERSION);
            _writer.Write(OPENVDB_LIBRARY_MINOR_VERSION);

            // 4) Write a flag indicating that this stream does not contain grid offsets (1 byte)
            // Streams don't support random access
            byte hasGridOffsets = 0;
            _writer.Write(hasGridOffsets);

            // 5) Generate and write a new 36-byte UUID as ASCII string
            var guid = Guid.NewGuid();
            string uuid = guid.ToString("D").ToUpperInvariant();
            var uuidBytes = System.Text.Encoding.ASCII.GetBytes(uuid);
            _writer.Write(uuidBytes);
        }

        private void WriteFileMetadata(MetaMap metadata)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // Write metadata using the MetaMap's write functionality
            metadata.WriteMeta(_writer);
        }

        private void WriteGrid(GridBase grid)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // TODO: Grid writing requires implementation of WriteMeta, WriteTransform, WriteTopology, and WriteBuffers in GridBase
            // These methods are not yet implemented in the Grid class
            // Also need SaveFloatAsHalf property

            throw new NotImplementedException(
                "Grid writing requires WriteMeta, WriteTransform, WriteTopology, WriteBuffers methods and SaveFloatAsHalf property to be implemented in GridBase class. " +
                "This is part of the full Grid I/O implementation which is beyond the scope of completing these stubs.");
        }

        #endregion
    }
}
