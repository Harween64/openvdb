// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from Util.h

using System;
using OpenVDB.Math;

namespace OpenVDB.Utils
{
    /// <summary>
    /// General utility functions and constants
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Invalid index constant
        /// </summary>
        public const uint InvalidIdx = uint.MaxValue;

        /// <summary>
        /// Coordinate offset table for neighboring voxels
        /// </summary>
        /// <remarks>
        /// Indices 0-5: Voxel-face adjacent neighbors
        /// Indices 6-17: Voxel-edge adjacent neighbors
        /// Indices 18-25: Voxel-corner adjacent neighbors
        /// </remarks>
        public static readonly Coord[] CoordOffsets = new Coord[26]
        {
            // Voxel-face adjacent neighbors (0 to 5)
            new Coord( 1,  0,  0),
            new Coord(-1,  0,  0),
            new Coord( 0,  1,  0),
            new Coord( 0, -1,  0),
            new Coord( 0,  0,  1),
            new Coord( 0,  0, -1),
            
            // Voxel-edge adjacent neighbors (6 to 17)
            new Coord( 1,  0, -1),
            new Coord(-1,  0, -1),
            new Coord( 1,  0,  1),
            new Coord(-1,  0,  1),
            new Coord( 1,  1,  0),
            new Coord(-1,  1,  0),
            new Coord( 1, -1,  0),
            new Coord(-1, -1,  0),
            new Coord( 0, -1,  1),
            new Coord( 0, -1, -1),
            new Coord( 0,  1,  1),
            new Coord( 0,  1, -1),
            
            // Voxel-corner adjacent neighbors (18 to 25)
            new Coord(-1, -1, -1),
            new Coord(-1, -1,  1),
            new Coord( 1, -1,  1),
            new Coord( 1, -1, -1),
            new Coord(-1,  1, -1),
            new Coord(-1,  1,  1),
            new Coord( 1,  1,  1),
            new Coord( 1,  1, -1)
        };

        /// <summary>
        /// Return voxelCoord rounded to the closest integer coordinates.
        /// </summary>
        /// <param name="voxelCoord">The voxel coordinates to round</param>
        /// <returns>Rounded coordinate</returns>
        public static Coord NearestCoord(Vec3<double> voxelCoord)
        {
            return new Coord(
                (int)System.Math.Floor(voxelCoord.X),
                (int)System.Math.Floor(voxelCoord.Y),
                (int)System.Math.Floor(voxelCoord.Z)
            );
        }

        // Note: The following tree-related utility classes (LeafTopologyIntOp, LeafTopologyDiffOp,
        // leafTopologyIntersection, leafTopologyDifference) are not ported yet as they depend
        // on Tree classes which will be ported in Lot 5. They will be added when the Tree
        // infrastructure is available.
    }
}
