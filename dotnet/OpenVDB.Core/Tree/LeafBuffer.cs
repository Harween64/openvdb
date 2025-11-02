// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// LeafBuffer.cs - C# port of LeafBuffer.h
//
// Array of fixed size 2^(3*Log2Dim) that stores the voxel values of a LeafNode

using System;
using System.Runtime.CompilerServices;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Array of fixed size 2^(3*Log2Dim) that stores the voxel values of a LeafNode
    /// </summary>
    /// <typeparam name="T">The value type stored in the buffer</typeparam>
    /// <remarks>
    /// In C++, Log2Dim is a template parameter. In C#, we use a constructor parameter
    /// since C# doesn't support integer template parameters.
    /// </remarks>
    public class LeafBuffer<T> 
        where T : struct
    {
        private T[]? _data;
        private readonly int _log2Dim;
        private readonly int _size;
        
        /// <summary>
        /// Total number of values in this buffer (2^(3*Log2Dim))
        /// </summary>
        public int Size => _size;
        
        /// <summary>
        /// Default constructor - allocates and zero-initializes the buffer
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        public LeafBuffer(int log2Dim)
        {
            _log2Dim = log2Dim;
            _size = 1 << (3 * log2Dim);
            _data = new T[_size];
        }
        
        /// <summary>
        /// Construct a buffer populated with the specified value
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="value">The value to fill the buffer with</param>
        public LeafBuffer(int log2Dim, T value)
        {
            _log2Dim = log2Dim;
            _size = 1 << (3 * log2Dim);
            _data = new T[_size];
            Fill(value);
        }
        
        /// <summary>
        /// Partial create constructor - does not allocate memory
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="partialCreate">Marker for partial creation</param>
        /// <param name="value">Unused value parameter for signature compatibility</param>
        public LeafBuffer(int log2Dim, PartialCreate partialCreate, T value)
        {
            _log2Dim = log2Dim;
            _size = 1 << (3 * log2Dim);
            _data = null;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The buffer to copy from</param>
        public LeafBuffer(LeafBuffer<T> other)
        {
            _log2Dim = other._log2Dim;
            _size = other._size;
            
            if (other._data != null)
            {
                _data = new T[_size];
                Array.Copy(other._data, _data, _size);
            }
            else
            {
                _data = null;
            }
        }
        
        /// <summary>
        /// Return true if memory for this buffer has not yet been allocated
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() => _data == null;
        
        /// <summary>
        /// Allocate memory for this buffer if it has not already been allocated
        /// </summary>
        public bool Allocate()
        {
            if (_data == null)
            {
                _data = new T[_size];
            }
            return true;
        }
        
        /// <summary>
        /// Populate this buffer with a constant value
        /// </summary>
        /// <param name="value">The value to fill the buffer with</param>
        public void Fill(T value)
        {
            if (_data != null)
            {
                Array.Fill(_data, value);
            }
        }
        
        /// <summary>
        /// Return a const reference to the i'th element of this buffer
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>The value at index i</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue(int i)
        {
            if (_data == null || i >= _size)
                return default(T);
            return _data[i];
        }
        
        /// <summary>
        /// Indexer for buffer access
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>The value at index i</returns>
        public T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetValue(i);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetValue(i, value);
        }
        
        /// <summary>
        /// Set the i'th value of this buffer to the specified value
        /// </summary>
        /// <param name="i">The index</param>
        /// <param name="value">The value to set</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int i, T value)
        {
            if (_data != null && i < _size)
            {
                _data[i] = value;
            }
        }
        
        /// <summary>
        /// Assignment operator - copy values from another buffer
        /// </summary>
        /// <param name="other">The buffer to copy from</param>
        public void CopyFrom(LeafBuffer<T> other)
        {
            if (ReferenceEquals(this, other))
                return;
                
            if (other._data != null)
            {
                Allocate();
                Array.Copy(other._data, _data!, _size);
            }
            else
            {
                _data = null;
            }
        }
        
        /// <summary>
        /// Return true if the contents of the other buffer exactly equal the contents of this buffer
        /// </summary>
        /// <param name="other">The buffer to compare to</param>
        /// <returns>True if buffers are equal</returns>
        public bool Equals(LeafBuffer<T> other)
        {
            if (ReferenceEquals(this, other))
                return true;
                
            if (_data == null && other._data == null)
                return true;
                
            if (_data == null || other._data == null)
                return false;
                
            for (int i = 0; i < _size; i++)
            {
                if (!_data[i].Equals(other._data[i]))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Exchange this buffer's values with the other buffer's values
        /// </summary>
        /// <param name="other">The buffer to swap with</param>
        public void Swap(LeafBuffer<T> other)
        {
            var temp = _data;
            _data = other._data;
            other._data = temp;
        }
        
        /// <summary>
        /// Return the memory footprint of this buffer in bytes
        /// </summary>
        /// <returns>Memory usage in bytes</returns>
        public int MemUsage()
        {
            int size = IntPtr.Size * 3; // Rough estimate of object overhead
            if (_data != null)
            {
                size += _size * System.Runtime.InteropServices.Marshal.SizeOf<T>();
            }
            return size;
        }
        
        /// <summary>
        /// Return a pointer to the array of voxel values (for unsafe operations)
        /// </summary>
        /// <returns>The underlying data array</returns>
        public T[]? Data()
        {
            Allocate();
            return _data;
        }
    }
    
    /// <summary>
    /// Marker type for partial creation of leaf buffers
    /// </summary>
    public struct PartialCreate
    {
        public static readonly PartialCreate Instance = new PartialCreate();
    }
}
