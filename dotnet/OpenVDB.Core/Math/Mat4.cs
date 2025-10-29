// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Mat4.cs - C# port of Mat4.h - 4x4 matrix class

using System;

namespace OpenVDB.Math
{
    /// <summary>
    /// 4x4 matrix class
    /// </summary>
    public struct Mat4<T> : IEquatable<Mat4<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _m00, _m01, _m02, _m03;
        private T _m10, _m11, _m12, _m13;
        private T _m20, _m21, _m22, _m23;
        private T _m30, _m31, _m32, _m33;

        public static Mat4<T> Identity
        {
            get
            {
                var zero = default(T);
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Mat4<T>
                {
                    _m00 = one, _m11 = one, _m22 = one, _m33 = one
                };
            }
        }

        public bool Equals(Mat4<T> other) =>
            _m00.Equals(other._m00) && _m01.Equals(other._m01) && _m02.Equals(other._m02) && _m03.Equals(other._m03) &&
            _m10.Equals(other._m10) && _m11.Equals(other._m11) && _m12.Equals(other._m12) && _m13.Equals(other._m13) &&
            _m20.Equals(other._m20) && _m21.Equals(other._m21) && _m22.Equals(other._m22) && _m23.Equals(other._m23) &&
            _m30.Equals(other._m30) && _m31.Equals(other._m31) && _m32.Equals(other._m32) && _m33.Equals(other._m33);
        public override bool Equals(object? obj) => obj is Mat4<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_m00, _m01, _m02, _m03, _m10, _m11, _m12, _m13);
        public static bool operator ==(Mat4<T> left, Mat4<T> right) => left.Equals(right);
        public static bool operator !=(Mat4<T> left, Mat4<T> right) => !left.Equals(right);
    }
}
