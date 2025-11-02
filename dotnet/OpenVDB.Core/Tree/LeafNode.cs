// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// LeafNode.cs - C# port of LeafNode.h
//
// Templated block class to hold specific data types and a fixed number of values

using System;
using OpenVDB.Math;
using OpenVDB.Utils;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Templated block class to hold specific data types and a fixed number of values
    /// determined by Log2Dim. The actual coordinate dimension of the block is 2^Log2Dim,
    /// i.e. Log2Dim=3 corresponds to a LeafNode that spans a 8^3 block.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    public class LeafNode<T> where T : struct
    {
        private readonly int _log2Dim;
        private readonly int _dim;
        private readonly int _size;
        private Coord _origin;
        private LeafBuffer<T> _buffer;
        private NodeMask3 _valueMask; // TODO: Make this generic when NodeMask design is fixed
        private uint _transientData;
        
        /// <summary>
        /// Level of this node (always 0 for leaf nodes)
        /// </summary>
        public const int LEVEL = 0;
        
        /// <summary>
        /// Log2 of the linear dimension
        /// </summary>
        public int LOG2DIM => _log2Dim;
        
        /// <summary>
        /// Linear dimension of the node
        /// </summary>
        public int DIM => _dim;
        
        /// <summary>
        /// Total number of voxels
        /// </summary>
        public int SIZE => _size;
        
        /// <summary>
        /// Total number of voxels
        /// </summary>
        public int NUM_VALUES => _size;
        
        /// <summary>
        /// Default constructor (Log2Dim=3, 8^3=512 voxels)
        /// </summary>
        public LeafNode() : this(3)
        {
        }
        
        /// <summary>
        /// Constructor with specific Log2Dim
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        public LeafNode(int log2Dim)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = new Coord(0, 0, 0);
            _buffer = new LeafBuffer<T>(log2Dim);
            _valueMask = new NodeMask3(); // TODO: Support different Log2Dim values
            _transientData = 0;
        }
        
        /// <summary>
        /// Constructor with coordinates and value
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="coords">Grid index coordinates of a voxel</param>
        /// <param name="value">Value to fill the buffer with</param>
        /// <param name="active">Active state for all voxels</param>
        public LeafNode(int log2Dim, Coord coords, T value, bool active = false)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = coords & ~(_dim - 1); // Align to node boundaries
            _buffer = new LeafBuffer<T>(log2Dim, value);
            _valueMask = new NodeMask3();
            if (active)
            {
                _valueMask.SetOn();
            }
            _transientData = 0;
        }
        
        /// <summary>
        /// Partial create constructor (does not allocate buffer)
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="partialCreate">Partial creation marker</param>
        /// <param name="coords">Grid index coordinates</param>
        /// <param name="value">Initial value</param>
        /// <param name="active">Active state</param>
        public LeafNode(int log2Dim, PartialCreate partialCreate, Coord coords, T value, bool active = false)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = coords & ~(_dim - 1);
            _buffer = new LeafBuffer<T>(log2Dim, partialCreate, value);
            _valueMask = new NodeMask3();
            if (active)
            {
                _valueMask.SetOn();
            }
            _transientData = 0;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The node to copy</param>
        public LeafNode(LeafNode<T> other)
        {
            _log2Dim = other._log2Dim;
            _dim = other._dim;
            _size = other._size;
            _origin = other._origin;
            _buffer = new LeafBuffer<T>(other._buffer);
            _valueMask = new NodeMask3(); // Copy constructor would be: new NodeMask3(other._valueMask)
            // TODO: Add proper copy constructor for NodeMask3
            for (uint i = 0; i < (uint)_size; i++)
            {
                if (other._valueMask.IsOn(i))
                    _valueMask.SetOn(i);
            }
            _transientData = other._transientData;
        }
        
        // Statistics methods
        
        /// <summary>
        /// Return log2 of the dimension of this LeafNode
        /// </summary>
        public int Log2Dim() => _log2Dim;
        
        /// <summary>
        /// Return the number of voxels in each coordinate dimension
        /// </summary>
        public int Dim() => _dim;
        
        /// <summary>
        /// Return the total number of voxels represented by this LeafNode
        /// </summary>
        public int GetSize() => _size;
        
        /// <summary>
        /// Return the level of this node (always 0 for leaf nodes)
        /// </summary>
        public static int GetLevel() => LEVEL;
        
        /// <summary>
        /// Return the number of voxels marked On
        /// </summary>
        public long OnVoxelCount() => _valueMask.CountOn();
        
        /// <summary>
        /// Return the number of voxels marked Off
        /// </summary>
        public long OffVoxelCount() => _valueMask.CountOff();
        
        /// <summary>
        /// Return true if this node has no active voxels
        /// </summary>
        public bool IsEmpty() => _valueMask.IsOff();
        
        /// <summary>
        /// Return true if this node contains only active voxels
        /// </summary>
        public bool IsDense() => _valueMask.IsOn();
        
        /// <summary>
        /// Return true if memory for this node's buffer has been allocated
        /// </summary>
        public bool IsAllocated() => !_buffer.IsEmpty();
        
        /// <summary>
        /// Allocate memory for this node's buffer if it has not already been allocated
        /// </summary>
        public bool Allocate() => _buffer.Allocate();
        
        /// <summary>
        /// Return the memory in bytes occupied by this node
        /// </summary>
        public long MemUsage()
        {
            return IntPtr.Size * 4 + _buffer.MemUsage() + (int)NodeMask3.MemUsage();
        }
        
        // Origin and coordinate methods
        
        /// <summary>
        /// Set the grid index coordinates of this node's local origin
        /// </summary>
        public void SetOrigin(Coord origin)
        {
            _origin = origin;
        }
        
        /// <summary>
        /// Return the grid index coordinates of this node's local origin
        /// </summary>
        public Coord Origin() => _origin;
        
        /// <summary>
        /// Return the linear table offset of the given coordinates
        /// </summary>
        public int CoordToOffset(Coord xyz)
        {
            var localCoord = xyz - _origin;
            return (localCoord.Z << (2 * _log2Dim)) + 
                   (localCoord.Y << _log2Dim) + 
                   localCoord.X;
        }
        
        /// <summary>
        /// Return the local coordinates for a linear table offset
        /// </summary>
        public Coord OffsetToLocalCoord(int n)
        {
            int x = n & (_dim - 1);
            n >>= _log2Dim;
            int y = n & (_dim - 1);
            n >>= _log2Dim;
            int z = n;
            return new Coord(x, y, z);
        }
        
        /// <summary>
        /// Return the global coordinates for a linear table offset
        /// </summary>
        public Coord OffsetToGlobalCoord(int n)
        {
            return _origin + OffsetToLocalCoord(n);
        }
        
        // Value access methods
        
        /// <summary>
        /// Return the value at the given coordinates
        /// </summary>
        public T GetValue(Coord xyz)
        {
            return _buffer.GetValue(CoordToOffset(xyz));
        }
        
        /// <summary>
        /// Return the value at the given offset
        /// </summary>
        public T GetValue(int offset)
        {
            return _buffer.GetValue(offset);
        }
        
        /// <summary>
        /// Set the value at the given coordinates
        /// </summary>
        public void SetValue(Coord xyz, T value)
        {
            int offset = CoordToOffset(xyz);
            _buffer.SetValue(offset, value);
        }
        
        /// <summary>
        /// Set the value at the given offset
        /// </summary>
        public void SetValue(int offset, T value)
        {
            _buffer.SetValue(offset, value);
        }
        
        /// <summary>
        /// Set the active state of the voxel at the given coordinates
        /// </summary>
        public void SetValueOn(Coord xyz, T value)
        {
            int offset = CoordToOffset(xyz);
            _buffer.SetValue(offset, value);
            _valueMask.SetOn((uint)offset);
        }
        
        /// <summary>
        /// Set the active state of the voxel at the given offset
        /// </summary>
        public void SetValueOn(int offset)
        {
            _valueMask.SetOn((uint)offset);
        }
        
        /// <summary>
        /// Mark the voxel at the given coordinates as inactive
        /// </summary>
        public void SetValueOff(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            _valueMask.SetOff((uint)offset);
        }
        
        /// <summary>
        /// Mark the voxel at the given offset as inactive
        /// </summary>
        public void SetValueOff(int offset)
        {
            _valueMask.SetOff((uint)offset);
        }
        
        /// <summary>
        /// Return true if the voxel at the given coordinates is active
        /// </summary>
        public bool IsValueOn(Coord xyz)
        {
            return _valueMask.IsOn((uint)CoordToOffset(xyz));
        }
        
        /// <summary>
        /// Return true if the voxel at the given offset is active
        /// </summary>
        public bool IsValueOn(int offset)
        {
            return _valueMask.IsOn((uint)offset);
        }
        
        /// <summary>
        /// Return the transient data value
        /// </summary>
        public uint TransientData() => _transientData;
        
        /// <summary>
        /// Set the transient data value
        /// </summary>
        public void SetTransientData(uint transientData)
        {
            _transientData = transientData;
        }
        
        /// <summary>
        /// Fill the buffer with the specified value
        /// </summary>
        public void Fill(T value)
        {
            _buffer.Fill(value);
        }
        
        /// <summary>
        /// Fill the buffer and set all voxels to active or inactive
        /// </summary>
        public void Fill(T value, bool active)
        {
            _buffer.Fill(value);
            if (active)
            {
                _valueMask.SetOn();
            }
            else
            {
                _valueMask.SetOff();
            }
        }
        
        /// <summary>
        /// Get the underlying buffer
        /// </summary>
        public LeafBuffer<T> Buffer() => _buffer;
        
        /// <summary>
        /// Get the value mask
        /// </summary>
        public NodeMask3 ValueMask() => _valueMask;
        
        /// <summary>
        /// Check for equivalence with another node
        /// </summary>
        public bool Equals(LeafNode<T> other)
        {
            if (ReferenceEquals(this, other))
                return true;
                
            return _origin == other._origin &&
                   _valueMask.Equals(other._valueMask) &&
                   _buffer.Equals(other._buffer);
        }
        
        /// <summary>
        /// String representation of this node
        /// </summary>
        public override string ToString()
        {
            return $"LeafNode[{typeof(T).Name}] origin={_origin} dim={_dim} active={OnVoxelCount()}/{_size}";
        }
    }
}
