using UnityEngine;

namespace Treasure
{
    // 포획 액션 중 사용할 수 있는 아이템 효과
    public enum ItemEffect
    {
        AddTime,
        SlowDown,
        FillGauge
    }

    // 상점에서 파는 아이템
    [CreateAssetMenu(menuName = "Treasure/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        // 소지 아이템 저장 키
        public string itemId;
        public string displayName;
        [TextArea] public string description;
        public int price = 100;
        public ItemEffect effect;
        // AddTime: 추가 초, SlowDown: 속도 배율, FillGauge: 채울 비율
        public float effectValue;
        // SlowDown처럼 지속되는 효과의 시간 (초)
        public float durationSeconds;
        public Color iconColor = Color.white;
    }
}
