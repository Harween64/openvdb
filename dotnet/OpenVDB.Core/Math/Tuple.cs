// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Tuple.cs - C# port of Tuple.h
//
// A base class for homogenous tuple types

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenVDB.Math
{
    /// <summary>
    /// A base class for homogenous tuple types
    /// </summary>
    /// <typeparam name="T">The value type of the tuple</typeparam>
    public class Tuple<T> : IEquatable<Tuple<T>>, IComparable<Tuple<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        /// <summary>
        /// Internal storage array for tuple elements
        /// </summary>
        protected T[] _mm;

        /// <summary>
        /// The size of the tuple
        /// </summary>
        public int Size => _mm.Length;

        /// <summary>
        /// Trivial constructor, the Tuple is NOT initialized
        /// </summary>
        /// <param name="size">The size of the tuple</param>
        internal Tuple(int size)
        {
            _mm = new T[size];
        }

        /// <summary>
        /// Conversion constructor.
        /// Tuples with different value types and different sizes can be
        /// interconverted using this member. Converting from a larger tuple
        /// results in truncation; converting from a smaller tuple results in
        /// the extra data members being zeroed out.
        /// </summary>
        /// <param name="src">Source tuple</param>
        protected Tuple(Tuple<T> src, int targetSize)
        {
            _mm = new T[targetSize];
            int copyEnd = System.Math.Min(targetSize, src.Size);
            for (int i = 0; i < copyEnd; ++i)
            {
                _mm[i] = src[i];
            }
            // Remaining elements are already zero-initialized in C#
        }

        /// <summary>
        /// Const access to an element in the tuple.
        /// </summary>
        /// <param name="i">Index of the element</param>
        /// <returns>Copy of the tuple data at index i</returns>
        public T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _mm[i];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _mm[i] = value;
        }

        /// <summary>
        /// Copies this tuple into an array of a compatible type
        /// </summary>
        /// <param name="v">Target array</param>
        public void ToArray<S>(S[] v) where S : struct
        {
            for (int i = 0; i < Size; ++i)
            {
                v[i] = (S)Convert.ChangeType(_mm[i], typeof(S));
            }
        }

        /// <summary>
        /// Exposes the internal array. Be careful when using this function.
        /// </summary>
        /// <returns>The internal array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsArray()
        {
            return _mm;
        }

        /// <summary>
        /// Return string representation of the tuple
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append('[');
            for (int j = 0; j < Size; j++)
            {
                if (j > 0) buffer.Append(", ");
                buffer.Append(_mm[j]);
            }
            buffer.Append(']');
            return buffer.ToString();
        }

        /// <summary>
        /// True if a NaN is present in this tuple
        /// </summary>
        /// <returns>True if any element is NaN</returns>
        public bool IsNan()
        {
            if (typeof(T) == typeof(float))
            {
                for (int i = 0; i < Size; ++i)
                {
                    if (float.IsNaN((float)(object)_mm[i]))
                        return true;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                for (int i = 0; i < Size; ++i)
                {
                    if (double.IsNaN((double)(object)_mm[i]))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// True if an Inf is present in this tuple
        /// </summary>
        /// <returns>True if any element is infinite</returns>
        public bool IsInfinite()
        {
            if (typeof(T) == typeof(float))
            {
                for (int i = 0; i < Size; ++i)
                {
                    if (float.IsInfinity((float)(object)_mm[i]))
                        return true;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                for (int i = 0; i < Size; ++i)
                {
                    if (double.IsInfinity((double)(object)_mm[i]))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// True if no NaN or Inf values are present
        /// </summary>
        /// <returns>True if all elements are finite</returns>
        public bool IsFinite()
        {
            if (typeof(T) == typeof(float))
            {
                for (int i = 0; i < Size; ++i)
                {
                    float val = (float)(object)_mm[i];
                    if (float.IsNaN(val) || float.IsInfinity(val))
                        return false;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                for (int i = 0; i < Size; ++i)
                {
                    double val = (double)(object)_mm[i];
                    if (double.IsNaN(val) || double.IsInfinity(val))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// True if all elements are exactly zero
        /// </summary>
        /// <returns>True if all elements are zero</returns>
        public bool IsZero()
        {
            T zero = default;
            for (int i = 0; i < Size; ++i)
            {
                if (!_mm[i].Equals(zero))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(Tuple<T>? other)
        {
            if (other == null || Size != other.Size)
                return false;

            for (int i = 0; i < Size; ++i)
            {
                if (!_mm[i].Equals(other._mm[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Tuple<T> other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (int i = 0; i < Size; ++i)
            {
                hash.Add(_mm[i]);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Compares this tuple to another, comparing components in order of significance.
        /// </summary>
        public int CompareTo(Tuple<T>? other)
        {
            if (other == null)
                return 1;

            int minSize = System.Math.Min(Size, other.Size);
            for (int i = 0; i < minSize; ++i)
            {
                int cmp = _mm[i].CompareTo(other._mm[i]);
                if (cmp != 0)
                    return cmp;
            }

            // If all compared elements are equal, the longer tuple is greater
            return Size.CompareTo(other.Size);
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Tuple<T>? left, Tuple<T>? right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Tuple<T>? left, Tuple<T>? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Less than operator
        /// </summary>
        public static bool operator <(Tuple<T>? left, Tuple<T>? right)
        {
            if (left is null)
                return right is not null;
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Greater than operator
        /// </summary>
        public static bool operator >(Tuple<T>? left, Tuple<T>? right)
        {
            if (right is null)
                return left is not null;
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Less than or equal operator
        /// </summary>
        public static bool operator <=(Tuple<T>? left, Tuple<T>? right)
        {
            if (left is null)
                return true;
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Greater than or equal operator
        /// </summary>
        public static bool operator >=(Tuple<T>? left, Tuple<T>? right)
        {
            if (right is null)
                return true;
            return left.CompareTo(right) >= 0;
        }
    }

    /// <summary>
    /// Utility functions for Tuple operations
    /// </summary>
    public static class TupleUtil
    {
        /// <summary>
        /// Return the absolute value of the given Tuple.
        /// </summary>
        public static Tuple<T> Abs<T>(Tuple<T> t) where T : struct, IEquatable<T>, IComparable<T>
        {
            if (typeof(T) == typeof(float))
            {
                var result = new Tuple<T>(t.Size);
                for (int i = 0; i < t.Size; ++i)
                {
                    float val = (float)(object)t[i];
                    result[i] = (T)(object)System.Math.Abs(val);
                }
                return result;
            }
            if (typeof(T) == typeof(double))
            {
                var result = new Tuple<T>(t.Size);
                for (int i = 0; i < t.Size; ++i)
                {
                    double val = (double)(object)t[i];
                    result[i] = (T)(object)System.Math.Abs(val);
                }
                return result;
            }
            if (typeof(T) == typeof(int))
            {
                var result = new Tuple<T>(t.Size);
                for (int i = 0; i < t.Size; ++i)
                {
                    int val = (int)(object)t[i];
                    result[i] = (T)(object)System.Math.Abs(val);
                }
                return result;
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Abs.");
        }

        /// <summary>
        /// Return true if a NaN is present in the tuple.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNan<T>(Tuple<T> t) where T : struct, IEquatable<T>, IComparable<T>
        {
            return t.IsNan();
        }

        /// <summary>
        /// Return true if an Inf is present in the tuple.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInfinite<T>(Tuple<T> t) where T : struct, IEquatable<T>, IComparable<T>
        {
            return t.IsInfinite();
        }

        /// <summary>
        /// Return true if no NaN or Inf values are present.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite<T>(Tuple<T> t) where T : struct, IEquatable<T>, IComparable<T>
        {
            return t.IsFinite();
        }

        /// <summary>
        /// Return true if all elements are exactly equal to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero<T>(Tuple<T> t) where T : struct, IEquatable<T>, IComparable<T>
        {
            return t.IsZero();
        }
    }
}
