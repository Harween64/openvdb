// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Stats.cs - C# port of Stats.h
//
// Classes to compute statistics and histograms

using System;
using System.Collections.Generic;
using System.Text;

namespace OpenVDB.Math
{
    /// <summary>
    /// Templated class to compute the minimum and maximum values.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public class MinMax<T> where T : struct, IComparable<T>
    {
        private T _min;
        private T _max;

        /// <summary>
        /// Empty constructor - initializes with extreme values
        /// </summary>
        public MinMax()
        {
            if (typeof(T) == typeof(float))
            {
                _min = (T)(object)float.MaxValue;
                _max = (T)(object)float.MinValue;
            }
            else if (typeof(T) == typeof(double))
            {
                _min = (T)(object)double.MaxValue;
                _max = (T)(object)double.MinValue;
            }
            else if (typeof(T) == typeof(int))
            {
                _min = (T)(object)int.MaxValue;
                _max = (T)(object)int.MinValue;
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T)} not supported for MinMax");
            }
        }

        /// <summary>
        /// Constructor with explicit min and max values
        /// </summary>
        public MinMax(T min, T max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>Add a single sample.</summary>
        public void Add(T val)
        {
            if (val.CompareTo(_min) < 0) _min = val;
            if (val.CompareTo(_max) > 0) _max = val;
        }

        /// <summary>Return the minimum value.</summary>
        public T Min => _min;

        /// <summary>Return the maximum value.</summary>
        public T Max => _max;

        /// <summary>Add the samples from the other MinMax instance.</summary>
        public void Add(MinMax<T> other)
        {
            if (other._min.CompareTo(_min) < 0) _min = other._min;
            if (other._max.CompareTo(_max) > 0) _max = other._max;
        }

        /// <summary>Print MinMax to string.</summary>
        public string Print(string name = "", int precision = 3)
        {
            var sb = new StringBuilder();
            sb.Append("MinMax ");
            if (!string.IsNullOrEmpty(name)) sb.Append($"for \"{name}\" ");
            sb.Append($"  Min={_min}, Max={_max}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// This class computes the minimum and maximum values of a population
    /// of floating-point values.
    /// </summary>
    public class Extrema
    {
        protected ulong _size;
        protected double _min;
        protected double _max;

        /// <summary>
        /// Constructor - initializes with extreme values
        /// </summary>
        public Extrema()
        {
            _size = 0;
            _min = double.MaxValue;
            _max = double.MinValue;
        }

        /// <summary>Add a single sample.</summary>
        public void Add(double val)
        {
            ++_size;
            _min = System.Math.Min(val, _min);
            _max = System.Math.Max(val, _max);
        }

        /// <summary>Add n samples with constant value val.</summary>
        public void Add(double val, ulong n)
        {
            _size += n;
            _min = System.Math.Min(val, _min);
            _max = System.Math.Max(val, _max);
        }

        /// <summary>Return the size of the population.</summary>
        public ulong Size => _size;

        /// <summary>Return the minimum value.</summary>
        public double Min => _min;

        /// <summary>Return the maximum value.</summary>
        public double Max => _max;

        /// <summary>Return the range (max - min).</summary>
        public double Range => _max - _min;

        /// <summary>Add the samples from another Extrema instance.</summary>
        public void Add(Extrema other)
        {
            if (other._size > 0) Join(other);
        }

        /// <summary>Print extrema to string.</summary>
        public virtual string Print(string name = "", int precision = 3)
        {
            var sb = new StringBuilder();
            sb.Append("Extrema ");
            if (!string.IsNullOrEmpty(name)) sb.Append($"for \"{name}\" ");
            if (_size > 0)
            {
                sb.AppendLine($"with {_size} samples:");
                string formatStr = "F" + precision;
                sb.Append($"  Min={_min.ToString(formatStr)}, Max={_max.ToString(formatStr)}, Range={Range.ToString(formatStr)}");
            }
            else
            {
                sb.Append(": no samples were added.");
            }
            return sb.ToString();
        }

        protected void Join(Extrema other)
        {
            // Join is only called when other._size > 0, validated in Add()
            _size += other._size;
            _min = System.Math.Min(_min, other._min);
            _max = System.Math.Max(_max, other._max);
        }
    }

    /// <summary>
    /// This class computes statistics (minimum value, maximum value, mean,
    /// variance and standard deviation) of a population of floating-point values.
    /// </summary>
    /// <remarks>
    /// variance = Mean[ (X-Mean[X])^2 ] = Mean[X^2] - Mean[X]^2
    /// standard deviation = sqrt(variance)
    /// 
    /// This class employs incremental computation and double precision.
    /// </remarks>
    public class Stats : Extrema
    {
        private double _avg;
        private double _aux;

        /// <summary>Constructor</summary>
        public Stats()
        {
            _avg = 0.0;
            _aux = 0.0;
        }

        /// <summary>Add a single sample.</summary>
        public new void Add(double val)
        {
            base.Add(val);
            double delta = val - _avg;
            _avg += delta / _size;
            _aux += delta * (val - _avg);
        }

        /// <summary>Add n samples with constant value val.</summary>
        public new void Add(double val, ulong n)
        {
            double denom = 1.0 / (_size + n);
            double delta = val - _avg;
            _avg += denom * delta * n;
            _aux += denom * delta * delta * _size * n;
            base.Add(val, n);
        }

        /// <summary>Add the samples from another Stats instance.</summary>
        public void Add(Stats other)
        {
            if (other._size > 0)
            {
                double denom = 1.0 / (_size + other._size);
                double delta = other._avg - _avg;
                _avg += denom * delta * other._size;
                _aux += other._aux + denom * delta * delta * _size * other._size;
                Join(other);
            }
        }

        /// <summary>Return the arithmetic mean (average) value.</summary>
        public double Avg => _avg;

        /// <summary>Return the arithmetic mean (average) value.</summary>
        public double Mean => _avg;

        /// <summary>
        /// Return the population variance.
        /// Note: The unbiased sample variance = population variance * num/(num-1)
        /// </summary>
        public double Var => _size < 2 ? 0.0 : _aux / _size;

        /// <summary>Return the population variance.</summary>
        public double Variance => Var;

        /// <summary>
        /// Return the standard deviation (sqrt(variance)) as defined
        /// from the (biased) population variance.
        /// </summary>
        public double Std => System.Math.Sqrt(Var);

        /// <summary>Return the standard deviation.</summary>
        public double StdDev => Std;

        /// <summary>Print statistics to string.</summary>
        public override string Print(string name = "", int precision = 3)
        {
            var sb = new StringBuilder();
            sb.Append("Statistics ");
            if (!string.IsNullOrEmpty(name)) sb.Append($"for \"{name}\" ");
            if (_size > 0)
            {
                sb.AppendLine($"with {_size} samples:");
                string formatStr = "F" + precision;
                sb.Append($"  Min={_min.ToString(formatStr)}, Max={_max.ToString(formatStr)}, ");
                sb.Append($"Ave={_avg.ToString(formatStr)}, Std={StdDev.ToString(formatStr)}, Var={Variance.ToString(formatStr)}");
            }
            else
            {
                sb.Append(": no samples were added.");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// This class computes a histogram, with a fixed interval width,
    /// of a population of floating-point values.
    /// </summary>
    public class Histogram
    {
        // Small epsilon to ensure max value falls within the histogram range
        private const double MaxBoundaryEpsilon = 1e-10;
        
        private ulong _size;
        private double _min;
        private double _max;
        private double _delta;
        private List<ulong> _bins;

        /// <summary>
        /// Construct with given minimum and maximum values and the given bin count.
        /// </summary>
        public Histogram(double min, double max, int numBins = 10)
        {
            if (max <= min)
                throw new ArgumentException("Histogram: expected min < max");
            if (numBins <= 0)
                throw new ArgumentException("Histogram: expected at least one bin");

            _size = 0;
            _min = min;
            _max = max + MaxBoundaryEpsilon;
            _delta = numBins / (max - min);
            _bins = new List<ulong>(numBins);
            for (int i = 0; i < numBins; ++i)
                _bins.Add(0);
        }

        /// <summary>
        /// Construct with the given bin count and with minimum and maximum values
        /// taken from a Stats object.
        /// </summary>
        public Histogram(Stats stats, int numBins = 10)
            : this(stats.Min, stats.Max, numBins)
        {
        }

        /// <summary>
        /// Add n samples with constant value val, provided that val falls
        /// within this histogram's value range.
        /// </summary>
        /// <returns>true if the sample value falls within this histogram's value range.</returns>
        public bool Add(double val, ulong n = 1)
        {
            if (val < _min || val > _max) return false;
            int binIndex = (int)(_delta * (val - _min));
            if (binIndex >= _bins.Count) binIndex = _bins.Count - 1;
            _bins[binIndex] += n;
            _size += n;
            return true;
        }

        /// <summary>
        /// Add all the contributions from the other histogram, provided that
        /// it has the same configuration as this histogram.
        /// </summary>
        public bool Add(Histogram other)
        {
            if (!MathUtil.IsApproxEqual(_min, other._min) ||
                !MathUtil.IsApproxEqual(_max, other._max) ||
                _bins.Count != other._bins.Count)
                return false;

            for (int i = 0; i < _bins.Count; ++i)
                _bins[i] += other._bins[i];
            _size += other._size;
            return true;
        }

        /// <summary>Return the number of bins in this histogram.</summary>
        public int NumBins => _bins.Count;

        /// <summary>Return the lower bound of this histogram's value range.</summary>
        public double Min => _min;

        /// <summary>Return the upper bound of this histogram's value range.</summary>
        public double Max => _max;

        /// <summary>Return the minimum value in the nth bin.</summary>
        public double MinBin(int n) => _min + n / _delta;

        /// <summary>Return the maximum value in the nth bin.</summary>
        public double MaxBin(int n) => _min + (n + 1) / _delta;

        /// <summary>Return the number of samples in the nth bin.</summary>
        public ulong Count(int n) => _bins[n];

        /// <summary>Return the population size (total number of samples).</summary>
        public ulong Size => _size;

        /// <summary>Print the histogram to string.</summary>
        public string Print(string name = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("Histogram ");
            if (!string.IsNullOrEmpty(name)) sb.Append($"for \"{name}\" ");
            if (_size > 0)
            {
                sb.AppendLine($"with {_size} samples:");
                sb.AppendLine("==============================================================");
                sb.AppendLine("||  #   |       Min      |       Max      | Frequency |  %  ||");
                sb.AppendLine("==============================================================");
                for (int i = 0; i < _bins.Count; ++i)
                {
                    double percentage = 100.0 * _bins[i] / _size;
                    sb.AppendLine($"|| {i,4} | {MinBin(i),14:F6} | {MaxBin(i),14:F6} | {_bins[i],9} | {percentage,3:F0} ||");
                }
                sb.AppendLine("==============================================================");
            }
            else
            {
                sb.AppendLine(": no samples were added.");
            }
            return sb.ToString();
        }
    }
}
