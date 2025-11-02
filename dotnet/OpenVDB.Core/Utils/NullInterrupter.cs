// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from NullInterrupter.h

namespace OpenVDB.Utils
{
    /// <summary>
    /// Base class for interrupters
    /// 
    /// The host application calls Start() at the beginning of an interruptible operation,
    /// End() at the end of the operation, and WasInterrupted() periodically during the
    /// operation.
    /// If any call to WasInterrupted() returns true, the operation will be aborted.
    /// </summary>
    /// <remarks>
    /// It's important to not call WasInterrupted() too frequently so as to balance 
    /// performance and the ability to interrupt an operation.
    /// </remarks>
    public class NullInterrupter
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NullInterrupter()
        {
        }

        /// <summary>
        /// Signal the start of an interruptible operation.
        /// </summary>
        /// <param name="name">An optional descriptive name for the operation</param>
        public virtual void Start(string? name = null)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Signal the end of an interruptible operation.
        /// </summary>
        public virtual void End()
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Check if an interruptible operation should be aborted.
        /// </summary>
        /// <param name="percent">An optional percentage indicating the fraction of the operation that has been completed (when >= 0)</param>
        /// <returns>True if the operation should be aborted</returns>
        /// <remarks>This method is assumed to be thread-safe.</remarks>
        public virtual bool WasInterrupted(int percent = -1)
        {
            return false;
        }

        /// <summary>
        /// Convenience method to return a reference to the base class from a derived class.
        /// </summary>
        /// <returns>Reference to this NullInterrupter</returns>
        public NullInterrupter Interrupter()
        {
            return this;
        }
    }

    /// <summary>
    /// Utility methods for interrupters
    /// </summary>
    public static class InterrupterUtils
    {
        /// <summary>
        /// This method is primarily for backwards-compatibility
        /// </summary>
        /// <typeparam name="T">Interrupter type</typeparam>
        /// <param name="interrupter">The interrupter to check (can be null)</param>
        /// <param name="percent">An optional percentage indicating progress</param>
        /// <returns>True if interrupter is not null and was interrupted</returns>
        public static bool WasInterrupted<T>(T? interrupter, int percent = -1) 
            where T : NullInterrupter
        {
            return interrupter != null && interrupter.WasInterrupted(percent);
        }
    }
}
