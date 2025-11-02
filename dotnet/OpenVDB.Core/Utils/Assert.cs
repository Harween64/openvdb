// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from Assert.h and Assert.cc

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Assertion utilities for OpenVDB.
    /// Provides assertion functionality similar to cassert but independent of NDEBUG define.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Trigger an abort after printing a formatted assertion message.
        /// Effectively performs the same functionality as cassert, but allows
        /// VDB code to call this independently of the NDEBUG define.
        /// </summary>
        /// <param name="assertion">The variable or expression that triggered the assertion as a string</param>
        /// <param name="file">The name of the file the assertion occurred</param>
        /// <param name="line">The line in the file the assertion occurred</param>
        /// <param name="function">The name of the function the assertion occurred in</param>
        /// <param name="message">An optional descriptive message</param>
        [DoesNotReturn]
        public static void AssertAbort(
            string assertion,
            string file,
            int line,
            string function,
            string? message = null)
        {
            Console.Error.Write($"{file}:{line}:");
            Console.Error.Write(" Assertion failed: ");
            Console.Error.Write($"'{assertion}'");
            Console.Error.Write(" in function: ");
            Console.Error.Write($"'{function}'");
            if (message != null)
            {
                Console.Error.Write($"\n{message}");
            }
            Console.Error.WriteLine();
            Console.Error.Flush();
            
            // Abort the application
            Environment.FailFast($"Assertion failed: {assertion}");
        }

        /// <summary>
        /// Assert that a condition is true. If not, abort with detailed information.
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="conditionText">The string representation of the condition</param>
        /// <param name="file">Source file (automatically provided)</param>
        /// <param name="line">Source line (automatically provided)</param>
        /// <param name="member">Member name (automatically provided)</param>
        [Conditional("OPENVDB_ENABLE_ASSERTS")]
        public static void AssertCondition(
            bool condition,
            string conditionText,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            if (!condition)
            {
                AssertAbort(conditionText, file, line, member);
            }
        }

        /// <summary>
        /// Assert that a condition is true with a custom message. If not, abort with detailed information.
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="conditionText">The string representation of the condition</param>
        /// <param name="message">A descriptive message</param>
        /// <param name="file">Source file (automatically provided)</param>
        /// <param name="line">Source line (automatically provided)</param>
        /// <param name="member">Member name (automatically provided)</param>
        [Conditional("OPENVDB_ENABLE_ASSERTS")]
        public static void AssertConditionMessage(
            bool condition,
            string conditionText,
            string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            if (!condition)
            {
                AssertAbort(conditionText, file, line, member, message);
            }
        }
    }

    /// <summary>
    /// Helper class to provide OPENVDB_ASSERT macro-like functionality
    /// </summary>
    public static class AssertMacros
    {
        /// <summary>
        /// Check if condition is likely true (optimization hint)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Likely(bool condition)
        {
            return condition;
        }
    }
}
