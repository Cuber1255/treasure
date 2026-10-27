using System.Collections.Generic;
using UnityEngine;

namespace Treasure
{
    // 벡터 타일 한 장을 MapSlot별 메시 데이터로 만든다. Unity 오브젝트를 쓰지 않아 백그라운드 스레드에서 돌린다
    // 여기 분류에 없는 레이어(글자·POI·지번·철도 등)는 그리지 않는다
    public static class VectorTileMeshBuilder
    {
        // 도로 테두리(케이싱) 한쪽 두께 (미터)
        private const float CASING_METERS = 1f;

        public static MapMeshData[] Build(byte[] data, int zoom, int tileX, int tileY, double originLat, double originLon)
        {
            List<MvtLayer> layers = MvtReader.Read(data);
            var meshes = new MapMeshData[VectorMapStyle.SLOT_COUNT];
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i] = new MapMeshData();
            }

            var triangles = new List<int>();
            foreach (MvtLayer layer in layers)
            {
                var projection = new TileProjection(zoom, tileX, tileY, layer.Extent, originLat, originLon);
                foreach (MvtFeature feature in layer.Features)
                {
                    AddFeature(layer, feature, projection, meshes, triangles);
                }
            }
            return meshes;
        }

        private static void AddFeature(MvtLayer layer, MvtFeature feature, TileProjection projection,
            MapMeshData[] meshes, List<int> triangles)
        {
            if (feature.Type == MvtFeature.TYPE_POLYGON && TryGetPolygonSlot(layer, feature, out MapSlot polygonSlot))
            {
                AddPolygons(feature, projection, meshes[(int)polygonSlot], triangles);
            }
            else if (feature.Type == MvtFeature.TYPE_LINE)
            {
                AddLineFeature(layer, feature, projection, meshes);
            }
        }

        private static bool TryGetPolygonSlot(MvtLayer layer, MvtFeature feature, out MapSlot slot)
        {
            string cls = layer.GetTag(feature, "class");
            slot = MapSlot.Green;
            switch (layer.Name)
            {
                case "landcover":
                    if (cls == "wood") { slot = MapSlot.Wood; return true; }
                    return cls == "grass" || cls == "wetland";
                case "landuse":
                    return cls == "pitch" || cls == "playground";
                case "water":
                    slot = MapSlot.Water;
                    return layer.GetTag(feature, "brunnel") != "tunnel" && cls != "swimming_pool";
                case "building":
                    slot = MapSlot.Building;
                    return true;
                case "transportation":
                    slot = MapSlot.Plaza;
                    return cls == "path" && layer.GetTag(feature, "subclass") == "pedestrian";
                default:
                    return false;
            }
        }

        private static void AddLineFeature(MvtLayer layer, MvtFeature feature, TileProjection projection,
            MapMeshData[] meshes)
        {
            if (layer.GetTag(feature, "brunnel") == "tunnel")
            {
                return;
            }
            string cls = layer.GetTag(feature, "class");
            if (layer.Name == "waterway" && TryGetWaterwayWidth(cls, out float waterWidth))
            {
                AddLines(feature, projection, meshes[(int)MapSlot.Water], waterWidth, null, 0f);
            }
            else if (layer.Name == "transportation" && layer.GetTag(feature, "indoor") != "1"
                && TryGetRoadStyle(cls, layer.GetTag(feature, "subclass"), out MapSlot slot, out float width, out bool casing))
            {
                MapMeshData casingMesh = casing ? meshes[(int)MapSlot.RoadCasing] : null;
                AddLines(feature, projection, meshes[(int)slot], width, casingMesh, width + 2f * CASING_METERS);
            }
        }

        // 물길 폭 (미터)
        private static bool TryGetWaterwayWidth(string cls, out float width)
        {
            switch (cls)
            {
                case "river": width = 12f; return true;
                case "canal": width = 8f; return true;
                case "stream": width = 2.5f; return true;
                default: width = 0f; return false;
            }
        }

        // 도로 종류별 전체 폭(미터)·색 슬롯·테두리 여부
        private static bool TryGetRoadStyle(string cls, string subclass, out MapSlot slot, out float width, out bool casing)
        {
            slot = MapSlot.RoadFill;
            casing = true;
            switch (cls)
            {
                case "motorway": slot = MapSlot.MajorRoadFill; width = 20f; return true;
                case "trunk": slot = MapSlot.MajorRoadFill; width = 18f; return true;
                case "primary": slot = MapSlot.MajorRoadFill; width = 16f; return true;
                case "secondary": width = 13f; return true;
                case "tertiary": width = 11f; return true;
                case "minor": width = 7f; return true;
                case "service":
                case "busway": width = 4.5f; return true;
                case "track": width = 3f; casing = false; return true;
                case "path": width = subclass == "pedestrian" ? 5f : 2.5f; casing = false; return true;
                default: width = 0f; casing = false; return false;
            }
        }

        // 바깥 링만 칠한다. 구멍은 무시하고 위 슬롯이 덮는다
        private static void AddPolygons(MvtFeature feature, TileProjection projection, MapMeshData mesh, List<int> triangles)
        {
            foreach (Vector2Int[] part in MvtReader.DecodeGeometry(feature.Geometry))
            {
                Vector2Int[] ring = RemoveDuplicates(part, true);
                if (ring.Length < 3 || PolygonTriangulator.SignedArea2(ring) <= 0)
                {
                    continue;
                }
                triangles.Clear();
                PolygonTriangulator.Triangulate(ring, triangles);
                if (triangles.Count > 0)
                {
                    mesh.AddPolygon(projection.ToMeters(ring), triangles);
                }
            }
        }

        private static void AddLines(MvtFeature feature, TileProjection projection, MapMeshData mesh, float width,
            MapMeshData casingMesh, float casingWidth)
        {
            foreach (Vector2Int[] part in MvtReader.DecodeGeometry(feature.Geometry))
            {
                Vector2Int[] line = RemoveDuplicates(part, false);
                if (line.Length < 2)
                {
                    continue;
                }
                Vector2[] meters = projection.ToMeters(line);
                casingMesh?.AddLine(meters, casingWidth);
                mesh.AddLine(meters, width);
            }
        }

        // 연속 중복 점을 뺀다. isRing이면 첫 점과 같은 마지막 점도 뺀다
        private static Vector2Int[] RemoveDuplicates(Vector2Int[] points, bool isRing)
        {
            var result = new List<Vector2Int>(points.Length);
            foreach (Vector2Int point in points)
            {
                if (result.Count == 0 || result[result.Count - 1] != point)
                {
                    result.Add(point);
                }
            }
            if (isRing && result.Count > 1 && result[result.Count - 1] == result[0])
            {
                result.RemoveAt(result.Count - 1);
            }
            return result.ToArray();
        }

        // 타일 정수 좌표 → 원점 기준 로컬 미터
        private readonly struct TileProjection
        {
            private readonly int _zoom;
            private readonly int _tileX;
            private readonly int _tileY;
            private readonly double _extent;
            private readonly double _originLat;
            private readonly double _originLon;

            public TileProjection(int zoom, int tileX, int tileY, int extent, double originLat, double originLon)
            {
                _zoom = zoom;
                _tileX = tileX;
                _tileY = tileY;
                _extent = extent;
                _originLat = originLat;
                _originLon = originLon;
            }

            public Vector2[] ToMeters(Vector2Int[] points)
            {
                var result = new Vector2[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    double lon = GeoUtil.TileXToLon(_tileX + points[i].x / _extent, _zoom);
                    double lat = GeoUtil.TileYToLat(_tileY + points[i].y / _extent, _zoom);
                    result[i] = GeoUtil.ToLocalMeters(lat, lon, _originLat, _originLon);
                }
                return result;
            }
        }
    }
}
