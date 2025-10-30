// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Half.cs - C# port of Half.h and Half.cc
using System;
namespace OpenVDB.Math
{
    /// <summary>Half-precision floating point type</summary>
    public struct Half : IEquatable<Half>
    {
        private readonly System.Half _value;
        public Half(float value) { _value = (System.Half)value; }
        public float ToFloat() => (float)_value;
        public bool Equals(Half other) => _value.Equals(other._value);
        public override bool Equals(object? obj) => obj is Half other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public static bool operator ==(Half left, Half right) => left.Equals(right);
        public static bool operator !=(Half left, Half right) => !left.Equals(right);
    }
}
