using UnityEngine;

namespace Treasure
{
    // 게임 전체 설정과 콘텐츠 목록
    [CreateAssetMenu(menuName = "Treasure/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("지도/위치")]
        // 지도 원점이자 시작 위치 (동성로 광장)
        public double originLatitude = 35.8697;
        public double originLongitude = 128.5955;
        // 벡터 지도 TileJSON 주소. tiles[0]의 {z}/{x}/{y} 템플릿으로 타일을 받는다 (OpenFreeMap, 키 없음)
        public string vectorTileJsonUrl = "https://tiles.openfreemap.org/planet";

        [Header("콘텐츠")]
        public SpotData[] spots;
        public ShopItemData[] shopItems;

        [Header("퍼즐")]
        public Texture2D puzzleImage;
        public int puzzleColumns = 4;
        public int puzzleRows = 4;

        [Header("경제/쿨다운")]
        public int startingCoins = 300;
        public int coinsPerCapture = 100;
        // 기획서: 포획 실패 시 3분 후 재시도
        public float failCooldownSeconds = 180f;
        public float successCooldownSeconds = 60f;
    }
}
