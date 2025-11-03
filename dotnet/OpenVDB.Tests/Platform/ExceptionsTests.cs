// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;
using OpenVDB;

namespace OpenVDB.Tests.Platform
{
    public class ExceptionsTests
    {
        [Fact]
        public void OpenVDBException_DefaultConstructor_ShouldWork()
        {
            var ex = new OpenVDBException();
            Assert.NotNull(ex);
            Assert.IsType<OpenVDBException>(ex);
        }

        [Fact]
        public void OpenVDBException_MessageConstructor_ShouldStoreMessage()
        {
            var ex = new OpenVDBException("test message");
            Assert.Equal("test message", ex.Message);
        }

        [Fact]
        public void OpenVDBException_InnerExceptionConstructor_ShouldStoreInnerException()
        {
            var inner = new Exception("inner");
            var ex = new OpenVDBException("outer", inner);
            Assert.Equal("outer", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void ArithmeticException_ShouldInheritFromOpenVDBException()
        {
            var ex = new ArithmeticException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void IndexException_ShouldInheritFromOpenVDBException()
        {
            var ex = new IndexException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void IoException_ShouldInheritFromOpenVDBException()
        {
            var ex = new IoException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void KeyException_ShouldInheritFromOpenVDBException()
        {
            var ex = new KeyException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void LookupException_ShouldInheritFromOpenVDBException()
        {
            var ex = new LookupException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void NotImplementedException_ShouldInheritFromOpenVDBException()
        {
            var ex = new NotImplementedException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void ReferenceException_ShouldInheritFromOpenVDBException()
        {
            var ex = new ReferenceException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void RuntimeException_ShouldInheritFromOpenVDBException()
        {
            var ex = new RuntimeException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void TypeException_ShouldInheritFromOpenVDBException()
        {
            var ex = new TypeException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }

        [Fact]
        public void ValueException_ShouldInheritFromOpenVDBException()
        {
            var ex = new ValueException("test");
            Assert.IsAssignableFrom<OpenVDBException>(ex);
        }
    }
}
