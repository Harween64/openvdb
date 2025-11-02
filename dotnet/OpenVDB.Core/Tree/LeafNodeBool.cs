// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// LeafNodeBool.cs - C# port of LeafNodeBool.h
//
// LeafNode specialization for boolean values

using System;
using OpenVDB.Math;
using OpenVDB.Utils;

namespace OpenVDB.Tree
{
    /// <summary>
    /// LeafNode specialization for values of type bool that stores both
    /// the active states and the values as bit masks
    /// </summary>
    /// <remarks>
    /// This is a specialized version of LeafNode for bool that is more memory efficient,
    /// storing both values and active states in bit masks.
    /// </remarks>
    public class LeafNodeBool
    {
        private readonly int _log2Dim;
        private readonly int _dim;
        private readonly int _size;
        private Coord _origin;
        private NodeMask3 _valueMask;   // Stores bool values
        private NodeMask3 _activeState; // Stores active states
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
        /// Default constructor (Log2Dim=3, 8^3=512 voxels)
        /// </summary>
        public LeafNodeBool() : this(3)
        {
        }
        
        /// <summary>
        /// Constructor with specific Log2Dim
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        public LeafNodeBool(int log2Dim)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = new Coord(0, 0, 0);
            _valueMask = new NodeMask3();
            _activeState = new NodeMask3();
            _transientData = 0;
        }
        
        /// <summary>
        /// Constructor with coordinates and value
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="coords">Grid index coordinates</param>
        /// <param name="value">Boolean value to fill with</param>
        /// <param name="active">Active state for all voxels</param>
        public LeafNodeBool(int log2Dim, Coord coords, bool value, bool active = false)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = coords & ~(_dim - 1);
            _valueMask = new NodeMask3();
            _activeState = new NodeMask3();
            
            if (value)
            {
                _valueMask.SetOn();
            }
            if (active)
            {
                _activeState.SetOn();
            }
            _transientData = 0;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The node to copy</param>
        public LeafNodeBool(LeafNodeBool other)
        {
            _log2Dim = other._log2Dim;
            _dim = other._dim;
            _size = other._size;
            _origin = other._origin;
            _valueMask = new NodeMask3();
            _activeState = new NodeMask3();
            // TODO: Copy masks properly
            _transientData = other._transientData;
        }
        
        /// <summary>
        /// Get the value at the given coordinates
        /// </summary>
        public bool GetValue(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            return _valueMask.IsOn((uint)offset);
        }
        
        /// <summary>
        /// Set the value at the given coordinates
        /// </summary>
        public void SetValue(Coord xyz, bool value)
        {
            int offset = CoordToOffset(xyz);
            if (value)
            {
                _valueMask.SetOn((uint)offset);
            }
            else
            {
                _valueMask.SetOff((uint)offset);
            }
        }
        
        /// <summary>
        /// Set value and active state
        /// </summary>
        public void SetValueOn(Coord xyz, bool value)
        {
            int offset = CoordToOffset(xyz);
            if (value)
            {
                _valueMask.SetOn((uint)offset);
            }
            else
            {
                _valueMask.SetOff((uint)offset);
            }
            _activeState.SetOn((uint)offset);
        }
        
        /// <summary>
        /// Return true if the voxel is active
        /// </summary>
        public bool IsValueOn(Coord xyz)
        {
            return _activeState.IsOn((uint)CoordToOffset(xyz));
        }
        
        /// <summary>
        /// Return the number of active voxels
        /// </summary>
        public long OnVoxelCount() => _activeState.CountOn();
        
        /// <summary>
        /// Return true if this node has no active voxels
        /// </summary>
        public bool IsEmpty() => _activeState.IsOff();
        
        /// <summary>
        /// Return true if all voxels are active
        /// </summary>
        public bool IsDense() => _activeState.IsOn();
        
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
        /// Convert coordinates to offset
        /// </summary>
        private int CoordToOffset(Coord xyz)
        {
            var localCoord = xyz - _origin;
            return (localCoord.Z << (2 * _log2Dim)) +
                   (localCoord.Y << _log2Dim) +
                   localCoord.X;
        }
        
        /// <summary>
        /// Return the memory footprint in bytes
        /// </summary>
        public long MemUsage()
        {
            return IntPtr.Size * 4 + (int)NodeMask3.MemUsage() * 2;
        }
    }
}
