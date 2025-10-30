// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Mat3.cs - C# port of Mat3.h - 3x3 matrix class

using System;

namespace OpenVDB.Math
{
    /// <summary>
    /// 3x3 matrix class
    /// </summary>
    public struct Mat3<T> : IEquatable<Mat3<T>>
        where T : struct, IEquatable<T>, IComparable<T>
    {
        private T _m00, _m01, _m02;
        private T _m10, _m11, _m12;
        private T _m20, _m21, _m22;

        public Mat3(T m00, T m01, T m02,
                    T m10, T m11, T m12,
                    T m20, T m21, T m22)
        {
            _m00 = m00; _m01 = m01; _m02 = m02;
            _m10 = m10; _m11 = m11; _m12 = m12;
            _m20 = m20; _m21 = m21; _m22 = m22;
        }

        public static Mat3<T> Identity
        {
            get
            {
                var zero = default(T);
                var one = (T)Convert.ChangeType(1, typeof(T));
                return new Mat3<T>(
                    one, zero, zero,
                    zero, one, zero,
                    zero, zero, one
                );
            }
        }

        public T this[int row, int col]
        {
            get
            {
                return (row, col) switch
                {
                    (0, 0) => _m00, (0, 1) => _m01, (0, 2) => _m02,
                    (1, 0) => _m10, (1, 1) => _m11, (1, 2) => _m12,
                    (2, 0) => _m20, (2, 1) => _m21, (2, 2) => _m22,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            set
            {
                switch (row, col)
                {
                    case (0, 0): _m00 = value; break;
                    case (0, 1): _m01 = value; break;
                    case (0, 2): _m02 = value; break;
                    case (1, 0): _m10 = value; break;
                    case (1, 1): _m11 = value; break;
                    case (1, 2): _m12 = value; break;
                    case (2, 0): _m20 = value; break;
                    case (2, 1): _m21 = value; break;
                    case (2, 2): _m22 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public bool Equals(Mat3<T> other) =>
            _m00.Equals(other._m00) && _m01.Equals(other._m01) && _m02.Equals(other._m02) &&
            _m10.Equals(other._m10) && _m11.Equals(other._m11) && _m12.Equals(other._m12) &&
            _m20.Equals(other._m20) && _m21.Equals(other._m21) && _m22.Equals(other._m22);
        public override bool Equals(object? obj) => obj is Mat3<T> other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_m00, _m01, _m02, _m10, _m11, _m12, _m20, _m21);
        public static bool operator ==(Mat3<T> left, Mat3<T> right) => left.Equals(right);
        public static bool operator !=(Mat3<T> left, Mat3<T> right) => !left.Equals(right);
    }
}
