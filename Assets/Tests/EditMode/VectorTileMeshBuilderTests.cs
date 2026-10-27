using System;
using NUnit.Framework;
using UnityEngine;
using static Treasure.Tests.MvtTestWriter;

namespace Treasure.Tests
{
    public class VectorTileMeshBuilderTests
    {
        private const double LAT = 35.8697;
        private const double LON = 128.5955;
        private const int ZOOM = 14;
        private const int TILE_X = 14044;
        private const int TILE_Y = 6441;

        private static MapMeshData[] Build(byte[] tile)
        {
            return VectorTileMeshBuilder.Build(tile, ZOOM, TILE_X, TILE_Y, LAT, LON);
        }

        [Test]
        public void Build_HidesTunnelRoads()
        {
            string[] keys = { "class", "brunnel" };
            string[] values = { "minor", "tunnel" };
            uint[] line = Line(0, 100, 200, 100);

            MapMeshData[] road = Build(Tile(Layer("transportation", keys, values,
                Feature(MvtFeature.TYPE_LINE, new uint[] { 0, 0 }, line))));
            MapMeshData[] tunnel = Build(Tile(Layer("transportation", keys, values,
                Feature(MvtFeature.TYPE_LINE, new uint[] { 0, 0, 1, 1 }, line))));

            Assert.Greater(road[(int)MapSlot.RoadFill].Vertices.Count, 0);
            Assert.Greater(road[(int)MapSlot.RoadCasing].Vertices.Count, 0);
            Assert.AreEqual(0, tunnel[(int)MapSlot.RoadFill].Vertices.Count);
            Assert.AreEqual(0, tunnel[(int)MapSlot.RoadCasing].Vertices.Count);
        }

        [Test]
        public void Build_PlacesFeatureAtGeographicPosition()
        {
            // 표준 Web Mercator 식으로 원점이 타일 안 어느 픽셀인지 직접 구한다
            double scale = 1 << ZOOM;
            double latRad = LAT * Math.PI / 180.0;
            double fx = (LON + 180.0) / 360.0 * scale - TILE_X;
            double fy = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * scale - TILE_Y;
            int px = (int)Math.Round(fx * 4096);
            int py = (int)Math.Round(fy * 4096);

            MapMeshData[] meshes = Build(Tile(Layer("building", new string[0], new string[0],
                Feature(MvtFeature.TYPE_POLYGON, new uint[0], Square(px - 2, py - 2, 4)))));

            var vertices = meshes[(int)MapSlot.Building].Vertices;
            Assert.Greater(vertices.Count, 0);
            Vector3 sum = Vector3.zero;
            foreach (Vector3 vertex in vertices)
            {
                sum += vertex;
            }
            Vector3 average = sum / vertices.Count;
            Assert.Less(new Vector2(average.x, average.y).magnitude, 1f);
        }
    }
}
