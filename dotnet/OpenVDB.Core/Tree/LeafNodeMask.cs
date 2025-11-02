// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// LeafNodeMask.cs - C# port of LeafNodeMask.h
//
// LeafNode specialization for ValueMask type

using System;
using OpenVDB.Math;
using OpenVDB.Utils;

namespace OpenVDB.Tree
{
    /// <summary>
    /// LeafNode specialization for values of type ValueMask that encodes both
    /// the active states and the boolean values in a single bit mask
    /// </summary>
    /// <remarks>
    /// Voxel values and states are indistinguishable in this specialization.
    /// A voxel is active if and only if its value is true.
    /// </remarks>
    public class LeafNodeMask
    {
        private readonly int _log2Dim;
        private readonly int _dim;
        private readonly int _size;
        private Coord _origin;
        private NodeMask3 _valueMask;   // Combined value and active state
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
        public LeafNodeMask() : this(3)
        {
        }
        
        /// <summary>
        /// Constructor with specific Log2Dim
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        public LeafNodeMask(int log2Dim)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = new Coord(0, 0, 0);
            _valueMask = new NodeMask3();
            _transientData = 0;
        }
        
        /// <summary>
        /// Constructor with coordinates and value
        /// </summary>
        /// <param name="log2Dim">Log2 of the linear dimension</param>
        /// <param name="coords">Grid index coordinates</param>
        /// <param name="value">Initial value = state for all voxels</param>
        /// <param name="dummy">Dummy parameter for signature compatibility</param>
        public LeafNodeMask(int log2Dim, Coord coords, bool value, bool dummy = false)
        {
            _log2Dim = log2Dim;
            _dim = 1 << log2Dim;
            _size = 1 << (3 * log2Dim);
            _origin = coords & ~(_dim - 1);
            _valueMask = new NodeMask3();
            
            if (value)
            {
                _valueMask.SetOn();
            }
            _transientData = 0;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The node to copy</param>
        public LeafNodeMask(LeafNodeMask other)
        {
            _log2Dim = other._log2Dim;
            _dim = other._dim;
            _size = other._size;
            _origin = other._origin;
            _valueMask = new NodeMask3();
            // TODO: Copy mask properly
            _transientData = other._transientData;
        }
        
        /// <summary>
        /// Get the value at the given coordinates
        /// </summary>
        /// <remarks>
        /// For ValueMask nodes, value == active state
        /// </remarks>
        public bool GetValue(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            return _valueMask.IsOn((uint)offset);
        }
        
        /// <summary>
        /// Set the value at the given coordinates
        /// </summary>
        /// <remarks>
        /// For ValueMask nodes, setting value also sets active state
        /// </remarks>
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
        /// Set value on (value = active = true)
        /// </summary>
        public void SetValueOn(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            _valueMask.SetOn((uint)offset);
        }
        
        /// <summary>
        /// Set value off (value = active = false)
        /// </summary>
        public void SetValueOff(Coord xyz)
        {
            int offset = CoordToOffset(xyz);
            _valueMask.SetOff((uint)offset);
        }
        
        /// <summary>
        /// Return true if the voxel is on (value == active state)
        /// </summary>
        public bool IsValueOn(Coord xyz)
        {
            return _valueMask.IsOn((uint)CoordToOffset(xyz));
        }
        
        /// <summary>
        /// Return the number of on voxels
        /// </summary>
        public long OnVoxelCount() => _valueMask.CountOn();
        
        /// <summary>
        /// Return true if this node has no on voxels
        /// </summary>
        public bool IsEmpty() => _valueMask.IsOff();
        
        /// <summary>
        /// Return true if all voxels are on
        /// </summary>
        public bool IsDense() => _valueMask.IsOn();
        
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
            return IntPtr.Size * 4 + (int)NodeMask3.MemUsage();
        }
    }
}
