// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// DelayedLoadMetadata.cs - C# port of DelayedLoadMetadata.h and DelayedLoadMetadata.cc
//
// This file provides the DelayedLoadMetadata class for storing buffers of data
// that can be optionally used during reading for faster delayed-load I/O performance.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Stores a buffer of data that can be optionally used during reading
    /// for faster delayed-load I/O performance.
    /// </summary>
    public class DelayedLoadMetadata : Metadata.Metadata
    {
        /// <summary>
        /// Type name constant for delayed load metadata.
        /// </summary>
        public const string TypeNameConst = "__delayedload";

        private List<sbyte> _mask;
        private List<long> _compressedSize;

        /// <summary>
        /// Initializes a new instance of the DelayedLoadMetadata class.
        /// </summary>
        public DelayedLoadMetadata()
        {
            _mask = new List<sbyte>();
            _compressedSize = new List<long>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public DelayedLoadMetadata(DelayedLoadMetadata other)
        {
            _mask = new List<sbyte>(other._mask);
            _compressedSize = new List<long>(other._compressedSize);
        }

        /// <summary>
        /// Gets the type name of this metadata.
        /// </summary>
        public override string TypeName => TypeNameConst;

        /// <summary>
        /// Creates a copy of this metadata.
        /// </summary>
        /// <returns>A new instance that is a copy of this one.</returns>
        public override Metadata.Metadata Copy()
        {
            return new DelayedLoadMetadata(this);
        }

        /// <summary>
        /// Copies the data from another metadata object.
        /// </summary>
        /// <param name="other">The metadata to copy from.</param>
        public override void CopyFrom(Metadata.Metadata other)
        {
            if (other is DelayedLoadMetadata dlm)
            {
                _mask = new List<sbyte>(dlm._mask);
                _compressedSize = new List<long>(dlm._compressedSize);
            }
        }

        /// <summary>
        /// Returns a string representation of this metadata.
        /// </summary>
        public override string AsString()
        {
            return $"DelayedLoadMetadata[Masks={_mask.Count}, CompressedSizes={_compressedSize.Count}]";
        }

        /// <summary>
        /// Returns a boolean representation of this metadata.
        /// </summary>
        /// <returns>True if the metadata is not empty.</returns>
        public override bool AsBool()
        {
            return !Empty();
        }

        /// <summary>
        /// Returns the size of this metadata in bytes.
        /// </summary>
        public override uint Size => (uint)(_mask.Count * sizeof(sbyte) + _compressedSize.Count * sizeof(long));

        /// <summary>
        /// Checks equality with another metadata object.
        /// </summary>
        public override bool Equals(Metadata.Metadata? other)
        {
            if (other is not DelayedLoadMetadata dlm)
                return false;

            if (_mask.Count != dlm._mask.Count || _compressedSize.Count != dlm._compressedSize.Count)
                return false;

            for (int i = 0; i < _mask.Count; i++)
            {
                if (_mask[i] != dlm._mask[i])
                    return false;
            }

            for (int i = 0; i < _compressedSize.Count; i++)
            {
                if (_compressedSize[i] != dlm._compressedSize[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the contents of the mask and compressed size arrays.
        /// </summary>
        public void Clear()
        {
            _mask.Clear();
            _compressedSize.Clear();
        }

        /// <summary>
        /// Returns true if both arrays are empty.
        /// </summary>
        public bool Empty()
        {
            return _mask.Count == 0 && _compressedSize.Count == 0;
        }

        /// <summary>
        /// Resizes the mask array.
        /// </summary>
        /// <param name="size">The new size of the mask array.</param>
        public void ResizeMask(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            while (_mask.Count < size)
            {
                _mask.Add(0);
            }

            if (_mask.Count > size)
            {
                _mask.RemoveRange(size, _mask.Count - size);
            }
        }

        /// <summary>
        /// Resizes the compressed size array.
        /// </summary>
        /// <param name="size">The new size of the compressed size array.</param>
        public void ResizeCompressedSize(int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            while (_compressedSize.Count < size)
            {
                _compressedSize.Add(0);
            }

            if (_compressedSize.Count > size)
            {
                _compressedSize.RemoveRange(size, _compressedSize.Count - size);
            }
        }

        /// <summary>
        /// Gets the mask value for a specific index.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The mask value at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">If the index is out of range.</exception>
        public sbyte GetMask(int index)
        {
            if (index < 0 || index >= _mask.Count)
            {
                throw new IndexOutOfRangeException($"Mask index {index} is out of range [0, {_mask.Count})");
            }
            return _mask[index];
        }

        /// <summary>
        /// Sets the mask value for a specific index.
        /// </summary>
        /// <param name="index">The index to set.</param>
        /// <param name="value">The mask value to set.</param>
        /// <exception cref="IndexOutOfRangeException">If the index is out of range.</exception>
        public void SetMask(int index, sbyte value)
        {
            if (index < 0 || index >= _mask.Count)
            {
                throw new IndexOutOfRangeException($"Mask index {index} is out of range [0, {_mask.Count})");
            }
            _mask[index] = value;
        }

        /// <summary>
        /// Gets the compressed size value for a specific index.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The compressed size value at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">If the index is out of range.</exception>
        public long GetCompressedSize(int index)
        {
            if (index < 0 || index >= _compressedSize.Count)
            {
                throw new IndexOutOfRangeException($"CompressedSize index {index} is out of range [0, {_compressedSize.Count})");
            }
            return _compressedSize[index];
        }

        /// <summary>
        /// Sets the compressed size value for a specific index.
        /// </summary>
        /// <param name="index">The index to set.</param>
        /// <param name="value">The compressed size value to set.</param>
        /// <exception cref="IndexOutOfRangeException">If the index is out of range.</exception>
        public void SetCompressedSize(int index, long value)
        {
            if (index < 0 || index >= _compressedSize.Count)
            {
                throw new IndexOutOfRangeException($"CompressedSize index {index} is out of range [0, {_compressedSize.Count})");
            }
            _compressedSize[index] = value;
        }

        /// <summary>
        /// Reads the metadata value from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader to read from.</param>
        /// <param name="numBytes">The number of bytes to read.</param>
        internal void ReadValue(BinaryReader reader, uint numBytes)
        {
            // Read mask count
            int maskCount = reader.ReadInt32();
            _mask.Clear();
            for (int i = 0; i < maskCount; i++)
            {
                _mask.Add(reader.ReadSByte());
            }

            // Read compressed size count
            int compressedSizeCount = reader.ReadInt32();
            _compressedSize.Clear();
            for (int i = 0; i < compressedSizeCount; i++)
            {
                _compressedSize.Add(reader.ReadInt64());
            }
        }

        /// <summary>
        /// Writes the metadata value to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer to write to.</param>
        internal void WriteValue(BinaryWriter writer)
        {
            // Write mask count and data
            writer.Write(_mask.Count);
            foreach (var mask in _mask)
            {
                writer.Write(mask);
            }

            // Write compressed size count and data
            writer.Write(_compressedSize.Count);
            foreach (var size in _compressedSize)
            {
                writer.Write(size);
            }
        }

        /// <summary>
        /// Registers this metadata type.
        /// </summary>
        public static void RegisterType()
        {
            Metadata.Metadata.RegisterType(TypeNameConst, () => new DelayedLoadMetadata());
        }

        /// <summary>
        /// Unregisters this metadata type.
        /// </summary>
        public static void UnregisterType()
        {
            Metadata.Metadata.UnregisterType(TypeNameConst);
        }

        /// <summary>
        /// Checks if this metadata type is registered.
        /// </summary>
        public static bool IsRegisteredType()
        {
            return Metadata.Metadata.IsRegisteredType(TypeNameConst);
        }
    }
}
