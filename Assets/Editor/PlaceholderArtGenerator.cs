using System.IO;
using UnityEditor;
using UnityEngine;

namespace Treasure.EditorTools
{
    // 아트가 들어오기 전까지 쓸 임시 이미지(원, 격자, 캐릭터 초상화, 퍼즐 그림)를 PNG로 만든다
    // 같은 경로의 PNG가 이미 있으면 건드리지 않으므로 아트를 교체해도 유지된다
    internal static class PlaceholderArtGenerator
    {
        public const string CIRCLE_PATH = "Assets/Art/Sprites/Map/Circle.png";
        public const string CHARACTER_FOLDER = "Assets/Art/Sprites/Characters";
        public const string PUZZLE_PATH = "Assets/Art/Sprites/Puzzle/DongseongnoPuzzle.png";
        public const string HEADING_ARROW_PATH = "Assets/Art/Sprites/Map/HeadingArrow.png";

        // 원 가장자리 안티앨리어싱 폭 (픽셀)
        private const float EDGE_SOFTNESS = 1.5f;
        private const int PUZZLE_SIZE = 1024;

        public static string CharacterPath(string id) => $"{CHARACTER_FOLDER}/{id}.png";

        public static void EnsureArt()
        {
            if (!File.Exists(CIRCLE_PATH))
            {
                CreateCircle();
            }
            foreach (GameContent.CharacterEntry character in GameContent.CHARACTERS)
            {
                if (!File.Exists(CharacterPath(character.id)))
                {
                    CreatePortrait(character);
                }
            }
            if (!File.Exists(PUZZLE_PATH))
            {
                CreatePuzzleImage();
            }
            if (!File.Exists(HEADING_ARROW_PATH))
            {
                CreateHeadingArrow();
            }
        }

        private static void CreateCircle()
        {
            const int size = 128;
            Color[] pixels = new Color[size * size];
            DrawCircle(pixels, size, new Vector2(size / 2f, size / 2f), size / 2f - 1f, Color.white);
            WritePng(CIRCLE_PATH, pixels, size);
            ImportSprite(CIRCLE_PATH, size, TextureWrapMode.Clamp);
        }

        // 위쪽을 가리키는 흰 삼각형. 플레이어 방향 화살표에 색을 입혀 쓴다
        private static void CreateHeadingArrow()
        {
            const int size = 128;
            Color[] pixels = new Color[size * size];
            DrawTriangle(pixels, size, new Vector2(64f, 118f), new Vector2(14f, 22f), new Vector2(114f, 22f), Color.white);
            WritePng(HEADING_ARROW_PATH, pixels, size);
            ImportSprite(HEADING_ARROW_PATH, size, TextureWrapMode.Clamp);
        }

        private static void CreatePortrait(GameContent.CharacterEntry character)
        {
            const int size = 256;
            Color[] pixels = new Color[size * size];
            Color body = character.Color;
            Color outline = new Color(body.r * 0.7f, body.g * 0.7f, body.b * 0.7f, 1f);

            DrawCircle(pixels, size, new Vector2(128, 120), 110, outline);
            DrawCircle(pixels, size, new Vector2(128, 120), 104, body);
            DrawCircle(pixels, size, new Vector2(95, 140), 22, Color.white);
            DrawCircle(pixels, size, new Vector2(161, 140), 22, Color.white);
            DrawCircle(pixels, size, new Vector2(95, 137), 10, Color.black);
            DrawCircle(pixels, size, new Vector2(161, 137), 10, Color.black);

            string path = CharacterPath(character.id);
            WritePng(path, pixels, size);
            ImportSprite(path, size, TextureWrapMode.Clamp);
        }

        private static void CreatePuzzleImage()
        {
            const int size = PUZZLE_SIZE;
            Color top = Hex("#AFC8E8");
            Color bottom = Hex("#E4EEF9");
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                // 텍스처 y=size-1이 위쪽
                Color row = Color.Lerp(bottom, top, y / (float)(size - 1));
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = row;
                }
            }
            DrawStreets(pixels, size);
            DrawSpots(pixels, size);

            WritePng(PUZZLE_PATH, pixels, size);
            AssetDatabase.ImportAsset(PUZZLE_PATH);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(PUZZLE_PATH);
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        // 64px 오프셋에서 128px 간격의 흰 길
        private static void DrawStreets(Color[] pixels, int size)
        {
            const int halfWidth = 3;
            for (int center = 64; center < size; center += 128)
            {
                for (int offset = -halfWidth; offset < halfWidth; offset++)
                {
                    int line = center + offset;
                    for (int i = 0; i < size; i++)
                    {
                        pixels[line * size + i] = Color.white;
                        pixels[i * size + line] = Color.white;
                    }
                }
            }
        }

        private static void DrawSpots(Color[] pixels, int size)
        {
            float pixelsPerMeter = size / (GameContent.PUZZLE_HALF_SIZE_METERS * 2f);
            for (int i = 0; i < GameContent.SPOTS.Length; i++)
            {
                GameContent.SpotEntry spot = GameContent.SPOTS[i];
                Vector2 meters = GeoUtil.ToLocalMeters(spot.lat, spot.lon, GameContent.ORIGIN_LAT, GameContent.ORIGIN_LON);
                Vector2 center = new Vector2(size / 2f, size / 2f) + meters * pixelsPerMeter;
                float radius = spot.isFinal ? 54f : 36f;
                Color color = spot.isFinal ? Hex("#E5484D") : GameContent.CHARACTERS[i].Color;
                // 흰 6px 링 위에 색 점
                DrawCircle(pixels, size, center, radius + 6f, Color.white);
                DrawCircle(pixels, size, center, radius, color);
            }
        }

        // 가장자리를 부드럽게 한 원을 기존 픽셀 위에 덧그린다
        private static void DrawCircle(Color[] pixels, int size, Vector2 center, float radius, Color color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius - 2));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(center.x + radius + 2));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius - 2));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(center.y + radius + 2));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float coverage = Mathf.Clamp01((radius - distance) / EDGE_SOFTNESS + 0.5f);
                    if (coverage <= 0f)
                    {
                        continue;
                    }
                    int index = y * size + x;
                    pixels[index] = BlendOver(pixels[index], color, coverage * color.a);
                }
            }
        }

        // 가장자리를 부드럽게 한 삼각형을 기존 픽셀 위에 덧그린다. 세 변까지의 거리 중 최솟값으로 덮임 정도를 정한다
        private static void DrawTriangle(Color[] pixels, int size, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            // 꼭짓점이 시계방향이면 순서를 바꿔 안쪽이 항상 양수가 되게 한다
            if ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0f)
            {
                (b, c) = (c, b);
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float inside = Mathf.Min(EdgeDistance(a, b, p), Mathf.Min(EdgeDistance(b, c, p), EdgeDistance(c, a, p)));
                    float coverage = Mathf.Clamp01(inside / EDGE_SOFTNESS + 0.5f);
                    if (coverage <= 0f)
                    {
                        continue;
                    }
                    int index = y * size + x;
                    pixels[index] = BlendOver(pixels[index], color, coverage * color.a);
                }
            }
        }

        // 변 from→to의 왼쪽이 양수인 부호 거리
        private static float EdgeDistance(Vector2 from, Vector2 to, Vector2 point)
        {
            Vector2 edge = to - from;
            Vector2 offset = point - from;
            return (edge.x * offset.y - edge.y * offset.x) / edge.magnitude;
        }

        private static Color BlendOver(Color destination, Color source, float sourceAlpha)
        {
            float outAlpha = sourceAlpha + destination.a * (1f - sourceAlpha);
            if (outAlpha <= 0f)
            {
                return Color.clear;
            }
            Color result = (source * sourceAlpha + destination * destination.a * (1f - sourceAlpha)) / outAlpha;
            result.a = outAlpha;
            return result;
        }

        private static void WritePng(string path, Color[] pixels, int size)
        {
            AssetFolders.EnsureParentOf(path);
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        // 픽셀 크기 = PPU라서 스프라이트 한 장이 1x1 유닛이 된다
        private static void ImportSprite(string path, int pixelsPerUnit, TextureWrapMode wrapMode)
        {
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = wrapMode;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            // 타일 그리기를 지원하고 원 모양이 고르게 나오도록 사각 메시를 쓴다
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
