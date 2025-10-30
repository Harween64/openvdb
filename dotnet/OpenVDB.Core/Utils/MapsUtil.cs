// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// Ported from MapsUtil.h

using System;
using OpenVDB.Math;

namespace OpenVDB.Utils
{
    /// <summary>
    /// Utility methods for calculating bounding boxes
    /// </summary>
    /// <remarks>
    /// NOTE: This is a stub implementation. Full functionality will be implemented
    /// when the Maps classes from Lot 2 are complete and available.
    /// These utilities are used to calculate bounding boxes in different coordinate spaces
    /// (e.g., index space vs world space) using map transformations.
    /// </remarks>
    public static class MapsUtil
    {
        /// <summary>
        /// Calculate an axis-aligned bounding box in the given map's domain
        /// (e.g., index space) from an axis-aligned bounding box in its range
        /// (e.g., world space)
        /// </summary>
        /// <typeparam name="TMap">The map type</typeparam>
        /// <param name="map">The transformation map</param>
        /// <param name="inputBox">Input bounding box in world space</param>
        /// <param name="outputBox">Output bounding box in index space</param>
        /// <remarks>
        /// This method computes the pre-image of the 8 corners of the input box
        /// and finds the axis-aligned bounding box that contains all these pre-images.
        /// </remarks>
        public static void CalculateBounds<TMap>(TMap map, BBox<double> inputBox, out BBox<double> outputBox)
        {
            // TODO: Implement when Maps infrastructure is available
            // This will transform the 8 corners of the box through the inverse map
            // and compute the bounding box of the result
            throw new NotImplementedException("CalculateBounds will be implemented when Maps classes are available in Lot 5");
        }

        /// <summary>
        /// Calculate an axis-aligned bounding box in the given map's domain
        /// from a spherical bounding box in its range
        /// </summary>
        /// <typeparam name="TMap">The map type</typeparam>
        /// <param name="map">The transformation map</param>
        /// <param name="center">Center of the sphere in world space</param>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="outputBox">Output bounding box in index space</param>
        /// <remarks>
        /// The image of a sphere under the inverse of a linear map will be an ellipsoid.
        /// This method computes an axis-aligned bounding box that encloses that ellipsoid.
        /// </remarks>
        public static void CalculateBounds<TMap>(TMap map, Vec3<double> center, double radius, out BBox<double> outputBox)
        {
            // TODO: Implement when Maps infrastructure is available
            // For linear maps, this uses the analytical solution
            // For non-linear maps, it falls back to corner-based computation
            throw new NotImplementedException("CalculateBounds for spheres will be implemented when Maps classes are available in Lot 5");
        }
    }
}
