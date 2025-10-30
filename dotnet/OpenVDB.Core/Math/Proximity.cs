// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
// Proximity.cs - C# port of Proximity.h and Proximity.cc

namespace OpenVDB.Math
{
    /// <summary>Proximity queries and functions</summary>
    public static class Proximity
    {
        /// <summary>
        /// Closest Point on Triangle to Point. Given a triangle abc and a point p,
        /// return the point on abc closest to p and the corresponding barycentric coordinates.
        /// </summary>
        /// <remarks>
        /// Algorithms from "Real-Time Collision Detection" pg 136 to 142 by Christer Ericson.
        /// The closest point is obtained by first determining which of the triangles'
        /// Voronoi feature regions p is in and then computing the orthogonal projection
        /// of p onto the corresponding feature.
        /// </remarks>
        /// <param name="a">The triangle's first vertex point.</param>
        /// <param name="b">The triangle's second vertex point.</param>
        /// <param name="c">The triangle's third vertex point.</param>
        /// <param name="p">Point to compute the closest point on abc for.</param>
        /// <param name="uvw">Barycentric coordinates, computed and returned.</param>
        /// <returns>The closest point on the triangle to p.</returns>
        public static Vec3<double> ClosestPointOnTriangleToPoint(
            Vec3<double> a, Vec3<double> b, Vec3<double> c, Vec3<double> p, out Vec3<double> uvw)
        {
            uvw = new Vec3<double>(0.0);

            // degenerate triangle, singular
            if (MathUtil.IsApproxEqual(a, b) && MathUtil.IsApproxEqual(a, c))
            {
                uvw = new Vec3<double>(1.0, 0.0, 0.0);
                return a;
            }

            Vec3<double> ab = b - a;
            Vec3<double> ac = c - a;
            Vec3<double> ap = p - a;
            double d1 = ab.Dot(ap);
            double d2 = ac.Dot(ap);

            // degenerate triangle edges
            if (MathUtil.IsApproxEqual(a, b))
            {
                Vec3<double> closestPt = ClosestPointOnSegmentToPoint(a, c, p, out double t);
                uvw = new Vec3<double>(1.0 - t, 0.0, t);
                return closestPt;
            }
            else if (MathUtil.IsApproxEqual(a, c) || MathUtil.IsApproxEqual(b, c))
            {
                Vec3<double> closestPt = ClosestPointOnSegmentToPoint(a, b, p, out double t);
                uvw = new Vec3<double>(1.0 - t, t, 0.0);
                return closestPt;
            }

            if (d1 <= 0.0 && d2 <= 0.0)
            {
                uvw = new Vec3<double>(1.0, 0.0, 0.0);
                return a; // barycentric coordinates (1,0,0)
            }

            // Check if P in vertex region outside B
            Vec3<double> bp = p - b;
            double d3 = ab.Dot(bp);
            double d4 = ac.Dot(bp);
            if (d3 >= 0.0 && d4 <= d3)
            {
                uvw = new Vec3<double>(0.0, 1.0, 0.0);
                return b; // barycentric coordinates (0,1,0)
            }

            // Check if P in edge region of AB, if so return projection of P onto AB
            double vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0 && d1 >= 0.0 && d3 <= 0.0)
            {
                double v = d1 / (d1 - d3);
                uvw = new Vec3<double>(1.0 - v, v, 0.0);
                return a + ab * v; // barycentric coordinates (1-v,v,0)
            }

            // Check if P in vertex region outside C
            Vec3<double> cp = p - c;
            double d5 = ab.Dot(cp);
            double d6 = ac.Dot(cp);
            if (d6 >= 0.0 && d5 <= d6)
            {
                uvw = new Vec3<double>(0.0, 0.0, 1.0);
                return c; // barycentric coordinates (0,0,1)
            }

            // Check if P in edge region of AC, if so return projection of P onto AC
            double vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0 && d2 >= 0.0 && d6 <= 0.0)
            {
                double w = d2 / (d2 - d6);
                uvw = new Vec3<double>(1.0 - w, 0.0, w);
                return a + ac * w; // barycentric coordinates (1-w,0,w)
            }

            // Check if P in edge region of BC, if so return projection of P onto BC
            double va = d3 * d6 - d5 * d4;
            if (va <= 0.0 && (d4 - d3) >= 0.0 && (d5 - d6) >= 0.0)
            {
                double w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                uvw = new Vec3<double>(0.0, 1.0 - w, w);
                return b + (c - b) * w; // barycentric coordinates (0,1-w,w)
            }

            // P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            double denom = 1.0 / (va + vb + vc);
            double wCoord = vc * denom;
            double vCoord = vb * denom;
            double uCoord = 1.0 - vCoord - wCoord;
            uvw = new Vec3<double>(uCoord, vCoord, wCoord);

            return a + ab * vCoord + ac * wCoord; // = u*a + v*b + w*c , u= va*denom = 1.0-v-w
        }

        /// <summary>
        /// Closest Point on Line Segment to Point. Given segment ab and point p,
        /// return the point on ab closest to p and t the parametric distance to b.
        /// </summary>
        /// <param name="a">The segment's first vertex point.</param>
        /// <param name="b">The segment's second vertex point.</param>
        /// <param name="p">Point to compute the closest point on ab for.</param>
        /// <param name="t">Parametric distance to b.</param>
        /// <returns>The closest point on the segment to p.</returns>
        public static Vec3<double> ClosestPointOnSegmentToPoint(
            Vec3<double> a, Vec3<double> b, Vec3<double> p, out double t)
        {
            Vec3<double> ab = b - a;
            t = (p - a).Dot(ab);

            if (t <= 0.0)
            {
                // c projects outside the [a,b] interval, on the a side.
                t = 0.0;
                return a;
            }
            else
            {
                // always nonnegative since denom = ||ab||^2
                double denom = ab.Dot(ab);

                if (t >= denom)
                {
                    // c projects outside the [a,b] interval, on the b side.
                    t = 1.0;
                    return b;
                }
                else
                {
                    // c projects inside the [a,b] interval.
                    t = t / denom;
                    return a + ab * t;
                }
            }
        }
    }
}
