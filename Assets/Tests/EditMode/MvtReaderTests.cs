using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using UnityEngine;
using static Treasure.Tests.MvtTestWriter;

namespace Treasure.Tests
{
    public class MvtReaderTests
    {
        private static readonly Vector2Int[] EXPECTED_RING =
        {
            new Vector2Int(10, 20), new Vector2Int(40, 20), new Vector2Int(40, 50), new Vector2Int(10, 50)
        };

        private static byte[] MinorRoadSquareTile()
        {
            return Tile(Layer("transportation", new[] { "class" }, new[] { "minor" },
                Feature(3, new uint[] { 0, 0 }, Square(10, 20, 30))));
        }

        [Test]
        public void Read_DecodesTagsAndPolygonRing()
        {
            List<MvtLayer> layers = MvtReader.Read(MinorRoadSquareTile());

            Assert.AreEqual(1, layers.Count);
            MvtLayer layer = layers[0];
            Assert.AreEqual("transportation", layer.Name);
            Assert.AreEqual(4096, layer.Extent);
            MvtFeature feature = layer.Features[0];
            Assert.AreEqual(MvtFeature.TYPE_POLYGON, feature.Type);
            Assert.AreEqual("minor", layer.GetTag(feature, "class"));
            Assert.IsNull(layer.GetTag(feature, "brunnel"));

            List<Vector2Int[]> parts = MvtReader.DecodeGeometry(feature.Geometry);
            Assert.AreEqual(1, parts.Count);
            CollectionAssert.AreEqual(EXPECTED_RING, parts[0]);
        }

        [Test]
        public void Read_AcceptsGzipCompressedTile()
        {
            byte[] compressed;
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    byte[] raw = MinorRoadSquareTile();
                    gzip.Write(raw, 0, raw.Length);
                }
                compressed = output.ToArray();
            }

            List<MvtLayer> layers = MvtReader.Read(compressed);

            Assert.AreEqual("transportation", layers[0].Name);
            CollectionAssert.AreEqual(EXPECTED_RING, MvtReader.DecodeGeometry(layers[0].Features[0].Geometry)[0]);
        }
    }
}
