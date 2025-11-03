// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using Xunit;
using OpenVDB.Platform;

namespace OpenVDB.Tests.Platform
{
    public class PlatformConfigTests
    {
        [Fact]
        public void VersionName_ShouldBeCorrect()
        {
            Assert.Equal("v11_0_0", PlatformConfig.VersionName);
        }
    }

    public class InteropHelpersTests
    {
        [Fact]
        public void IsDynamicLibrary_ShouldBeTrue()
        {
            Assert.True(InteropHelpers.IsDynamicLibrary);
        }

        [Fact]
        public void SuppressUnusedWarning_ShouldNotThrow()
        {
            // Should not throw for any type
            InteropHelpers.SuppressUnusedWarning(42);
            InteropHelpers.SuppressUnusedWarning("test");
            InteropHelpers.SuppressUnusedWarning(new object());
        }
    }

    public class ThreadSafeStaticAttributeTests
    {
        [Fact]
        public void Constructor_WithoutParameter_ShouldWork()
        {
            var attr = new ThreadSafeStaticAttribute();
            Assert.Null(attr.AccessType);
        }

        [Fact]
        public void Constructor_WithParameter_ShouldStoreAccessType()
        {
            var attr = new ThreadSafeStaticAttribute("reference");
            Assert.Equal("reference", attr.AccessType);
        }
    }

    public class NonThreadSafeStaticAttributeTests
    {
        [Fact]
        public void Constructor_WithoutParameter_ShouldWork()
        {
            var attr = new NonThreadSafeStaticAttribute();
            Assert.Null(attr.AccessType);
        }

        [Fact]
        public void Constructor_WithParameter_ShouldStoreAccessType()
        {
            var attr = new NonThreadSafeStaticAttribute("write");
            Assert.Equal("write", attr.AccessType);
        }
    }

    public class PlatformInitTests
    {
        [Fact]
        public void Initialize_ShouldSetIsInitialized()
        {
            PlatformInit.Initialize();
            Assert.True(PlatformInit.IsInitialized);
        }

        [Fact]
        public void Initialize_CalledMultipleTimes_ShouldBeIdempotent()
        {
            PlatformInit.Initialize();
            PlatformInit.Initialize();
            PlatformInit.Initialize();
            Assert.True(PlatformInit.IsInitialized);
        }
    }
}
