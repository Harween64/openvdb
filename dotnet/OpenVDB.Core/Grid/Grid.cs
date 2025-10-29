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
    }

    /// <summary>
    /// Generic grid class (stub for Lot 1).
    /// </summary>
    /// <typeparam name="TTree">The tree type for this grid.</typeparam>
    /// <remarks>
    /// This is a minimal stub. Full implementation will be provided in Lot 5 (Tree System).
    /// </remarks>
    public class Grid<TTree> : GridBase
    {
        /// <summary>
        /// Initializes a new instance of the Grid class.
        /// </summary>
        public Grid()
        {
        }

        /// <summary>
        /// Gets the grid type name.
        /// </summary>
        public override string GridType => $"Grid<{typeof(TTree).Name}>";

        /// <summary>
        /// Gets the tree type name.
        /// </summary>
        public override string TreeType => typeof(TTree).Name;

        /// <summary>
        /// Gets the value type name (stub - will be determined from tree in Lot 5).
        /// </summary>
        public override string ValueType => "unknown";

        // Tree-related functionality will be implemented in Lot 5
        // public TTree Tree { get; }
        // public Transform Transform { get; }
    }
}
