// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Platform.cs - C# port of Platform.h and Platform.cc
//
// This file provides platform-specific definitions and utilities for the OpenVDB C# port.
// Since C# has better cross-platform support through .NET, many C++ preprocessor macros
// are replaced with runtime checks, attributes, or are simply not needed.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenVDB.Platform
{
    /// <summary>
    /// Platform configuration and utilities for OpenVDB.
    /// </summary>
    public static class PlatformConfig
    {
        /// <summary>
        /// OpenVDB version namespace compatibility.
        /// In C++, this is managed via macros. In C#, we use a constant.
        /// </summary>
        public const string VersionName = "v11_0_0";
    }

    /// <summary>
    /// DLL import/export attributes for interop scenarios.
    /// In C#, DLL visibility is managed differently than in C++.
    /// Public types are automatically visible when the assembly is referenced.
    /// </summary>
    /// <remarks>
    /// The OPENVDB_API, OPENVDB_EXPORT, OPENVDB_IMPORT macros from C++
    /// are not needed in C# as the language has built-in support for assemblies and visibility.
    /// </remarks>
    public static class InteropHelpers
    {
        /// <summary>
        /// Check if the current assembly is being used as a DLL.
        /// </summary>
        public static bool IsDynamicLibrary => true; // In .NET, everything is a DLL

        /// <summary>
        /// Helper method to suppress compiler warnings about unused code.
        /// Equivalent to the various warning suppression macros in C++.
        /// </summary>
        /// <param name="value">The value that might be unused</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SuppressUnusedWarning<T>(T value)
        {
            // Intentionally empty - just prevents unused variable warnings
        }
    }

    /// <summary>
    /// Thread-safety markers for static access patterns.
    /// In C++, these are ICC-specific pragmas. In C#, we use attributes for documentation.
    /// </summary>
    /// <remarks>
    /// C# handles thread-safety differently than C++. The .NET runtime and JIT
    /// provide memory model guarantees. These attributes are for documentation purposes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
                    Inherited = false, AllowMultiple = false)]
    public sealed class ThreadSafeStaticAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of access (e.g., "reference", "write", "address").
        /// </summary>
        public string? AccessType { get; set; }

        /// <summary>
        /// Initializes a new instance of the ThreadSafeStaticAttribute class.
        /// </summary>
        /// <param name="accessType">Optional access type description</param>
        public ThreadSafeStaticAttribute(string? accessType = null)
        {
            AccessType = accessType;
        }
    }

    /// <summary>
    /// Indicates that a static member has non-thread-safe access patterns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method,
                    Inherited = false, AllowMultiple = false)]
    public sealed class NonThreadSafeStaticAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type of access (e.g., "reference", "write", "address").
        /// </summary>
        public string? AccessType { get; set; }

        /// <summary>
        /// Initializes a new instance of the NonThreadSafeStaticAttribute class.
        /// </summary>
        /// <param name="accessType">Optional access type description</param>
        public NonThreadSafeStaticAttribute(string? accessType = null)
        {
            AccessType = accessType;
        }
    }

    /// <summary>
    /// Platform initialization helpers.
    /// Equivalent to the includes and initialization in Platform.cc
    /// </summary>
    public static class PlatformInit
    {
        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        /// <summary>
        /// Initialize platform-specific settings.
        /// This is called automatically but can be called explicitly if needed.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            lock (_initLock)
            {
                if (_initialized)
                    return;

                // In C++, Platform.cc includes openvdb.h and Exceptions.h
                // In C#, we ensure those types are loaded by triggering static constructors
                // This will be done by the main OpenVDB initialization

                _initialized = true;
            }
        }

        /// <summary>
        /// Check if platform has been initialized.
        /// </summary>
        public static bool IsInitialized => _initialized;
    }
}
