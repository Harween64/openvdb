// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Archive.cs - C# port of Archive.h and Archive.cc
//
// This file provides the Archive class, which is the base class for grid serialization/deserialization.

using System;
using System.Collections.Generic;
using System.IO;
using OpenVDB.Grid;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Base class for grid serialization/deserialization (to/from files or streams).
    /// </summary>
    public class Archive
    {
        /// <summary>
        /// Default compression flags.
        /// </summary>
        public static readonly uint DefaultCompressionFlags = 
            (uint)(CompressionFlags.Zip | CompressionFlags.ActiveMask);

        private string _uniqueTag;
        private uint _fileVersion;
        private VersionId _libraryVersion;
        private bool _enableInstancing;
        private uint _compression;
        private bool _enableGridStats;
        private bool _inputHasGridOffsets;

        /// <summary>
        /// Initializes a new instance of the Archive class.
        /// </summary>
        public Archive()
        {
            _uniqueTag = Guid.NewGuid().ToString();
            _fileVersion = 0;
            _libraryVersion = new VersionId(11, 0); // OpenVDB 11.0
            _enableInstancing = true;
            _compression = DefaultCompressionFlags;
            _enableGridStats = true;
            _inputHasGridOffsets = false;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public Archive(Archive other)
        {
            _uniqueTag = other._uniqueTag;
            _fileVersion = other._fileVersion;
            _libraryVersion = other._libraryVersion;
            _enableInstancing = other._enableInstancing;
            _compression = other._compression;
            _enableGridStats = other._enableGridStats;
            _inputHasGridOffsets = other._inputHasGridOffsets;
        }

        /// <summary>
        /// Create a copy of this archive.
        /// </summary>
        /// <returns>A new Archive instance that is a copy of this one.</returns>
        public virtual Archive Copy()
        {
            return new Archive(this);
        }

        /// <summary>
        /// Gets the unique tag (UUID) that was most recently written or read.
        /// </summary>
        public string UniqueTag => _uniqueTag;

        /// <summary>
        /// Check if the given UUID matches this archive's UUID.
        /// </summary>
        /// <param name="uuidStr">The UUID string to compare.</param>
        /// <returns>True if the UUIDs match.</returns>
        public bool IsIdentical(string uuidStr)
        {
            return string.Equals(_uniqueTag, uuidStr, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the file format version number of the input stream.
        /// </summary>
        public uint FileVersion => _fileVersion;

        /// <summary>
        /// Gets the library version number that was used to write the input stream.
        /// </summary>
        public VersionId LibraryVersion => _libraryVersion;

        /// <summary>
        /// Gets a string of the form "major.minor/format", giving the library
        /// and file format version numbers.
        /// </summary>
        public string Version => $"{_libraryVersion}/{_fileVersion}";

        /// <summary>
        /// Gets or sets whether trees shared by multiple grids should be written out
        /// only once (true) or once per grid (false).
        /// </summary>
        /// <remarks>
        /// Instancing is enabled by default.
        /// </remarks>
        public bool IsInstancingEnabled
        {
            get => _enableInstancing;
            set => _enableInstancing = value;
        }

        /// <summary>
        /// Check if the OpenVDB library includes support for the Blosc compressor.
        /// </summary>
        /// <returns>True if Blosc compression is available.</returns>
        public static bool HasBloscCompression()
        {
            return CompressionUtilities.HasBloscCompression();
        }

        /// <summary>
        /// Check if the OpenVDB library includes support for the ZLib compressor.
        /// </summary>
        /// <returns>True if ZLib compression is available.</returns>
        public static bool HasZLibCompression()
        {
            return CompressionUtilities.HasZLibCompression();
        }

        /// <summary>
        /// Gets a bit mask specifying compression options for the data stream.
        /// </summary>
        public uint Compression => _compression;

        /// <summary>
        /// Sets the compression options for the data stream.
        /// </summary>
        /// <param name="compressionFlags">Bitwise OR of compression option flags.</param>
        /// <remarks>
        /// Not all combinations of compression options are supported.
        /// </remarks>
        public void SetCompression(uint compressionFlags)
        {
            _compression = compressionFlags;
        }

        /// <summary>
        /// Gets whether grid statistics (active voxel count and bounding box, etc.)
        /// are computed and written as grid metadata.
        /// </summary>
        public bool IsGridStatsMetadataEnabled => _enableGridStats;

        /// <summary>
        /// Sets whether grid statistics should be computed and written as grid metadata.
        /// </summary>
        public void SetGridStatsMetadataEnabled(bool enable)
        {
            _enableGridStats = enable;
        }

        /// <summary>
        /// Write the grids to this archive's output stream.
        /// </summary>
        /// <param name="grids">The grids to write.</param>
        /// <param name="metadata">File-level metadata.</param>
        public virtual void Write(IEnumerable<GridBase> grids, MetaMap? metadata = null)
        {
            // Base implementation does nothing - override in derived classes
        }

        /// <summary>
        /// Check if delayed loading is enabled.
        /// </summary>
        /// <returns>True if delayed loading is enabled.</returns>
        /// <remarks>
        /// If enabled, delayed loading can be disabled for individual files,
        /// but not vice-versa.
        /// Define the environment variable OPENVDB_DISABLE_DELAYED_LOAD
        /// to disable delayed loading unconditionally.
        /// </remarks>
        public static bool IsDelayedLoadingEnabled()
        {
            var envVar = Environment.GetEnvironmentVariable("OPENVDB_DISABLE_DELAYED_LOAD");
            return string.IsNullOrEmpty(envVar);
        }

        /// <summary>
        /// Gets whether the input stream contains grid offsets that allow for
        /// random access or partial reading.
        /// </summary>
        protected bool InputHasGridOffsets
        {
            get => _inputHasGridOffsets;
            set => _inputHasGridOffsets = value;
        }

        /// <summary>
        /// Sets the file version from the stream.
        /// </summary>
        protected void SetFileVersion(uint version)
        {
            _fileVersion = version;
        }

        /// <summary>
        /// Sets the library version from the stream.
        /// </summary>
        protected void SetLibraryVersion(VersionId version)
        {
            _libraryVersion = version;
        }

        /// <summary>
        /// Sets the unique tag for this archive.
        /// </summary>
        protected void SetUniqueTag(string tag)
        {
            _uniqueTag = tag;
        }

        /// <summary>
        /// Read the grid count from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <returns>The number of grids in the stream.</returns>
        protected static int ReadGridCount(BinaryReader stream)
        {
            return stream.ReadInt32();
        }

        /// <summary>
        /// Write the grid count to a stream.
        /// </summary>
        /// <param name="stream">The output stream.</param>
        /// <param name="count">The number of grids.</param>
        protected static void WriteGridCount(BinaryWriter stream, int count)
        {
            stream.Write(count);
        }
    }
}
