// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from Name.h

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Type alias for name strings
    /// </summary>
    public class Name : IEquatable<Name>
    {
        private readonly string _value;

        public Name(string value)
        {
            _value = value ?? string.Empty;
        }

        public static implicit operator string(Name name) => name._value;
        public static implicit operator Name(string value) => new Name(value);

        public override string ToString() => _value;
        public bool Equals(Name? other) => other != null && _value == other._value;
        public override bool Equals(object? obj) => obj is Name name && Equals(name);
        public override int GetHashCode() => _value.GetHashCode();
    }

    /// <summary>
    /// String I/O utilities
    /// </summary>
    public static class NameIO
    {
        /// <summary>
        /// Read a string from a binary stream
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>The read string</returns>
        public static string ReadString(BinaryReader reader)
        {
            uint size = reader.ReadUInt32();
            if (size == 0)
                return string.Empty;
            
            byte[] buffer = reader.ReadBytes((int)size);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Write a string to a binary stream
        /// </summary>
        /// <param name="writer">The binary writer</param>
        /// <param name="name">The string to write</param>
        public static void WriteString(BinaryWriter writer, string name)
        {
            uint size = (uint)name.Length;
            writer.Write(size);
            if (size > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(name);
                writer.Write(bytes);
            }
        }
    }

    /// <summary>
    /// String manipulation utilities
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Split a string by a delimiter character
        /// </summary>
        /// <typeparam name="TContainer">The container type (e.g., List&lt;string&gt;)</typeparam>
        /// <param name="input">The input string</param>
        /// <param name="delimiter">The delimiter character</param>
        /// <returns>Collection of split strings</returns>
        public static List<string> Split(string input, char delimiter)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            result.AddRange(input.Split(delimiter));
            return result;
        }

        /// <summary>
        /// Split a string by multiple delimiter characters
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="delimiters">The set of delimiter characters</param>
        /// <returns>Collection of split strings</returns>
        public static List<string> Split(string input, ISet<char> delimiters)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(input))
                return result;

            result.AddRange(input.Split(delimiters.ToArray()));
            return result;
        }

        /// <summary>
        /// Check if a string starts with a given prefix
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="prefix">The prefix to check</param>
        /// <returns>True if input starts with prefix</returns>
        public static bool StartsWith(string input, string prefix)
        {
            if (prefix.Length > input.Length)
                return false;
            return input.StartsWith(prefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Check if a string ends with a given suffix
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="suffix">The suffix to check</param>
        /// <returns>True if input ends with suffix</returns>
        public static bool EndsWith(string input, string suffix)
        {
            if (suffix.Length > input.Length)
                return false;
            return input.EndsWith(suffix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Trim whitespace from both ends of a string (in-place style)
        /// </summary>
        /// <param name="s">The string to trim</param>
        /// <returns>The trimmed string</returns>
        public static string Trim(string s)
        {
            return s.Trim();
        }

        /// <summary>
        /// Convert a string to lowercase (in-place style)
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>The lowercase string</returns>
        public static string ToLower(string s)
        {
            return s.ToLowerInvariant();
        }
    }
}
