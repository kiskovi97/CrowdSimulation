using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.Utilities
{
    public class Graph
    {
        class Point : IComparable<Point>
        {
            public float3 point;
            public List<Point> neighbours = new List<Point>();

            public int CompareTo(Point other)
            {
                if (point.x == other.point.x) return point.z.CompareTo(other.point.z);
                return point.x.CompareTo(other.point.x);
            }

            public static explicit operator Point(float3 b) => new Point() { point = b };
        }

        class Comperer : IComparer<float3>
        {
            float3 from;
            public Comperer(float3 from)
            {
                this.from = from;
            }

            public int Compare(float3 x, float3 y)
            {
                return math.lengthsq(x - from).CompareTo(math.lengthsq(y - from));
            }
        }

        List<Point> points = new List<Point>();

        public List<List<float3>> GetShapes()
        {
            return new List<List<float3>>();
        }

        public void AddPoints(List<float3> input)
        {
            List<Point> circle = new List<Point>();
            for (int i = 0; i < input.Count; i++)
            {
                circle.Add((Point)input[i]);
            }
            for (int i = 0; i < circle.Count; i++)
            {
                var next = i + 1;
                if (next > circle.Count - 1) next = 0;
                var prev = i - 1;
                if (prev < 0) prev = circle.Count - 1;

                circle[i].neighbours.Add(circle[next]);
                circle[i].neighbours.Add(circle[prev]);
                var intersections = GetIntersections(circle[i].point, circle[next].point);
                intersections.Sort(new Comperer(circle[i].point));
            }
            points.AddRange(circle);
        }

        public List<float3> GetIntersections(float3 A, float3 B)
        {
            List<float3> list = new List<float3>();
            foreach (var C in points)
            {
                foreach (var D in C.neighbours)
                {
                    if (MyMath.DoIntersect(A, B, C.point, D.point))
                    {
                        var intersection = MyMath.Intersect(A, B, C.point, D.point);
                        list.Add(intersection);
                        Debug.DrawLine(intersection, intersection + new Vector3(0, 1, 0), Color.blue, 100f);
                    } else
                    {
                        if (MyMath.Orientation(A, B, C.point) == 0 && MyMath.Orientation(A, B, D.point) == 0) {
                            var tmp = new List<Point>() { (Point)A, (Point)B, C, D };
                            tmp.Sort();
                            Debug.DrawLine(tmp[1].point, tmp[1].point + new float3(0, 1, 0), Color.red, 100f);
                            Debug.DrawLine(tmp[2].point, tmp[2].point + new float3(0, 1, 0), Color.red, 100f);
                        }
                    }
                }
            }
            return list;
        }

        public void Draw()
        {
            foreach (var A in points)
            {
                foreach (var B in A.neighbours)
                {
                    Debug.DrawLine(A.point, B.point * 0.4f + A.point * 0.6f, Color.green, 100f);
                }
            }
        }

        private void Resolve(Point A, Point B, Point C, Point D)
        {
            var intersection = MyMath.Intersect(A.point, B.point, C.point, D.point);
            var newPoint = (Point)(float3)intersection;
            newPoint.neighbours.Add(A);
            newPoint.neighbours.Add(B);
            newPoint.neighbours.Add(C);
            newPoint.neighbours.Add(D);

            A.neighbours.Remove(B);
            B.neighbours.Remove(A);
            C.neighbours.Remove(D);
            D.neighbours.Remove(C);

            points.Add(newPoint);
        }
    }
}
