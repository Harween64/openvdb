// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Quat.cs - C# port of Quat.h - Quaternion class

using System;

namespace OpenVDB.Math
{
    /// <summary>
    /// Quaternion class for representing rotations
    /// </summary>
    public struct Quat<T> : IEquatable<Quat<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _x, _y, _z, _w;

        public Quat(T x, T y, T z, T w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public T X => _x;
        public T Y => _y;
        public T Z => _z;
        public T W => _w;

        public static Quat<T> Identity
        {
            get
            {
                var zero = default(T);
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Quat<T>(zero, zero, zero, one);
            }
        }

        public bool Equals(Quat<T> other) =>
            _x.Equals(other._x) && _y.Equals(other._y) &&
            _z.Equals(other._z) && _w.Equals(other._w);
        public override bool Equals(object? obj) => obj is Quat<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z, _w);
        public override string ToString() => $"Quat[{_x}, {_y}, {_z}, {_w}]";
        public static bool operator ==(Quat<T> left, Quat<T> right) => left.Equals(right);
        public static bool operator !=(Quat<T> left, Quat<T> right) => !left.Equals(right);
    }
}
