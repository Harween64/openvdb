// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from NodeMasks.h

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Bit counting and manipulation utilities
    /// </summary>
    public static class BitUtils
    {
        /// <summary>
        /// Return the number of on bits in the given 8-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOn(byte v)
        {
            return (uint)BitOperations.PopCount(v);
        }

        /// <summary>
        /// Return the number of off bits in the given 8-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOff(byte v)
        {
            return CountOn((byte)~v);
        }

        /// <summary>
        /// Return the number of on bits in the given 32-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOn(uint v)
        {
            return (uint)BitOperations.PopCount(v);
        }

        /// <summary>
        /// Return the number of off bits in the given 32-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOff(uint v)
        {
            return CountOn(~v);
        }

        /// <summary>
        /// Return the number of on bits in the given 64-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOn(ulong v)
        {
            return (uint)BitOperations.PopCount(v);
        }

        /// <summary>
        /// Return the number of off bits in the given 64-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CountOff(ulong v)
        {
            return CountOn(~v);
        }

        /// <summary>
        /// Return the least significant on bit of the given 8-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FindLowestOn(byte v)
        {
            if (v == 0) throw new InvalidOperationException("Cannot find lowest on bit in zero value");
            return (uint)BitOperations.TrailingZeroCount(v);
        }

        /// <summary>
        /// Return the least significant on bit of the given 32-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FindLowestOn(uint v)
        {
            if (v == 0) throw new InvalidOperationException("Cannot find lowest on bit in zero value");
            return (uint)BitOperations.TrailingZeroCount(v);
        }

        /// <summary>
        /// Return the least significant on bit of the given 64-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FindLowestOn(ulong v)
        {
            if (v == 0) throw new InvalidOperationException("Cannot find lowest on bit in zero value");
            return (uint)BitOperations.TrailingZeroCount(v);
        }

        /// <summary>
        /// Return the most significant on bit of the given 32-bit value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FindHighestOn(uint v)
        {
            if (v == 0) throw new InvalidOperationException("Cannot find highest on bit in zero value");
            return (uint)BitOperations.Log2(v);
        }
    }

    /// <summary>
    /// Bit mask for the internal and leaf nodes of VDB. This is a 64-bit implementation.
    /// </summary>
    /// <typeparam name="TLog2Dim">Log base 2 of the dimension</typeparam>
    public class NodeMask<TLog2Dim> where TLog2Dim : struct
    {
        // These need to be set based on the TLog2Dim parameter
        // For now, we'll use a concrete implementation that can be specialized
        // This is a placeholder that would need proper generic math in C# 11+
        
        public const uint Log2Dim = 3; // This should be TLog2Dim value
        public const uint Dim = 1u << (int)Log2Dim;
        public const uint Size = 1u << (int)(3 * Log2Dim);
        public const uint WordCount = Size >> 6; // 2^6=64
        
        private readonly ulong[] _words;

        /// <summary>
        /// Default constructor sets all bits off
        /// </summary>
        public NodeMask()
        {
            _words = new ulong[WordCount];
        }

        /// <summary>
        /// All bits are set to the specified state
        /// </summary>
        public NodeMask(bool on) : this()
        {
            Set(on);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public NodeMask(NodeMask<TLog2Dim> other)
        {
            _words = new ulong[WordCount];
            Array.Copy(other._words, _words, WordCount);
        }

        /// <summary>
        /// Set all bits to the specified state
        /// </summary>
        public void Set(bool on)
        {
            ulong state = on ? ~0UL : 0UL;
            for (int i = 0; i < WordCount; i++)
            {
                _words[i] = state;
            }
        }

        /// <summary>
        /// Set all bits off
        /// </summary>
        public void SetOff()
        {
            Array.Clear(_words, 0, (int)WordCount);
        }

        /// <summary>
        /// Set all bits on
        /// </summary>
        public void SetOn()
        {
            Set(true);
        }

        /// <summary>
        /// Toggle all bits
        /// </summary>
        public void Toggle()
        {
            for (int i = 0; i < WordCount; i++)
            {
                _words[i] = ~_words[i];
            }
        }

        /// <summary>
        /// Set the specified bit on
        /// </summary>
        public void SetOn(uint n)
        {
            if (n >= Size) throw new ArgumentOutOfRangeException(nameof(n));
            _words[n >> 6] |= 1UL << (int)(n & 63);
        }

        /// <summary>
        /// Set the specified bit off
        /// </summary>
        public void SetOff(uint n)
        {
            if (n >= Size) throw new ArgumentOutOfRangeException(nameof(n));
            _words[n >> 6] &= ~(1UL << (int)(n & 63));
        }

        /// <summary>
        /// Set the specified bit to the given state
        /// </summary>
        public void Set(uint n, bool on)
        {
            if (on)
                SetOn(n);
            else
                SetOff(n);
        }

        /// <summary>
        /// Toggle the state of the specified bit
        /// </summary>
        public void Toggle(uint n)
        {
            if (n >= Size) throw new ArgumentOutOfRangeException(nameof(n));
            _words[n >> 6] ^= 1UL << (int)(n & 63);
        }

        /// <summary>
        /// Check if the specified bit is on
        /// </summary>
        public bool IsOn(uint n)
        {
            if (n >= Size) throw new ArgumentOutOfRangeException(nameof(n));
            return (_words[n >> 6] & (1UL << (int)(n & 63))) != 0;
        }

        /// <summary>
        /// Check if the specified bit is off
        /// </summary>
        public bool IsOff(uint n)
        {
            return !IsOn(n);
        }

        /// <summary>
        /// Return the total number of on bits
        /// </summary>
        public uint CountOn()
        {
            uint sum = 0;
            for (int i = 0; i < WordCount; i++)
            {
                sum += BitUtils.CountOn(_words[i]);
            }
            return sum;
        }

        /// <summary>
        /// Return the total number of off bits
        /// </summary>
        public uint CountOff()
        {
            return Size - CountOn();
        }

        /// <summary>
        /// Check if all bits are on
        /// </summary>
        public bool IsOn()
        {
            return CountOn() == Size;
        }

        /// <summary>
        /// Check if all bits are off
        /// </summary>
        public bool IsOff()
        {
            return CountOn() == 0;
        }

        /// <summary>
        /// Find the index of the first on bit
        /// </summary>
        public uint FindFirstOn()
        {
            for (uint i = 0; i < WordCount; i++)
            {
                if (_words[i] != 0)
                {
                    return (i << 6) + BitUtils.FindLowestOn(_words[i]);
                }
            }
            return Size; // No on bits found
        }

        /// <summary>
        /// Find the index of the first off bit
        /// </summary>
        public uint FindFirstOff()
        {
            for (uint i = 0; i < WordCount; i++)
            {
                if (_words[i] != ~0UL)
                {
                    return (i << 6) + BitUtils.FindLowestOn(~_words[i]);
                }
            }
            return Size; // No off bits found
        }

        /// <summary>
        /// Find the next on bit starting from the specified position
        /// </summary>
        public uint FindNextOn(uint start)
        {
            if (start >= Size) return Size;
            
            uint wordIdx = start >> 6;
            uint bitIdx = start & 63;
            
            // Check remaining bits in current word
            ulong word = _words[wordIdx] & (~0UL << (int)bitIdx);
            if (word != 0)
            {
                return (wordIdx << 6) + BitUtils.FindLowestOn(word);
            }
            
            // Check subsequent words
            for (uint i = wordIdx + 1; i < WordCount; i++)
            {
                if (_words[i] != 0)
                {
                    return (i << 6) + BitUtils.FindLowestOn(_words[i]);
                }
            }
            
            return Size;
        }

        /// <summary>
        /// Find the next off bit starting from the specified position
        /// </summary>
        public uint FindNextOff(uint start)
        {
            if (start >= Size) return Size;
            
            uint wordIdx = start >> 6;
            uint bitIdx = start & 63;
            
            // Check remaining bits in current word
            ulong word = (~_words[wordIdx]) & (~0UL << (int)bitIdx);
            if (word != 0)
            {
                return (wordIdx << 6) + BitUtils.FindLowestOn(word);
            }
            
            // Check subsequent words
            for (uint i = wordIdx + 1; i < WordCount; i++)
            {
                if (_words[i] != ~0UL)
                {
                    return (i << 6) + BitUtils.FindLowestOn(~_words[i]);
                }
            }
            
            return Size;
        }

        /// <summary>
        /// Bitwise AND operation
        /// </summary>
        public static NodeMask<TLog2Dim> operator &(NodeMask<TLog2Dim> a, NodeMask<TLog2Dim> b)
        {
            var result = new NodeMask<TLog2Dim>();
            for (int i = 0; i < WordCount; i++)
            {
                result._words[i] = a._words[i] & b._words[i];
            }
            return result;
        }

        /// <summary>
        /// Bitwise OR operation
        /// </summary>
        public static NodeMask<TLog2Dim> operator |(NodeMask<TLog2Dim> a, NodeMask<TLog2Dim> b)
        {
            var result = new NodeMask<TLog2Dim>();
            for (int i = 0; i < WordCount; i++)
            {
                result._words[i] = a._words[i] | b._words[i];
            }
            return result;
        }

        /// <summary>
        /// Bitwise XOR operation
        /// </summary>
        public static NodeMask<TLog2Dim> operator ^(NodeMask<TLog2Dim> a, NodeMask<TLog2Dim> b)
        {
            var result = new NodeMask<TLog2Dim>();
            for (int i = 0; i < WordCount; i++)
            {
                result._words[i] = a._words[i] ^ b._words[i];
            }
            return result;
        }

        /// <summary>
        /// Bitwise NOT operation
        /// </summary>
        public static NodeMask<TLog2Dim> operator !(NodeMask<TLog2Dim> a)
        {
            var result = new NodeMask<TLog2Dim>(a);
            result.Toggle();
            return result;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public static bool operator ==(NodeMask<TLog2Dim>? a, NodeMask<TLog2Dim>? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            
            for (int i = 0; i < WordCount; i++)
            {
                if (a._words[i] != b._words[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Inequality comparison
        /// </summary>
        public static bool operator !=(NodeMask<TLog2Dim>? a, NodeMask<TLog2Dim>? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is NodeMask<TLog2Dim> mask && this == mask;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (int i = 0; i < WordCount; i++)
            {
                hash.Add(_words[i]);
            }
            return hash.ToHashCode();
        }

        /// <summary>
        /// Return the byte size of this NodeMask
        /// </summary>
        public static uint MemUsage()
        {
            return WordCount * sizeof(ulong);
        }
    }

    // Common NodeMask specializations
    public class NodeMask3 : NodeMask<int> { } // For Log2Dim = 3
    public class NodeMask4 : NodeMask<int> { } // For Log2Dim = 4
    public class NodeMask5 : NodeMask<int> { } // For Log2Dim = 5
}
