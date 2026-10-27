using System.Collections.Generic;
using UnityEngine;

namespace Treasure
{
    // 한 그리기 단위(MapSlot)의 메시 데이터. 백그라운드 스레드에서 채우고 메인 스레드에서 Mesh로 옮긴다
    public class MapMeshData
    {
        // 선 이음새와 끝을 둥글게 만드는 원의 분할 수
        private const int JOIN_SEGMENTS = 10;
        // 이보다 짧은 선분은 건너뛴다 (미터)
        private const float MIN_SEGMENT_METERS = 0.01f;

        public readonly List<Vector3> Vertices = new();
        public readonly List<int> Triangles = new();

        // ringTriangles에는 삼각형마다 ring 인덱스가 3개씩 들어 있다
        public void AddPolygon(Vector2[] ring, List<int> ringTriangles)
        {
            int baseIndex = Vertices.Count;
            foreach (Vector2 point in ring)
            {
                Vertices.Add(new Vector3(point.x, point.y, 0f));
            }
            foreach (int index in ringTriangles)
            {
                Triangles.Add(baseIndex + index);
            }
        }

        // 선분마다 사각형, 점마다 원판을 붙여 이음새가 둥근 두꺼운 선을 만든다
        public void AddLine(Vector2[] points, float width)
        {
            float half = width / 2f;
            for (int i = 0; i + 1 < points.Length; i++)
            {
                AddSegment(points[i], points[i + 1], half);
            }
            foreach (Vector2 point in points)
            {
                AddDisc(point, half);
            }
        }

        private void AddSegment(Vector2 p0, Vector2 p1, float half)
        {
            Vector2 direction = p1 - p0;
            float length = direction.magnitude;
            if (length <= MIN_SEGMENT_METERS)
            {
                return;
            }
            Vector2 normal = new Vector2(-direction.y, direction.x) / length * half;

            int baseIndex = Vertices.Count;
            Vertices.Add(p0 + normal);
            Vertices.Add(p0 - normal);
            Vertices.Add(p1 - normal);
            Vertices.Add(p1 + normal);
            Triangles.Add(baseIndex);
            Triangles.Add(baseIndex + 1);
            Triangles.Add(baseIndex + 2);
            Triangles.Add(baseIndex);
            Triangles.Add(baseIndex + 2);
            Triangles.Add(baseIndex + 3);
        }

        private void AddDisc(Vector2 center, float radius)
        {
            int centerIndex = Vertices.Count;
            Vertices.Add(center);
            for (int i = 0; i < JOIN_SEGMENTS; i++)
            {
                float angle = i * Mathf.PI * 2f / JOIN_SEGMENTS;
                Vertices.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
            for (int i = 0; i < JOIN_SEGMENTS; i++)
            {
                Triangles.Add(centerIndex);
                Triangles.Add(centerIndex + 1 + i);
                Triangles.Add(centerIndex + 1 + (i + 1) % JOIN_SEGMENTS);
            }
        }
    }
}
