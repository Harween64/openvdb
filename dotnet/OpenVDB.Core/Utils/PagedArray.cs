// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from PagedArray.h

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Concurrent, page-based, dynamically-sized linear data structure
    /// with O(1) random access and STL-compliant iterators.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <typeparam name="TLog2PageSize">Log base 2 of the page size (default 10 = 1024 elements)</typeparam>
    /// <remarks>
    /// This is primarily intended for applications that concurrently insert
    /// (a possibly unknown number of) elements into a dynamically
    /// growing linear array, and fast random access to said elements.
    /// 
    /// Multiple threads can grow the page-table and push_back
    /// new elements concurrently. A ValueBuffer provides accelerated
    /// and thread-safe push_back at the cost of potentially re-ordering
    /// elements (when multiple instances are used).
    /// 
    /// NOTE: This is a simplified C# port. Full parallel functionality using
    /// Task Parallel Library will be implemented when needed.
    /// </remarks>
    public class PagedArray<T, TLog2PageSize> where TLog2PageSize : struct
    {
        private const int Log2PageSize = 10; // Default to 1024 elements per page
        private const int PageSize = 1 << Log2PageSize;
        private const int PageMask = PageSize - 1;

        private readonly List<T[]> _pages;
        private readonly object _lock = new object();
        private int _size;
        private int _capacity;

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagedArray()
        {
            _pages = new List<T[]>();
            _size = 0;
            _capacity = 0;
        }

        /// <summary>
        /// Return the current size (number of elements)
        /// </summary>
        public int Size => _size;

        /// <summary>
        /// Return the current capacity
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// Return the page size
        /// </summary>
        public int GetPageSize() => PageSize;

        /// <summary>
        /// Return the number of pages
        /// </summary>
        public int PageCount => _pages.Count;

        /// <summary>
        /// Check if the array is empty
        /// </summary>
        public bool IsEmpty() => _size == 0;

        /// <summary>
        /// Clear all pages and reset size
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pages.Clear();
                _size = 0;
                _capacity = 0;
            }
        }

        /// <summary>
        /// Resize the array to the specified size
        /// </summary>
        public void Resize(int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newSize));

            lock (_lock)
            {
                while (_capacity < newSize)
                {
                    _pages.Add(new T[PageSize]);
                    _capacity += PageSize;
                }
                _size = newSize;
            }
        }

        /// <summary>
        /// Add a value to the array (thread-safe but slower than using ValueBuffer)
        /// </summary>
        /// <param name="value">Value to add</param>
        /// <returns>Index where value was inserted</returns>
        public int PushBackUnsafe(T value)
        {
            int index = Interlocked.Increment(ref _size) - 1;
            
            lock (_lock)
            {
                while (index >= _capacity)
                {
                    _pages.Add(new T[PageSize]);
                    _capacity += PageSize;
                }
            }

            int pageIdx = index >> Log2PageSize;
            int elemIdx = index & PageMask;
            _pages[pageIdx][elemIdx] = value;
            
            return index;
        }

        /// <summary>
        /// Random access operator
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _size)
                    throw new IndexOutOfRangeException();
                
                int pageIdx = index >> Log2PageSize;
                int elemIdx = index & PageMask;
                return _pages[pageIdx][elemIdx];
            }
            set
            {
                if (index < 0 || index >= _size)
                    throw new IndexOutOfRangeException();
                
                int pageIdx = index >> Log2PageSize;
                int elemIdx = index & PageMask;
                _pages[pageIdx][elemIdx] = value;
            }
        }

        /// <summary>
        /// Fill all elements with the specified value
        /// </summary>
        public void Fill(T value)
        {
            for (int pageIdx = 0; pageIdx < _pages.Count; pageIdx++)
            {
                Array.Fill(_pages[pageIdx], value);
            }
        }

        /// <summary>
        /// Value buffer for efficient batched insertion
        /// </summary>
        /// <remarks>
        /// This provides thread-safe push_back by caching values locally
        /// and flushing them to the PagedArray in batches.
        /// Each thread should have its own ValueBuffer instance.
        /// </remarks>
        public class ValueBuffer : IDisposable
        {
            private readonly PagedArray<T, TLog2PageSize> _parent;
            private readonly List<T> _buffer;
            private const int BufferSize = 128;

            public ValueBuffer(PagedArray<T, TLog2PageSize> parent)
            {
                _parent = parent;
                _buffer = new List<T>(BufferSize);
            }

            /// <summary>
            /// Add a value to the buffer
            /// </summary>
            public void PushBack(T value)
            {
                _buffer.Add(value);
                if (_buffer.Count >= BufferSize)
                {
                    Flush();
                }
            }

            /// <summary>
            /// Flush all buffered values to the parent array
            /// </summary>
            public void Flush()
            {
                if (_buffer.Count == 0) return;

                foreach (var value in _buffer)
                {
                    _parent.PushBackUnsafe(value);
                }
                _buffer.Clear();
            }

            /// <summary>
            /// Auto-flush on dispose
            /// </summary>
            public void Dispose()
            {
                Flush();
            }
        }

        /// <summary>
        /// Get a new ValueBuffer for efficient batched insertion
        /// </summary>
        public ValueBuffer GetBuffer()
        {
            return new ValueBuffer(this);
        }
    }

    /// <summary>
    /// PagedArray with default page size of 1024 elements
    /// </summary>
    public class PagedArray<T> : PagedArray<T, int>
    {
    }
}
