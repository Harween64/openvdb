// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0

using OpenVDB.Tools;
using Xunit;

namespace OpenVDB.Tests.Tools
{
    /// <summary>
    /// Tests for the basic tool functions in lot 7A
    /// </summary>
    public class BasicToolsTests
    {
        [Fact]
        public void Activate_NullGridOrTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Activate.ActivateValues<object, float>(null!, 0.0f));
        }

        [Fact]
        public void Activate_Deactivate_NullGridOrTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Activate.DeactivateValues<object, float>(null!, 0.0f));
        }

        [Fact]
        public void ChangeBackground_NullGridOrTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ChangeBackground.ChangeBackgroundValue<object, float>(null!, 0.0f));
        }

        [Fact]
        public void Clip_ByBBox_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Clip.ClipByBBox<object>(null!, new object()));
        }

        [Fact]
        public void Clip_ByMask_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Clip.ClipByMask<object, object>(null!, new object()));
        }

        [Fact]
        public void Clip_ByMask_NullMask_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Clip.ClipByMask<object, object>(new object(), null!));
        }

        [Fact]
        public void Composite_NullGridA_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Composite.CompositeGrids<object, object>(null!, new object(), CompositeOperation.Union));
        }

        [Fact]
        public void Composite_NullGridB_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Composite.CompositeGrids<object, object>(new object(), null!, CompositeOperation.Union));
        }

        [Fact]
        public void Count_ActiveVoxels_NullTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Count.CountActiveVoxels<object>(null!));
        }

        [Fact]
        public void Count_ActiveLeafVoxels_NullTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Count.CountActiveLeafVoxels<object>(null!));
        }

        [Fact]
        public void Count_InactiveVoxels_NullTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Count.CountInactiveVoxels<object>(null!));
        }

        [Fact]
        public void Count_MemUsage_NullTree_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Count.MemUsage<object>(null!));
        }

        [Fact]
        public void Diagnostics_CheckGrid_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Diagnostics.CheckGrid<object>(null!));
        }

        [Fact]
        public void Diagnostics_CheckTopology_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Diagnostics.CheckTopology<object>(null!));
        }

        [Fact]
        public void Mask_InteriorMask_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Mask.InteriorMask<object>(null!));
        }

        [Fact]
        public void Merge_NullGridA_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Merge.MergeGrids<object>(null!, new object(), MergePolicy.ActiveValuesOnly));
        }

        [Fact]
        public void Merge_NullGridB_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Merge.MergeGrids<object>(new object(), null!, MergePolicy.ActiveValuesOnly));
        }

        [Fact]
        public void Dense_Constructor_CreatesValidInstance()
        {
            // Arrange & Act
            var dense = new Dense<float>(10, 10, 10);

            // Assert
            Assert.Equal(10, dense.XSize);
            Assert.Equal(10, dense.YSize);
            Assert.Equal(10, dense.ZSize);
            Assert.NotNull(dense.Data);
            Assert.Equal(1000, dense.Data.Length);
        }

        [Fact]
        public void Dense_Indexer_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var dense = new Dense<float>(5, 5, 5);

            // Act
            dense[2, 3, 4] = 42.0f;
            var value = dense[2, 3, 4];

            // Assert
            Assert.Equal(42.0f, value);
        }

        [Fact]
        public void Dense_WithBackground_InitializesCorrectly()
        {
            // Arrange & Act
            var dense = new Dense<int>(3, 3, 3, 5);

            // Assert
            Assert.All(dense.Data, x => Assert.Equal(5, x));
        }

        [Fact]
        public void DenseTools_CopyToDense_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange
            var dense = new Dense<float>(1, 1, 1);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseTools.CopyToDense<object, float>(null!, dense));
        }

        [Fact]
        public void DenseTools_CopyToDense_NullDense_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseTools.CopyToDense<object, float>(new object(), null!));
        }

        [Fact]
        public void DenseTools_CopyFromDense_NullDense_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseTools.CopyFromDense<object, float>(null!, new object()));
        }

        [Fact]
        public void DenseTools_CopyFromDense_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange
            var dense = new Dense<float>(1, 1, 1);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseTools.CopyFromDense<object, float>(dense, null!));
        }

        [Fact]
        public void DenseSparseTools_Densify_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseSparseTools.Densify<object>(null!, new object()));
        }

        [Fact]
        public void DenseSparseTools_Sparsify_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseSparseTools.Sparsify<object, float>(null!));
        }

        [Fact]
        public void DenseSparseTools_ExtractRegion_NullGrid_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                DenseSparseTools.ExtractRegion<object, float>(null!, new object()));
        }
    }
}
