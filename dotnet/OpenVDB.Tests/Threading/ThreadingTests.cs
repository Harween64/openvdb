// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OVDBThreading = OpenVDB.Threading.Threading;

namespace OpenVDB.Tests.Threading
{
    public class ThreadingTests
    {
        [Fact]
        public void CurrentCancellationToken_WithNoSource_ShouldReturnNone()
        {
            OVDBThreading.CurrentCancellationTokenSource = null;
            Assert.Equal(CancellationToken.None, OVDBThreading.CurrentCancellationToken);
        }

        [Fact]
        public void CurrentCancellationTokenSource_ShouldBeThreadLocal()
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();
            
            OVDBThreading.CurrentCancellationTokenSource = cts1;
            
            var task = Task.Run(() =>
            {
                OVDBThreading.CurrentCancellationTokenSource = cts2;
                Assert.Same(cts2, OVDBThreading.CurrentCancellationTokenSource);
            });
            
            task.Wait();
            Assert.Same(cts1, OVDBThreading.CurrentCancellationTokenSource);
            
            OVDBThreading.CurrentCancellationTokenSource = null;
        }

        [Fact]
        public void CancelGroupExecution_WithNoSource_ShouldReturnFalse()
        {
            OVDBThreading.CurrentCancellationTokenSource = null;
            Assert.False(OVDBThreading.CancelGroupExecution());
        }

        [Fact]
        public void CancelGroupExecution_WithSource_ShouldCancelAndReturnTrue()
        {
            using var cts = new CancellationTokenSource();
            OVDBThreading.CurrentCancellationTokenSource = cts;
            
            Assert.False(cts.IsCancellationRequested);
            Assert.True(OVDBThreading.CancelGroupExecution());
            Assert.True(cts.IsCancellationRequested);
            
            OVDBThreading.CurrentCancellationTokenSource = null;
        }

        [Fact]
        public void CancelGroupExecution_AlreadyCancelled_ShouldReturnFalse()
        {
            using var cts = new CancellationTokenSource();
            OVDBThreading.CurrentCancellationTokenSource = cts;
            
            cts.Cancel();
            Assert.False(OVDBThreading.CancelGroupExecution());
            
            OVDBThreading.CurrentCancellationTokenSource = null;
        }

        [Fact]
        public void IsGroupExecutionCancelled_WithNoSource_ShouldReturnFalse()
        {
            OVDBThreading.CurrentCancellationTokenSource = null;
            Assert.False(OVDBThreading.IsGroupExecutionCancelled());
        }

        [Fact]
        public void IsGroupExecutionCancelled_WithUncancelledSource_ShouldReturnFalse()
        {
            using var cts = new CancellationTokenSource();
            OVDBThreading.CurrentCancellationTokenSource = cts;
            
            Assert.False(OVDBThreading.IsGroupExecutionCancelled());
            
            OVDBThreading.CurrentCancellationTokenSource = null;
        }

        [Fact]
        public void IsGroupExecutionCancelled_WithCancelledSource_ShouldReturnTrue()
        {
            using var cts = new CancellationTokenSource();
            OVDBThreading.CurrentCancellationTokenSource = cts;
            
            cts.Cancel();
            Assert.True(OVDBThreading.IsGroupExecutionCancelled());
            
            OVDBThreading.CurrentCancellationTokenSource = null;
        }
    }

    public class CancellationScopeTests
    {
        [Fact]
        public void Constructor_ShouldCreateNewCancellationTokenSource()
        {
            using var scope = new OVDBThreading.CancellationScope();
            Assert.NotNull(scope.Token);
            Assert.False(scope.IsCancellationRequested);
        }

        [Fact]
        public void Constructor_ShouldSetCurrentCancellationTokenSource()
        {
            using var scope = new OVDBThreading.CancellationScope();
            Assert.NotNull(OVDBThreading.CurrentCancellationTokenSource);
        }

        [Fact]
        public void Dispose_ShouldRestorePreviousCancellationTokenSource()
        {
            using var cts = new CancellationTokenSource();
            OVDBThreading.CurrentCancellationTokenSource = cts;
            
            using (var scope = new OVDBThreading.CancellationScope())
            {
                Assert.NotSame(cts, OVDBThreading.CurrentCancellationTokenSource);
            }
            
            Assert.Same(cts, OVDBThreading.CurrentCancellationTokenSource);
            OVDBThreading.CurrentCancellationTokenSource = null;
        }

        [Fact]
        public void Cancel_ShouldSetIsCancellationRequested()
        {
            using var scope = new OVDBThreading.CancellationScope();
            Assert.False(scope.IsCancellationRequested);
            
            scope.Cancel();
            Assert.True(scope.IsCancellationRequested);
        }

        [Fact]
        public void NestedScopes_ShouldWorkCorrectly()
        {
            using var scope1 = new OVDBThreading.CancellationScope();
            var token1 = scope1.Token;
            
            using (var scope2 = new OVDBThreading.CancellationScope())
            {
                var token2 = scope2.Token;
                Assert.NotEqual(token1, token2);
                
                scope2.Cancel();
                Assert.True(scope2.IsCancellationRequested);
                Assert.False(scope1.IsCancellationRequested);
            }
            
            Assert.False(scope1.IsCancellationRequested);
        }

        [Fact]
        public void ParallelFor_WithCancellation_ShouldWork()
        {
            using var scope = new OVDBThreading.CancellationScope();
            int counter = 0;
            bool cancelled = false;
            
            try
            {
                Parallel.For(0, 1000, new ParallelOptions 
                { 
                    CancellationToken = scope.Token 
                }, i =>
                {
                    Interlocked.Increment(ref counter);
                    
                    if (i == 10)
                    {
                        scope.Cancel();
                    }
                });
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }
            
            Assert.True(cancelled || counter < 1000);
        }
    }
}
