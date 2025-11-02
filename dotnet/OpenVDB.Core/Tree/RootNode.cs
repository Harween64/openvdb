// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// RootNode.cs - C# port of RootNode.h
//
// The root node of an OpenVDB tree

using System;
using System.Collections.Generic;
using OpenVDB.Math;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Root node of an OpenVDB tree
    /// </summary>
    /// <typeparam name="TChild">The child node type</typeparam>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <remarks>
    /// The RootNode is the top-level node in the OpenVDB tree hierarchy.
    /// It uses a hash map to store sparse children and tiles.
    /// </remarks>
    public class RootNode<TChild, TValue> 
        where TChild : class
        where TValue : struct
    {
        private Dictionary<Coord, NodeUnion<TValue, TChild>> _table;
        private TValue _background;
        
        /// <summary>
        /// Level of this node (root is highest level)
        /// </summary>
        public const int LEVEL = 3; // TODO: Calculate from TChild
        
        /// <summary>
        /// Construct a new tree with a background value of default
        /// </summary>
        public RootNode()
        {
            _table = new Dictionary<Coord, NodeUnion<TValue, TChild>>();
            _background = default(TValue);
        }
        
        /// <summary>
        /// Construct a new tree with the given background value
        /// </summary>
        /// <param name="background">The background value</param>
        public RootNode(TValue background)
        {
            _table = new Dictionary<Coord, NodeUnion<TValue, TChild>>();
            _background = background;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other">The root node to copy</param>
        public RootNode(RootNode<TChild, TValue> other)
        {
            _background = other._background;
            _table = new Dictionary<Coord, NodeUnion<TValue, TChild>>(other._table);
        }
        
        /// <summary>
        /// Get the background value
        /// </summary>
        public TValue GetBackground() => _background;
        
        /// <summary>
        /// Set the background value
        /// </summary>
        public void SetBackground(TValue background)
        {
            _background = background;
        }
        
        /// <summary>
        /// Return the number of child nodes and tiles
        /// </summary>
        public int GetTableSize() => _table.Count;
        
        /// <summary>
        /// Return true if this root node is empty
        /// </summary>
        public bool IsEmpty() => _table.Count == 0;
        
        /// <summary>
        /// Clear all nodes and tiles
        /// </summary>
        public void Clear()
        {
            _table.Clear();
        }
        
        /// <summary>
        /// Get the value at the given coordinates
        /// </summary>
        /// <param name="xyz">The coordinates</param>
        /// <returns>The value at those coordinates</returns>
        public TValue GetValue(Coord xyz)
        {
            // TODO: Implement proper hierarchical lookup
            return _background;
        }
        
        /// <summary>
        /// Set the value at the given coordinates
        /// </summary>
        /// <param name="xyz">The coordinates</param>
        /// <param name="value">The value to set</param>
        public void SetValue(Coord xyz, TValue value)
        {
            // TODO: Implement proper hierarchical value setting
        }
        
        /// <summary>
        /// Set the value and active state at the given coordinates
        /// </summary>
        public void SetValueOn(Coord xyz, TValue value)
        {
            // TODO: Implement
        }
        
        /// <summary>
        /// Mark the voxel at the given coordinates as inactive
        /// </summary>
        public void SetValueOff(Coord xyz)
        {
            // TODO: Implement
        }
        
        /// <summary>
        /// Return true if the voxel at the given coordinates is active
        /// </summary>
        public bool IsValueOn(Coord xyz)
        {
            // TODO: Implement proper hierarchical lookup
            return false;
        }
        
        /// <summary>
        /// Return the memory footprint of this root node in bytes
        /// </summary>
        public long MemUsage()
        {
            // TODO: Calculate properly including children
            return IntPtr.Size * 8;
        }
        
        /// <summary>
        /// Return the number of active voxels
        /// </summary>
        public long ActiveVoxelCount()
        {
            // TODO: Implement proper counting
            return 0;
        }
    }
}
