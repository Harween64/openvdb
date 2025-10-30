// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Math.cs - C# port of Math.h
//
// General-purpose arithmetic and comparison routines, most of which
// accept arbitrary value types (or at least arbitrary numeric value types)

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenVDB.Math
{
    /// <summary>
    /// Dummy class for tag dispatch of conversion constructors
    /// </summary>
    public struct Conversion { }

    /// <summary>
    /// Tolerance for floating-point comparison
    /// </summary>
    public static class Tolerance
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Value<T>() where T : struct
        {
            if (typeof(T) == typeof(float))
                return (T)(object)1e-8f;
            if (typeof(T) == typeof(double))
                return (T)(object)1e-15;
            return default;
        }
    }

    /// <summary>
    /// Delta for small floating-point offsets
    /// </summary>
    public static class Delta
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Value<T>() where T : struct
        {
            if (typeof(T) == typeof(float))
                return (T)(object)1e-5f;
            if (typeof(T) == typeof(double))
                return (T)(object)1e-9;
            return default;
        }
    }

    /// <summary>
    /// Simple generator of random numbers over the range [0, 1)
    /// Thread-safe as long as each thread has its own Rand01 instance
    /// </summary>
    /// <typeparam name="T">The floating-point type (float or double)</typeparam>
    public class Rand01<T> where T : struct
    {
        private readonly Random _random;

        /// <summary>
        /// Initialize the generator with a seed value.
        /// </summary>
        /// <param name="seed">Seed value for the random number generator</param>
        public Rand01(int seed)
        {
            _random = new Random(seed);
        }

        /// <summary>
        /// Initialize the generator with the default seed.
        /// </summary>
        public Rand01()
        {
            _random = new Random();
        }

        /// <summary>
        /// Set the seed value for the random number generator
        /// </summary>
        public void SetSeed(int seed)
        {
            // Note: .NET Random doesn't support re-seeding, so we'd need to create a new instance
            // For now, this is a limitation of the C# port
            throw new NotSupportedException("Random re-seeding is not supported in .NET Random. Create a new instance instead.");
        }

        /// <summary>
        /// Return a uniformly distributed random number in the range [0, 1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Generate()
        {
            if (typeof(T) == typeof(float))
                return (T)(object)(float)_random.NextDouble();
            if (typeof(T) == typeof(double))
                return (T)(object)_random.NextDouble();
            throw new NotSupportedException($"Type {typeof(T)} is not supported for random generation.");
        }
    }

    /// <summary>
    /// Simple random integer generator
    /// Thread-safe as long as each thread has its own RandInt instance
    /// </summary>
    /// <typeparam name="T">The integer type</typeparam>
    public class RandInt<T> where T : struct
    {
        private readonly Random _random;
        private T _min;
        private T _max;

        /// <summary>
        /// Initialize the generator.
        /// </summary>
        /// <param name="seed">Seed value for the random number generator</param>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        public RandInt(int seed, T min, T max)
        {
            _random = new Random(seed);
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Initialize the generator with default seed.
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (inclusive)</param>
        public RandInt(T min, T max)
        {
            _random = new Random();
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Change the range over which integers are distributed to [min, max].
        /// </summary>
        public void SetRange(T min, T max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Return a randomly-generated integer in the current range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Generate()
        {
            if (typeof(T) == typeof(int))
            {
                int min = (int)(object)_min;
                int max = (int)(object)_max;
                return (T)(object)_random.Next(min, max + 1);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for random integer generation.");
        }

        /// <summary>
        /// Return a randomly-generated integer in the new range [min, max],
        /// without changing the current range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Generate(T min, T max)
        {
            if (typeof(T) == typeof(int))
            {
                int minVal = (int)(object)min;
                int maxVal = (int)(object)max;
                return (T)(object)_random.Next(minVal, maxVal + 1);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for random integer generation.");
        }
    }

    /// <summary>
    /// General purpose math utilities
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Pi constant
        /// </summary>
        public const double Pi = 3.141592653589793238462643383279502884;
        public const float PiF = 3.141592653589793238462643383279502884F;

        // ==========> Clamp <==================

        /// <summary>
        /// Return x clamped to [min, max]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp<T>(T x, T min, T max) where T : IComparable<T>
        {
            return x.CompareTo(min) > 0 ? (x.CompareTo(max) < 0 ? x : max) : min;
        }

        /// <summary>
        /// Return x clamped to [0, 1]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp01<T>(T x) where T : struct, IComparable<T>
        {
            T zero = default;
            T one = (T)Convert.ChangeType(1, typeof(T));
            return x.CompareTo(zero) > 0 ? (x.CompareTo(one) < 0 ? x : one) : zero;
        }

        /// <summary>
        /// Return true if x is outside [0,1]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ClampTest01<T>(ref T x) where T : struct, IComparable<T>
        {
            T zero = default;
            T one = (T)Convert.ChangeType(1, typeof(T));
            if (x.CompareTo(zero) >= 0 && x.CompareTo(one) <= 0) return false;
            x = x.CompareTo(zero) < 0 ? zero : one;
            return true;
        }

        /// <summary>
        /// Return 0 if x &lt; 0, 1 if x &gt; 1 or else (3 - 2*x)*x^2.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SmoothUnitStep<T>(T x) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                float result = val > 0 ? (val < 1 ? (3 - 2 * val) * val * val : 1) : 0;
                return (T)(object)result;
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                double result = val > 0 ? (val < 1 ? (3 - 2 * val) * val * val : 1) : 0;
                return (T)(object)result;
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for SmoothUnitStep.");
        }

        /// <summary>
        /// Return 0 if x &lt; min, 1 if x &gt; max or else (3 - 2*t)*t^2,
        /// where t = (x - min)/(max - min).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SmoothUnitStep<T>(T x, T min, T max) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float xVal = (float)(object)x;
                float minVal = (float)(object)min;
                float maxVal = (float)(object)max;
                float t = (xVal - minVal) / (maxVal - minVal);
                return SmoothUnitStep((T)(object)t);
            }
            if (typeof(T) == typeof(double))
            {
                double xVal = (double)(object)x;
                double minVal = (double)(object)min;
                double maxVal = (double)(object)max;
                double t = (xVal - minVal) / (maxVal - minVal);
                return SmoothUnitStep((T)(object)t);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for SmoothUnitStep.");
        }

        // ==========> Absolute Value <==================

        /// <summary>
        /// Return the absolute value of the given quantity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Abs<T>(T value) where T : struct
        {
            if (typeof(T) == typeof(int))
                return (T)(object)System.Math.Abs((int)(object)value);
            if (typeof(T) == typeof(long))
                return (T)(object)System.Math.Abs((long)(object)value);
            if (typeof(T) == typeof(float))
                return (T)(object)System.Math.Abs((float)(object)value);
            if (typeof(T) == typeof(double))
                return (T)(object)System.Math.Abs((double)(object)value);
            if (typeof(T) == typeof(uint))
                return value;
            if (typeof(T) == typeof(ulong))
                return value;
            if (typeof(T) == typeof(bool))
                return value;
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Abs.");
        }

        // ==========> Value Comparison <==================

        /// <summary>
        /// Return true if x is exactly equal to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero<T>(T x) where T : struct, IEquatable<T>
        {
            return x.Equals(default(T));
        }

        /// <summary>
        /// Return true if x is equal to zero to within the default floating-point comparison tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxZero<T>(T x) where T : struct
        {
            T tolerance = Tolerance.Value<T>();
            return IsApproxZero(x, tolerance);
        }

        /// <summary>
        /// Return true if x is equal to zero to within the given tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxZero<T>(T x, T tolerance) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                float tol = (float)(object)tolerance;
                return !(val > tol) && !(val < -tol);
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                double tol = (double)(object)tolerance;
                return !(val > tol) && !(val < -tol);
            }
            return x.Equals(default(T));
        }

        /// <summary>
        /// Return true if x is less than zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegative<T>(T x) where T : struct, IComparable<T>
        {
            return x.CompareTo(default(T)) < 0;
        }

        /// <summary>
        /// Return true if x is finite.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(float x) => !float.IsInfinity(x) && !float.IsNaN(x);

        /// <summary>
        /// Return true if x is finite.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(double x) => !double.IsInfinity(x) && !double.IsNaN(x);

        /// <summary>
        /// Return true if x is an infinity value (either positive infinity or negative infinity).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInfinite(float x) => float.IsInfinity(x);

        /// <summary>
        /// Return true if x is an infinity value (either positive infinity or negative infinity).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInfinite(double x) => double.IsInfinity(x);

        /// <summary>
        /// Return true if x is a NaN (Not-A-Number) value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNan(float x) => float.IsNaN(x);

        /// <summary>
        /// Return true if x is a NaN (Not-A-Number) value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNan(double x) => double.IsNaN(x);

        /// <summary>
        /// Return true if a is equal to b to within the given tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxEqual<T>(T a, T b, T tolerance) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float aVal = (float)(object)a;
                float bVal = (float)(object)b;
                float tol = (float)(object)tolerance;
                return System.Math.Abs(aVal - bVal) <= tol;
            }
            if (typeof(T) == typeof(double))
            {
                double aVal = (double)(object)a;
                double bVal = (double)(object)b;
                double tol = (double)(object)tolerance;
                return System.Math.Abs(aVal - bVal) <= tol;
            }
            if (typeof(T) == typeof(bool))
                return a.Equals(b);
            if (typeof(T) == typeof(string))
                return a.Equals(b);
            return a.Equals(b);
        }

        /// <summary>
        /// Return true if a is equal to b to within the default floating-point comparison tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxEqual<T>(T a, T b) where T : struct
        {
            T tolerance = Tolerance.Value<T>();
            return IsApproxEqual(a, b, tolerance);
        }

        /// <summary>
        /// Return true if Vec3 a is equal to Vec3 b to within the default floating-point comparison tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxEqual<T>(Vec3<T> a, Vec3<T> b) 
            where T : struct, IEquatable<T>, IComparable<T>
        {
            T tolerance = Tolerance.Value<T>();
            return a.Eq(b, tolerance);
        }

        /// <summary>
        /// Return true if a is larger than b to within the given tolerance, i.e., if b - a &lt; tolerance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsApproxLarger<T>(T a, T b, T tolerance) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float aVal = (float)(object)a;
                float bVal = (float)(object)b;
                float tol = (float)(object)tolerance;
                return (bVal - aVal) < tol;
            }
            if (typeof(T) == typeof(double))
            {
                double aVal = (double)(object)a;
                double bVal = (double)(object)b;
                double tol = (double)(object)tolerance;
                return (bVal - aVal) < tol;
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for IsApproxLarger.");
        }

        /// <summary>
        /// Return true if a is exactly equal to b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExactlyEqual<T>(T a, T b) where T : IEquatable<T>
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Return true if a and b are equal within relative or absolute tolerance.
        /// </summary>
        public static bool IsRelOrApproxEqual<T>(T a, T b, T absTol, T relTol) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float aVal = (float)(object)a;
                float bVal = (float)(object)b;
                float absT = (float)(object)absTol;
                float relT = (float)(object)relTol;

                if (System.Math.Abs(aVal - bVal) <= absT) return true;

                float relError;
                if (System.Math.Abs(bVal) > System.Math.Abs(aVal))
                    relError = System.Math.Abs((aVal - bVal) / bVal);
                else
                    relError = System.Math.Abs((aVal - bVal) / aVal);
                return relError <= relT;
            }
            if (typeof(T) == typeof(double))
            {
                double aVal = (double)(object)a;
                double bVal = (double)(object)b;
                double absT = (double)(object)absTol;
                double relT = (double)(object)relTol;

                if (System.Math.Abs(aVal - bVal) <= absT) return true;

                double relError;
                if (System.Math.Abs(bVal) > System.Math.Abs(aVal))
                    relError = System.Math.Abs((aVal - bVal) / bVal);
                else
                    relError = System.Math.Abs((aVal - bVal) / aVal);
                return relError <= relT;
            }
            if (typeof(T) == typeof(bool))
                return a.Equals(b);
            throw new NotSupportedException($"Type {typeof(T)} is not supported for IsRelOrApproxEqual.");
        }

        /// <summary>
        /// Convert float to int32 for bit-level comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloatToInt32(float f)
        {
            return BitConverter.SingleToInt32Bits(f);
        }

        /// <summary>
        /// Convert double to int64 for bit-level comparison
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long DoubleToInt64(double d)
        {
            return BitConverter.DoubleToInt64Bits(d);
        }

        /// <summary>
        /// ULPs (Units in the Last Place) equality comparison for doubles
        /// </summary>
        public static bool IsUlpsEqual(double left, double right, long unitsInLastPlace)
        {
            long longLeft = DoubleToInt64(left);
            if (longLeft < 0)
                longLeft = long.MinValue - longLeft;

            long longRight = DoubleToInt64(right);
            if (longRight < 0)
                longRight = long.MinValue - longRight;

            long difference = System.Math.Abs(longLeft - longRight);
            return difference <= unitsInLastPlace;
        }

        /// <summary>
        /// ULPs (Units in the Last Place) equality comparison for floats
        /// </summary>
        public static bool IsUlpsEqual(float left, float right, int unitsInLastPlace)
        {
            int intLeft = FloatToInt32(left);
            if (intLeft < 0)
                intLeft = int.MinValue - intLeft;

            int intRight = FloatToInt32(right);
            if (intRight < 0)
                intRight = int.MinValue - intRight;

            int difference = System.Math.Abs(intLeft - intRight);
            return difference <= unitsInLastPlace;
        }

        // ==========> Pow <==================

        /// <summary>
        /// Return x^2.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pow2<T>(T x) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                return (T)(object)(val * val);
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                return (T)(object)(val * val);
            }
            if (typeof(T) == typeof(int))
            {
                int val = (int)(object)x;
                return (T)(object)(val * val);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Pow2.");
        }

        /// <summary>
        /// Return x^3.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pow3<T>(T x) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                return (T)(object)(val * val * val);
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                return (T)(object)(val * val * val);
            }
            if (typeof(T) == typeof(int))
            {
                int val = (int)(object)x;
                return (T)(object)(val * val * val);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Pow3.");
        }

        /// <summary>
        /// Return x^4.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pow4<T>(T x) where T : struct
        {
            T x2 = Pow2(x);
            return Pow2(x2);
        }

        /// <summary>
        /// Return b^e for floating point types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float b, float e)
        {
            return MathF.Pow(b, e);
        }

        /// <summary>
        /// Return b^e for floating point types.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Pow(double b, double e)
        {
            return System.Math.Pow(b, e);
        }

        // ==========> Max/Min <==================

        /// <summary>
        /// Return the maximum of two values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        /// <summary>
        /// Return the maximum of three values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(T a, T b, T c) where T : IComparable<T>
        {
            return Max(Max(a, b), c);
        }

        /// <summary>
        /// Return the maximum of four values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Max<T>(T a, T b, T c, T d) where T : IComparable<T>
        {
            return Max(Max(a, b), Max(c, d));
        }

        /// <summary>
        /// Return the minimum of two values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        /// <summary>
        /// Return the minimum of three values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(T a, T b, T c) where T : IComparable<T>
        {
            return Min(Min(a, b), c);
        }

        /// <summary>
        /// Return the minimum of four values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Min<T>(T a, T b, T c, T d) where T : IComparable<T>
        {
            return Min(Min(a, b), Min(c, d));
        }

        // ==========> Exp, Sin, Cos <==================

        /// <summary>
        /// Return e^x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float x) => MathF.Exp(x);

        /// <summary>
        /// Return e^x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Exp(double x) => System.Math.Exp(x);

        /// <summary>
        /// Return sin(x).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float x) => MathF.Sin(x);

        /// <summary>
        /// Return sin(x).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sin(double x) => System.Math.Sin(x);

        /// <summary>
        /// Return cos(x).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float x) => MathF.Cos(x);

        /// <summary>
        /// Return cos(x).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cos(double x) => System.Math.Cos(x);

        // ==========> Sign and Zero Crossing <==================

        /// <summary>
        /// Return the sign of the given value as an integer (either -1, 0 or 1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign<T>(T x) where T : struct, IComparable<T>
        {
            T zero = default;
            return (zero.CompareTo(x) < 0 ? 1 : 0) - (x.CompareTo(zero) < 0 ? 1 : 0);
        }

        /// <summary>
        /// Return true if a and b have different signs.
        /// Note: Zero is considered a positive number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SignChange<T>(T a, T b) where T : struct, IComparable<T>
        {
            T zero = default;
            return (a.CompareTo(zero) < 0) ^ (b.CompareTo(zero) < 0);
        }

        /// <summary>
        /// Return true if the interval [a, b] includes zero,
        /// i.e., if either a or b is zero or if they have different signs.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ZeroCrossing<T>(T a, T b) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float aVal = (float)(object)a;
                float bVal = (float)(object)b;
                return aVal * bVal <= 0;
            }
            if (typeof(T) == typeof(double))
            {
                double aVal = (double)(object)a;
                double bVal = (double)(object)b;
                return aVal * bVal <= 0;
            }
            if (typeof(T) == typeof(int))
            {
                int aVal = (int)(object)a;
                int bVal = (int)(object)b;
                return aVal * bVal <= 0;
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for ZeroCrossing.");
        }

        // ==========> Sqrt, Cbrt <==================

        /// <summary>
        /// Return the square root of a floating-point value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x) => MathF.Sqrt(x);

        /// <summary>
        /// Return the square root of a floating-point value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sqrt(double x) => System.Math.Sqrt(x);

        /// <summary>
        /// Return the cube root of a floating-point value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cbrt(float x) => MathF.Cbrt(x);

        /// <summary>
        /// Return the cube root of a floating-point value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cbrt(double x) => System.Math.Cbrt(x);

        // ==========> Mod, Round, Floor, Ceil <==================

        /// <summary>
        /// Return the remainder of x / y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Mod(int x, int y) => x % y;

        /// <summary>
        /// Return the remainder of x / y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mod(float x, float y) => x % y;

        /// <summary>
        /// Return the remainder of x / y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Mod(double x, double y) => x % y;

        /// <summary>
        /// Return x rounded up to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundUp(float x) => MathF.Ceiling(x);

        /// <summary>
        /// Return x rounded up to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RoundUp(double x) => System.Math.Ceiling(x);

        /// <summary>
        /// Return x rounded down to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundDown(float x) => MathF.Floor(x);

        /// <summary>
        /// Return x rounded down to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RoundDown(double x) => System.Math.Floor(x);

        /// <summary>
        /// Return x rounded to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Round(float x) => MathF.Round(x);

        /// <summary>
        /// Return x rounded to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(double x) => System.Math.Round(x);

        /// <summary>
        /// Return the floor of x as an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Floor(float x) => (int)MathF.Floor(x);

        /// <summary>
        /// Return the floor of x as an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Floor(double x) => (int)System.Math.Floor(x);

        /// <summary>
        /// Return the ceiling of x as an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Ceil(float x) => (int)MathF.Ceiling(x);

        /// <summary>
        /// Return the ceiling of x as an integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Ceil(double x) => (int)System.Math.Ceiling(x);

        /// <summary>
        /// Return x if it is greater or equal in magnitude than delta. Otherwise, return zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Chop<T>(T x, T delta) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                float d = (float)(object)delta;
                return (T)(object)(System.Math.Abs(val) < d ? 0f : val);
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                double d = (double)(object)delta;
                return (T)(object)(System.Math.Abs(val) < d ? 0.0 : val);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Chop.");
        }

        /// <summary>
        /// Return the inverse of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Inv<T>(T x) where T : struct
        {
            if (typeof(T) == typeof(float))
            {
                float val = (float)(object)x;
                return (T)(object)(1f / val);
            }
            if (typeof(T) == typeof(double))
            {
                double val = (double)(object)x;
                return (T)(object)(1.0 / val);
            }
            throw new NotSupportedException($"Type {typeof(T)} is not supported for Inv.");
        }

        /// <summary>
        /// Return the index [0,1,2] of the smallest value in a 3D vector.
        /// </summary>
        public static int MinIndex<T>(T v0, T v1, T v2) where T : IComparable<T>
        {
            int r = 0;
            if (v1.CompareTo(v0) <= 0) r = 1;
            if (v2.CompareTo(r == 0 ? v0 : v1) <= 0) r = 2;
            return r;
        }

        /// <summary>
        /// Return the index [0,1,2] of the largest value in a 3D vector.
        /// </summary>
        public static int MaxIndex<T>(T v0, T v1, T v2) where T : IComparable<T>
        {
            int r = 0;
            if (v1.CompareTo(v0) >= 0) r = 1;
            if (v2.CompareTo(r == 0 ? v0 : v1) >= 0) r = 2;
            return r;
        }
    }

    /// <summary>
    /// Axis enumeration
    /// </summary>
    public enum Axis
    {
        /// <summary>X axis</summary>
        X = 0,
        /// <summary>Y axis</summary>
        Y = 1,
        /// <summary>Z axis</summary>
        Z = 2
    }

    /// <summary>
    /// Rotation order enumeration (consistent with historical mx analogs)
    /// </summary>
    public enum RotationOrder
    {
        /// <summary>XYZ rotation</summary>
        XYZ = 0,
        /// <summary>XZY rotation</summary>
        XZY,
        /// <summary>YXZ rotation</summary>
        YXZ,
        /// <summary>YZX rotation</summary>
        YZX,
        /// <summary>ZXY rotation</summary>
        ZXY,
        /// <summary>ZYX rotation</summary>
        ZYX,
        /// <summary>XZX rotation</summary>
        XZX,
        /// <summary>ZXZ rotation</summary>
        ZXZ
    }
}
