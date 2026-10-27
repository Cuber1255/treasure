using System;
using UnityEngine;

namespace Treasure
{
    // 진행 상황을 PlayerPrefs에 JSON으로 저장/불러온다
    public static class SaveSystem
    {
        // 저장 형식이 바뀌면 버전을 올려 이전 데이터와 섞이지 않게 한다
        public const string SAVE_KEY = "dongseongno_save_v1";

        public static SaveData Load(int startingCoins)
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                return new SaveData { coins = startingCoins };
            }

            SaveData data = null;
            try
            {
                data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SAVE_KEY));
            }
            catch (Exception)
            {
                data = null;
            }

            if (data == null)
            {
                Debug.LogWarning("저장 데이터를 읽지 못해 새로 시작합니다");
                return new SaveData { coins = startingCoins };
            }
            return data;
        }

        public static void Save(SaveData data)
        {
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }
    }
}
