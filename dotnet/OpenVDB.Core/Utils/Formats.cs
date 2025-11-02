// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from Formats.h and Formats.cc

using System;
using System.IO;
using System.Text;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Utility routines to output nicely-formatted numeric values
    /// </summary>
    public static class Formats
    {
        /// <summary>
        /// Output a byte count with the correct binary suffix (KB, MB, GB or TB).
        /// </summary>
        /// <param name="writer">The output stream</param>
        /// <param name="bytes">The byte count to be output</param>
        /// <param name="head">A string to be output before the numeric text</param>
        /// <param name="tail">A string to be output after the numeric text</param>
        /// <param name="exact">If true, also output the unmodified count, e.g., "4.6 KB (4620 Bytes)"</param>
        /// <param name="width">A fixed width for the numeric text</param>
        /// <param name="precision">The number of digits after the decimal point</param>
        /// <returns>0, 1, 2, 3 or 4, denoting the order of magnitude of the count</returns>
        public static int PrintBytes(TextWriter writer, ulong bytes,
            string head = "",
            string tail = "\n",
            bool exact = false,
            int width = 8,
            int precision = 3)
        {
            const ulong one = 1;
            int group = 0;

            var sb = new StringBuilder();
            sb.Append(head);

            if (bytes >> 40 != 0)
            {
                sb.Append(FormatNumber(bytes / (double)(one << 40), width, precision));
                sb.Append(" TB");
                group = 4;
            }
            else if (bytes >> 30 != 0)
            {
                sb.Append(FormatNumber(bytes / (double)(one << 30), width, precision));
                sb.Append(" GB");
                group = 3;
            }
            else if (bytes >> 20 != 0)
            {
                sb.Append(FormatNumber(bytes / (double)(one << 20), width, precision));
                sb.Append(" MB");
                group = 2;
            }
            else if (bytes >> 10 != 0)
            {
                sb.Append(FormatNumber(bytes / (double)(one << 10), width, precision));
                sb.Append(" KB");
                group = 1;
            }
            else
            {
                sb.Append(bytes.ToString().PadLeft(width));
                sb.Append(" Bytes");
            }

            if (exact && group > 0)
            {
                sb.Append($" ({bytes} Bytes)");
            }
            sb.Append(tail);

            writer.Write(sb.ToString());
            return group;
        }

        /// <summary>
        /// Output a number with the correct SI suffix (thousand, million, billion or trillion)
        /// </summary>
        /// <param name="writer">The output stream</param>
        /// <param name="number">The number to be output</param>
        /// <param name="head">A string to be output before the numeric text</param>
        /// <param name="tail">A string to be output after the numeric text</param>
        /// <param name="exact">If true, also output the unmodified count, e.g., "4.6 Thousand (4620)"</param>
        /// <param name="width">A fixed width for the numeric text</param>
        /// <param name="precision">The number of digits after the decimal point</param>
        /// <returns>0, 1, 2, 3 or 4, denoting the order of magnitude of the number</returns>
        public static int PrintNumber(TextWriter writer, ulong number,
            string head = "",
            string tail = "\n",
            bool exact = true,
            int width = 8,
            int precision = 3)
        {
            int group = 0;

            var sb = new StringBuilder();
            sb.Append(head);

            if (number / 1_000_000_000_000UL != 0)
            {
                sb.Append(FormatNumber(number / 1_000_000_000_000.0, width, precision));
                sb.Append(" trillion");
                group = 4;
            }
            else if (number / 1_000_000_000UL != 0)
            {
                sb.Append(FormatNumber(number / 1_000_000_000.0, width, precision));
                sb.Append(" billion");
                group = 3;
            }
            else if (number / 1_000_000UL != 0)
            {
                sb.Append(FormatNumber(number / 1_000_000.0, width, precision));
                sb.Append(" million");
                group = 2;
            }
            else if (number / 1_000UL != 0)
            {
                sb.Append(FormatNumber(number / 1_000.0, width, precision));
                sb.Append(" thousand");
                group = 1;
            }
            else
            {
                sb.Append(number.ToString().PadLeft(width));
            }

            if (exact && group > 0)
            {
                sb.Append($" ({number})");
            }
            sb.Append(tail);

            writer.Write(sb.ToString());
            return group;
        }

        /// <summary>
        /// Output a time in milliseconds with the correct suffix (days, hours, minutes, seconds and milliseconds)
        /// </summary>
        /// <param name="writer">The output stream</param>
        /// <param name="milliseconds">The time to be output</param>
        /// <param name="head">A string to be output before the time</param>
        /// <param name="tail">A string to be output after the time</param>
        /// <param name="width">A fixed width for the numeric text</param>
        /// <param name="precision">The number of digits after the decimal point</param>
        /// <param name="verbose">Verbose level, 0 is compact format and 1 is long format</param>
        /// <returns>0, 1, 2, 3, or 4 denoting the order of magnitude of the time</returns>
        public static int PrintTime(TextWriter writer, double milliseconds,
            string head = "",
            string tail = "\n",
            int width = 4,
            int precision = 1,
            int verbose = 0)
        {
            int group = 0;

            var sb = new StringBuilder();
            sb.Append(head);

            if (milliseconds >= 1000.0) // one second or longer
            {
                uint seconds = (uint)(milliseconds / 1000.0) % 60;
                uint minutes = (uint)(milliseconds / (1000.0 * 60)) % 60;
                uint hours = (uint)(milliseconds / (1000.0 * 60 * 60)) % 24;
                uint days = (uint)(milliseconds / (1000.0 * 60 * 60 * 24));

                if (days > 0)
                {
                    sb.Append(days);
                    sb.Append(verbose == 0 ? "d " : days > 1 ? " days, " : " day, ");
                    group = 4;
                }

                if (hours > 0)
                {
                    sb.Append(hours);
                    sb.Append(verbose == 0 ? "h " : hours > 1 ? " hours, " : " hour, ");
                    if (group == 0) group = 3;
                }

                if (minutes > 0)
                {
                    sb.Append(minutes);
                    sb.Append(verbose == 0 ? "m " : minutes > 1 ? " minutes, " : " minute, ");
                    if (group == 0) group = 2;
                }

                if (seconds > 0)
                {
                    if (verbose != 0)
                    {
                        sb.Append(seconds);
                        sb.Append(seconds > 1 ? " seconds and " : " second and ");
                        double msec = milliseconds - (seconds + (minutes + (hours + days * 24) * 60) * 60) * 1000.0;
                        sb.Append(FormatNumber(msec, width, precision));
                        sb.Append($" milliseconds ({milliseconds}ms)");
                    }
                    else
                    {
                        double sec = milliseconds / 1000.0 - (minutes + (hours + days * 24) * 60) * 60;
                        sb.Append(FormatNumber(sec, width, precision));
                        sb.Append("s");
                    }
                }
                else // zero seconds
                {
                    double msec = milliseconds - (minutes + (hours + days * 24) * 60) * 60 * 1000.0;
                    if (verbose != 0)
                    {
                        sb.Append(FormatNumber(msec, width, precision));
                        sb.Append($" milliseconds ({milliseconds}ms)");
                    }
                    else
                    {
                        sb.Append(FormatNumber(msec, width, precision));
                        sb.Append("ms");
                    }
                }

                if (group == 0) group = 1;
            }
            else // less than a second
            {
                sb.Append(FormatNumber(milliseconds, width, precision));
                sb.Append(verbose != 0 ? " milliseconds" : "ms");
            }

            sb.Append(tail);
            writer.Write(sb.ToString());
            return group;
        }

        /// <summary>
        /// Helper method to format a number with specified width and precision
        /// </summary>
        private static string FormatNumber(double value, int width, int precision)
        {
            string format = $"F{precision}";
            string result = value.ToString(format);
            return result.PadLeft(width);
        }
    }

    /// <summary>
    /// I/O manipulator that formats integer values with thousands separators
    /// </summary>
    /// <typeparam name="T">Integer type</typeparam>
    public class FormattedInt<T> where T : struct
    {
        private readonly T _value;

        public FormattedInt(T value)
        {
            _value = value;
        }

        public static char Separator => ',';

        public override string ToString()
        {
            // Convert the integer to a string
            string s = _value.ToString() ?? "";

            // Prefix the string with spaces if its length is not a multiple of three
            int padding = (s.Length % 3) != 0 ? 3 - (s.Length % 3) : 0;
            s = new string(' ', padding) + s;

            // Construct a new string in which groups of three digits are followed by a separator
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                sb.Append(s[i]);
                if (i >= padding && (i + 1) % 3 == 0 && i < s.Length - 1)
                {
                    sb.Append(Separator);
                }
            }

            // Remove any padding that was added
            string result = sb.ToString();
            return result.Substring(padding);
        }
    }

    /// <summary>
    /// Helper method to create a FormattedInt
    /// </summary>
    public static class FormattedIntExtensions
    {
        public static FormattedInt<T> AsFormattedInt<T>(this T value) where T : struct
        {
            return new FormattedInt<T>(value);
        }
    }
}
