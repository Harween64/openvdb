// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// OpenVDB.cs - C# port of openvdb.h and openvdb.cc
//
// This file provides the main initialization and registration functions for OpenVDB,
// as well as common type definitions.

using System;
using System.Threading;

namespace OpenVDB
{
    /// <summary>
    /// Main OpenVDB library initialization and version information.
    /// </summary>
    public static class OpenVDB
    {
        private static int _isInitialized = 0;
        private static readonly object _initLock = new();

        /// <summary>
        /// Gets the OpenVDB library version name.
        /// </summary>
        public const string VersionName = "v11_0_0";

        /// <summary>
        /// Gets the OpenVDB library major version.
        /// </summary>
        public const int VersionMajor = 11;

        /// <summary>
        /// Gets the OpenVDB library minor version.
        /// </summary>
        public const int VersionMinor = 0;

        /// <summary>
        /// Gets the OpenVDB library patch version.
        /// </summary>
        public const int VersionPatch = 0;

        /// <summary>
        /// Gets the OpenVDB ABI version.
        /// </summary>
        /// <remarks>
        /// The ABI version is used to ensure binary compatibility between different
        /// versions of the library.
        /// </remarks>
        public const int AbiVersion = 11;

        /// <summary>
        /// Gets a value indicating whether the library has been initialized.
        /// </summary>
        public static bool IsInitialized => Interlocked.CompareExchange(ref _isInitialized, 0, 0) == 1;

        /// <summary>
        /// Global registration of native Grid, Transform, Metadata types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Calling this method registers factory callbacks for the set of native grid,
        /// transform, and metadata types that OpenVDB supports by default.
        /// </para>
        /// <para>
        /// For most types, calling OpenVDB.Initialize() is only required for serialization
        /// support. However, Initialize() must be called for PointDataGrid attribute usage
        /// as these callbacks are used in various tools.
        /// </para>
        /// <para>
        /// This method is thread-safe - it can be concurrently called multiple times,
        /// exiting early if it has already been called (unless Uninitialize() has been called).
        /// </para>
        /// </remarks>
        public static void Initialize()
        {
            // Fast path: check if already initialized without locking
            if (Interlocked.CompareExchange(ref _isInitialized, 0, 0) == 1)
                return;

            lock (_initLock)
            {
                // Double-checked locking pattern
                if (Interlocked.CompareExchange(ref _isInitialized, 0, 0) == 1)
                    return;

                // Initialize logging (to be implemented in Lot 3: Utilities)
                // Logging.Initialize();

                // Register metadata types (to be implemented in Lot 1C: Metadata)
                // Metadata.ClearRegistry();
                // RegisterMetadataTypes();

                // Register map types (to be implemented in Lot 2: Mathematics)
                // MapRegistry.Clear();
                // RegisterMapTypes();

                // Register common grid types (to be implemented in Lot 5: Tree & Grid)
                // GridBase.ClearRegistry();
                // RegisterGridTypes();

                // Register point-related types (to be implemented in Lot 8: Points)
                // PointsInternal.Initialize();

                // Note: Blosc compression initialization would go here if implemented
                // In C#, we might use a different compression library

                // Mark as initialized
                Interlocked.Exchange(ref _isInitialized, 1);
            }
        }

        /// <summary>
        /// Global deregistration of native Grid, Transform, and Metadata types.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clears all registered factory callbacks. This includes everything registered
        /// by Initialize() but will also include any manually registered types.
        /// </para>
        /// <para>
        /// This method is thread-safe - it can be concurrently called multiple times.
        /// </para>
        /// </remarks>
        public static void Uninitialize()
        {
            lock (_initLock)
            {
                // Mark as uninitialized
                Interlocked.Exchange(ref _isInitialized, 0);

                // Clear registries (to be implemented when those systems are ported)
                // Metadata.ClearRegistry();
                // GridBase.ClearRegistry();
                // MapRegistry.Clear();
                // PointsInternal.Uninitialize();

                // Note: Unlike C++ version, we don't need to worry about Blosc cleanup
                // as we'll use managed compression libraries
            }
        }

        /// <summary>
        /// Gets the full version string.
        /// </summary>
        /// <returns>A string containing the version information.</returns>
        public static string GetVersionString()
        {
            return $"OpenVDB {VersionMajor}.{VersionMinor}.{VersionPatch} (ABI {AbiVersion})";
        }
    }

    // ========================================================================
    // PLACEHOLDER TYPES (to be fully implemented in later lots)
    // ========================================================================

    // Note: These are minimal placeholders. They will be fully implemented
    // in their respective lots according to the porting plan.

    // Lot 2: Mathematics - Transform types will be defined here
    // Lot 5: Tree - Tree types will be defined here  
    // Lot 5: Grid - Grid types will be defined here
    // Lot 6: IO - File and Stream types will be defined here
    // Lot 8: Points - PointDataGrid will be defined here
}
