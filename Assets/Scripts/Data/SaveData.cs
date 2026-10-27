using System;
using System.Collections.Generic;

namespace Treasure
{
    // PlayerPrefs에 JSON으로 저장하는 진행 상황
    [Serializable]
    public class SaveData
    {
        public int coins;
        public List<int> collectedPieces = new();
        public List<string> capturedCharacters = new();
        public List<ItemStack> items = new();
        public List<SpotCooldown> cooldowns = new();
        public bool cleared;
    }

    [Serializable]
    public class ItemStack
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class SpotCooldown
    {
        public string spotId;
        // 쿨다운이 끝나는 UTC 시각 (DateTime.Ticks)
        public long endUtcTicks;
    }
}
