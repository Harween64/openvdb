// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Coord.cs - C# port of Coord.h
//
// Signed (x, y, z) 32-bit integer coordinates

using System;
using System.Runtime.CompilerServices;

namespace OpenVDB.Math
{
    /// <summary>
    /// Signed (x, y, z) 32-bit integer coordinates
    /// </summary>
    public struct Coord : IEquatable<Coord>, IComparable<Coord>
    {
        private int _x, _y, _z;

        /// <summary>
        /// X coordinate
        /// </summary>
        public int X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _x;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _x = value;
        }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _y = value;
        }

        /// <summary>
        /// Z coordinate
        /// </summary>
        public int Z
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _z = value;
        }

        /// <summary>
        /// Default constructor - initializes to (0, 0, 0)
        /// </summary>
        public Coord()
        {
            _x = _y = _z = 0;
        }

        /// <summary>
        /// Construct with all coordinates set to the same value
        /// </summary>
        public Coord(int xyz)
        {
            _x = _y = _z = xyz;
        }

        /// <summary>
        /// Construct with explicit x, y, z coordinates
        /// </summary>
        public Coord(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Construct from Vec3&lt;int&gt;
        /// </summary>
        public Coord(Vec3<int> v)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
        }

        /// <summary>
        /// Return the smallest possible coordinate
        /// </summary>
        public static Coord Min => new Coord(int.MinValue);

        /// <summary>
        /// Return the largest possible coordinate
        /// </summary>
        public static Coord Max => new Coord(int.MaxValue);

        /// <summary>
        /// Return xyz rounded to the closest integer coordinates (cell centered conversion).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord Round<T>(Vec3<T> xyz) where T : struct, IEquatable<T>, IComparable<T>
        {
            if (typeof(T) == typeof(float))
            {
                Vec3<float> v = (Vec3<float>)(object)xyz;
                return new Coord(
                    MathUtil.Floor(v.X + 0.5f),
                    MathUtil.Floor(v.Y + 0.5f),
                    MathUtil.Floor(v.Z + 0.5f)
                );
            }
            if (typeof(T) == typeof(double))
            {
                Vec3<double> v = (Vec3<double>)(object)xyz;
                return new Coord(
                    MathUtil.Floor(v.X + 0.5),
                    MathUtil.Floor(v.Y + 0.5),
                    MathUtil.Floor(v.Z + 0.5)
                );
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Return the largest integer coordinates that are not greater than xyz (node centered conversion).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord Floor<T>(Vec3<T> xyz) where T : struct, IEquatable<T>, IComparable<T>
        {
            if (typeof(T) == typeof(float))
            {
                Vec3<float> v = (Vec3<float>)(object)xyz;
                return new Coord(MathUtil.Floor(v.X), MathUtil.Floor(v.Y), MathUtil.Floor(v.Z));
            }
            if (typeof(T) == typeof(double))
            {
                Vec3<double> v = (Vec3<double>)(object)xyz;
                return new Coord(MathUtil.Floor(v.X), MathUtil.Floor(v.Y), MathUtil.Floor(v.Z));
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Return the largest integer coordinates that are not greater than xyz+1 (node centered conversion).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord Ceil<T>(Vec3<T> xyz) where T : struct, IEquatable<T>, IComparable<T>
        {
            if (typeof(T) == typeof(float))
            {
                Vec3<float> v = (Vec3<float>)(object)xyz;
                return new Coord(MathUtil.Ceil(v.X), MathUtil.Ceil(v.Y), MathUtil.Ceil(v.Z));
            }
            if (typeof(T) == typeof(double))
            {
                Vec3<double> v = (Vec3<double>)(object)xyz;
                return new Coord(MathUtil.Ceil(v.X), MathUtil.Ceil(v.Y), MathUtil.Ceil(v.Z));
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reset all three coordinates
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        /// <summary>
        /// Reset all three coordinates to the same value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int xyz)
        {
            _x = _y = _z = xyz;
        }

        /// <summary>
        /// Offset this coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(int dx, int dy, int dz)
        {
            _x += dx;
            _y += dy;
            _z += dz;
        }

        /// <summary>
        /// Return a new coordinate offset from this one
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coord OffsetBy(int dx, int dy, int dz)
        {
            return new Coord(_x + dx, _y + dy, _z + dz);
        }

        /// <summary>
        /// Indexer for accessing coordinates
        /// </summary>
        public int this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => i switch
            {
                0 => _x,
                1 => _y,
                2 => _z,
                _ => throw new IndexOutOfRangeException()
            };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (i)
                {
                    case 0: _x = value; break;
                    case 1: _y = value; break;
                    case 2: _z = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Convert to Vec3&lt;double&gt;
        /// </summary>
        public Vec3<double> AsVec3d() => new Vec3<double>(_x, _y, _z);

        /// <summary>
        /// Convert to Vec3&lt;float&gt;
        /// </summary>
        public Vec3<float> AsVec3f() => new Vec3<float>(_x, _y, _z);

        /// <summary>
        /// Convert to Vec3&lt;int&gt;
        /// </summary>
        public Vec3<int> AsVec3i() => new Vec3<int>(_x, _y, _z);

        // Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator +(Coord a, Coord b) => new Coord(a._x + b._x, a._y + b._y, a._z + b._z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator -(Coord a, Coord b) => new Coord(a._x - b._x, a._y - b._y, a._z - b._z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator -(Coord a) => new Coord(-a._x, -a._y, -a._z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator >>(Coord a, int n) => new Coord(a._x >> n, a._y >> n, a._z >> n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator <<(Coord a, int n) => new Coord(a._x << n, a._y << n, a._z << n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator &(Coord a, int n) => new Coord(a._x & n, a._y & n, a._z & n);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coord operator |(Coord a, int n) => new Coord(a._x | n, a._y | n, a._z | n);

        public bool Equals(Coord other) => _x == other._x && _y == other._y && _z == other._z;
        public override bool Equals(object? obj) => obj is Coord other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z);

        public int CompareTo(Coord other)
        {
            if (_x != other._x) return _x.CompareTo(other._x);
            if (_y != other._y) return _y.CompareTo(other._y);
            return _z.CompareTo(other._z);
        }

        public override string ToString() => $"[{_x}, {_y}, {_z}]";

        public static bool operator ==(Coord left, Coord right) => left.Equals(right);
        public static bool operator !=(Coord left, Coord right) => !left.Equals(right);
        public static bool operator <(Coord left, Coord right) => left.CompareTo(right) < 0;
        public static bool operator >(Coord left, Coord right) => left.CompareTo(right) > 0;
        public static bool operator <=(Coord left, Coord right) => left.CompareTo(right) <= 0;
        public static bool operator >=(Coord left, Coord right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// Static zero coordinate
        /// </summary>
        public static Coord Zero => new Coord(0, 0, 0);
    }
}
