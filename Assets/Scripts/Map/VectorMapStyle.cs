using UnityEngine;

namespace Treasure
{
    // 벡터 지도 그리기 단위. 값 순서가 곧 그리는 순서(아래→위)다
    public enum MapSlot
    {
        Green,
        Wood,
        Water,
        Plaza,
        Building,
        RoadCasing,
        RoadFill,
        MajorRoadFill
    }

    // 벡터 지도 색과 그리는 순서 (밝은 기본 톤)
    public static class VectorMapStyle
    {
        public const int SLOT_COUNT = 8;

        // 지도 바탕(땅) = 카메라 배경색
        public static readonly Color LAND_COLOR = new Color32(0xF2, 0xF1, 0xED, 0xFF);

        // 스팟 마커(0~11)와 플레이어(19~21)보다 아래
        private const int FIRST_SORTING_ORDER = -95;

        // MapSlot 순서와 같은 순서의 색
        private static readonly Color32[] SLOT_COLORS =
        {
            new Color32(0xCF, 0xE6, 0xC4, 0xFF), // Green: 공원·잔디
            new Color32(0xBB, 0xDB, 0xA9, 0xFF), // Wood: 숲
            new Color32(0xA9, 0xD2, 0xF0, 0xFF), // Water: 물
            new Color32(0xFF, 0xFF, 0xFF, 0xFF), // Plaza: 보행자 광장
            new Color32(0xE2, 0xDF, 0xD8, 0xFF), // Building: 건물
            new Color32(0xD2, 0xD0, 0xC9, 0xFF), // RoadCasing: 도로 테두리
            new Color32(0xFF, 0xFF, 0xFF, 0xFF), // RoadFill: 일반 도로
            new Color32(0xFC, 0xE9, 0xAE, 0xFF)  // MajorRoadFill: 큰길
        };

        public static Color GetColor(MapSlot slot)
        {
            return SLOT_COLORS[(int)slot];
        }

        public static int GetSortingOrder(MapSlot slot) => FIRST_SORTING_ORDER + (int)slot;
    }
}
