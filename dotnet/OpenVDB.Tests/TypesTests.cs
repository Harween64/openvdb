// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using Xunit;
using OpenVDB;

namespace OpenVDB.Tests
{
    public class TypesTests
    {
        [Fact]
        public void ValueMask_Equality_ShouldWork()
        {
            var mask1 = new ValueMask();
            var mask2 = new ValueMask();
            
            Assert.True(mask1 == mask2);
            Assert.False(mask1 != mask2);
            Assert.True(mask1.Equals(mask2));
        }

        [Fact]
        public void ValueMask_GetHashCode_ShouldBeConsistent()
        {
            var mask1 = new ValueMask();
            var mask2 = new ValueMask();
            
            Assert.Equal(mask1.GetHashCode(), mask2.GetHashCode());
        }

        [Fact]
        public void PointIndex32_Construction_ShouldWork()
        {
            var index = new PointIndex32(42);
            Assert.Equal(42u, index.Value);
        }

        [Fact]
        public void PointIndex32_ImplicitConversion_ShouldWork()
        {
            var index = new PointIndex32(100);
            uint value = index;
            Assert.Equal(100u, value);
        }

        [Fact]
        public void PointIndex32_ExplicitConversion_ShouldWork()
        {
            var index = (PointIndex32)50u;
            Assert.Equal(50u, index.Value);
        }

        [Fact]
        public void PointIndex32_Equality_ShouldWork()
        {
            var index1 = new PointIndex32(42);
            var index2 = new PointIndex32(42);
            var index3 = new PointIndex32(43);
            
            Assert.Equal(index1, index2);
            Assert.NotEqual(index1, index3);
            Assert.True(index1 == index2);
            Assert.False(index1 == index3);
        }

        [Fact]
        public void PointIndex64_Construction_ShouldWork()
        {
            var index = new PointIndex64(1000000000ul);
            Assert.Equal(1000000000ul, index.Value);
        }

        [Fact]
        public void PointDataIndex32_Construction_ShouldWork()
        {
            var index = new PointDataIndex32(123);
            Assert.Equal(123u, index.Value);
        }

        [Fact]
        public void PointDataIndex64_Construction_ShouldWork()
        {
            var index = new PointDataIndex64(999999ul);
            Assert.Equal(999999ul, index.Value);
        }

        [Fact]
        public void TypeUtility_ZeroValue_ShouldReturnDefault()
        {
            Assert.Equal(0, TypeUtility.ZeroValue<int>());
            Assert.Equal(0.0, TypeUtility.ZeroValue<double>());
            Assert.Equal(0.0f, TypeUtility.ZeroValue<float>());
        }

        [Fact]
        public void TypeUtility_TypeNameAsString_ShouldReturnTypeName()
        {
            var name = TypeUtility.TypeNameAsString<int>();
            Assert.Contains("Int32", name);
        }
    }
}
