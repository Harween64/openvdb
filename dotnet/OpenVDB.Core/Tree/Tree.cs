// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Tree.cs - C# port of Tree.h
//
// Main tree class for OpenVDB

using System;
using OpenVDB.Math;
using OpenVDB.Metadata;

namespace OpenVDB.Tree
{
    /// <summary>
    /// Base class for typed trees
    /// </summary>
    public abstract class TreeBase
    {
        /// <summary>
        /// Return the name of this tree's type
        /// </summary>
        public abstract string Type();
        
        /// <summary>
        /// Return the name of the type of a voxel's value
        /// </summary>
        public abstract string ValueType();
        
        /// <summary>
        /// Return a deep copy of this tree
        /// </summary>
        public abstract TreeBase Copy();
        
        /// <summary>
        /// Return the background value wrapped as metadata
        /// </summary>
        public abstract Metadata.Metadata GetBackgroundValue();
        
        /// <summary>
        /// Return true if the bounding box of all active voxels is empty
        /// </summary>
        public abstract bool EvalActiveVoxelBoundingBox(out BBox<Coord> bbox);
        
        /// <summary>
        /// Clear all nodes and voxels
        /// </summary>
        public abstract void Clear();
        
        /// <summary>
        /// Return the number of active voxels
        /// </summary>
        public abstract long ActiveVoxelCount();
        
        /// <summary>
        /// Return the memory footprint of this tree in bytes
        /// </summary>
        public abstract long MemUsage();
    }
    
    /// <summary>
    /// Templated tree class
    /// </summary>
    /// <typeparam name="TRoot">The root node type</typeparam>
    public class Tree<TRoot> : TreeBase
        where TRoot : class
    {
        private TRoot? _root;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public Tree()
        {
            _root = null;
        }
        
        /// <summary>
        /// Constructor with root node
        /// </summary>
        /// <param name="root">The root node</param>
        public Tree(TRoot root)
        {
            _root = root;
        }
        
        /// <summary>
        /// Get the root node
        /// </summary>
        public TRoot? Root() => _root;
        
        /// <summary>
        /// Return the name of this tree's type
        /// </summary>
        public override string Type()
        {
            return $"Tree<{typeof(TRoot).Name}>";
        }
        
        /// <summary>
        /// Return the name of the type of a voxel's value
        /// </summary>
        public override string ValueType()
        {
            // TODO: Extract value type from TRoot
            return "unknown";
        }
        
        /// <summary>
        /// Return a deep copy of this tree
        /// </summary>
        public override TreeBase Copy()
        {
            // TODO: Implement deep copy
            throw new NotImplementedException("Tree.Copy() not yet implemented");
        }
        
        /// <summary>
        /// Return the background value wrapped as metadata
        /// </summary>
        public override Metadata.Metadata GetBackgroundValue()
        {
            // TODO: Implement
            throw new NotImplementedException("Tree.GetBackgroundValue() not yet implemented");
        }
        
        /// <summary>
        /// Return true if the bounding box of all active voxels is empty
        /// </summary>
        public override bool EvalActiveVoxelBoundingBox(out BBox<Coord> bbox)
        {
            bbox = new BBox<Coord>();
            // TODO: Implement
            return false;
        }
        
        /// <summary>
        /// Clear all nodes and voxels
        /// </summary>
        public override void Clear()
        {
            // TODO: Implement
        }
        
        /// <summary>
        /// Return the number of active voxels
        /// </summary>
        public override long ActiveVoxelCount()
        {
            // TODO: Implement
            return 0;
        }
        
        /// <summary>
        /// Return the memory footprint of this tree in bytes
        /// </summary>
        public override long MemUsage()
        {
            // TODO: Implement
            return IntPtr.Size * 2;
        }
    }
}
