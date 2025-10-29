// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// BBox.cs - C# port of BBox.h - Axis-aligned bounding box

using System;

namespace OpenVDB.Math
{
    /// <summary>
    /// Axis-aligned bounding box
    /// </summary>
    public struct BBox<T> : IEquatable<BBox<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private Vec3<T> _min;
        private Vec3<T> _max;

        public BBox(Vec3<T> min, Vec3<T> max)
        {
            _min = min;
            _max = max;
        }

        public Vec3<T> Min
        {
            get => _min;
            set => _min = value;
        }

        public Vec3<T> Max
        {
            get => _max;
            set => _max = value;
        }

        public Vec3<T> Center()
        {
            if (typeof(T) == typeof(float))
            {
                var min = (Vec3<float>)(object)_min;
                var max = (Vec3<float>)(object)_max;
                var center = new Vec3<float>(
                    (min.X + max.X) * 0.5f,
                    (min.Y + max.Y) * 0.5f,
                    (min.Z + max.Z) * 0.5f
                );
                return (Vec3<T>)(object)center;
            }
            if (typeof(T) == typeof(double))
            {
                var min = (Vec3<double>)(object)_min;
                var max = (Vec3<double>)(object)_max;
                var center = new Vec3<double>(
                    (min.X + max.X) * 0.5,
                    (min.Y + max.Y) * 0.5,
                    (min.Z + max.Z) * 0.5
                );
                return (Vec3<T>)(object)center;
            }
            throw new NotSupportedException();
        }

        public Vec3<T> Extents()
        {
            if (typeof(T) == typeof(float))
            {
                var min = (Vec3<float>)(object)_min;
                var max = (Vec3<float>)(object)_max;
                return (Vec3<T>)(object)(max - min);
            }
            if (typeof(T) == typeof(double))
            {
                var min = (Vec3<double>)(object)_min;
                var max = (Vec3<double>)(object)_max;
                return (Vec3<T>)(object)(max - min);
            }
            if (typeof(T) == typeof(int))
            {
                var min = (Vec3<int>)(object)_min;
                var max = (Vec3<int>)(object)_max;
                return (Vec3<T>)(object)(max - min);
            }
            throw new NotSupportedException();
        }

        public bool Equals(BBox<T> other) => _min.Equals(other._min) && _max.Equals(other._max);
        public override bool Equals(object? obj) => obj is BBox<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_min, _max);
        public override string ToString() => $"BBox[{_min} - {_max}]";
        public static bool operator ==(BBox<T> left, BBox<T> right) => left.Equals(right);
        public static bool operator !=(BBox<T> left, BBox<T> right) => !left.Equals(right);
    }
}
