// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Ray.cs - C# port of Ray.h - Ray class for ray tracing

using System;

namespace OpenVDB.Math
{
    /// <summary>
    /// A ray defined by an origin and direction
    /// </summary>
    public struct Ray<T> : IEquatable<Ray<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private Vec3<T> _eye;
        private Vec3<T> _dir;
        private T _tStart;
        private T _tEnd;

        public Ray(Vec3<T> eye, Vec3<T> direction)
        {
            _eye = eye;
            _dir = direction;
            _tStart = default;
            if (typeof(T) == typeof(float))
                _tEnd = (T)(object)float.MaxValue;
            else if (typeof(T) == typeof(double))
                _tEnd = (T)(object)double.MaxValue;
            else
                _tEnd = default;
        }

        public Vec3<T> Eye => _eye;
        public Vec3<T> Direction => _dir;
        public T TStart => _tStart;
        public T TEnd => _tEnd;

        public bool Equals(Ray<T> other) =>
            _eye.Equals(other._eye) && _dir.Equals(other._dir) &&
            _tStart.Equals(other._tStart) && _tEnd.Equals(other._tEnd);
        public override bool Equals(object? obj) => obj is Ray<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_eye, _dir, _tStart, _tEnd);
        public static bool operator ==(Ray<T> left, Ray<T> right) => left.Equals(right);
        public static bool operator !=(Ray<T> left, Ray<T> right) => !left.Equals(right);
    }
}
