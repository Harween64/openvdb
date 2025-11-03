// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Compression.cs - C# port of Compression.h and Compression.cc
//
// This file provides compression-related constants and utilities.

using System;
using System.Text;

namespace OpenVDB.IO
{
    /// <summary>
    /// Compression option flags that can be OR-ed together.
    /// </summary>
    [Flags]
    public enum CompressionFlags : uint
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// ZLIB compression for internal and leaf node value buffers.
        /// ZLIB compresses well but is slow.
        /// </summary>
        Zip = 0x1,

        /// <summary>
        /// Don't output inactive values if a node has two or fewer distinct values.
        /// Allows lossless reconstruction of inactive values.
        /// </summary>
        ActiveMask = 0x2,

        /// <summary>
        /// Blosc compression for internal and leaf node value buffers.
        /// Blosc is much faster than ZLIB and produces comparable file sizes.
        /// </summary>
        Blosc = 0x4
    }

    /// <summary>
    /// Per-node metadata indicator for inactive value compression.
    /// </summary>
    internal enum InactiveValueCompression : byte
    {
        /// <summary>
        /// No inactive values, or all inactive values are +background.
        /// </summary>
        NoMaskOrInactiveVals = 0,

        /// <summary>
        /// All inactive values are -background.
        /// </summary>
        NoMaskAndMinusBg = 1,

        /// <summary>
        /// All inactive values have the same non-background value.
        /// </summary>
        NoMaskAndOneInactiveVal = 2,

        /// <summary>
        /// Mask selects between -background and +background.
        /// </summary>
        MaskAndNoInactiveVals = 3,

        /// <summary>
        /// Mask selects between background and one other inactive value.
        /// </summary>
        MaskAndOneInactiveVal = 4,

        /// <summary>
        /// Mask selects between two non-background inactive values.
        /// </summary>
        MaskAndTwoInactiveVals = 5,

        /// <summary>
        /// More than 2 inactive values, so no mask compression.
        /// </summary>
        NoMaskAndAllVals = 6
    }

    /// <summary>
    /// Compression utilities.
    /// </summary>
    public static class CompressionUtilities
    {
        /// <summary>
        /// Convert compression flags to a descriptive string.
        /// </summary>
        /// <param name="flags">The compression flags.</param>
        /// <returns>A string describing the compression settings.</returns>
        public static string CompressionToString(uint flags)
        {
            if (flags == 0)
            {
                return "none";
            }

            var sb = new StringBuilder();
            bool first = true;

            if ((flags & (uint)CompressionFlags.Zip) != 0)
            {
                if (!first) sb.Append(" | ");
                sb.Append("zip");
                first = false;
            }

            if ((flags & (uint)CompressionFlags.Blosc) != 0)
            {
                if (!first) sb.Append(" | ");
                sb.Append("blosc");
                first = false;
            }

            if ((flags & (uint)CompressionFlags.ActiveMask) != 0)
            {
                if (!first) sb.Append(" | ");
                sb.Append("active_mask");
                first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Check if Blosc compression is available.
        /// </summary>
        /// <returns>True if Blosc compression is supported.</returns>
        /// <remarks>
        /// In C#, we would need to reference a Blosc library.
        /// For now, this returns false until a compression library is integrated.
        /// </remarks>
        public static bool HasBloscCompression()
        {
            // TODO: Integrate a Blosc compression library for C#
            return false;
        }

        /// <summary>
        /// Check if ZLib compression is available.
        /// </summary>
        /// <returns>True if ZLib compression is supported.</returns>
        /// <remarks>
        /// .NET has built-in support for deflate/gzip which is compatible with zlib.
        /// </remarks>
        public static bool HasZLibCompression()
        {
            // .NET has System.IO.Compression.DeflateStream
            return true;
        }

        /// <summary>
        /// Compress data using ZLib (Deflate).
        /// </summary>
        /// <param name="input">The input data to compress.</param>
        /// <returns>The compressed data.</returns>
        public static byte[] CompressZip(byte[] input)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            using var outputStream = new System.IO.MemoryStream();
            using (var compressionStream = new System.IO.Compression.DeflateStream(
                outputStream, System.IO.Compression.CompressionMode.Compress))
            {
                compressionStream.Write(input, 0, input.Length);
            }
            return outputStream.ToArray();
        }

        /// <summary>
        /// Decompress data using ZLib (Deflate).
        /// </summary>
        /// <param name="input">The compressed data.</param>
        /// <param name="expectedSize">The expected size of the decompressed data.</param>
        /// <returns>The decompressed data.</returns>
        public static byte[] DecompressZip(byte[] input, int expectedSize)
        {
            if (input == null || input.Length == 0)
                return Array.Empty<byte>();

            using var inputStream = new System.IO.MemoryStream(input);
            using var decompressionStream = new System.IO.Compression.DeflateStream(
                inputStream, System.IO.Compression.CompressionMode.Decompress);
            using var outputStream = new System.IO.MemoryStream(expectedSize);
            
            decompressionStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        /// <summary>
        /// Compress data using Blosc.
        /// </summary>
        /// <param name="input">The input data to compress.</param>
        /// <returns>The compressed data.</returns>
        /// <exception cref="NotSupportedException">Blosc compression is not yet supported.</exception>
        public static byte[] CompressBlosc(byte[] input)
        {
            // TODO: Integrate Blosc compression library
            throw new NotSupportedException("Blosc compression is not yet implemented in the C# port.");
        }

        /// <summary>
        /// Decompress data using Blosc.
        /// </summary>
        /// <param name="input">The compressed data.</param>
        /// <param name="expectedSize">The expected size of the decompressed data.</param>
        /// <returns>The decompressed data.</returns>
        /// <exception cref="NotSupportedException">Blosc compression is not yet supported.</exception>
        public static byte[] DecompressBlosc(byte[] input, int expectedSize)
        {
            // TODO: Integrate Blosc compression library
            throw new NotSupportedException("Blosc decompression is not yet implemented in the C# port.");
        }
    }
}
