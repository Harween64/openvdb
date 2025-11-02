// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Threading.cs - C# port of Threading.h
//
// This file provides threading utilities for OpenVDB, adapting TBB (Thread Building Blocks)
// concepts to .NET's Task Parallel Library (TPL).

using System;
using System.Threading;

namespace OpenVDB.Threading
{
    /// <summary>
    /// Threading utilities for OpenVDB operations.
    /// </summary>
    /// <remarks>
    /// In C++, OpenVDB uses Intel's Threading Building Blocks (TBB) for parallel operations.
    /// This C# port adapts those concepts to .NET's Task Parallel Library (TPL) using
    /// CancellationToken and related constructs.
    /// 
    /// The C++ version provides task group cancellation through TBB's task API:
    /// - cancelGroupExecution() - Cancels the current task group
    /// - isGroupExecutionCancelled() - Checks if cancellation was requested
    /// 
    /// In .NET, we use thread-local CancellationTokenSource instances to achieve
    /// similar functionality with the TPL.
    /// </remarks>
    public static class Threading
    {
        /// <summary>
        /// Thread-local storage for the current operation's CancellationTokenSource.
        /// </summary>
        /// <remarks>
        /// This allows each parallel operation to manage its own cancellation state,
        /// similar to how TBB manages task groups.
        /// </remarks>
        private static readonly ThreadLocal<CancellationTokenSource?> _currentCancellationTokenSource =
            new ThreadLocal<CancellationTokenSource?>();

        /// <summary>
        /// Gets or sets the current CancellationTokenSource for the calling thread.
        /// </summary>
        /// <remarks>
        /// This property should be set at the beginning of a parallel operation
        /// and cleared when the operation completes. It enables child tasks to
        /// access the cancellation token for their parent operation.
        /// </remarks>
        public static CancellationTokenSource? CurrentCancellationTokenSource
        {
            get => _currentCancellationTokenSource.Value;
            set => _currentCancellationTokenSource.Value = value;
        }

        /// <summary>
        /// Gets the CancellationToken for the current thread's operation.
        /// </summary>
        /// <remarks>
        /// Returns CancellationToken.None if no cancellation token source is set.
        /// </remarks>
        public static CancellationToken CurrentCancellationToken =>
            CurrentCancellationTokenSource?.Token ?? CancellationToken.None;

        /// <summary>
        /// Request cancellation of the current task group execution.
        /// </summary>
        /// <returns>
        /// True if a cancellation token source was available and cancellation was requested;
        /// false otherwise.
        /// </returns>
        /// <remarks>
        /// This is equivalent to TBB's task::self().cancel_group_execution() in older TBB versions,
        /// or task::current_context()->cancel_group_execution() in TBB 2021+.
        /// 
        /// In the .NET port, this cancels the CancellationTokenSource associated with the
        /// current thread's operation. Parallel tasks should periodically check
        /// IsGroupExecutionCancelled() or use the CancellationToken to respond to cancellation.
        /// </remarks>
        public static bool CancelGroupExecution()
        {
            var cts = CurrentCancellationTokenSource;
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the current task group execution has been cancelled.
        /// </summary>
        /// <returns>
        /// True if cancellation has been requested for the current operation;
        /// false otherwise.
        /// </returns>
        /// <remarks>
        /// This is equivalent to TBB's task::self().is_cancelled() in older TBB versions,
        /// or task::current_context()->is_group_execution_cancelled() in TBB 2021+.
        /// 
        /// In the .NET port, this checks the CancellationToken associated with the
        /// current thread's operation. This method is thread-safe and can be called
        /// frequently from within parallel operations to check for cancellation.
        /// </remarks>
        public static bool IsGroupExecutionCancelled()
        {
            var cts = CurrentCancellationTokenSource;
            return cts?.IsCancellationRequested ?? false;
        }

        /// <summary>
        /// Helper class to automatically manage CancellationTokenSource for a scope.
        /// </summary>
        /// <remarks>
        /// Usage:
        /// <code>
        /// using (var scope = new CancellationScope())
        /// {
        ///     Parallel.For(0, count, new ParallelOptions 
        ///     { 
        ///         CancellationToken = scope.Token 
        ///     }, i =>
        ///     {
        ///         // Work here
        ///         if (someCondition)
        ///             Threading.CancelGroupExecution();
        ///     });
        /// }
        /// </code>
        /// </remarks>
        public sealed class CancellationScope : IDisposable
        {
            private readonly CancellationTokenSource _cts;
            private readonly CancellationTokenSource? _previousCts;

            /// <summary>
            /// Gets the CancellationToken for this scope.
            /// </summary>
            public CancellationToken Token => _cts.Token;

            /// <summary>
            /// Initializes a new CancellationScope and sets it as the current scope.
            /// </summary>
            public CancellationScope()
            {
                _cts = new CancellationTokenSource();
                _previousCts = CurrentCancellationTokenSource;
                CurrentCancellationTokenSource = _cts;
            }

            /// <summary>
            /// Disposes the CancellationScope and restores the previous scope.
            /// </summary>
            public void Dispose()
            {
                CurrentCancellationTokenSource = _previousCts;
                _cts.Dispose();
            }

            /// <summary>
            /// Requests cancellation for this scope.
            /// </summary>
            public void Cancel()
            {
                _cts.Cancel();
            }

            /// <summary>
            /// Gets whether cancellation has been requested for this scope.
            /// </summary>
            public bool IsCancellationRequested => _cts.IsCancellationRequested;
        }
    }
}
