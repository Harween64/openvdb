// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// InternalNode.cs - C# port of InternalNode.h
//
// Internal table nodes for OpenVDB trees

using System;
using OpenVDB.Math;
using OpenVDB.Utils;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Internal node in the OpenVDB tree hierarchy
    /// </summary>
    /// <typeparam name="TChild">The child node type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <remarks>
    /// Internal nodes sit between the root and leaf nodes, providing
    /// multiple levels of hierarchical spatial indexing.
    /// </remarks>
    public class InternalNode<TChild, TValue>
        where TChild : class
        where TValue : struct
    {
        private readonly int _log2Dim;
        private readonly int _dim;
        private readonly int _numValues;
        private Coord _origin;
        private NodeUnion<TValue, TChild>[] _table;
        private NodeMask3 _valueMask;  // Marks which table entries are active values
        private NodeMask3 _childMask;  // Marks which table entries are child nodes
        
        /// <summary>
        /// Level of this node in the tree
        /// </summary>
        public const int LEVEL = 1; // TODO: Calculate from TChild
        
        /// <summary>
        /// Log2 of the node dimension
        /// </summary>
        public int LOG2DIM => _log2Dim;
        
        /// <summary>
        /// Dimension of the node
        /// </summary>
        public int DIM => _dim;
        
        /// <summary>
        /// Number of table entries
        /// </summary>
        public int NUM_VALUES => _numValues;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public InternalNode() : this(4) // Default Log2Dim = 4 (16^3 = 4096 entries)
        {
        }
        
        /// <summary>
        /// Constructor with log2dim
        /// </summary>
        /// <param name="log2Dim">Log2 of the node dimension</param>
        public InternalNode(int log2Dim)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _numValues = 1 << (3 * log2Dim);
            _origin = new Coord(0, 0, 0);
            _table = new NodeUnion<TValue, TChild>[_numValues];
            for (int i = 0; i < _numValues; i++)
            {
                _table[i] = new NodeUnion<TValue, TChild>();
            }
            _valueMask = new NodeMask3();
            _childMask = new NodeMask3();
        }
        
        /// <summary>
        /// Constructor with background value
        /// </summary>
        /// <param name="log2Dim">Log2 of the node dimension</param>
        /// <param name="offValue">Background value for inactive entries</param>
        public InternalNode(int log2Dim, TValue offValue) : this(log2Dim)
        {
            // Fill all entries with the background value
            for (int i = 0; i < _numValues; i++)
            {
                _table[i].SetValue(offValue);
            }
        }
        
        /// <summary>
        /// Constructor with origin and fill value
        /// </summary>
        /// <param name="log2Dim">Log2 of the node dimension</param>
        /// <param name="origin">Origin coordinates</param>
        /// <param name="fillValue">Value to fill all entries with</param>
        /// <param name="active">Active state for all entries</param>
        public InternalNode(int log2Dim, Coord origin, TValue fillValue, bool active = false) : this(log2Dim)
        {
            _origin = origin & ~(_dim - 1); // Align to node boundaries
            
            for (int i = 0; i < _numValues; i++)
            {
                _table[i].SetValue(fillValue);
                if (active)
                {
                    _valueMask.SetOn((uint)i);
                }
            }
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The node to copy</param>
        public InternalNode(InternalNode<TChild, TValue> other)
        {
            _log2Dim = other._log2Dim;
            _dim = other._dim;
            _numValues = other._numValues;
            _origin = other._origin;
            _table = new NodeUnion<TValue, TChild>[_numValues];
            for (int i = 0; i < _numValues; i++)
            {
                _table[i] = new NodeUnion<TValue, TChild>();
                // TODO: Deep copy table entries
            }
            _valueMask = new NodeMask3();
            _childMask = new NodeMask3();
            // TODO: Copy masks properly
        }
        
        /// <summary>
        /// Get the origin of this node
        /// </summary>
        public Coord Origin() => _origin;
        
        /// <summary>
        /// Set the origin of this node
        /// </summary>
        public void SetOrigin(Coord origin)
        {
            _origin = origin;
        }
        
        /// <summary>
        /// Return the linear table offset of the given coordinates
        /// </summary>
        public int CoordToOffset(Coord xyz)
        {
            var localCoord = xyz - _origin;
            localCoord >>= 3; // TODO: Get child log2dim properly - using 3 for now (8^3 leaf)
            return (localCoord.Z << (2 * _log2Dim)) +
                   (localCoord.Y << _log2Dim) +
                   localCoord.X;
        }
        
        /// <summary>
        /// Return the global coordinates for a linear table offset
        /// </summary>
        public Coord OffsetToGlobalCoord(int n)
        {
            int x = n & (_dim - 1);
            n >>= _log2Dim;
            int y = n & (_dim - 1);
            n >>= _log2Dim;
            int z = n;
            
            int childDim = 8; // TODO: Get from child properly - using 8 for now (2^3)
            return _origin + new Coord(x * childDim, y * childDim, z * childDim);
        }
        
        /// <summary>
        /// Get the value at the given coordinates
        /// </summary>
        public TValue GetValue(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            if (_childMask.IsOn((uint)offset))
            {
                var child = _table[offset].GetChild();
                // TODO: Recursively get value from child
                return default(TValue);
            }
            else
            {
                return _table[offset].GetValue();
            }
        }
        
        /// <summary>
        /// Set the value at the given coordinates
        /// </summary>
        public void SetValue(Coord xyz, TValue value)
        {
            int offset = CoordToOffset(xyz);
            // TODO: Implement proper hierarchical value setting
            _table[offset].SetValue(value);
        }
        
        /// <summary>
        /// Return true if this node is empty
        /// </summary>
        public bool IsEmpty() => _valueMask.IsOff() && _childMask.IsOff();
        
        /// <summary>
        /// Return the memory footprint of this node in bytes
        /// </summary>
        public long MemUsage()
        {
            long total = IntPtr.Size * 4;
            total += _numValues * IntPtr.Size; // Table array
            total += (int)NodeMask3.MemUsage() * 2; // Two masks
            // TODO: Add child node memory
            return total;
        }
        
        /// <summary>
        /// Return the number of active voxels
        /// </summary>
        public long OnVoxelCount()
        {
            // TODO: Implement proper counting including children
            return 0;
        }
    }
}
