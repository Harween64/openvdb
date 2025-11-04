// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Grid.cs - C# port of Grid.h and Grid.cc (Partial - Lot 1)
//
// This file provides a minimal Grid stub for Phase 1, Lot 1.
// The full Grid implementation will be completed in Phase 1, Lot 5 (Tree System).
//
// For Lot 1, we only implement the metadata-related functionality.

using System;
using System.Collections.Generic;
using OpenVDB.Metadata;

namespace OpenVDB.Grid
{
    /// <summary>
    /// Base class for all grid types.
    /// </summary>
    /// <remarks>
    /// This is a minimal implementation for Lot 1 (Metadata support).
    /// The complete grid functionality will be implemented in Lot 5 (Tree System).
    /// </remarks>
    public abstract class GridBase
    {
        private readonly MetaMap _metadata;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the GridBase class.
        /// </summary>
        protected GridBase()
        {
            _metadata = new MetaMap();
            _name = string.Empty;
        }

        /// <summary>
        /// Gets or sets the name of this grid.
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }

        /// <summary>
        /// Gets the metadata map for this grid.
        /// </summary>
        public MetaMap MetadataMap => _metadata;

        /// <summary>
        /// Insert or update a metadata field.
        /// </summary>
        public void InsertMeta(string name, Metadata.Metadata value)
        {
            _metadata.InsertMeta(name, value);
        }

        /// <summary>
        /// Insert or update a metadata field with a typed value.
        /// </summary>
        public void InsertMeta<T>(string name, T value) where T : notnull
        {
            _metadata.SetMetaValue(name, value);
        }

        /// <summary>
        /// Remove a metadata field.
        /// </summary>
        public void RemoveMeta(string name)
        {
            _metadata.RemoveMeta(name);
        }

        /// <summary>
        /// Get a metadata field by name.
        /// </summary>
        public Metadata.Metadata? GetMetadata(string name)
        {
            return _metadata[name];
        }

        /// <summary>
        /// Get a typed metadata value.
        /// </summary>
        public T MetaValue<T>(string name) where T : notnull
        {
            return _metadata.MetaValue<T>(name);
        }

        /// <summary>
        /// Clear all metadata.
        /// </summary>
        public void ClearMetadata()
        {
            _metadata.ClearMetadata();
        }

        /// <summary>
        /// Return the name of this grid's type.
        /// </summary>
        public abstract string GridType { get; }

        /// <summary>
        /// Return the name of the type of the tree held by this grid.
        /// </summary>
        public abstract string TreeType { get; }

        /// <summary>
        /// Return the name of the type of the values in this grid's tree.
        /// </summary>
        public abstract string ValueType { get; }

        // ========================================================================
        // GRID REGISTRY (Stubs for now - full implementation in Lot 5)
        // ========================================================================

        private static readonly Dictionary<string, Func<GridBase>> _gridRegistry = new();
        private static readonly object _registryLock = new();

        /// <summary>
        /// Register a grid type.
        /// </summary>
        public static void RegisterGrid(string gridType, Func<GridBase> factory)
        {
            lock (_registryLock)
            {
                _gridRegistry[gridType] = factory;
            }
        }

        /// <summary>
        /// Create a grid of the specified type.
        /// </summary>
        public static GridBase? CreateGrid(string gridType)
        {
            lock (_registryLock)
            {
                if (_gridRegistry.TryGetValue(gridType, out var factory))
                {
                    return factory();
                }
            }
            return null;
        }

        /// <summary>
        /// Clear the grid registry.
        /// </summary>
        public static void ClearRegistry()
        {
            lock (_registryLock)
            {
                _gridRegistry.Clear();
            }
        }

        /// <summary>
        /// Check if a grid type is registered.
        /// </summary>
        public static bool IsRegistered(string gridType)
        {
            lock (_registryLock)
            {
                return _gridRegistry.ContainsKey(gridType);
            }
        }

        // ========================================================================
        // TOPOLOGY OPERATIONS (Stub implementations for Lot 7A)
        // ========================================================================

        /// <summary>
        /// Union this grid's active voxel topology with another grid's.
        /// </summary>
        /// <param name="other">The other grid</param>
        /// <remarks>
        /// TODO: Full implementation requires tree-level topology merge
        /// </remarks>
        public virtual void TopologyUnion(GridBase other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            // TODO: Implement topology union at tree level
        }

        /// <summary>
        /// Intersect this grid's active voxel topology with another grid's.
        /// </summary>
        /// <param name="other">The other grid</param>
        /// <remarks>
        /// TODO: Full implementation requires tree-level topology merge
        /// </remarks>
        public virtual void TopologyIntersection(GridBase other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            // TODO: Implement topology intersection at tree level
        }

        /// <summary>
        /// Subtract another grid's active voxel topology from this grid's.
        /// </summary>
        /// <param name="other">The other grid</param>
        /// <remarks>
        /// TODO: Full implementation requires tree-level topology merge
        /// </remarks>
        public virtual void TopologyDifference(GridBase other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            // TODO: Implement topology difference at tree level
        }

        /// <summary>
        /// Return a deep copy of this grid.
        /// </summary>
        /// <returns>A new grid that is a copy of this grid</returns>
        public abstract GridBase DeepCopy();

        /// <summary>
        /// Return the grid class for this grid.
        /// </summary>
        public virtual GridClass GridClass { get; set; } = GridClass.Unknown;

        /// <summary>
        /// Get the transform for this grid.
        /// </summary>
        public virtual object? Transform { get; set; }
    }

    /// <summary>
    /// Grid classification enum
    /// </summary>
    public enum GridClass
    {
        /// <summary>Unknown grid type</summary>
        Unknown,
        /// <summary>Level set (narrow-band signed distance field)</summary>
        LevelSet,
        /// <summary>Fog volume (density field)</summary>
        FogVolume,
        /// <summary>Staggered vector field</summary>
        Staggered
    }

    /// <summary>
    /// Generic grid class with a tree and transform.
    /// </summary>
    /// <typeparam name="TValue">The value type for this grid.</typeparam>
    /// <remarks>
    /// Enhanced implementation for Lot 7A with basic topology support.
    /// Full tree integration will be completed with NodeManager/DynamicNodeManager.
    /// </remarks>
    public class Grid<TValue> : GridBase where TValue : struct
    {
        private TValue _background;
        private object? _tree;

        /// <summary>
        /// Initializes a new instance of the Grid class with default background.
        /// </summary>
        public Grid() : this(default(TValue))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Grid class with specified background.
        /// </summary>
        /// <param name="background">The background value</param>
        public Grid(TValue background)
        {
            _background = background;
            _tree = null;
        }

        /// <summary>
        /// Gets or sets the background value for this grid.
        /// </summary>
        public TValue Background
        {
            get => _background;
            set => _background = value;
        }

        /// <summary>
        /// Gets the grid type name.
        /// </summary>
        public override string GridType => $"Grid<{typeof(TValue).Name}>";

        /// <summary>
        /// Gets the tree type name.
        /// </summary>
        public override string TreeType => $"Tree<{typeof(TValue).Name}>";

        /// <summary>
        /// Gets the value type name.
        /// </summary>
        public override string ValueType => typeof(TValue).Name;

        /// <summary>
        /// Return a deep copy of this grid.
        /// </summary>
        /// <returns>A new grid that is a copy of this grid</returns>
        public override GridBase DeepCopy()
        {
            var copy = new Grid<TValue>(_background)
            {
                Name = this.Name,
                GridClass = this.GridClass,
                Transform = this.Transform
            };
            
            // TODO: Copy tree structure when tree implementation is complete
            // TODO: Copy metadata
            
            return copy;
        }

        // Tree-related functionality will be implemented with NodeManager/DynamicNodeManager
        // public TreeBase Tree { get; set; }
    }
}
