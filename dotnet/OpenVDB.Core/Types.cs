// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Types.cs - C# port of Types.h
//
// This file defines common type aliases and fundamental types for OpenVDB.
// Note: Mathematical types (Vec2, Vec3, Mat3, etc.) are defined in the Math namespace
// and will be fully implemented in Phase 1, Lot 2 (Mathematics).

// Type aliases - C# global using directives (must come first)
global using Index32 = System.UInt32;
global using Index64 = System.UInt64;
global using Index = System.UInt32;
global using Int16 = System.Int16;
global using Int32 = System.Int32;
global using Int64 = System.Int64;
global using Int = System.Int32;
global using Byte = System.Byte;
global using Real = System.Double;

using System;

namespace OpenVDB
{

    // ========================================================================
    // SPECIAL VALUE TYPES
    // ========================================================================

    /// <summary>
    /// Dummy type for a voxel with a binary mask value (e.g., the active state).
    /// </summary>
    /// <remarks>
    /// In C++, this is an empty class. In C#, we use a struct for value semantics.
    /// </remarks>
    public struct ValueMask : IEquatable<ValueMask>
    {
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(ValueMask other) => true;

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj) => obj is ValueMask;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode() => 0;

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(ValueMask left, ValueMask right) => true;

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(ValueMask left, ValueMask right) => false;
    }

    // ========================================================================
    // POINT INDEX TYPES
    // ========================================================================

    /// <summary>
    /// Integer wrapper, required to distinguish PointIndexGrid and PointDataGrid
    /// from Int32Grid and Int64Grid.
    /// </summary>
    /// <typeparam name="TInt">The underlying integer type.</typeparam>
    /// <typeparam name="Kind">A discriminator to create distinct types.</typeparam>
    /// <remarks>
    /// The Kind parameter is a dummy used to create distinct types even when
    /// TInt is the same.
    /// </remarks>
    public class PointIndex<TInt, Kind> : IEquatable<PointIndex<TInt, Kind>>
        where TInt : struct, IEquatable<TInt>, IComparable<TInt>
        where Kind : struct
    {
        private readonly TInt _index;

        /// <summary>
        /// Initializes a new instance of the PointIndex struct.
        /// </summary>
        public PointIndex(TInt index)
        {
            _index = index;
        }

        /// <summary>
        /// Gets the underlying index value.
        /// </summary>
        public TInt Value => _index;

        /// <summary>
        /// Implicit conversion to the underlying integer type.
        /// </summary>
        public static implicit operator TInt(PointIndex<TInt, Kind> pointIndex) => pointIndex._index;

        /// <summary>
        /// Explicit conversion from the underlying integer type.
        /// </summary>
        public static explicit operator PointIndex<TInt, Kind>(TInt value) => new(value);

        /// <summary>
        /// Determines whether two point indices are equal.
        /// </summary>
        public bool Equals(PointIndex<TInt, Kind>? other)
        {
            return other != null && _index.Equals(other._index);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj) => obj is PointIndex<TInt, Kind> other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode() => _index.GetHashCode();

        /// <summary>
        /// Returns a string representation of this point index.
        /// </summary>
        public override string ToString() => _index.ToString() ?? "";

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(PointIndex<TInt, Kind>? left, PointIndex<TInt, Kind>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(PointIndex<TInt, Kind>? left, PointIndex<TInt, Kind>? right) => !(left == right);
    }

    // ========================================================================
    // TYPE DISCRIMINATORS
    // ========================================================================

    /// <summary>Discriminator for PointIndex types.</summary>
    public readonly struct PointIndexKind { }
    
    /// <summary>Discriminator for PointDataIndex types.</summary>
    public readonly struct PointDataIndexKind { }

    // ========================================================================
    // POINT INDEX TYPE ALIASES
    // ========================================================================

    /// <summary>32-bit point index.</summary>
    public sealed class PointIndex32 : PointIndex<uint, PointIndexKind>
    {
        /// <summary>Creates a new PointIndex32.</summary>
        public PointIndex32(uint index) : base(index) { }
    }
    
    /// <summary>64-bit point index.</summary>
    public sealed class PointIndex64 : PointIndex<ulong, PointIndexKind>
    {
        /// <summary>Creates a new PointIndex64.</summary>
        public PointIndex64(ulong index) : base(index) { }
    }
    
    /// <summary>32-bit point data index.</summary>
    public sealed class PointDataIndex32 : PointIndex<uint, PointDataIndexKind>
    {
        /// <summary>Creates a new PointDataIndex32.</summary>
        public PointDataIndex32(uint index) : base(index) { }
    }
    
    /// <summary>64-bit point data index.</summary>
    public sealed class PointDataIndex64 : PointIndex<ulong, PointDataIndexKind>
    {
        /// <summary>Creates a new PointDataIndex64.</summary>
        public PointDataIndex64(ulong index) : base(index) { }
    }

    // ========================================================================
    // COPY SEMANTICS MARKERS
    // ========================================================================

    /// <summary>
    /// Tag class to specify shallow copy semantics.
    /// </summary>
    public sealed class ShallowCopy { private ShallowCopy() { } }

    /// <summary>
    /// Tag class to specify topology-only copy semantics.
    /// </summary>
    public sealed class TopologyCopy { private TopologyCopy() { } }

    /// <summary>
    /// Tag class to specify deep copy semantics.
    /// </summary>
    public sealed class DeepCopy { private DeepCopy() { } }

    /// <summary>
    /// Tag class to specify move/steal semantics (transferring ownership).
    /// </summary>
    public sealed class Steal { private Steal() { } }

    /// <summary>
    /// Tag class to specify partial creation semantics.
    /// </summary>
    public sealed class PartialCreate { private PartialCreate() { } }

    // ========================================================================
    // UTILITY FUNCTIONS
    // ========================================================================

    /// <summary>
    /// Utility class for type operations.
    /// </summary>
    public static class TypeUtility
    {
        /// <summary>
        /// Returns a zero value for the given type.
        /// </summary>
        /// <typeparam name="T">The type to get a zero value for.</typeparam>
        /// <returns>The zero/default value for the type.</returns>
        public static T ZeroValue<T>() where T : struct => default;

        /// <summary>
        /// Returns the type name as a string.
        /// </summary>
        /// <typeparam name="T">The type to get the name of.</typeparam>
        /// <returns>The full name of the type.</returns>
        public static string TypeNameAsString<T>() => typeof(T).FullName ?? typeof(T).Name;
    }
}
