// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from logging.h

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Message severity level
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    /// <summary>
    /// Logging functionality for OpenVDB
    /// </summary>
    /// <remarks>
    /// This is a simplified logging system adapted from the C++ log4cplus implementation.
    /// In production, you may want to integrate with Microsoft.Extensions.Logging.
    /// </remarks>
    public static class Logging
    {
        private static LogLevel _currentLevel = LogLevel.Warn;
        private static string _programName = string.Empty;
        private static bool _useColor = true;
        private static bool _initialized = false;

        // ANSI color codes
        private const string ColorReset = "\u001b[0m";
        private const string ColorGreen = "\u001b[32m";   // Debug
        private const string ColorCyan = "\u001b[36m";    // Info
        private const string ColorMagenta = "\u001b[35m"; // Warn
        private const string ColorRed = "\u001b[31m";     // Error/Fatal

        /// <summary>
        /// Return the current logging level
        /// </summary>
        public static LogLevel GetLevel()
        {
            return _currentLevel;
        }

        /// <summary>
        /// Set the logging level. (Lower-level messages will be suppressed.)
        /// </summary>
        public static void SetLevel(LogLevel level)
        {
            _currentLevel = level;
        }

        /// <summary>
        /// If "-debug", "-info", "-warn", "-error" or "-fatal" is found
        /// in the given array of command-line arguments, set the logging level
        /// appropriately and remove the relevant argument(s) from the array.
        /// </summary>
        public static void SetLevel(ref string[] args)
        {
            var argsList = new List<string>(args);
            
            for (int i = argsList.Count - 1; i >= 0; i--)
            {
                string arg = argsList[i];
                bool remove = true;

                switch (arg)
                {
                    case "-debug":
                        SetLevel(LogLevel.Debug);
                        break;
                    case "-info":
                        SetLevel(LogLevel.Info);
                        break;
                    case "-warn":
                        SetLevel(LogLevel.Warn);
                        break;
                    case "-error":
                        SetLevel(LogLevel.Error);
                        break;
                    case "-fatal":
                        SetLevel(LogLevel.Fatal);
                        break;
                    default:
                        remove = false;
                        break;
                }

                if (remove)
                {
                    argsList.RemoveAt(i);
                }
            }

            args = argsList.ToArray();
        }

        /// <summary>
        /// Specify a program name to be displayed in log messages.
        /// </summary>
        public static void SetProgramName(string progName, bool useColor = true)
        {
            _programName = progName;
            _useColor = useColor;
        }

        /// <summary>
        /// Initialize the logging system if it is not already initialized.
        /// </summary>
        public static void Initialize(bool useColor = true)
        {
            if (_initialized)
                return;

            _useColor = useColor;
            _currentLevel = LogLevel.Warn;
            _programName = string.Empty;
            _initialized = true;
        }

        /// <summary>
        /// Initialize the logging system from command-line arguments.
        /// </summary>
        public static void Initialize(ref string[] args, bool useColor = true)
        {
            Initialize(useColor);
            SetLevel(ref args);

            if (args.Length > 0)
            {
                string progName = args[0];
                int lastSlash = progName.LastIndexOfAny(new[] { '/', '\\' });
                if (lastSlash >= 0)
                {
                    progName = progName.Substring(lastSlash + 1);
                }
                SetProgramName(progName, useColor);
            }
        }

        /// <summary>
        /// Log a message at the specified level
        /// </summary>
        private static void Log(LogLevel level, string message, string file = "", int line = 0)
        {
            if (level < _currentLevel)
                return;

            string colorCode = string.Empty;
            string levelStr;

            if (_useColor)
            {
                colorCode = level switch
                {
                    LogLevel.Debug => ColorGreen,
                    LogLevel.Info => ColorCyan,
                    LogLevel.Warn => ColorMagenta,
                    LogLevel.Error or LogLevel.Fatal => ColorRed,
                    _ => string.Empty
                };
            }

            levelStr = level switch
            {
                LogLevel.Debug => "DEBUG",
                LogLevel.Info => " INFO",
                LogLevel.Warn => " WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Fatal => "FATAL",
                _ => "     "
            };

            string prefix = string.IsNullOrEmpty(_programName) ? string.Empty : $"{_programName} ";
            
            Console.Error.Write(colorCode);
            Console.Error.Write($"{prefix}{levelStr}: {message}");
            Console.Error.Write(ColorReset);
            Console.Error.WriteLine();
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        public static void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Log a fatal error message
        /// </summary>
        public static void LogFatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        /// <summary>
        /// Log a debug message (only in debug builds)
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Log a debug message in both debug and release builds
        /// </summary>
        /// <remarks>Don't use this in performance-critical code.</remarks>
        public static void LogDebugRuntime(string message)
        {
            Log(LogLevel.Debug, message);
        }
    }

    /// <summary>
    /// A LevelScope object sets the logging level to a given level
    /// and restores it to the current level when the object is disposed.
    /// </summary>
    public class LevelScope : IDisposable
    {
        private readonly LogLevel _previousLevel;

        public LevelScope(LogLevel newLevel)
        {
            _previousLevel = Logging.GetLevel();
            Logging.SetLevel(newLevel);
        }

        public void Dispose()
        {
            Logging.SetLevel(_previousLevel);
        }
    }
}
