using System;
using System.Collections.Generic;
using UnityEngine;

namespace Treasure
{
    // 단순 다각형 링을 귀 자르기로 삼각형으로 나눈다. 좌표는 타일 정수 좌표라 외적이 정확하다
    public static class PolygonTriangulator
    {
        // ring은 마지막 점이 첫 점과 겹치지 않아야 한다. 삼각형마다 ring 인덱스 3개를 result에 붙인다
        // 진행이 막히면(꼬인 링) 그때까지 만든 삼각형만 남긴다
        public static void Triangulate(Vector2Int[] ring, List<int> result)
        {
            int n = ring.Length;
            long area2 = n < 3 ? 0 : SignedArea2(ring);
            if (area2 == 0)
            {
                return;
            }
            int sign = Math.Sign(area2);

            var remaining = new List<int>(n);
            for (int k = 0; k < n; k++)
            {
                remaining.Add(k);
            }

            int i = 0;
            int misses = 0;
            while (remaining.Count > 3 && misses < remaining.Count)
            {
                int count = remaining.Count;
                int a = remaining[(i - 1 + count) % count];
                int b = remaining[i];
                int c = remaining[(i + 1) % count];
                long cross = Cross(ring[a], ring[b], ring[c]);

                if (cross == 0)
                {
                    // 일직선·되돌아가는 점은 삼각형 없이 뺀다
                    remaining.RemoveAt(i);
                    misses = 0;
                }
                else if (Math.Sign(cross) == sign && !ContainsOtherPoint(ring, remaining, a, b, c))
                {
                    result.Add(a);
                    result.Add(b);
                    result.Add(c);
                    remaining.RemoveAt(i);
                    misses = 0;
                }
                else
                {
                    i++;
                    misses++;
                }

                if (i >= remaining.Count)
                {
                    i = 0;
                }
            }

            if (remaining.Count == 3 && Cross(ring[remaining[0]], ring[remaining[1]], ring[remaining[2]]) != 0)
            {
                result.Add(remaining[0]);
                result.Add(remaining[1]);
                result.Add(remaining[2]);
            }
        }

        // Σ(xᵢ·yᵢ₊₁ − xᵢ₊₁·yᵢ) (면적의 2배). MVT 바깥 링은 양수
        public static long SignedArea2(Vector2Int[] ring)
        {
            long sum = 0;
            for (int i = 0; i < ring.Length; i++)
            {
                Vector2Int p = ring[i];
                Vector2Int q = ring[(i + 1) % ring.Length];
                sum += (long)p.x * q.y - (long)q.x * p.y;
            }
            return sum;
        }

        // (b − a) × (c − b)
        private static long Cross(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return (long)(b.x - a.x) * (c.y - b.y) - (long)(b.y - a.y) * (c.x - b.x);
        }

        private static bool ContainsOtherPoint(Vector2Int[] ring, List<int> remaining, int a, int b, int c)
        {
            Vector2Int pa = ring[a];
            Vector2Int pb = ring[b];
            Vector2Int pc = ring[c];
            foreach (int index in remaining)
            {
                if (index == a || index == b || index == c)
                {
                    continue;
                }
                Vector2Int p = ring[index];
                // 자기 접촉 링에서 같은 좌표의 점은 귀를 막지 않는다
                if (p == pa || p == pb || p == pc)
                {
                    continue;
                }
                if (PointInTriangle(p, pa, pb, pc))
                {
                    return true;
                }
            }
            return false;
        }

        // 경계 포함
        private static bool PointInTriangle(Vector2Int p, Vector2Int a, Vector2Int b, Vector2Int c)
        {
            long d1 = EdgeSide(a, b, p);
            long d2 = EdgeSide(b, c, p);
            long d3 = EdgeSide(c, a, p);
            bool hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            bool hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNegative && hasPositive);
        }

        // (v − u) × (p − u)
        private static long EdgeSide(Vector2Int u, Vector2Int v, Vector2Int p)
        {
            return (long)(v.x - u.x) * (p.y - u.y) - (long)(v.y - u.y) * (p.x - u.x);
        }
    }
}
