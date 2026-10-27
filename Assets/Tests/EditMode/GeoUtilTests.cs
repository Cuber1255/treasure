using NUnit.Framework;
using UnityEngine;

namespace Treasure.Tests
{
    public class GeoUtilTests
    {
        private const double LAT = 35.8697;
        private const double LON = 128.5955;

        [Test]
        public void DistanceOfThousandthDegreeLatitude_IsAbout111Meters()
        {
            double distance = GeoUtil.DistanceMeters(LAT, LON, LAT + 0.001, LON);
            Assert.AreEqual(111.19, distance, 0.5);
        }

        [Test]
        public void LocalMeters_RoundTrip()
        {
            Vector2 meters = GeoUtil.ToLocalMeters(35.86551, 128.59341, LAT, LON);
            GeoUtil.FromLocalMeters(meters, LAT, LON, out double lat, out double lon);

            Assert.AreEqual(35.86551, lat, 1e-6);
            Assert.AreEqual(128.59341, lon, 1e-6);
        }

        [Test]
        public void TileBounds_ContainPoint()
        {
            const int zoom = 17;
            int x = GeoUtil.LonToTileX(LON, zoom);
            int y = GeoUtil.LatToTileY(LAT, zoom);

            Assert.LessOrEqual(GeoUtil.TileXToLon(x, zoom), LON);
            Assert.Less(LON, GeoUtil.TileXToLon(x + 1, zoom));
            Assert.GreaterOrEqual(GeoUtil.TileYToLat(y, zoom), LAT);
            Assert.Greater(LAT, GeoUtil.TileYToLat(y + 1, zoom));
        }

        [Test]
        public void DirectionToHeading_MapsAxesToCompassDegrees()
        {
            Assert.AreEqual(0f, GeoUtil.DirectionToHeading(new Vector2(0f, 1f)), 0.01f);
            Assert.AreEqual(90f, GeoUtil.DirectionToHeading(new Vector2(1f, 0f)), 0.01f);
            Assert.AreEqual(180f, GeoUtil.DirectionToHeading(new Vector2(0f, -1f)), 0.01f);
            Assert.AreEqual(270f, GeoUtil.DirectionToHeading(new Vector2(-1f, 0f)), 0.01f);
        }
    }
}
