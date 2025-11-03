// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;
using OpenVDB.Math;

namespace OpenVDB.Tests.Math
{
    public class CoordTests
    {
        [Fact]
        public void Constructor_Default_ShouldBeZero()
        {
            var coord = new Coord();
            Assert.Equal(0, coord.X);
            Assert.Equal(0, coord.Y);
            Assert.Equal(0, coord.Z);
        }

        [Fact]
        public void Constructor_WithValues_ShouldSetComponents()
        {
            var coord = new Coord(1, 2, 3);
            Assert.Equal(1, coord.X);
            Assert.Equal(2, coord.Y);
            Assert.Equal(3, coord.Z);
        }

        [Fact]
        public void Indexer_ShouldAccessComponents()
        {
            var coord = new Coord(10, 20, 30);
            Assert.Equal(10, coord[0]);
            Assert.Equal(20, coord[1]);
            Assert.Equal(30, coord[2]);
        }

        [Fact]
        public void Indexer_OutOfRange_ShouldThrow()
        {
            var coord = new Coord(1, 2, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => coord[3]);
            Assert.Throws<ArgumentOutOfRangeException>(() => coord[-1]);
        }

        [Fact]
        public void Equality_ShouldWork()
        {
            var coord1 = new Coord(1, 2, 3);
            var coord2 = new Coord(1, 2, 3);
            var coord3 = new Coord(1, 2, 4);
            
            Assert.Equal(coord1, coord2);
            Assert.NotEqual(coord1, coord3);
            Assert.True(coord1 == coord2);
            Assert.False(coord1 == coord3);
        }

        [Fact]
        public void Addition_ShouldWork()
        {
            var a = new Coord(1, 2, 3);
            var b = new Coord(4, 5, 6);
            var result = a + b;
            
            Assert.Equal(new Coord(5, 7, 9), result);
        }

        [Fact]
        public void Subtraction_ShouldWork()
        {
            var a = new Coord(10, 20, 30);
            var b = new Coord(1, 2, 3);
            var result = a - b;
            
            Assert.Equal(new Coord(9, 18, 27), result);
        }

        [Fact]
        public void Negation_ShouldWork()
        {
            var coord = new Coord(1, -2, 3);
            var negated = -coord;
            
            Assert.Equal(new Coord(-1, 2, -3), negated);
        }

        [Fact]
        public void AsVec3_ShouldConvertToVec3()
        {
            var coord = new Coord(1, 2, 3);
            var vec = coord.AsVec3d();
            
            Assert.Equal(1.0, vec.X);
            Assert.Equal(2.0, vec.Y);
            Assert.Equal(3.0, vec.Z);
        }

        [Fact]
        public void LessThan_ShouldCompareComponentWise()
        {
            var a = new Coord(1, 2, 3);
            var b = new Coord(2, 3, 4);
            
            Assert.True(a < b);
            Assert.False(b < a);
        }

        [Fact]
        public void GreaterThan_ShouldCompareComponentWise()
        {
            var a = new Coord(2, 3, 4);
            var b = new Coord(1, 2, 3);
            
            Assert.True(a > b);
            Assert.False(b > a);
        }
    }

    public class BBoxTests
    {
        [Fact]
        public void Constructor_WithMinMax_ShouldSetBounds()
        {
            var min = new Vec3<double>(0, 0, 0);
            var max = new Vec3<double>(1, 1, 1);
            var bbox = new BBox<double>(min, max);
            
            Assert.Equal(min, bbox.Min);
            Assert.Equal(max, bbox.Max);
        }

        [Fact]
        public void Center_ShouldCalculateMiddlePoint()
        {
            var bbox = new BBox<double>(
                new Vec3<double>(0, 0, 0),
                new Vec3<double>(10, 10, 10)
            );
            
            var center = bbox.Center();
            Assert.Equal(5.0, center.X);
            Assert.Equal(5.0, center.Y);
            Assert.Equal(5.0, center.Z);
        }

        [Fact]
        public void Extents_ShouldCalculateSize()
        {
            var bbox = new BBox<double>(
                new Vec3<double>(0, 0, 0),
                new Vec3<double>(10, 20, 30)
            );
            
            var extents = bbox.Extents();
            Assert.Equal(10.0, extents.X);
            Assert.Equal(20.0, extents.Y);
            Assert.Equal(30.0, extents.Z);
        }

        [Fact]
        public void Equality_ShouldWork()
        {
            var bbox1 = new BBox<double>(
                new Vec3<double>(0, 0, 0),
                new Vec3<double>(1, 1, 1)
            );
            var bbox2 = new BBox<double>(
                new Vec3<double>(0, 0, 0),
                new Vec3<double>(1, 1, 1)
            );
            var bbox3 = new BBox<double>(
                new Vec3<double>(0, 0, 0),
                new Vec3<double>(2, 2, 2)
            );
            
            Assert.Equal(bbox1, bbox2);
            Assert.NotEqual(bbox1, bbox3);
        }
    }

    public class Vec2Tests
    {
        [Fact]
        public void Constructor_Default_ShouldBeZero()
        {
            var vec = new Vec2<double>();
            Assert.Equal(0.0, vec.X);
            Assert.Equal(0.0, vec.Y);
        }

        [Fact]
        public void Constructor_WithValues_ShouldSetComponents()
        {
            var vec = new Vec2<double>(3.0, 4.0);
            Assert.Equal(3.0, vec.X);
            Assert.Equal(4.0, vec.Y);
        }

        [Fact]
        public void Addition_ShouldWork()
        {
            var a = new Vec2<double>(1.0, 2.0);
            var b = new Vec2<double>(3.0, 4.0);
            var result = a + b;
            
            Assert.Equal(4.0, result.X);
            Assert.Equal(6.0, result.Y);
        }

        [Fact]
        public void Subtraction_ShouldWork()
        {
            var a = new Vec2<double>(5.0, 6.0);
            var b = new Vec2<double>(1.0, 2.0);
            var result = a - b;
            
            Assert.Equal(4.0, result.X);
            Assert.Equal(4.0, result.Y);
        }

        [Fact]
        public void ScalarMultiplication_ShouldWork()
        {
            var vec = new Vec2<double>(2.0, 3.0);
            var result = vec * 2.0;
            
            Assert.Equal(4.0, result.X);
            Assert.Equal(6.0, result.Y);
        }

        [Fact]
        public void Dot_ShouldCalculateDotProduct()
        {
            var a = new Vec2<double>(1.0, 0.0);
            var b = new Vec2<double>(0.0, 1.0);
            
            Assert.Equal(0.0, a.Dot(b));
            
            var c = new Vec2<double>(2.0, 3.0);
            var d = new Vec2<double>(4.0, 5.0);
            Assert.Equal(23.0, c.Dot(d)); // 2*4 + 3*5 = 8 + 15 = 23
        }

        [Fact]
        public void Length_ShouldCalculateMagnitude()
        {
            var vec = new Vec2<double>(3.0, 4.0);
            Assert.Equal(5.0, vec.Length(), 1e-10);
        }

        [Fact]
        public void LengthSqr_ShouldCalculateSquaredMagnitude()
        {
            var vec = new Vec2<double>(3.0, 4.0);
            Assert.Equal(25.0, vec.LengthSqr(), 1e-10);
        }
    }

    public class Vec3Tests
    {
        [Fact]
        public void Constructor_Default_ShouldBeZero()
        {
            var vec = new Vec3<double>();
            Assert.Equal(0.0, vec.X);
            Assert.Equal(0.0, vec.Y);
            Assert.Equal(0.0, vec.Z);
        }

        [Fact]
        public void Constructor_WithValues_ShouldSetComponents()
        {
            var vec = new Vec3<double>(1.0, 2.0, 3.0);
            Assert.Equal(1.0, vec.X);
            Assert.Equal(2.0, vec.Y);
            Assert.Equal(3.0, vec.Z);
        }

        [Fact]
        public void Addition_ShouldWork()
        {
            var a = new Vec3<double>(1.0, 2.0, 3.0);
            var b = new Vec3<double>(4.0, 5.0, 6.0);
            var result = a + b;
            
            Assert.Equal(5.0, result.X);
            Assert.Equal(7.0, result.Y);
            Assert.Equal(9.0, result.Z);
        }

        [Fact]
        public void Subtraction_ShouldWork()
        {
            var a = new Vec3<double>(10.0, 20.0, 30.0);
            var b = new Vec3<double>(1.0, 2.0, 3.0);
            var result = a - b;
            
            Assert.Equal(9.0, result.X);
            Assert.Equal(18.0, result.Y);
            Assert.Equal(27.0, result.Z);
        }

        [Fact]
        public void ScalarMultiplication_ShouldWork()
        {
            var vec = new Vec3<double>(1.0, 2.0, 3.0);
            var result = vec * 2.0;
            
            Assert.Equal(2.0, result.X);
            Assert.Equal(4.0, result.Y);
            Assert.Equal(6.0, result.Z);
        }

        [Fact]
        public void Dot_ShouldCalculateDotProduct()
        {
            var a = new Vec3<double>(1.0, 0.0, 0.0);
            var b = new Vec3<double>(0.0, 1.0, 0.0);
            
            Assert.Equal(0.0, a.Dot(b));
            
            var c = new Vec3<double>(1.0, 2.0, 3.0);
            var d = new Vec3<double>(4.0, 5.0, 6.0);
            Assert.Equal(32.0, c.Dot(d)); // 1*4 + 2*5 + 3*6 = 4 + 10 + 18 = 32
        }

        [Fact]
        public void Cross_ShouldCalculateCrossProduct()
        {
            var a = new Vec3<double>(1.0, 0.0, 0.0);
            var b = new Vec3<double>(0.0, 1.0, 0.0);
            var result = a.Cross(b);
            
            Assert.Equal(0.0, result.X, 1e-10);
            Assert.Equal(0.0, result.Y, 1e-10);
            Assert.Equal(1.0, result.Z, 1e-10);
        }

        [Fact]
        public void Length_ShouldCalculateMagnitude()
        {
            var vec = new Vec3<double>(3.0, 4.0, 0.0);
            Assert.Equal(5.0, vec.Length(), 1e-10);
        }

        [Fact]
        public void LengthSqr_ShouldCalculateSquaredMagnitude()
        {
            var vec = new Vec3<double>(3.0, 4.0, 0.0);
            Assert.Equal(25.0, vec.LengthSqr(), 1e-10);
        }

        [Fact]
        public void Normalize_ShouldMakeUnitVector()
        {
            var vec = new Vec3<double>(3.0, 4.0, 0.0);
            vec.Normalize(1e-10);
            
            Assert.Equal(1.0, vec.Length(), 1e-10);
            Assert.Equal(0.6, vec.X, 1e-10);
            Assert.Equal(0.8, vec.Y, 1e-10);
        }
    }
}
