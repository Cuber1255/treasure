using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Treasure.Tests
{
    public class PolygonTriangulatorTests
    {
        [Test]
        public void Triangulate_ConcaveRing_CoversExactArea()
        {
            Vector2Int[] lShape =
            {
                new Vector2Int(0, 0), new Vector2Int(4, 0), new Vector2Int(4, 2),
                new Vector2Int(2, 2), new Vector2Int(2, 4), new Vector2Int(0, 4)
            };
            Vector2Int[] reversed = (Vector2Int[])lShape.Clone();
            Array.Reverse(reversed);

            foreach (Vector2Int[] ring in new[] { lShape, reversed })
            {
                var result = new List<int>();
                PolygonTriangulator.Triangulate(ring, result);

                Assert.AreEqual(12, result.Count);
                Assert.AreEqual(24, SumAbsArea2(ring, result));
            }
        }

        [Test]
        public void Triangulate_CollinearPoint_NoDegenerateTriangles()
        {
            Vector2Int[] ring =
            {
                new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(4, 0),
                new Vector2Int(4, 4), new Vector2Int(0, 4)
            };
            var result = new List<int>();
            PolygonTriangulator.Triangulate(ring, result);

            Assert.AreEqual(32, SumAbsArea2(ring, result));
            for (int i = 0; i < result.Count; i += 3)
            {
                Assert.AreNotEqual(0, TriangleArea2(ring[result[i]], ring[result[i + 1]], ring[result[i + 2]]));
            }
        }

        private static long SumAbsArea2(Vector2Int[] ring, List<int> triangles)
        {
            long sum = 0;
            for (int i = 0; i < triangles.Count; i += 3)
            {
                sum += Math.Abs(TriangleArea2(ring[triangles[i]], ring[triangles[i + 1]], ring[triangles[i + 2]]));
            }
            return sum;
        }

        private static long TriangleArea2(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return (long)(b.x - a.x) * (c.y - a.y) - (long)(b.y - a.y) * (c.x - a.x);
        }
    }
}
