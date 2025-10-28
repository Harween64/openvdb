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

        /// <summary>
        /// Check if running on Windows platform.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Check if running on Linux platform.
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Check if running on macOS platform.
        /// </summary>
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Check if running on 64-bit architecture.
        /// </summary>
        public static bool Is64Bit => Environment.Is64BitProcess;
    }

    /// <summary>
    /// Preprocessor utilities for string operations.
    /// In C#, these are implemented as methods rather than macros.
    /// </summary>
    public static class PreprocessorUtils
    {
        /// <summary>
        /// Convert a value to string. Equivalent to OPENVDB_PREPROC_STRINGIFY.
        /// </summary>
        /// <param name="value">The value to stringify</param>
        /// <returns>String representation of the value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Stringify<T>(T value) => value?.ToString() ?? string.Empty;

        /// <summary>
        /// Concatenate two strings. Equivalent to OPENVDB_PREPROC_CONCAT.
        /// </summary>
        /// <param name="x">First string</param>
        /// <param name="y">Second string</param>
        /// <returns>Concatenated string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Concat(string x, string y) => x + y;
    }

    /// <summary>
    /// Branch prediction hints for the JIT compiler.
    /// Equivalent to OPENVDB_LIKELY and OPENVDB_UNLIKELY macros in C++.
    /// Note: .NET JIT already does branch prediction, but these can help in critical paths.
    /// </summary>
    public static class BranchHints
    {
        /// <summary>
        /// Hint that the condition is likely to be true.
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The condition value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Likely(bool condition) => condition;

        /// <summary>
        /// Hint that the condition is unlikely to be true.
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The condition value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unlikely(bool condition) => condition;
    }

    /// <summary>
    /// Attribute to mark methods that should be aggressively inlined.
    /// Equivalent to OPENVDB_FORCE_INLINE in C++.
    /// </summary>
    /// <remarks>
    /// Use this sparingly on performance-critical methods.
    /// The .NET JIT compiler is already quite good at inlining decisions.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ForceInlineAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ForceInlineAttribute class.
        /// </summary>
        public ForceInlineAttribute() { }
    }

    /// <summary>
    /// Attribute to mark deprecated APIs.
    /// Equivalent to OPENVDB_DEPRECATED and OPENVDB_DEPRECATED_MESSAGE in C++.
    /// </summary>
    /// <remarks>
    /// In C#, we use the standard [Obsolete] attribute instead.
    /// This class is provided for documentation purposes.
    /// </remarks>
    public static class DeprecationHelpers
    {
        /// <summary>
        /// Create an Obsolete attribute for deprecated APIs.
        /// </summary>
        /// <param name="message">The deprecation message</param>
        /// <returns>An ObsoleteAttribute instance</returns>
        public static ObsoleteAttribute CreateDeprecationAttribute(string message)
            => new ObsoleteAttribute(message, error: false);

        /// <summary>
        /// Create an Obsolete attribute for deprecated APIs that will cause compilation errors.
        /// </summary>
        /// <param name="message">The deprecation message</param>
        /// <returns>An ObsoleteAttribute instance</returns>
        public static ObsoleteAttribute CreateDeprecationError(string message)
            => new ObsoleteAttribute(message, error: true);
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
