using System.Collections;
using System.Collections.Generic;
using Assets.CrowdSimulation.Scripts.Utilities;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;
using static Assets.CrowdSimulation.Scripts.Utilities.Graph;

namespace Tests
{
    public class MyMathTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void DoIntersect_SeperatePoints_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(1, 0, 0);
            var pointC = new Vector3(2, 0, 0);
            var pointD = new Vector3(3, 0, 0);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.IsFalse(intersect, "They shouldn't intersected");
        }


        // A Test behaves as an ordinary method
        [Test]
        public void DoIntersect_CrossPoints_Intersect()
        {
            // Arrange
            var pointA = new Vector3(-1, 0, 0);
            var pointB = new Vector3(1, 0, 0);
            var pointC = new Vector3(0, 0, 1);
            var pointD = new Vector3(0, 0, -1);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.IsTrue(intersect, "They should intersected");
        }

        [Test]
        public void DoIntersect_InLinePoints_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            var pointD = new Vector3(3, 0, 0);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.IsFalse(intersect, "They shouldn't intersected");
        }

        [Test]
        public void DoIntersect_TShape_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            var pointD = new Vector3(1, 0, 3);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.IsFalse(intersect, "They should intersected");
        }

        [Test]
        public void DoIntersect_SamePointAlign_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointC, pointB, pointC);
            // Assert
            Assert.IsFalse(intersect, "They shouldn't intersected");
        }

        [Test]
        public void DoIntersect_SamePointInner_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointB, pointC);
            // Assert
            Assert.IsFalse(intersect, "They shouldn't intersected");
        }

        [Test]
        public void DoIntersect_SamePointNotAlign_NotIntersect()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 1);
            var pointB = new Vector3(0, 0, -1);
            var pointC = new Vector3(1, 0, 0);
            // Act
            var intersect = MyMath.DoIntersect(pointA, pointB, pointB, pointC);
            // Assert
            Assert.IsFalse(intersect, "They shouldn't intersected");
        }

        [Test]
        public void Intersect_Crossing()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 1);
            var pointB = new Vector3(0, 0, -3);
            var pointC = new Vector3(2, 0, 0);
            var pointD = new Vector3(-1, 0, 0);
            // Act
            var intersect = MyMath.Intersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.AreEqual(Vector3.zero, intersect,"They shouldn't intersected");
        }

        [Test]
        public void Intersect_Crossing_InLine()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            var pointD = new Vector3(3, 0, 0);
            // Act
            var intersect = MyMath.Intersect(pointA, pointB, pointC, pointD);
            // Assert
            Assert.AreEqual(new Vector3(1,0,0), intersect, "They shouldn't intersected");
        }

        [Test]
        public void Intersect_Crossing_InLine2()
        {
            // Arrange
            var pointA = new Vector3(0, 0, 0);
            var pointB = new Vector3(2, 0, 0);
            var pointC = new Vector3(1, 0, 0);
            var pointD = new Vector3(3, 0, 0);
            // Act
            var intersect = MyMath.Intersect(pointA, pointB, pointD, pointC);
            // Assert
            Assert.AreEqual(new Vector3(1, 0, 0), intersect, "They shouldn't intersected");
        }

        [Test]
        public void Sort_AlignX_Sorted()
        {
            // Arrange
            var pointA = (Point)new Vector3(0, 0, 0);
            var pointB = (Point)new Vector3(0, 0, 1);
            var pointC = (Point)new Vector3(0, 0, 2);
            var pointD = (Point)new Vector3(0, 0, 3);

            var list = new List<Point>()
            {
                pointB,
                pointD,
                pointC,
                pointA,
            };

            // Act
            list.Sort(new PointComperer(pointA));
            // Assert
            Assert.AreEqual(pointA, list[0]);
            Assert.AreEqual(pointB, list[1]);
            Assert.AreEqual(pointC, list[2]);
            Assert.AreEqual(pointD, list[3]);
        }

        [Test]
        public void Sort_AlignZ_Sorted()
        {
            // Arrange
            var pointA = (Point)new Vector3(-3, 0, 0);
            var pointB = (Point)new Vector3(1, 0, 0);
            var pointC = (Point)new Vector3(2, 0, 0);
            var pointD = (Point)new Vector3(3, 0, 0);

            var list = new List<Point>()
            {
                pointB,
                pointD,
                pointC,
                pointA,
            };

            // Act
            list.Sort(new PointComperer(pointA));
            // Assert
            Assert.AreEqual(pointA, list[0]);
            Assert.AreEqual(pointB, list[1]);
            Assert.AreEqual(pointC, list[2]);
            Assert.AreEqual(pointD, list[3]);
        }

        [Test]
        public void Sort_Complex_Sorted()
        {
            // Arrange
            var pointA = (Point)new Vector3(-3, 0, 0);
            var pointB = (Point)new Vector3(1, 1, 0);
            var pointC = (Point)new Vector3(1, 2, 0);
            var pointD = (Point)new Vector3(3, 0, 0);

            var list = new List<Point>()
            {
                pointC,
                pointD,
                pointB,
                pointA,
            };

            // Act
            list.Sort(new PointComperer(pointA));
            // Assert
            Assert.AreEqual(pointA, list[0]);
            Assert.AreEqual(pointB, list[1]);
            Assert.AreEqual(pointC, list[2]);
            Assert.AreEqual(pointD, list[3]);
        }
    }
}
