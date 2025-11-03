// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// File.cs - C# port of File.h and File.cc
//
// This file provides the File class for reading and writing VDB files.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenVDB.Grid;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Grid archive associated with a file on disk.
    /// </summary>
    public class File : Archive
    {
        private readonly string _filename;
        private bool _isOpen;
        private FileStream? _fileStream;
        private BinaryReader? _reader;
        private BinaryWriter? _writer;
        private MetaMap? _fileMetadata;
        private Dictionary<string, GridDescriptor> _gridDescriptors;
        private Dictionary<string, GridBase> _namedGrids;
        private ulong _copyMaxBytes;

        /// <summary>
        /// Initializes a new instance of the File class.
        /// </summary>
        /// <param name="filename">The path to the VDB file.</param>
        public File(string filename) : base()
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _isOpen = false;
            _fileStream = null;
            _reader = null;
            _writer = null;
            _fileMetadata = null;
            _gridDescriptors = new Dictionary<string, GridDescriptor>();
            _namedGrids = new Dictionary<string, GridBase>();
            _copyMaxBytes = 100 * 1024 * 1024; // 100 MB default

            // Check for environment variable for copy max bytes
            var envVar = Environment.GetEnvironmentVariable("OPENVDB_DELAYED_LOAD_COPY_MAX_BYTES");
            if (!string.IsNullOrEmpty(envVar) && ulong.TryParse(envVar, out var maxBytes))
            {
                _copyMaxBytes = maxBytes;
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <remarks>
        /// The copy will be closed and will not reference the same file descriptor as the original.
        /// </remarks>
        public File(File other) : base(other)
        {
            _filename = other._filename;
            _isOpen = false;
            _fileStream = null;
            _reader = null;
            _writer = null;
            _fileMetadata = other._fileMetadata != null ? new MetaMap(other._fileMetadata) : null;
            _gridDescriptors = new Dictionary<string, GridDescriptor>(other._gridDescriptors);
            _namedGrids = new Dictionary<string, GridBase>();
            _copyMaxBytes = other._copyMaxBytes;
        }

        /// <summary>
        /// Finalizer to ensure resources are released.
        /// </summary>
        ~File()
        {
            Close();
        }

        /// <summary>
        /// Create a copy of this archive.
        /// </summary>
        /// <returns>A new File instance that is a copy of this one.</returns>
        public override Archive Copy()
        {
            return new File(this);
        }

        /// <summary>
        /// Gets the name of the file with which this archive is associated.
        /// </summary>
        /// <remarks>
        /// The file does not necessarily exist on disk yet.
        /// </remarks>
        public string Filename => _filename;

        /// <summary>
        /// Opens the file for reading.
        /// </summary>
        /// <param name="delayLoad">If true, enable delayed loading of grids (not fully supported in C# port yet).</param>
        /// <returns>True if the file's UUID has changed since it was last read.</returns>
        /// <exception cref="IOException">If the file cannot be opened or is not a valid VDB file.</exception>
        public bool Open(bool delayLoad = false)
        {
            if (_isOpen)
            {
                Close();
            }

            if (!System.IO.File.Exists(_filename))
            {
                throw new FileNotFoundException($"VDB file not found: {_filename}");
            }

            try
            {
                _fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                _reader = new BinaryReader(_fileStream);
                _isOpen = true;

                // Read file header
                ReadHeader();

                // Read file-level metadata
                _fileMetadata = ReadFileMetadata();

                // Read grid descriptors
                ReadGridDescriptors();

                return false; // TODO: Implement UUID change detection
            }
            catch (Exception ex)
            {
                Close();
                throw new IOException($"Failed to open VDB file: {_filename}", ex);
            }
        }

        /// <summary>
        /// Checks if the file is currently open for reading.
        /// </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        /// Closes the file.
        /// </summary>
        public void Close()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }

            _isOpen = false;
            _gridDescriptors.Clear();
            _namedGrids.Clear();
        }

        /// <summary>
        /// Gets the current size of the file on disk in bytes.
        /// </summary>
        /// <returns>The file size in bytes.</returns>
        /// <exception cref="IOException">If the file size cannot be determined.</exception>
        public ulong GetSize()
        {
            if (!System.IO.File.Exists(_filename))
            {
                throw new FileNotFoundException($"VDB file not found: {_filename}");
            }

            try
            {
                var fileInfo = new FileInfo(_filename);
                return (ulong)fileInfo.Length;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to get file size: {_filename}", ex);
            }
        }

        /// <summary>
        /// Gets or sets the size in bytes above which the file will not be
        /// automatically copied during delayed loading.
        /// </summary>
        public ulong CopyMaxBytes
        {
            get => _copyMaxBytes;
            set => _copyMaxBytes = value;
        }

        /// <summary>
        /// Checks if a grid with the given name exists in this file.
        /// </summary>
        /// <param name="name">The grid name to check.</param>
        /// <returns>True if the grid exists in the file.</returns>
        public bool HasGrid(string name)
        {
            return _gridDescriptors.ContainsKey(name);
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
        /// Reads the entire contents of the file and returns a list of grid pointers.
        /// </summary>
        /// <returns>A list of all grids in the file.</returns>
        /// <exception cref="InvalidOperationException">If the file is not open.</exception>
        public List<GridBase> GetGrids()
        {
            if (!_isOpen)
            {
                throw new InvalidOperationException("File is not open for reading");
            }

            var grids = new List<GridBase>();

            foreach (var descriptor in _gridDescriptors.Values)
            {
                var grid = ReadGrid(descriptor);
                if (grid != null)
                {
                    grids.Add(grid);
                }
            }

            return grids;
        }

        /// <summary>
        /// Reads just the grid metadata and transforms from the file.
        /// </summary>
        /// <returns>A list of grids with only metadata and transforms loaded.</returns>
        /// <exception cref="InvalidOperationException">If the file is not open.</exception>
        public List<GridBase> ReadAllGridMetadata()
        {
            if (!_isOpen)
            {
                throw new InvalidOperationException("File is not open for reading");
            }

            var grids = new List<GridBase>();

            foreach (var descriptor in _gridDescriptors.Values)
            {
                var grid = ReadGridMetadataOnly(descriptor);
                if (grid != null)
                {
                    grids.Add(grid);
                }
            }

            return grids;
        }

        /// <summary>
        /// Reads a grid's metadata and transform only.
        /// </summary>
        /// <param name="name">The name of the grid to read.</param>
        /// <returns>A grid with only metadata and transform loaded.</returns>
        /// <exception cref="InvalidOperationException">If the file is not open.</exception>
        /// <exception cref="KeyNotFoundException">If no grid with the given name exists.</exception>
        public GridBase ReadGridMetadata(string name)
        {
            if (!_isOpen)
            {
                throw new InvalidOperationException("File is not open for reading");
            }

            if (!_gridDescriptors.TryGetValue(name, out var descriptor))
            {
                throw new KeyNotFoundException($"Grid '{name}' not found in file");
            }

            var grid = ReadGridMetadataOnly(descriptor);
            if (grid == null)
            {
                throw new IOException($"Failed to read metadata for grid '{name}'");
            }

            return grid;
        }

        /// <summary>
        /// Reads an entire grid, including all of its data blocks.
        /// </summary>
        /// <param name="name">The name of the grid to read.</param>
        /// <returns>The fully loaded grid.</returns>
        /// <exception cref="InvalidOperationException">If the file is not open.</exception>
        /// <exception cref="KeyNotFoundException">If no grid with the given name exists.</exception>
        public GridBase ReadGrid(string name)
        {
            if (!_isOpen)
            {
                throw new InvalidOperationException("File is not open for reading");
            }

            if (!_gridDescriptors.TryGetValue(name, out var descriptor))
            {
                throw new KeyNotFoundException($"Grid '{name}' not found in file");
            }

            var grid = ReadGrid(descriptor);
            if (grid == null)
            {
                throw new IOException($"Failed to read grid '{name}'");
            }

            return grid;
        }

        /// <summary>
        /// Writes the grids to the file.
        /// </summary>
        /// <param name="grids">The grids to write.</param>
        /// <param name="metadata">Optional file-level metadata.</param>
        public override void Write(IEnumerable<GridBase> grids, MetaMap? metadata = null)
        {
            var gridList = grids.ToList();

            try
            {
                _fileStream = new FileStream(_filename, FileMode.Create, FileAccess.Write, FileShare.None);
                _writer = new BinaryWriter(_fileStream);

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
            }
            finally
            {
                if (_writer != null)
                {
                    _writer.Dispose();
                    _writer = null;
                }

                if (_fileStream != null)
                {
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }
        }

        /// <summary>
        /// Gets an enumerable of all grid names in the file.
        /// </summary>
        /// <returns>An enumerable of grid names.</returns>
        public IEnumerable<string> GetGridNames()
        {
            return _gridDescriptors.Keys;
        }

        #region Private Helper Methods

        private void ReadHeader()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // TODO: Implement VDB file header reading
            // Magic number, version, etc.
            throw new NotImplementedException("VDB file header reading not yet implemented");
        }

        private MetaMap ReadFileMetadata()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // TODO: Implement metadata reading
            return new MetaMap();
        }

        private void ReadGridDescriptors()
        {
            if (_reader == null)
                throw new InvalidOperationException("Reader is not initialized");

            // TODO: Implement grid descriptor reading
            // This requires GridDescriptor to be ported (Lot 6B)
        }

        private GridBase? ReadGridMetadataOnly(GridDescriptor descriptor)
        {
            // TODO: Implement metadata-only grid reading
            return null;
        }

        private GridBase? ReadGrid(GridDescriptor descriptor)
        {
            // TODO: Implement full grid reading
            return null;
        }

        private void WriteHeader()
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // TODO: Implement VDB file header writing
            throw new NotImplementedException("VDB file header writing not yet implemented");
        }

        private void WriteFileMetadata(MetaMap metadata)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // TODO: Implement metadata writing
        }

        private void WriteGrid(GridBase grid)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writer is not initialized");

            // TODO: Implement grid writing
            throw new NotImplementedException("Grid writing not yet implemented");
        }

        #endregion
    }

    /// <summary>
    /// Placeholder for GridDescriptor class (to be ported in Lot 6B).
    /// </summary>
    internal class GridDescriptor
    {
        public string Name { get; set; } = string.Empty;
        public string GridType { get; set; } = string.Empty;
        public long DataOffset { get; set; }
        public long BlockCount { get; set; }
    }
}
