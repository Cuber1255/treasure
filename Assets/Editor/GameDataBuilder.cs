using UnityEditor;
using UnityEngine;

namespace Treasure.EditorTools
{
    // GameContent 표로 캐릭터/스팟/아이템/설정 ScriptableObject를 만든다 (없을 때만)
    internal static class GameDataBuilder
    {
        public const string CONFIG_PATH = "Assets/ScriptableObjects/GameConfig.asset";
        private const string CHARACTER_FOLDER = "Assets/ScriptableObjects/Characters";
        private const string SPOT_FOLDER = "Assets/ScriptableObjects/Spots";
        private const string ITEM_FOLDER = "Assets/ScriptableObjects/Items";

        public static GameConfig EnsureAssets()
        {
            AssetFolders.Ensure(CHARACTER_FOLDER);
            AssetFolders.Ensure(SPOT_FOLDER);
            AssetFolders.Ensure(ITEM_FOLDER);

            int count = GameContent.SPOTS.Length;
            SpotData[] spots = new SpotData[count];
            for (int i = 0; i < count; i++)
            {
                CharacterData character = EnsureCharacter(GameContent.CHARACTERS[i]);
                spots[i] = EnsureSpot(GameContent.SPOTS[i], character);
            }

            ShopItemData[] items = new ShopItemData[GameContent.ITEMS.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = EnsureItem(GameContent.ITEMS[i]);
            }

            GameConfig config = EnsureConfig(spots, items);
            AssetDatabase.SaveAssets();
            return config;
        }

        private static CharacterData EnsureCharacter(GameContent.CharacterEntry entry)
        {
            string path = $"{CHARACTER_FOLDER}/{entry.id}.asset";
            CharacterData existing = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (existing != null)
            {
                return existing;
            }

            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.characterId = entry.id;
            data.displayName = entry.name;
            data.story = entry.story;
            data.portrait = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderArtGenerator.CharacterPath(entry.id));
            data.themeColor = entry.Color;
            data.tapsToCapture = entry.taps;
            data.timeLimitSeconds = entry.timeLimit;
            data.moveSpeed = entry.speed;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static SpotData EnsureSpot(GameContent.SpotEntry entry, CharacterData character)
        {
            string path = $"{SPOT_FOLDER}/{entry.id}.asset";
            SpotData existing = AssetDatabase.LoadAssetAtPath<SpotData>(path);
            if (existing != null)
            {
                return existing;
            }

            SpotData data = ScriptableObject.CreateInstance<SpotData>();
            data.spotId = entry.id;
            data.displayName = entry.name;
            data.description = entry.description;
            data.latitude = entry.lat;
            data.longitude = entry.lon;
            data.captureRadiusMeters = GameContent.CAPTURE_RADIUS_METERS;
            data.character = character;
            data.puzzlePieceIndices = entry.pieces;
            data.isFinalSpot = entry.isFinal;
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static ShopItemData EnsureItem(GameContent.ItemEntry entry)
        {
            string path = $"{ITEM_FOLDER}/{entry.id}.asset";
            ShopItemData existing = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            if (existing != null)
            {
                return existing;
            }

            ShopItemData data = ScriptableObject.CreateInstance<ShopItemData>();
            data.itemId = entry.id;
            data.displayName = entry.name;
            data.description = entry.description;
            data.price = entry.price;
            data.effect = entry.effect;
            data.effectValue = entry.value;
            data.durationSeconds = entry.duration;
            ColorUtility.TryParseHtmlString(entry.colorHex, out data.iconColor);
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static GameConfig EnsureConfig(SpotData[] spots, ShopItemData[] items)
        {
            GameConfig existing = AssetDatabase.LoadAssetAtPath<GameConfig>(CONFIG_PATH);
            if (existing != null)
            {
                return existing;
            }

            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            config.originLatitude = GameContent.ORIGIN_LAT;
            config.originLongitude = GameContent.ORIGIN_LON;
            config.spots = spots;
            config.shopItems = items;
            config.puzzleImage = AssetDatabase.LoadAssetAtPath<Texture2D>(PlaceholderArtGenerator.PUZZLE_PATH);
            AssetDatabase.CreateAsset(config, CONFIG_PATH);
            return config;
        }
    }
}
