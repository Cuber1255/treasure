using System;
using UnityEngine;

namespace Treasure
{
    // 위경도 ↔ 로컬 미터 변환, 거리, 지도 타일 좌표 계산
    public static class GeoUtil
    {
        public const double EARTH_RADIUS_METERS = 6371000.0;
        // 위도 1도에 해당하는 미터
        public const double METERS_PER_DEGREE = EARTH_RADIUS_METERS * Math.PI / 180.0;

        // 원점 기준 동쪽(x)/북쪽(y) 미터. 동성로 범위(수 km)에서는 등장방형 근사로 충분하다
        public static Vector2 ToLocalMeters(double lat, double lon, double originLat, double originLon)
        {
            double x = (lon - originLon) * METERS_PER_DEGREE * Math.Cos(originLat * Math.PI / 180.0);
            double y = (lat - originLat) * METERS_PER_DEGREE;
            return new Vector2((float)x, (float)y);
        }

        public static void FromLocalMeters(Vector2 meters, double originLat, double originLon,
            out double lat, out double lon)
        {
            lat = originLat + meters.y / METERS_PER_DEGREE;
            lon = originLon + meters.x / (METERS_PER_DEGREE * Math.Cos(originLat * Math.PI / 180.0));
        }

        public static void OffsetByMeters(ref double lat, ref double lon, Vector2 meters)
        {
            FromLocalMeters(meters, lat, lon, out double newLat, out double newLon);
            lat = newLat;
            lon = newLon;
        }

        // 로컬 미터 방향 벡터를 나침반 방위로 바꾼다. 북쪽 0, 시계방향 (동쪽 90)
        public static float DirectionToHeading(Vector2 direction)
        {
            return (Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + 360f) % 360f;
        }

        // 하버사인 공식
        public static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double toRad = Math.PI / 180.0;
            double dLat = (lat2 - lat1) * toRad;
            double dLon = (lon2 - lon1) * toRad;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1 * toRad) * Math.Cos(lat2 * toRad) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return 2 * EARTH_RADIUS_METERS * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
        }

        // ---- Web Mercator 타일 좌표 (OSM 표준식) ----

        public static int LonToTileX(double lon, int zoom)
        {
            return (int)Math.Floor((lon + 180.0) / 360.0 * (1 << zoom));
        }

        public static int LatToTileY(double lat, int zoom)
        {
            double latRad = lat * Math.PI / 180.0;
            double n = 1 << zoom;
            return (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n);
        }

        // 소수 타일 좌표도 받는다 (벡터 타일 안의 점 위치 계산)
        public static double TileXToLon(double x, int zoom)
        {
            return x / (double)(1 << zoom) * 360.0 - 180.0;
        }

        public static double TileYToLat(double y, int zoom)
        {
            double n = Math.PI - 2.0 * Math.PI * y / (1 << zoom);
            return 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
        }
    }
}
