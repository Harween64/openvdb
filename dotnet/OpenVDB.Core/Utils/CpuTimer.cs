// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from CpuTimer.h

using System;
using System.Diagnostics;
using System.IO;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Simple timer for basic profiling.
    /// </summary>
    /// <example>
    /// Basic usage:
    /// <code>
    /// var timer = new CpuTimer();
    /// // code here will not be timed!
    /// timer.Start("algorithm");
    /// // code to be timed goes here
    /// timer.Stop();
    /// </code>
    /// 
    /// Or to time multiple blocks of code:
    /// <code>
    /// var timer = new CpuTimer("algorithm 1");
    /// // code to be timed goes here
    /// timer.Restart("algorithm 2");
    /// // code to be timed goes here
    /// timer.Stop();
    /// </code>
    /// 
    /// Or to measure speedup between multiple runs:
    /// <code>
    /// var timer = new CpuTimer("algorithm 1");
    /// // code for the first run goes here
    /// double t1 = timer.Restart("algorithm 2");
    /// // code for the second run goes here
    /// double t2 = timer.Stop();
    /// Console.Error.WriteLine($"Algorithm 1 is {t2/t1} times faster than algorithm 2");
    /// </code>
    /// 
    /// Or to measure multiple blocks with deferred output:
    /// <code>
    /// var timer = new CpuTimer();
    /// // code here will not be timed!
    /// timer.Start();
    /// // code for the first run goes here
    /// double t1 = timer.Restart(); // time in milliseconds
    /// // code for the second run goes here
    /// double t2 = timer.Restart(); // time in milliseconds
    /// // code here will not be timed!
    /// Formats.PrintTime(Console.Out, t1, "Algorithm 1 completed in ");
    /// Formats.PrintTime(Console.Out, t2, "Algorithm 2 completed in ");
    /// </code>
    /// </example>
    public class CpuTimer
    {
        private readonly TextWriter _outStream;
        private long _t0;

        /// <summary>
        /// Initiate timer
        /// </summary>
        /// <param name="outputStream">Output stream for messages (defaults to Console.Error)</param>
        public CpuTimer(TextWriter? outputStream = null)
        {
            _outStream = outputStream ?? Console.Error;
            _t0 = Now();
        }

        /// <summary>
        /// Prints message and starts timer.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="outputStream">Output stream for messages (defaults to Console.Error)</param>
        /// <remarks>Should normally be followed by a call to Stop()</remarks>
        public CpuTimer(string message, TextWriter? outputStream = null)
        {
            _outStream = outputStream ?? Console.Error;
            Start(message);
        }

        /// <summary>
        /// Start timer.
        /// </summary>
        /// <remarks>Should normally be followed by a call to Milliseconds() or Stop(string)</remarks>
        public void Start()
        {
            _t0 = Now();
        }

        /// <summary>
        /// Print message and start timer.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <remarks>Should normally be followed by a call to Stop()</remarks>
        public void Start(string message)
        {
            _outStream.Write($"{message} ...");
            Start();
        }

        /// <summary>
        /// Return time difference in microseconds since construction or Start was called.
        /// </summary>
        /// <returns>Time in microseconds</returns>
        /// <remarks>Combine this method with Start() to get timing without any outputs.</remarks>
        public long Microseconds()
        {
            return Now() - _t0;
        }

        /// <summary>
        /// Return time difference in milliseconds since construction or Start was called.
        /// </summary>
        /// <returns>Time in milliseconds</returns>
        /// <remarks>Combine this method with Start() to get timing without any outputs.</remarks>
        public double Milliseconds()
        {
            const double resolution = 1.0 / 1000.0;
            return Microseconds() * resolution;
        }

        /// <summary>
        /// Return time difference in seconds since construction or Start was called.
        /// </summary>
        /// <returns>Time in seconds</returns>
        /// <remarks>Combine this method with Start() to get timing without any outputs.</remarks>
        public double Seconds()
        {
            const double resolution = 1.0 / 1_000_000.0;
            return Microseconds() * resolution;
        }

        /// <summary>
        /// Get formatted time string
        /// </summary>
        /// <returns>Formatted time string</returns>
        public string Time()
        {
            double msec = Milliseconds();
            var sw = new StringWriter();
            Formats.PrintTime(sw, msec, "", "", 4, 1, 1);
            return sw.ToString();
        }

        /// <summary>
        /// Returns and prints time in milliseconds since construction or Start was called.
        /// </summary>
        /// <returns>Time in milliseconds</returns>
        /// <remarks>Combine this method with Start(string) to print at start and stop of task being timed.</remarks>
        public double Stop()
        {
            double msec = Milliseconds();
            Formats.PrintTime(_outStream, msec, " completed in ", "\n", 4, 3, 1);
            return msec;
        }

        /// <summary>
        /// Returns and prints time in milliseconds since construction or Start was called.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <returns>Time in milliseconds</returns>
        /// <remarks>Combine this method with Start() to delay output of task being timed.</remarks>
        public double Stop(string message)
        {
            double msec = Milliseconds();
            _outStream.Write($"{message} ...");
            Formats.PrintTime(_outStream, msec, " completed in ", "\n", 4, 3, 1);
            return msec;
        }

        /// <summary>
        /// Re-start timer.
        /// </summary>
        /// <returns>Time in milliseconds since previous Start or Restart</returns>
        /// <remarks>Should normally be followed by a call to Stop() or Restart()</remarks>
        public double Restart()
        {
            double msec = Milliseconds();
            Start();
            return msec;
        }

        /// <summary>
        /// Stop previous timer, print message and re-start timer.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <returns>Time in milliseconds since previous Start or Restart</returns>
        /// <remarks>Should normally be followed by a call to Stop() or Restart()</remarks>
        public double Restart(string message)
        {
            double delta = Stop();
            Start(message);
            return delta;
        }

        /// <summary>
        /// Get current time in microseconds
        /// </summary>
        /// <returns>Current time in microseconds</returns>
        private static long Now()
        {
            // Use Stopwatch for high-precision timing
            // Stopwatch.GetTimestamp() is the most accurate way to measure time in .NET
            long timestamp = Stopwatch.GetTimestamp();
            long microseconds = (timestamp * 1_000_000) / Stopwatch.Frequency;
            return microseconds;
        }
    }
}
