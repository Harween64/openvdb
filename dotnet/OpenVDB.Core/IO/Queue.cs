// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Queue.cs - C# port of Queue.h and Queue.cc
//
// This file provides the Queue class for asynchronous output of grids to files or streams.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenVDB.Grid;
using OpenVDB.Metadata;

namespace OpenVDB.IO
{
    /// <summary>
    /// Queue for asynchronous output of grids to files or streams.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The queue holds references to grids. It is not safe to modify a grid
    /// that has been placed in the queue. Instead, make a deep copy of the grid first.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var queue = new Queue();
    /// queue.AddNotifier((id, status) => {
    ///     Console.WriteLine($"Task {id} completed with status {status}");
    /// });
    /// 
    /// for (int step = 0; step &lt; 10; step++) {
    ///     var grid = CreateGrid(); // Your grid creation logic
    ///     var id = queue.WriteGrid(grid, new File($"output_{step}.vdb"));
    /// }
    /// 
    /// queue.WaitForCompletion();
    /// </code>
    /// </para>
    /// </remarks>
    public class Queue : IDisposable
    {
        /// <summary>
        /// Unique identifier for a queued task.
        /// </summary>
        public struct Id : IEquatable<Id>
        {
            private readonly long _value;

            internal Id(long value)
            {
                _value = value;
            }

            public bool Equals(Id other) => _value == other._value;
            public override bool Equals(object? obj) => obj is Id other && Equals(other);
            public override int GetHashCode() => _value.GetHashCode();
            public static bool operator ==(Id left, Id right) => left.Equals(right);
            public static bool operator !=(Id left, Id right) => !left.Equals(right);
            public override string ToString() => _value.ToString();
        }

        /// <summary>
        /// Status of a queued task.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// The task is pending execution.
            /// </summary>
            Pending,

            /// <summary>
            /// The task is currently executing.
            /// </summary>
            InProgress,

            /// <summary>
            /// The task completed successfully.
            /// </summary>
            Succeeded,

            /// <summary>
            /// The task failed.
            /// </summary>
            Failed,

            /// <summary>
            /// The task was cancelled.
            /// </summary>
            Cancelled
        }

        /// <summary>
        /// Notification callback for task completion.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="status">The final status of the task.</param>
        public delegate void NotifierCallback(Id id, Status status);

        private class QueuedTask
        {
            public Id Id { get; set; }
            public GridBase Grid { get; set; } = null!;
            public Archive Archive { get; set; } = null!;
            public MetaMap? Metadata { get; set; }
            public Status Status { get; set; }
            public Task? Task { get; set; }
        }

        private long _nextId;
        private readonly ConcurrentDictionary<Id, QueuedTask> _tasks;
        private readonly List<NotifierCallback> _notifiers;
        private readonly object _notifierLock;
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the Queue class.
        /// </summary>
        /// <param name="maxConcurrentTasks">Maximum number of tasks to execute concurrently. Default is the processor count.</param>
        public Queue(int maxConcurrentTasks = -1)
        {
            _nextId = 0;
            _tasks = new ConcurrentDictionary<Id, QueuedTask>();
            _notifiers = new List<NotifierCallback>();
            _notifierLock = new object();
            
            var concurrency = maxConcurrentTasks > 0 ? maxConcurrentTasks : Environment.ProcessorCount;
            _semaphore = new SemaphoreSlim(concurrency, concurrency);
            _cancellationTokenSource = new CancellationTokenSource();
            _disposed = false;
        }

        /// <summary>
        /// Adds a notifier callback that will be called when tasks complete.
        /// </summary>
        /// <param name="notifier">The callback to invoke.</param>
        /// <remarks>
        /// Notifier callbacks must be thread-safe.
        /// </remarks>
        public void AddNotifier(NotifierCallback notifier)
        {
            if (notifier == null)
                throw new ArgumentNullException(nameof(notifier));

            lock (_notifierLock)
            {
                _notifiers.Add(notifier);
            }
        }

        /// <summary>
        /// Removes a notifier callback.
        /// </summary>
        /// <param name="notifier">The callback to remove.</param>
        public void RemoveNotifier(NotifierCallback notifier)
        {
            if (notifier == null)
                return;

            lock (_notifierLock)
            {
                _notifiers.Remove(notifier);
            }
        }

        /// <summary>
        /// Queues a grid for writing to a file.
        /// </summary>
        /// <param name="grid">The grid to write.</param>
        /// <param name="archive">The archive (File or Stream) to write to.</param>
        /// <param name="metadata">Optional file-level metadata.</param>
        /// <returns>A unique ID for this queued task.</returns>
        public Id WriteGrid(GridBase grid, Archive archive, MetaMap? metadata = null)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            var id = new Id(Interlocked.Increment(ref _nextId));
            var task = new QueuedTask
            {
                Id = id,
                Grid = grid,
                Archive = archive,
                Metadata = metadata,
                Status = Status.Pending
            };

            _tasks[id] = task;

            // Start the task asynchronously
            task.Task = Task.Run(async () => await ExecuteTask(task), _cancellationTokenSource.Token);

            return id;
        }

        /// <summary>
        /// Queues multiple grids for writing to a file.
        /// </summary>
        /// <param name="grids">The grids to write.</param>
        /// <param name="archive">The archive (File or Stream) to write to.</param>
        /// <param name="metadata">Optional file-level metadata.</param>
        /// <returns>A unique ID for this queued task.</returns>
        public Id WriteGrids(IEnumerable<GridBase> grids, Archive archive, MetaMap? metadata = null)
        {
            if (grids == null)
                throw new ArgumentNullException(nameof(grids));
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            var id = new Id(Interlocked.Increment(ref _nextId));
            var gridList = new List<GridBase>(grids);

            var task = new QueuedTask
            {
                Id = id,
                Grid = null!, // Not used for multiple grids
                Archive = archive,
                Metadata = metadata,
                Status = Status.Pending
            };

            _tasks[id] = task;

            // Start the task asynchronously
            task.Task = Task.Run(async () => await ExecuteTaskMultiple(task, gridList), _cancellationTokenSource.Token);

            return id;
        }

        /// <summary>
        /// Gets the status of a queued task.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>The current status of the task.</returns>
        public Status GetStatus(Id id)
        {
            if (_tasks.TryGetValue(id, out var task))
            {
                return task.Status;
            }
            return Status.Failed; // Unknown task
        }

        /// <summary>
        /// Waits for all queued tasks to complete.
        /// </summary>
        public void WaitForCompletion()
        {
            var tasks = _tasks.Values.Select(t => t.Task).Where(t => t != null).ToArray();
            Task.WaitAll(tasks!);
        }

        /// <summary>
        /// Waits for all queued tasks to complete with a timeout.
        /// </summary>
        /// <param name="timeout">The maximum time to wait.</param>
        /// <returns>True if all tasks completed within the timeout.</returns>
        public bool WaitForCompletion(TimeSpan timeout)
        {
            var tasks = _tasks.Values.Select(t => t.Task).Where(t => t != null).ToArray();
            return Task.WaitAll(tasks!, timeout);
        }

        /// <summary>
        /// Cancels all pending and in-progress tasks.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Clears all completed tasks from the queue.
        /// </summary>
        public void ClearCompletedTasks()
        {
            var completedIds = _tasks
                .Where(kvp => kvp.Value.Status == Status.Succeeded || 
                              kvp.Value.Status == Status.Failed || 
                              kvp.Value.Status == Status.Cancelled)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in completedIds)
            {
                _tasks.TryRemove(id, out _);
            }
        }

        /// <summary>
        /// Disposes of resources used by the Queue.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            WaitForCompletion();
            _cancellationTokenSource.Dispose();
            _semaphore.Dispose();
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        #region Private Helper Methods

        private async Task ExecuteTask(QueuedTask task)
        {
            await _semaphore.WaitAsync(_cancellationTokenSource.Token);

            try
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    task.Status = Status.Cancelled;
                    NotifyCompletion(task.Id, task.Status);
                    return;
                }

                task.Status = Status.InProgress;

                // Write the grid
                task.Archive.Write(new[] { task.Grid }, task.Metadata);

                task.Status = Status.Succeeded;
            }
            catch (Exception)
            {
                task.Status = Status.Failed;
            }
            finally
            {
                _semaphore.Release();
                NotifyCompletion(task.Id, task.Status);
            }
        }

        private async Task ExecuteTaskMultiple(QueuedTask task, List<GridBase> grids)
        {
            await _semaphore.WaitAsync(_cancellationTokenSource.Token);

            try
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    task.Status = Status.Cancelled;
                    NotifyCompletion(task.Id, task.Status);
                    return;
                }

                task.Status = Status.InProgress;

                // Write the grids
                task.Archive.Write(grids, task.Metadata);

                task.Status = Status.Succeeded;
            }
            catch (Exception)
            {
                task.Status = Status.Failed;
            }
            finally
            {
                _semaphore.Release();
                NotifyCompletion(task.Id, task.Status);
            }
        }

        private void NotifyCompletion(Id id, Status status)
        {
            List<NotifierCallback> notifiersCopy;
            lock (_notifierLock)
            {
                notifiersCopy = new List<NotifierCallback>(_notifiers);
            }

            foreach (var notifier in notifiersCopy)
            {
                try
                {
                    notifier(id, status);
                }
                catch
                {
                    // Ignore exceptions from notifiers
                }
            }
        }

        #endregion
    }
}
