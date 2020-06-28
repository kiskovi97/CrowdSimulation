using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.Utilities
{
    public class MyMath
    {
        class Point
        {
            public Point(float a, float b)
            {
                x = a;
                y = b;
            }
            public float x;
            public float y;
        };

        static float Max(float x, float y)
        {
            return (x > y) ? x : y;
        }
        static float Min(float x, float y)
        {
            return (x < y) ? x : y;
        }

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        static bool OnSegment(Point p, Point q, Point r)
        {
            if (q.x <= Max(p.x, r.x) && q.x >= Min(p.x, r.x) &&
                q.y <= Max(p.y, r.y) && q.y >= Min(p.y, r.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        static int Orientation(Point p, Point q, Point r)
        {
            // for details of below formula.
            float val = (q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y);

            if (math.abs(val) < 0.03f) return 0;  // colinear

            return (val > 0) ? 1 : -1; // clock or counterclock wise
        }

        public static int Orientation(float3 A, float3 B, float3 C) {
            return Orientation(new Point(A.x, A.z), new Point(B.x, B.z), new Point(C.x, C.z));
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            if (o1 == 0 || o2 == 0 || o3 == 0 || o4 == 0)
                return false;
            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            return false; // Doesn't fall in any of the above cases
        }

        public static bool DoIntersect(Vector3 pv1, Vector3 qv1, Vector3 pv2, Vector3 qv2)
        {
            if (pv1 == qv1 || pv1 == pv2 || pv1 == qv2) return false;
            if (qv1 == pv2 || qv1 == qv2) return false;
            if (pv2 == qv2) return false;

            Point p1 = new Point(pv1.x, pv1.z);
            Point q1 = new Point(qv1.x, qv1.z);
            Point p2 = new Point(pv2.x, pv2.z);
            Point q2 = new Point(qv2.x, qv2.z);
            return DoIntersect(p1, q1, p2, q2);

        }

        public static bool InnerPoint(float3 a, float3[] polygon)
        {
            float kereszt = 0;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (a.Equals(polygon[i])) return false;
                int j = i + 1;
                if (j > polygon.Length - 1) j = 0;
                if (Between(polygon[i], polygon[j], a)) return false;

                kereszt += Vector3.SignedAngle(polygon[i] - a, polygon[j] - a, new float3(0, 1, 0));
            }
            return math.abs(kereszt) > 0.1f;
        }

        public static bool Between(Vector3 one, Vector3 other, Vector3 middle)
        {
            Vector3 egyikFele = one - middle;
            Vector3 masikFele = other - middle;
            float angle = Vector3.Angle(egyikFele, masikFele);
            if (one == middle || other == middle) return false;
            return (angle > 179.9f) && (angle < 180.1f);
        }

        public static Vector3 Intersect(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            var P = A;
            var V = (B - A);
            var Q = C;
            var U = (D - C);

            V = To2D(V);
            U = To2D(U);
            float div = (U.x * V.z - U.z * V.x);
            if (Math.Abs(div) < 0.01f)
            {
                var c = (A - C).magnitude + (B - C).magnitude;
                var d = (A - D).magnitude + (B - D).magnitude;
                if (c < d) return C;
                return D;
            }
            float t2 = (Q.z * V.x + P.x * V.z - P.z * V.x - Q.x * V.z) / div;
            return (Q + t2 * U);
        }

        public static Vector3 To2D(Vector3 vector)
        {
            Vector3 ki = vector;
            ki.y = 0;
            return ki.normalized;
        }
    }
}
