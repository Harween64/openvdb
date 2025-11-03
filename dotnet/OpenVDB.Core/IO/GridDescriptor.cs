// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// GridDescriptor.cs - C# port of GridDescriptor.h and GridDescriptor.cc
//
// This file provides the GridDescriptor class for storing grid metadata during I/O.

using System;
using System.IO;
using System.Text;
using OpenVDB.Grid;

namespace OpenVDB.IO
{
    /// <summary>
    /// Stores useful information that describes a grid on disk.
    /// Can be used to retrieve I/O information about the grid such as offsets into the file
    /// where the grid is located, its type, etc.
    /// </summary>
    public class GridDescriptor
    {
        private const string HalfFloatTypenameSuffix = "_HalfFloat";
        private const char RecordSeparator = '\x1e'; // ASCII record separator

        private string _gridName;
        private string _uniqueName;
        private string _instanceParentName;
        private string _gridType;
        private bool _saveFloatAsHalf;
        private long _gridPos;
        private long _blockPos;
        private long _endPos;

        /// <summary>
        /// Initializes a new instance of the GridDescriptor class.
        /// </summary>
        public GridDescriptor()
        {
            _gridName = string.Empty;
            _uniqueName = string.Empty;
            _instanceParentName = string.Empty;
            _gridType = string.Empty;
            _saveFloatAsHalf = false;
            _gridPos = 0;
            _blockPos = 0;
            _endPos = 0;
        }

        /// <summary>
        /// Initializes a new instance of the GridDescriptor class with the specified name and type.
        /// </summary>
        /// <param name="name">The grid name.</param>
        /// <param name="gridType">The grid type.</param>
        /// <param name="saveFloatAsHalf">Whether to save floats as half precision.</param>
        public GridDescriptor(string name, string gridType, bool saveFloatAsHalf = false)
        {
            _gridName = StripSuffix(name);
            _uniqueName = name;
            _instanceParentName = string.Empty;
            _gridType = gridType;
            _saveFloatAsHalf = saveFloatAsHalf;
            _gridPos = 0;
            _blockPos = 0;
            _endPos = 0;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public GridDescriptor(GridDescriptor other)
        {
            _gridName = other._gridName;
            _uniqueName = other._uniqueName;
            _instanceParentName = other._instanceParentName;
            _gridType = other._gridType;
            _saveFloatAsHalf = other._saveFloatAsHalf;
            _gridPos = other._gridPos;
            _blockPos = other._blockPos;
            _endPos = other._endPos;
        }

        /// <summary>
        /// Gets the grid type.
        /// </summary>
        public string GridType => _gridType;

        /// <summary>
        /// Gets the grid name.
        /// </summary>
        public string GridName => _gridName;

        /// <summary>
        /// Gets the unique name for this descriptor.
        /// </summary>
        public string UniqueName => _uniqueName;

        /// <summary>
        /// Gets or sets the instance parent name.
        /// If nonempty, this grid shares a tree with another grid.
        /// </summary>
        public string InstanceParentName
        {
            get => _instanceParentName;
            set => _instanceParentName = value ?? string.Empty;
        }

        /// <summary>
        /// Gets whether this grid is an instance (shares a tree with another grid).
        /// </summary>
        public bool IsInstance => !string.IsNullOrEmpty(_instanceParentName);

        /// <summary>
        /// Gets whether floats are quantized to 16 bits on disk.
        /// </summary>
        public bool SaveFloatAsHalf => _saveFloatAsHalf;

        /// <summary>
        /// Gets or sets the location in the stream where the grid data is stored.
        /// </summary>
        public long GridPos
        {
            get => _gridPos;
            set => _gridPos = value;
        }

        /// <summary>
        /// Gets or sets the location in the stream where the grid blocks are stored.
        /// </summary>
        public long BlockPos
        {
            get => _blockPos;
            set => _blockPos = value;
        }

        /// <summary>
        /// Gets or sets the location in the stream where the next grid descriptor begins.
        /// </summary>
        public long EndPos
        {
            get => _endPos;
            set => _endPos = value;
        }

        /// <summary>
        /// Seeks to the grid position in the given stream.
        /// </summary>
        /// <param name="stream">The stream to seek in.</param>
        public void SeekToGrid(System.IO.Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(_gridPos, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Seeks to the blocks position in the given stream.
        /// </summary>
        /// <param name="stream">The stream to seek in.</param>
        public void SeekToBlocks(System.IO.Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(_blockPos, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Seeks to the end position in the given stream.
        /// </summary>
        /// <param name="stream">The stream to seek in.</param>
        public void SeekToEnd(System.IO.Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(_endPos, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Writes the descriptor header to the given stream.
        /// </summary>
        /// <param name="writer">The binary writer to write to.</param>
        public void WriteHeader(BinaryWriter writer)
        {
            // Write unique name
            WriteString(writer, _uniqueName);

            // Write grid type with half float suffix if needed
            var gridType = _gridType;
            if (_saveFloatAsHalf)
            {
                gridType += HalfFloatTypenameSuffix;
            }
            WriteString(writer, gridType);

            // Write instance parent name
            WriteString(writer, _instanceParentName);
        }

        /// <summary>
        /// Writes the stream positions to the given stream.
        /// </summary>
        /// <param name="writer">The binary writer to write to.</param>
        public void WriteStreamPos(BinaryWriter writer)
        {
            writer.Write(_gridPos);
            writer.Write(_blockPos);
            writer.Write(_endPos);
        }

        /// <summary>
        /// Reads a grid descriptor from the given stream.
        /// </summary>
        /// <param name="reader">The binary reader to read from.</param>
        /// <returns>An empty grid of the type specified by the grid descriptor.</returns>
        public GridBase? Read(BinaryReader reader)
        {
            // Read the unique name
            _uniqueName = ReadString(reader);
            _gridName = StripSuffix(_uniqueName);

            // Read the grid type
            _gridType = ReadString(reader);
            if (_gridType.EndsWith(HalfFloatTypenameSuffix))
            {
                _saveFloatAsHalf = true;
                _gridType = _gridType.Substring(0, _gridType.Length - HalfFloatTypenameSuffix.Length);
            }

            // Read the instance parent name
            _instanceParentName = ReadString(reader);

            // Create the grid of the specified type if it has been registered
            if (!GridBase.IsRegistered(_gridType))
            {
                throw new InvalidOperationException($"Cannot read grid. Grid type {_gridType} is not registered.");
            }

            var grid = GridBase.CreateGrid(_gridType);
            // TODO: Set save float as half flag when Grid implementation supports it

            // Read the offsets
            _gridPos = reader.ReadInt64();
            _blockPos = reader.ReadInt64();
            _endPos = reader.ReadInt64();

            return grid;
        }

        #region Static Helper Methods

        /// <summary>
        /// Appends the number n to the given name (separated by a record separator character)
        /// and returns the resulting name.
        /// </summary>
        /// <param name="name">The base name.</param>
        /// <param name="n">The suffix number.</param>
        /// <returns>The name with suffix.</returns>
        public static string AddSuffix(string name, int n)
        {
            return $"{name}{RecordSeparator}{n}";
        }

        /// <summary>
        /// Strips from the given name any suffix that is separated by a record separator character
        /// and returns the resulting name.
        /// </summary>
        /// <param name="name">The name to strip.</param>
        /// <returns>The name without suffix.</returns>
        public static string StripSuffix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var sepIndex = name.IndexOf(RecordSeparator);
            if (sepIndex >= 0)
            {
                return name.Substring(0, sepIndex);
            }
            return name;
        }

        /// <summary>
        /// Given a name with suffix N, returns "name[N]", otherwise just returns "name".
        /// Use this to produce a human-readable string from a descriptor's unique name.
        /// </summary>
        /// <param name="name">The unique name.</param>
        /// <returns>A human-readable string representation.</returns>
        public static string NameAsString(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var sepIndex = name.IndexOf(RecordSeparator);
            if (sepIndex >= 0 && sepIndex < name.Length - 1)
            {
                var baseName = name.Substring(0, sepIndex);
                var suffix = name.Substring(sepIndex + 1);
                return $"{baseName}[{suffix}]";
            }
            return name;
        }

        /// <summary>
        /// Given a string of the form "name[N]", returns "name" with the suffix N
        /// separated by a record separator character. Otherwise just returns the string as is.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>The unique name.</returns>
        public static string StringAsUniqueName(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var openBracket = str.IndexOf('[');
            var closeBracket = str.IndexOf(']');

            if (openBracket > 0 && closeBracket > openBracket + 1 && closeBracket == str.Length - 1)
            {
                var baseName = str.Substring(0, openBracket);
                var suffix = str.Substring(openBracket + 1, closeBracket - openBracket - 1);
                return $"{baseName}{RecordSeparator}{suffix}";
            }
            return str;
        }

        #endregion

        #region Private Helper Methods

        private static void WriteString(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static string ReadString(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 0)
                throw new IOException("Invalid string length");

            if (length == 0)
                return string.Empty;

            var bytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion
    }
}
