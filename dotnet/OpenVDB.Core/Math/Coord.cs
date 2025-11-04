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
        /// Return a new coordinate offset by the same amount in all dimensions
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coord OffsetBy(int n)
        {
            return new Coord(_x + n, _y + n, _z + n);
        }

        /// <summary>
        /// Offset this coordinate by the same amount in all dimensions
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Offset(int n)
        {
            _x += n;
            _y += n;
            _z += n;
        }

        /// <summary>
        /// Set this coordinate to the component-wise minimum with another
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coord MinComponent(Coord other)
        {
            return new Coord(
                System.Math.Min(_x, other._x),
                System.Math.Min(_y, other._y),
                System.Math.Min(_z, other._z)
            );
        }

        /// <summary>
        /// Set this coordinate to the component-wise maximum with another
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coord MaxComponent(Coord other)
        {
            return new Coord(
                System.Math.Max(_x, other._x),
                System.Math.Max(_y, other._y),
                System.Math.Max(_z, other._z)
            );
        }

        /// <summary>
        /// Return true if any component of a is less than the corresponding component of b
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessThan(Coord a, Coord b)
        {
            return a._x < b._x || a._y < b._y || a._z < b._z;
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

    /// <summary>
    /// Axis-aligned bounding box of integer coordinates
    /// </summary>
    public struct CoordBBox : IEquatable<CoordBBox>
    {
        private Coord _min;
        private Coord _max;

        /// <summary>
        /// The default constructor produces an empty bounding box.
        /// </summary>
        public CoordBBox()
        {
            _min = Coord.Max;
            _max = Coord.Min;
        }

        /// <summary>
        /// Construct a bounding box with the given min and max bounds.
        /// </summary>
        public CoordBBox(Coord min, Coord max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Construct from individual components of the min and max bounds.
        /// </summary>
        public CoordBBox(int xMin, int yMin, int zMin, int xMax, int yMax, int zMax)
        {
            _min = new Coord(xMin, yMin, zMin);
            _max = new Coord(xMax, yMax, zMax);
        }

        /// <summary>
        /// Create a cubical bounding box with the given minimum and dimension.
        /// </summary>
        public static CoordBBox CreateCube(Coord min, int dim)
        {
            return new CoordBBox(min, min.OffsetBy(dim - 1));
        }

        /// <summary>
        /// Return an "infinite" bounding box, as defined by the Coord value range.
        /// </summary>
        public static CoordBBox Infinity => new CoordBBox(Coord.Min, Coord.Max);

        /// <summary>
        /// Get or set the minimum coordinate.
        /// </summary>
        public Coord Min
        {
            get => _min;
            set => _min = value;
        }

        /// <summary>
        /// Get or set the maximum coordinate.
        /// </summary>
        public Coord Max
        {
            get => _max;
            set => _max = value;
        }

        /// <summary>
        /// Reset to an empty bounding box.
        /// </summary>
        public void Reset()
        {
            _min = Coord.Max;
            _max = Coord.Min;
        }

        /// <summary>
        /// Reset with the given min and max bounds.
        /// </summary>
        public void Reset(Coord min, Coord max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Reset to a cubical bounding box with the given minimum and dimension.
        /// </summary>
        public void ResetToCube(Coord min, int dim)
        {
            _min = min;
            _max = min.OffsetBy(dim - 1);
        }

        /// <summary>
        /// Return the minimum coordinate (inclusive).
        /// </summary>
        public Coord GetStart() => _min;

        /// <summary>
        /// Return the maximum coordinate plus one (exclusive).
        /// </summary>
        public Coord GetEnd() => _max.OffsetBy(1);

        /// <summary>
        /// Return true if this bounding box is empty (i.e., encloses no coordinates).
        /// </summary>
        public bool IsEmpty => _min.X > _max.X || _min.Y > _max.Y || _min.Z > _max.Z;

        /// <summary>
        /// Return true if this bounding box is nonempty (i.e., encloses at least one coordinate).
        /// </summary>
        public bool HasVolume => !IsEmpty;

        /// <summary>
        /// Return the floating-point position of the center of this bounding box.
        /// </summary>
        public Vec3<double> GetCenter()
        {
            return new Vec3<double>(
                (_min.X + _max.X) * 0.5,
                (_min.Y + _max.Y) * 0.5,
                (_min.Z + _max.Z) * 0.5
            );
        }

        /// <summary>
        /// Return the dimensions of the coordinates spanned by this bounding box.
        /// Since coordinates are inclusive, a bounding box with min = max has dimensions of (1, 1, 1).
        /// </summary>
        public Coord Dim()
        {
            return IsEmpty ? new Coord(0) : (_max.OffsetBy(1) - _min);
        }

        /// <summary>
        /// Return the integer volume of coordinates spanned by this bounding box.
        /// Since coordinates are inclusive, a bounding box with min = max has volume one.
        /// </summary>
        public ulong Volume
        {
            get
            {
                var d = Dim();
                return (ulong)d.X * (ulong)d.Y * (ulong)d.Z;
            }
        }

        /// <summary>
        /// Return true if this bounding box can be subdivided.
        /// </summary>
        public bool IsDivisible => _min.X < _max.X && _min.Y < _max.Y && _min.Z < _max.Z;

        /// <summary>
        /// Return the index (0, 1 or 2) of the shortest axis.
        /// </summary>
        public int MinExtent()
        {
            var d = Dim();
            if (d.X <= d.Y && d.X <= d.Z) return 0;
            if (d.Y <= d.Z) return 1;
            return 2;
        }

        /// <summary>
        /// Return the index (0, 1 or 2) of the longest axis.
        /// </summary>
        public int MaxExtent()
        {
            var d = Dim();
            if (d.X >= d.Y && d.X >= d.Z) return 0;
            if (d.Y >= d.Z) return 1;
            return 2;
        }

        /// <summary>
        /// Return true if the given coordinate is inside this bounding box.
        /// </summary>
        public bool IsInside(Coord xyz)
        {
            return !(Coord.LessThan(xyz, _min) || Coord.LessThan(_max, xyz));
        }

        /// <summary>
        /// Return true if the given bounding box is inside this bounding box.
        /// </summary>
        public bool IsInside(CoordBBox b)
        {
            return !(Coord.LessThan(b._min, _min) || Coord.LessThan(_max, b._max));
        }

        /// <summary>
        /// Return true if the given bounding box overlaps with this bounding box.
        /// </summary>
        public bool HasOverlap(CoordBBox b)
        {
            return !(Coord.LessThan(_max, b._min) || Coord.LessThan(b._max, _min));
        }

        /// <summary>
        /// Pad this bounding box with the specified padding.
        /// </summary>
        public void Expand(int padding)
        {
            _min = _min.OffsetBy(-padding);
            _max = _max.OffsetBy(padding);
        }

        /// <summary>
        /// Return a new instance that is expanded by the specified padding.
        /// </summary>
        public CoordBBox ExpandBy(int padding)
        {
            return new CoordBBox(_min.OffsetBy(-padding), _max.OffsetBy(padding));
        }

        /// <summary>
        /// Expand this bounding box to enclose the given coordinate.
        /// </summary>
        public void Expand(Coord xyz)
        {
            _min = _min.MinComponent(xyz);
            _max = _max.MaxComponent(xyz);
        }

        /// <summary>
        /// Union this bounding box with the given bounding box.
        /// </summary>
        public void Expand(CoordBBox bbox)
        {
            _min = _min.MinComponent(bbox._min);
            _max = _max.MaxComponent(bbox._max);
        }

        /// <summary>
        /// Intersect this bounding box with the given bounding box.
        /// </summary>
        public void Intersect(CoordBBox bbox)
        {
            _min = _min.MaxComponent(bbox._min);
            _max = _max.MinComponent(bbox._max);
        }

        /// <summary>
        /// Translate this bounding box by the given offset.
        /// </summary>
        public void Translate(Coord t)
        {
            _min += t;
            _max += t;
        }

        /// <summary>
        /// Move this bounding box to the specified minimum.
        /// </summary>
        public void MoveMin(Coord min)
        {
            _max += min - _min;
            _min = min;
        }

        /// <summary>
        /// Move this bounding box to the specified maximum.
        /// </summary>
        public void MoveMax(Coord max)
        {
            _min += max - _max;
            _max = max;
        }

        public bool Equals(CoordBBox other) => _min.Equals(other._min) && _max.Equals(other._max);
        public override bool Equals(object? obj) => obj is CoordBBox other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_min, _max);
        public override string ToString() => $"CoordBBox[{_min} - {_max}]";

        public static bool operator ==(CoordBBox left, CoordBBox right) => left.Equals(right);
        public static bool operator !=(CoordBBox left, CoordBBox right) => !left.Equals(right);
    }
}
