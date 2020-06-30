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

            public int HashCode => GetHashCode();

            public override int GetHashCode()
            {
                return (math.round(point * 100f) / 100f).GetHashCode();
            }

            public override string ToString()
            {
                return point.ToString();
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
            public float3 direction;
            public PointComperer(Point center, float3 direction)
            {
                this.center = center;
                this.direction = math.normalize(direction);
            }

            public int Compare(Point one, Point other)
            {
                var valueOne = math.dot(center.point - one.point, direction);
                var valueOther = math.dot(center.point - other.point, direction);
                return valueOne.CompareTo(valueOther);
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
            var circles = new List<List<float3>>();

            var circlePoints = new List<List<Point>>();
            while (points.Count > 0)
            {
                var circle = GetCircle();
                for (int i = 1; i < circle.Count; i++)
                {
                    Remove(circle[i]);
                }
                circlePoints.Add(circle);
                circles.Add(circle.Select((one) => one.point).ToList());
            }

            for (int cI = 0; cI < circlePoints.Count; cI++)
            {
                for (int i = 1; i < circlePoints[cI].Count; i++)
                {
                    circlePoints[cI][i].neighbours.Add(circlePoints[cI][i - 1]);
                    circlePoints[cI][i - 1].neighbours.Add(circlePoints[cI][i]);
                    points.Add(circlePoints[cI][i]);
                }
            }

            return circles;
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
                var intersections = GetIntersections(circle[i], circle[next]);
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
                    circle.InsertRange(i + 1, intersections.Select((one) => one.Center));
                    i += intersections.Count;
                }
            }
            for (int i = 0; i < circle.Count; i++)
            {
                var next = i + 1;
                if (next > circle.Count - 1) next = 0;
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
            CreateCircles();
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
                                neighbour.neighbours.Remove(points[j]);
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

        private List<Intersection> GetIntersections(Point A, Point B)
        {
            List<Intersection> list = new List<Intersection>();
            for (int i = 0; i < points.Count; i++)
            {
                var C = points[i];
                foreach (var D in C.neighbours)
                {
                    if (MyMath.DoIntersect(A.point, B.point, C.point, D.point))
                    {
                        var intersection = (Point)MyMath.Intersect(A.point, B.point, C.point, D.point);
                        list.Add(new Intersection(C, D, intersection));
                        points.Add(intersection);
                    }
                    else
                    {
                        if (MyMath.Orientation(A.point, B.point, C.point) == 0 && MyMath.Orientation(A.point, B.point, D.point) == 0)
                        {
                            var tmp = new List<Point>() { A, B, C, D };
                            tmp.Sort();
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
                    if (AreInLine(A, B, C.point, D.point))
                    {
                        if (AreIntersect(A, B, C.point, D.point))
                        {
                            var tmp = new List<Point>() { C, D };
                            list.AddRange(tmp);
                        }
                    }

                }
            }
            return list;
        }

        private bool AreInLine(float3 A, float3 B, float3 C, float3 D)
        {
            return MyMath.Orientation(A, B, C) == 0 && MyMath.Orientation(A, B, D) == 0;
        }

        private bool AreIntersect(float3 A, float3 B, float3 C, float3 D)
        {
            return MyMath.Between(A, B, C)
                || MyMath.Between(A, B, D)
                || MyMath.Between(C, D, A)
                || MyMath.Between(C, D, B);
        }

        public void Draw()
        {
            //CreateCircles();

            foreach (var A in points)
            {
                foreach (var B in A.neighbours)
                {
                    Debug.DrawLine(A.point, B.point * 0.4f + A.point * 0.6f + new float3(0, 1, 0), Color.green, 100f);
                }
            }
        }

        public void CreateCircles()
        {
            var circles = new List<List<Point>>();
            while (points.Count > 0)
            {
                var circle = GetCircle();
                for (int i = 1; i < circle.Count; i++)
                {
                    Remove(circle[i]);
                }
                circles.Add(circle);
            }

            for (int cI = 0; cI < circles.Count; cI++)
            {
                for (int i = 1; i < circles[cI].Count; i++)
                {
                    circles[cI][i].neighbours.Add(circles[cI][i - 1]);
                    circles[cI][i - 1].neighbours.Add(circles[cI][i]);
                    points.Add(circles[cI][i]);
                }
            }

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                if (point.neighbours.Count == 2)
                {
                    var A = point.neighbours.First();
                    var B = point.neighbours.Last();

                    if (MyMath.Between(A.point, B.point, point.point))
                    {

                        A.neighbours.Remove(point);
                        B.neighbours.Remove(point);
                        point.neighbours.Clear();

                        A.neighbours.Add(B);
                        B.neighbours.Add(A);

                        points.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        void Remove(Point point)
        {
            points.Remove(point);
            foreach (var neighbour in point.neighbours)
            {
                if (points.Contains(neighbour))
                    Remove(neighbour);
            }
            point.neighbours.Clear();
        }

        private List<Point> GetCircle()
        {
            var list = new List<Point>();
            var prev = points.Min();
            list.Add(prev);
            var first = prev;
            var current = NextNeighbour(prev, new float3(-1, 0, -1), prev);
            list.Add(current);
            int iteration = 0;
            while (iteration < points.Count && !first.Equals(current))
            {
                iteration++;
                var from = prev.point - current.point;
                var next = NextNeighbour(current, from, prev);
                prev = current;
                current = next;
                list.Add(current);
            }
            return list;
        }

        public static Point NextNeighbour(Point current, float3 from, Point prev)
        {
            if (current.neighbours.Count == 0) return current;
            var next = current.neighbours.First();
            foreach (var point in current.neighbours)
            {
                if (point.Equals(prev)) continue;
                if (GetAngle(from, next.point - current.point) > GetAngle(from, point.point - current.point))
                {
                    next = point;
                }
            }
            return next;
        }

        public static float GetAngle(float3 from, float3 to, bool right = true)
        {
            var angle = Vector3.SignedAngle(math.normalize(from), math.normalize(to), new float3(0, 1, 0));
            if (math.abs(angle) < 0.2f) return 360;
            if (angle < 0f) angle += 360f;
            return right ? angle * -1f : angle;
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
        }

        private void ResolveLine(List<Point> line)
        {
            var point = line[0];
            var vector = line[0].point - line[1].point;

            line.Sort(new PointComperer(point, vector));

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
