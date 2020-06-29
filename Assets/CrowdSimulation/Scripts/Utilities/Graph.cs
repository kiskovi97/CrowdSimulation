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
        public class Point : IComparable<Point>, IEquatable<Point>
        {
            public float3 point;
            public HashSet<Point> neighbours = new HashSet<Point>();

            public int CompareTo(Point other)
            {
                if (point.x == other.point.x) return point.z.CompareTo(other.point.z);
                return point.x.CompareTo(other.point.x);
            }

            public bool Equals(Point other)
            {
                if (other == null) return false;
                return math.length(point - other.point) < 0.02f;
            }

            public static explicit operator Point(float3 b) => new Point() { point = b };
            public static explicit operator Point(Vector3 b) => new Point() { point = b };

            public override bool Equals(object obj)
            {
                if (obj is Point other)
                {
                    return Equals(other);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return 1595967545 + EqualityComparer<float3>.Default.GetHashCode(point);
            }
        }

        public class Intersection
        {
            public Point A;
            public Point B;
            public Point Center;
            public Intersection(Point A, Point B, Point Center)
            {
                this.A = A;
                this.B = B;
                this.Center = Center;
            }
        }

        public class PointComperer : IComparer<Point>
        {
            public Point center;
            public PointComperer(Point center)
            {
                this.center = center;
            }

            public int Compare(Point one, Point other)
            {
                return math.length(center.point - one.point).CompareTo(math.length(center.point - other.point));
            }
        }

        class Comperer : IComparer<Intersection>
        {
            float3 from;
            public Comperer(float3 from)
            {
                this.from = from;
            }

            public int Compare(Intersection x, Intersection y)
            {
                return math.lengthsq(x.Center.point - from).CompareTo(math.lengthsq(y.Center.point - from));
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
            }
            for (int i = 0; i < circle.Count; i++)
            {
                var next = i + 1;
                if (next > circle.Count - 1) next = 0;
                var intersections = GetIntersections(circle[i].point, circle[next].point);
                intersections.Sort(new Comperer(circle[i].point));

                if (intersections.Count > 0)
                {
                    circle[i].neighbours.Remove(circle[next]);
                    circle[next].neighbours.Remove(circle[i]);
                    for (int x = 0; x < intersections.Count; x++)
                    {
                        var inter = intersections[x];
                        var C = circle[i];
                        var D = circle[next];
                        if (x > 0) C = intersections[x - 1].Center;
                        if (x < intersections.Count - 1) D = intersections[x + 1].Center;

                        Resolve(inter.A, inter.B, C, D, inter.Center);
                    }
                }

                var lines = GetLines(circle[i].point, circle[next].point);
                if (lines.Count > 0)
                {
                    lines.Add(circle[i]);
                    lines.Add(circle[next]);
                    ResolveLine(lines);
                }
            }
            points.AddRange(circle);
            ClearPoints();
        }

        private void ClearPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (points[i].Equals(points[j]))
                    {
                        foreach (var neighbour in points[j].neighbours)
                        {
                            if (!neighbour.Equals(points[i]))
                            {
                                points[i].neighbours.Add(neighbour);
                                neighbour.neighbours.Add(points[i]);
                            }
                        }
                        points[i].neighbours.Remove(points[j]);
                        points.RemoveAt(j);
                        j--;
                    }
                }
                points[i].neighbours.Remove(points[i]);
            }
        }

        private List<Intersection> GetIntersections(float3 A, float3 B)
        {
            List<Intersection> list = new List<Intersection>();
            foreach (var C in points)
            {
                foreach (var D in C.neighbours)
                {
                    if (MyMath.DoIntersect(A, B, C.point, D.point))
                    {
                        var intersection = (Point)MyMath.Intersect(A, B, C.point, D.point);
                        list.Add(new Intersection(C, D, intersection));
                    }
                    else
                    {
                        if (MyMath.Orientation(A, B, C.point) == 0 && MyMath.Orientation(A, B, D.point) == 0)
                        {
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

        private List<Point> GetLines(float3 A, float3 B)
        {
            List<Point> list = new List<Point>();
            foreach (var C in points)
            {
                foreach (var D in C.neighbours)
                {
                    if (MyMath.Orientation(A, B, C.point) == 0 && MyMath.Orientation(A, B, D.point) == 0)
                    {
                        var tmp = new List<Point>() { C, D };
                        list.AddRange(tmp);
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
                    Debug.DrawLine(A.point, B.point * 0.4f + A.point * 0.6f + new float3(0, 1, 0), Color.green, 100f);
                }
            }
        }

        private void Resolve(Point A, Point B, Point C, Point D, Point newPoint)
        {
            newPoint.neighbours.Add(A);
            newPoint.neighbours.Add(B);
            newPoint.neighbours.Add(C);
            newPoint.neighbours.Add(D);

            A.neighbours.Remove(B);
            B.neighbours.Remove(A);
            C.neighbours.Remove(D);
            D.neighbours.Remove(C);


            A.neighbours.Add(newPoint);
            B.neighbours.Add(newPoint);
            C.neighbours.Add(newPoint);
            D.neighbours.Add(newPoint);

            points.Add(newPoint);
        }
        private void ResolveLine(List<Point> line)
        {
            var point = line.Min();
            line.Sort(new PointComperer(point));

            for (int i = 0; i < line.Count; i++)
            {
                for (int j = 0; j < line.Count; j++)
                {
                    line[i].neighbours.Remove(line[j]);
                }
            }

            for (int i = 0; i < line.Count; i++)
            {
                if (i > 0)
                {
                    //Debug.DrawLine(line[i - 1].point + new float3(0, i - 1, 0), line[i].point + new float3(0, i, 0), Color.magenta, 100f);
                    line[i].neighbours.Add(line[i - 1]);
                    line[i - 1].neighbours.Add(line[i]);

                }

                if (i < line.Count - 1)
                {
                    line[i].neighbours.Add(line[i + 1]);
                    line[i + 1].neighbours.Add(line[i]);
                }
            }
        }
    }
}
