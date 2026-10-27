using UnityEngine;

namespace Treasure
{
    // 지도 위의 탐험 스팟 하나
    [CreateAssetMenu(menuName = "Treasure/Spot Data")]
    public class SpotData : ScriptableObject
    {
        // 저장 데이터의 쿨다운 키로 쓰이므로 출시 후 바꾸지 않는다
        public string spotId;
        public string displayName;
        // 스팟 소개 스토리 (도감 상세에 함께 보여 준다)
        [TextArea] public string description;
        public double latitude;
        public double longitude;
        public float captureRadiusMeters = 40f;
        public CharacterData character;
        // 이 스팟에서 포획하면 순서대로 지급하는 퍼즐 조각 번호
        public int[] puzzlePieceIndices;
        // 퍼즐을 모두 모아야 열리는 최종 스팟인지
        public bool isFinalSpot;
    }
}
