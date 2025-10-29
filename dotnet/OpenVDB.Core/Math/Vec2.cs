// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Vec2.cs - C# port of Vec2.h - 2D vector class

using System;
using System.Runtime.CompilerServices;

namespace OpenVDB.Math
{
    /// <summary>
    /// A 2D vector class
    /// </summary>
    /// <typeparam name="T">The value type of the vector</typeparam>
    public struct Vec2<T> : IEquatable<Vec2<T>>, IComparable<Vec2<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _x, _y;

        public T X
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _x;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _x = value;
        }

        public T Y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _y = value;
        }

        public Vec2(T val) { _x = _y = val; }
        public Vec2(T x, T y) { _x = x; _y = y; }

        public T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => i switch { 0 => _x, 1 => _y, _ => throw new IndexOutOfRangeException() };
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { if (i == 0) _x = value; else if (i == 1) _y = value; else throw new IndexOutOfRangeException(); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dot(Vec2<T> v)
        {
            if (typeof(T) == typeof(float))
                return (T)(object)((float)(object)_x * (float)(object)v._x + (float)(object)_y * (float)(object)v._y);
            if (typeof(T) == typeof(double))
                return (T)(object)((double)(object)_x * (double)(object)v._x + (double)(object)_y * (double)(object)v._y);
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Length()
        {
            if (typeof(T) == typeof(float))
            {
                float x = (float)(object)_x, y = (float)(object)_y;
                return (T)(object)MathF.Sqrt(x * x + y * y);
            }
            if (typeof(T) == typeof(double))
            {
                double x = (double)(object)_x, y = (double)(object)_y;
                return (T)(object)System.Math.Sqrt(x * x + y * y);
            }
            throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T LengthSqr()
        {
            if (typeof(T) == typeof(float))
            {
                float x = (float)(object)_x, y = (float)(object)_y;
                return (T)(object)(x * x + y * y);
            }
            if (typeof(T) == typeof(double))
            {
                double x = (double)(object)_x, y = (double)(object)_y;
                return (T)(object)(x * x + y * y);
            }
            throw new NotSupportedException();
        }

        public static Vec2<T> operator -(Vec2<T> v)
        {
            if (typeof(T) == typeof(float))
                return new Vec2<T>((T)(object)(-(float)(object)v._x), (T)(object)(-(float)(object)v._y));
            if (typeof(T) == typeof(double))
                return new Vec2<T>((T)(object)(-(double)(object)v._x), (T)(object)(-(double)(object)v._y));
            throw new NotSupportedException();
        }

        public static Vec2<T> operator +(Vec2<T> v1, Vec2<T> v2)
        {
            if (typeof(T) == typeof(float))
                return new Vec2<T>((T)(object)((float)(object)v1._x + (float)(object)v2._x), (T)(object)((float)(object)v1._y + (float)(object)v2._y));
            if (typeof(T) == typeof(double))
                return new Vec2<T>((T)(object)((double)(object)v1._x + (double)(object)v2._x), (T)(object)((double)(object)v1._y + (double)(object)v2._y));
            throw new NotSupportedException();
        }

        public static Vec2<T> operator -(Vec2<T> v1, Vec2<T> v2)
        {
            if (typeof(T) == typeof(float))
                return new Vec2<T>((T)(object)((float)(object)v1._x - (float)(object)v2._x), (T)(object)((float)(object)v1._y - (float)(object)v2._y));
            if (typeof(T) == typeof(double))
                return new Vec2<T>((T)(object)((double)(object)v1._x - (double)(object)v2._x), (T)(object)((double)(object)v1._y - (double)(object)v2._y));
            throw new NotSupportedException();
        }

        public static Vec2<T> operator *(Vec2<T> v, T scalar)
        {
            if (typeof(T) == typeof(float))
            {
                float s = (float)(object)scalar;
                return new Vec2<T>((T)(object)((float)(object)v._x * s), (T)(object)((float)(object)v._y * s));
            }
            if (typeof(T) == typeof(double))
            {
                double s = (double)(object)scalar;
                return new Vec2<T>((T)(object)((double)(object)v._x * s), (T)(object)((double)(object)v._y * s));
            }
            throw new NotSupportedException();
        }

        public static Vec2<T> operator *(T scalar, Vec2<T> v) => v * scalar;

        public static Vec2<T> operator /(Vec2<T> v, T scalar)
        {
            if (typeof(T) == typeof(float))
            {
                float s = (float)(object)scalar;
                return new Vec2<T>((T)(object)((float)(object)v._x / s), (T)(object)((float)(object)v._y / s));
            }
            if (typeof(T) == typeof(double))
            {
                double s = (double)(object)scalar;
                return new Vec2<T>((T)(object)((double)(object)v._x / s), (T)(object)((double)(object)v._y / s));
            }
            throw new NotSupportedException();
        }

        public bool Equals(Vec2<T> other) => _x.Equals(other._x) && _y.Equals(other._y);
        public override bool Equals(object? obj) => obj is Vec2<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_x, _y);
        public int CompareTo(Vec2<T> other)
        {
            int cmp = _x.CompareTo(other._x);
            return cmp != 0 ? cmp : _y.CompareTo(other._y);
        }
        public override string ToString() => $"[{_x}, {_y}]";
        public static bool operator ==(Vec2<T> left, Vec2<T> right) => left.Equals(right);
        public static bool operator !=(Vec2<T> left, Vec2<T> right) => !left.Equals(right);
        public static Vec2<T> Zero => new Vec2<T>(default);
    }
}
