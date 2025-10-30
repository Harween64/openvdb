// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Vec4.cs - C# port of Vec4.h - 4D vector class

using System;
using System.Runtime.CompilerServices;

namespace OpenVDB.Math
{
    public struct Vec4<T> : IEquatable<Vec4<T>> where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _x, _y, _z, _w;
        public T X { get => _x; set => _x = value; }
        public T Y { get => _y; set => _y = value; }
        public T Z { get => _z; set => _z = value; }
        public T W { get => _w; set => _w = value; }
        public Vec4(T val) { _x = _y = _z = _w = val; }
        public Vec4(T x, T y, T z, T w) { _x = x; _y = y; _z = z; _w = w; }
        public T this[int i]
        {
            get => i switch { 0 => _x, 1 => _y, 2 => _z, 3 => _w, _ => throw new IndexOutOfRangeException() };
            set { if (i == 0) _x = value; else if (i == 1) _y = value; else if (i == 2) _z = value; else if (i == 3) _w = value; else throw new IndexOutOfRangeException(); }
        }
        public T Length()
        {
            if (typeof(T) == typeof(float))
            {
                float x = (float)(object)_x, y = (float)(object)_y, z = (float)(object)_z, w = (float)(object)_w;
                return (T)(object)MathF.Sqrt(x*x + y*y + z*z + w*w);
            }
            if (typeof(T) == typeof(double))
            {
                double x = (double)(object)_x, y = (double)(object)_y, z = (double)(object)_z, w = (double)(object)_w;
                return (T)(object)System.Math.Sqrt(x*x + y*y + z*z + w*w);
            }
            throw new NotSupportedException();
        }
        public static Vec4<T> operator +(Vec4<T> v1, Vec4<T> v2)
        {
            if (typeof(T) == typeof(float))
                return new Vec4<T>((T)(object)((float)(object)v1._x + (float)(object)v2._x), 
                                   (T)(object)((float)(object)v1._y + (float)(object)v2._y),
                                   (T)(object)((float)(object)v1._z + (float)(object)v2._z),
                                   (T)(object)((float)(object)v1._w + (float)(object)v2._w));
            throw new NotSupportedException();
        }
        public static Vec4<T> operator *(Vec4<T> v, T scalar)
        {
            if (typeof(T) == typeof(float))
            {
                float s = (float)(object)scalar;
                return new Vec4<T>((T)(object)((float)(object)v._x * s), (T)(object)((float)(object)v._y * s), 
                                   (T)(object)((float)(object)v._z * s), (T)(object)((float)(object)v._w * s));
            }
            throw new NotSupportedException();
        }
        public bool Equals(Vec4<T> other) => _x.Equals(other._x) && _y.Equals(other._y) && _z.Equals(other._z) && _w.Equals(other._w);
        public override bool Equals(object? obj) => obj is Vec4<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_x, _y, _z, _w);
        public override string ToString() => $"[{_x}, {_y}, {_z}, {_w}]";
        public static bool operator ==(Vec4<T> left, Vec4<T> right) => left.Equals(right);
        public static bool operator !=(Vec4<T> left, Vec4<T> right) => !left.Equals(right);
        public static Vec4<T> Zero => new Vec4<T>(default);
    }
}
