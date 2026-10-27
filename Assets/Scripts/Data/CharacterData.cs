using UnityEngine;

namespace Treasure
{
    // 스팟에서 잡을 수 있는 캐릭터
    [CreateAssetMenu(menuName = "Treasure/Character Data")]
    public class CharacterData : ScriptableObject
    {
        // 도감 카드 저장 키
        public string characterId;
        public string displayName;
        [TextArea] public string story;
        public Sprite portrait;
        public Color themeColor = Color.white;
        // 포획에 필요한 탭 횟수
        public int tapsToCapture = 8;
        public float timeLimitSeconds = 15f;
        // 추격 화면에서 움직이는 속도 (캔버스 단위/초)
        public float moveSpeed = 400f;
    }
}
