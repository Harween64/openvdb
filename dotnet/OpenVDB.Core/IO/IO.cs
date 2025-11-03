// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// IO.cs - C# port of io.h
//
// This file provides the StreamMetadata class and utility functions for I/O operations.

using System;
using System.Collections.Generic;
using System.IO;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Container for metadata describing how to serialize/deserialize grids from/to a stream.
    /// This includes file format, compression scheme, etc.
    /// </summary>
    /// <remarks>
    /// This class is mainly for internal use.
    /// </remarks>
    public class StreamMetadata
    {
        private uint _fileVersion;
        private VersionId _libraryVersion;
        private uint _compression;
        private uint _gridClass;
        private object? _backgroundPtr;
        private bool _halfFloat;
        private bool _writeGridStats;
        private bool _seekable;
        private bool _countingPasses;
        private uint _pass;
        private ulong _leaf;
        private MetaMap _gridMetadata;
        private Dictionary<string, object> _auxData;

        /// <summary>
        /// Initializes a new instance of the StreamMetadata class.
        /// </summary>
        public StreamMetadata()
        {
            _fileVersion = 0;
            _libraryVersion = new VersionId(0, 0);
            _compression = 0;
            _gridClass = 0;
            _backgroundPtr = null;
            _halfFloat = false;
            _writeGridStats = false;
            _seekable = true;
            _countingPasses = false;
            _pass = 0;
            _leaf = 0;
            _gridMetadata = new MetaMap();
            _auxData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public StreamMetadata(StreamMetadata other)
        {
            _fileVersion = other._fileVersion;
            _libraryVersion = other._libraryVersion;
            _compression = other._compression;
            _gridClass = other._gridClass;
            _backgroundPtr = other._backgroundPtr;
            _halfFloat = other._halfFloat;
            _writeGridStats = other._writeGridStats;
            _seekable = other._seekable;
            _countingPasses = other._countingPasses;
            _pass = other._pass;
            _leaf = other._leaf;
            _gridMetadata = new MetaMap(other._gridMetadata);
            _auxData = new Dictionary<string, object>(other._auxData);
        }

        /// <summary>
        /// Gets or sets the file format version number.
        /// </summary>
        public uint FileVersion
        {
            get => _fileVersion;
            set => _fileVersion = value;
        }

        /// <summary>
        /// Gets or sets the library version.
        /// </summary>
        public VersionId LibraryVersion
        {
            get => _libraryVersion;
            set => _libraryVersion = value;
        }

        /// <summary>
        /// Gets or sets the compression flags.
        /// </summary>
        public uint Compression
        {
            get => _compression;
            set => _compression = value;
        }

        /// <summary>
        /// Gets or sets the grid class.
        /// </summary>
        public uint GridClass
        {
            get => _gridClass;
            set => _gridClass = value;
        }

        /// <summary>
        /// Gets or sets the background value pointer.
        /// </summary>
        public object? BackgroundPtr
        {
            get => _backgroundPtr;
            set => _backgroundPtr = value;
        }

        /// <summary>
        /// Gets or sets whether half-float precision is used.
        /// </summary>
        public bool HalfFloat
        {
            get => _halfFloat;
            set => _halfFloat = value;
        }

        /// <summary>
        /// Gets or sets whether to write grid statistics.
        /// </summary>
        public bool WriteGridStats
        {
            get => _writeGridStats;
            set => _writeGridStats = value;
        }

        /// <summary>
        /// Gets or sets whether the stream is seekable.
        /// </summary>
        public bool Seekable
        {
            get => _seekable;
            set => _seekable = value;
        }

        /// <summary>
        /// Gets whether delayed loading metadata is present.
        /// </summary>
        public bool DelayedLoadMeta => false; // TODO: Implement when DelayedLoadMetadata is ported

        /// <summary>
        /// Gets or sets whether counting passes is enabled.
        /// </summary>
        public bool CountingPasses
        {
            get => _countingPasses;
            set => _countingPasses = value;
        }

        /// <summary>
        /// Gets or sets the current pass number.
        /// </summary>
        public uint Pass
        {
            get => _pass;
            set => _pass = value;
        }

        /// <summary>
        /// Gets or sets the current leaf node index.
        /// </summary>
        public ulong Leaf
        {
            get => _leaf;
            set => _leaf = value;
        }

        /// <summary>
        /// Gets the grid metadata map.
        /// </summary>
        public MetaMap GridMetadata => _gridMetadata;

        /// <summary>
        /// Gets the auxiliary data map.
        /// </summary>
        public Dictionary<string, object> AuxData => _auxData;

        /// <summary>
        /// Returns a string describing this stream metadata.
        /// </summary>
        public override string ToString()
        {
            return $"StreamMetadata[FileVersion={_fileVersion}, " +
                   $"LibraryVersion={_libraryVersion}, " +
                   $"Compression={_compression:X}, " +
                   $"GridClass={_gridClass}, " +
                   $"HalfFloat={_halfFloat}, " +
                   $"WriteGridStats={_writeGridStats}]";
        }
    }

    /// <summary>
    /// Marker interface for leaf nodes that require multi-pass I/O.
    /// </summary>
    /// <remarks>
    /// Leaf nodes that require multi-pass I/O must implement this interface.
    /// </remarks>
    public interface IMultiPass
    {
        // Marker interface - no methods required
    }

    /// <summary>
    /// Utility functions for I/O operations.
    /// </summary>
    public static class IOUtilities
    {
        /// <summary>
        /// Get the error string for the last system error.
        /// </summary>
        /// <returns>A string describing the error, or empty string if no error.</returns>
        public static string GetErrorString()
        {
            // In C#, exceptions are used instead of errno
            // This is kept for compatibility
            return string.Empty;
        }

        /// <summary>
        /// Get the error string for a specific error code.
        /// </summary>
        /// <param name="errorNum">The error code.</param>
        /// <returns>A string describing the error.</returns>
        public static string GetErrorString(int errorNum)
        {
            // In C#, we don't have direct errno access
            // Return a generic message
            return $"Error code: {errorNum}";
        }
    }

    /// <summary>
    /// Version identifier containing major and minor version numbers.
    /// </summary>
    public struct VersionId : IEquatable<VersionId>
    {
        /// <summary>
        /// Major version number.
        /// </summary>
        public uint Major { get; set; }

        /// <summary>
        /// Minor version number.
        /// </summary>
        public uint Minor { get; set; }

        /// <summary>
        /// Initializes a new instance of the VersionId struct.
        /// </summary>
        public VersionId(uint major, uint minor)
        {
            Major = major;
            Minor = minor;
        }

        /// <summary>
        /// Checks if this version equals another.
        /// </summary>
        public bool Equals(VersionId other)
        {
            return Major == other.Major && Minor == other.Minor;
        }

        /// <summary>
        /// Checks if this version equals another object.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is VersionId other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this version.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor);
        }

        /// <summary>
        /// Converts the version to a string.
        /// </summary>
        public override string ToString()
        {
            return $"{Major}.{Minor}";
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(VersionId left, VersionId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(VersionId left, VersionId right)
        {
            return !left.Equals(right);
        }
    }
}
