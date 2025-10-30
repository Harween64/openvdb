// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Vec3.cs - C# port of Vec3.h
//
// 3D vector class

using System;
using System.Runtime.CompilerServices;

namespace OpenVDB.Math
{
    /// <summary>
    /// A 3D vector class
    /// </summary>
    /// <typeparam name="T">The value type of the vector</typeparam>
    public struct Vec3<T> : IEquatable<Vec3<T>>, IComparable<Vec3<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _x, _y, _z;

        /// <summary>
        /// X component
        /// </summary>
        public T X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _x;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _x = value;
        }

        /// <summary>
        /// Y component
        /// </summary>
        public T Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _y = value;
        }

        /// <summary>
        /// Z component
        /// </summary>
        public T Z
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _z = value;
        }

        /// <summary>
        /// Construct a vector all of whose components have the given value.
        /// </summary>
        public Vec3(T val)
        {
            _x = _y = _z = val;
        }

        /// <summary>
        /// Constructor with three arguments
        /// </summary>
        public Vec3(T x, T y, T z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Indexer for accessing components
        /// </summary>
        public T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => i switch
            {
                0 => _x,
                1 => _y,
                2 => _z,
                _ => throw new IndexOutOfRangeException($"Index {i} is out of range for Vec3")
            };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (i)
                {
                    case 0: _x = value; break;
                    case 1: _y = value; break;
                    case 2: _z = value; break;
                    default: throw new IndexOutOfRangeException($"Index {i} is out of range for Vec3");
                }
            }
        }

        /// <summary>
        /// Initialize this vector to [x, y, z]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(T x, T y, T z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Set this vector to zero
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetZero()
        {
            _x = _y = _z = default;
        }

        /// <summary>
        /// Test if this vector is equivalent to vector v with tolerance
        /// </summary>
        public bool Eq(Vec3<T> v, T eps)
        {
            if (typeof(T) == typeof(float))
            {
                float x1 = (float)(object)_x, x2 = (float)(object)v._x;
                float y1 = (float)(object)_y, y2 = (float)(object)v._y;
                float z1 = (float)(object)_z, z2 = (float)(object)v._z;
                float e = (float)(object)eps;
                return MathUtil.IsRelOrApproxEqual(x1, x2, e, e) &&
                       MathUtil.IsRelOrApproxEqual(y1, y2, e, e) &&
                       MathUtil.IsRelOrApproxEqual(z1, z2, e, e);
            }
            if (typeof(T) == typeof(double))
            {
                double x1 = (double)(object)_x, x2 = (double)(object)v._x;
                double y1 = (double)(object)_y, y2 = (double)(object)v._y;
                double z1 = (double)(object)_z, z2 = (double)(object)v._z;
                double e = (double)(object)eps;
                return MathUtil.IsRelOrApproxEqual(x1, x2, e, e) &&
                       MathUtil.IsRelOrApproxEqual(y1, y2, e, e) &&
                       MathUtil.IsRelOrApproxEqual(z1, z2, e, e);
            }
            return _x.Equals(v._x) && _y.Equals(v._y) && _z.Equals(v._z);
        }

        /// <summary>
        /// Dot product
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dot(Vec3<T> v)
        {
            if (typeof(T) == typeof(float))
            {
                float x1 = (float)(object)_x, x2 = (float)(object)v._x;
                float y1 = (float)(object)_y, y2 = (float)(object)v._y;
                float z1 = (float)(object)_z, z2 = (float)(object)v._z;
                return (T)(object)(x1 * x2 + y1 * y2 + z1 * z2);
            }
            if (typeof(T) == typeof(double))
            {
                double x1 = (double)(object)_x, x2 = (double)(object)v._x;
                double y1 = (double)(object)_y, y2 = (double)(object)v._y;
                double z1 = (double)(object)_z, z2 = (double)(object)v._z;
                return (T)(object)(x1 * x2 + y1 * y2 + z1 * z2);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Dot.");
        }

        /// <summary>
        /// Length of the vector
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Length()
        {
            if (typeof(T) == typeof(float))
            {
                float x = (float)(object)_x;
                float y = (float)(object)_y;
                float z = (float)(object)_z;
                return (T)(object)MathF.Sqrt(x * x + y * y + z * z);
            }
            if (typeof(T) == typeof(double))
            {
                double x = (double)(object)_x;
                double y = (double)(object)_y;
                double z = (double)(object)_z;
                return (T)(object)System.Math.Sqrt(x * x + y * y + z * z);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Length.");
        }

        /// <summary>
        /// Squared length of the vector
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T LengthSqr()
        {
            if (typeof(T) == typeof(float))
            {
                float x = (float)(object)_x;
                float y = (float)(object)_y;
                float z = (float)(object)_z;
                return (T)(object)(x * x + y * y + z * z);
            }
            if (typeof(T) == typeof(double))
            {
                double x = (double)(object)_x;
                double y = (double)(object)_y;
                double z = (double)(object)_z;
                return (T)(object)(x * x + y * y + z * z);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for LengthSqr.");
        }

        /// <summary>
        /// Return the cross product of this vector and v
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec3<T> Cross(Vec3<T> v)
        {
            if (typeof(T) == typeof(float))
            {
                float x1 = (float)(object)_x, x2 = (float)(object)v._x;
                float y1 = (float)(object)_y, y2 = (float)(object)v._y;
                float z1 = (float)(object)_z, z2 = (float)(object)v._z;
                return new Vec3<T>(
                    (T)(object)(y1 * z2 - z1 * y2),
                    (T)(object)(z1 * x2 - x1 * z2),
                    (T)(object)(x1 * y2 - y1 * x2)
                );
            }
            if (typeof(T) == typeof(double))
            {
                double x1 = (double)(object)_x, x2 = (double)(object)v._x;
                double y1 = (double)(object)_y, y2 = (double)(object)v._y;
                double z1 = (double)(object)_z, z2 = (double)(object)v._z;
                return new Vec3<T>(
                    (T)(object)(y1 * z2 - z1 * y2),
                    (T)(object)(z1 * x2 - x1 * z2),
                    (T)(object)(x1 * y2 - y1 * x2)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Cross.");
        }

        /// <summary>
        /// Normalize this vector
        /// </summary>
        public T Normalize(T eps)
        {
            T len = Length();
            if (typeof(T) == typeof(float))
            {
                float l = (float)(object)len;
                float e = (float)(object)eps;
                if (MathUtil.IsApproxZero(l, e))
                {
                    SetZero();
                    return default;
                }
                float inv = 1.0f / l;
                _x = (T)(object)((float)(object)_x * inv);
                _y = (T)(object)((float)(object)_y * inv);
                _z = (T)(object)((float)(object)_z * inv);
                return len;
            }
            if (typeof(T) == typeof(double))
            {
                double l = (double)(object)len;
                double e = (double)(object)eps;
                if (MathUtil.IsApproxZero(l, e))
                {
                    SetZero();
                    return default;
                }
                double inv = 1.0 / l;
                _x = (T)(object)((double)(object)_x * inv);
                _y = (T)(object)((double)(object)_y * inv);
                _z = (T)(object)((double)(object)_z * inv);
                return len;
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Normalize.");
        }

        // Operators

        /// <summary>
        /// Negation operator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator -(Vec3<T> v)
        {
            if (typeof(T) == typeof(float))
            {
                return new Vec3<T>(
                    (T)(object)(-(float)(object)v._x),
                    (T)(object)(-(float)(object)v._y),
                    (T)(object)(-(float)(object)v._z)
                );
            }
            if (typeof(T) == typeof(double))
            {
                return new Vec3<T>(
                    (T)(object)(-(double)(object)v._x),
                    (T)(object)(-(double)(object)v._y),
                    (T)(object)(-(double)(object)v._z)
                );
            }
            if (typeof(T) == typeof(int))
            {
                return new Vec3<T>(
                    (T)(object)(-(int)(object)v._x),
                    (T)(object)(-(int)(object)v._y),
                    (T)(object)(-(int)(object)v._z)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for negation.");
        }

        /// <summary>
        /// Addition operator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator +(Vec3<T> v1, Vec3<T> v2)
        {
            if (typeof(T) == typeof(float))
            {
                return new Vec3<T>(
                    (T)(object)((float)(object)v1._x + (float)(object)v2._x),
                    (T)(object)((float)(object)v1._y + (float)(object)v2._y),
                    (T)(object)((float)(object)v1._z + (float)(object)v2._z)
                );
            }
            if (typeof(T) == typeof(double))
            {
                return new Vec3<T>(
                    (T)(object)((double)(object)v1._x + (double)(object)v2._x),
                    (T)(object)((double)(object)v1._y + (double)(object)v2._y),
                    (T)(object)((double)(object)v1._z + (double)(object)v2._z)
                );
            }
            if (typeof(T) == typeof(int))
            {
                return new Vec3<T>(
                    (T)(object)((int)(object)v1._x + (int)(object)v2._x),
                    (T)(object)((int)(object)v1._y + (int)(object)v2._y),
                    (T)(object)((int)(object)v1._z + (int)(object)v2._z)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for addition.");
        }

        /// <summary>
        /// Subtraction operator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator -(Vec3<T> v1, Vec3<T> v2)
        {
            if (typeof(T) == typeof(float))
            {
                return new Vec3<T>(
                    (T)(object)((float)(object)v1._x - (float)(object)v2._x),
                    (T)(object)((float)(object)v1._y - (float)(object)v2._y),
                    (T)(object)((float)(object)v1._z - (float)(object)v2._z)
                );
            }
            if (typeof(T) == typeof(double))
            {
                return new Vec3<T>(
                    (T)(object)((double)(object)v1._x - (double)(object)v2._x),
                    (T)(object)((double)(object)v1._y - (double)(object)v2._y),
                    (T)(object)((double)(object)v1._z - (double)(object)v2._z)
                );
            }
            if (typeof(T) == typeof(int))
            {
                return new Vec3<T>(
                    (T)(object)((int)(object)v1._x - (int)(object)v2._x),
                    (T)(object)((int)(object)v1._y - (int)(object)v2._y),
                    (T)(object)((int)(object)v1._z - (int)(object)v2._z)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for subtraction.");
        }

        /// <summary>
        /// Scalar multiplication operator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator *(Vec3<T> v, T scalar)
        {
            if (typeof(T) == typeof(float))
            {
                float s = (float)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((float)(object)v._x * s),
                    (T)(object)((float)(object)v._y * s),
                    (T)(object)((float)(object)v._z * s)
                );
            }
            if (typeof(T) == typeof(double))
            {
                double s = (double)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((double)(object)v._x * s),
                    (T)(object)((double)(object)v._y * s),
                    (T)(object)((double)(object)v._z * s)
                );
            }
            if (typeof(T) == typeof(int))
            {
                int s = (int)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((int)(object)v._x * s),
                    (T)(object)((int)(object)v._y * s),
                    (T)(object)((int)(object)v._z * s)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for multiplication.");
        }

        /// <summary>
        /// Scalar multiplication operator (scalar * vector)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator *(T scalar, Vec3<T> v) => v * scalar;

        /// <summary>
        /// Scalar division operator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3<T> operator /(Vec3<T> v, T scalar)
        {
            if (typeof(T) == typeof(float))
            {
                float s = (float)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((float)(object)v._x / s),
                    (T)(object)((float)(object)v._y / s),
                    (T)(object)((float)(object)v._z / s)
                );
            }
            if (typeof(T) == typeof(double))
            {
                double s = (double)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((double)(object)v._x / s),
                    (T)(object)((double)(object)v._y / s),
                    (T)(object)((double)(object)v._z / s)
                );
            }
            if (typeof(T) == typeof(int))
            {
                int s = (int)(object)scalar;
                return new Vec3<T>(
                    (T)(object)((int)(object)v._x / s),
                    (T)(object)((int)(object)v._y / s),
                    (T)(object)((int)(object)v._z / s)
                );
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for division.");
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public bool Equals(Vec3<T> other)
        {
            return _x.Equals(other._x) && _y.Equals(other._y) && _z.Equals(other._z);
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is Vec3<T> other && Equals(other);
        }

        /// <summary>
        /// Hash code
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(_x, _y, _z);
        }

        /// <summary>
        /// Comparison for ordering
        /// </summary>
        public int CompareTo(Vec3<T> other)
        {
            int cmp = _x.CompareTo(other._x);
            if (cmp != 0) return cmp;
            cmp = _y.CompareTo(other._y);
            if (cmp != 0) return cmp;
            return _z.CompareTo(other._z);
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return $"[{_x}, {_y}, {_z}]";
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(Vec3<T> left, Vec3<T> right) => left.Equals(right);

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Vec3<T> left, Vec3<T> right) => !left.Equals(right);

        /// <summary>
        /// Static zero vector
        /// </summary>
        public static Vec3<T> Zero => new Vec3<T>(default);

        /// <summary>
        /// Static unit X vector
        /// </summary>
        public static Vec3<T> UnitX
        {
            get
            {
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Vec3<T>(one, default, default);
            }
        }

        /// <summary>
        /// Static unit Y vector
        /// </summary>
        public static Vec3<T> UnitY
        {
            get
            {
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Vec3<T>(default, one, default);
            }
        }

        /// <summary>
        /// Static unit Z vector
        /// </summary>
        public static Vec3<T> UnitZ
        {
            get
            {
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Vec3<T>(default, default, one);
            }
        }
    }
}
