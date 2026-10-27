using TMPro;
using UnityEngine;

namespace Treasure
{
    // 지도 화면 위 HUD: 코인/퍼즐 현황, GPS 상태, 힌트, 하단 메뉴 버튼, 디버그/내 위치 버튼, 조이스틱
    public class MainHud : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private CaptureFlow _captureFlow;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private TMP_Text _coinText;
        [SerializeField] private TMP_Text _puzzleText;
        [SerializeField] private TMP_Text _gpsText;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private UnityEngine.UI.Button _catchButton;
        [SerializeField] private UnityEngine.UI.Button _puzzleButton;
        [SerializeField] private UnityEngine.UI.Button _collectionButton;
        [SerializeField] private UnityEngine.UI.Button _shopButton;
        [SerializeField] private MapView _mapView;
        [SerializeField] private DebugPanel _debugPanel;
        [SerializeField] private UnityEngine.UI.Button _debugButton;
        [SerializeField] private UnityEngine.UI.Button _recenterButton;
        [SerializeField] private VirtualJoystick _joystick;

        private void Awake()
        {
            _catchButton.onClick.AddListener(() => _captureFlow.Begin());
            _puzzleButton.onClick.AddListener(() => _router.Show(ScreenId.Puzzle));
            _collectionButton.onClick.AddListener(() => _router.Show(ScreenId.Collection));
            _shopButton.onClick.AddListener(() => _router.Show(ScreenId.Shop));
            _backButton.onClick.AddListener(() => _router.Back());
            _debugButton.onClick.AddListener(() => _debugPanel.Show());
            _recenterButton.onClick.AddListener(() => _mapView.SetFollow(true));
            // 디버그 설정은 개발 빌드와 에디터에서만 보인다
            _debugButton.gameObject.SetActive(Debug.isDebugBuild);
        }

        private void Update()
        {
            PlayerProgress progress = _game.Progress;
            _coinText.text = $"코인 {progress.Coins}";
            _puzzleText.text = $"퍼즐 {progress.CollectedPieceCount}/{_game.TotalPieces}";

            SpotData capturable = _game.FindCapturableSpot();
            _catchButton.interactable = capturable != null;
            _gpsText.text = GetGpsText();
            _hintText.text = GetHintText(capturable);
            _recenterButton.gameObject.SetActive(!_mapView.IsFollowing);
            _joystick.gameObject.SetActive(_game.Location.Mode == LocationMode.Simulated);
        }

        private string GetGpsText()
        {
            LocationProvider location = _game.Location;
            switch (location.Mode)
            {
                case LocationMode.Simulated:
                    return Application.isEditor
                        ? "GPS 시뮬레이션 · WASD/방향키 또는 조이스틱 (Shift 빠르게)"
                        : "GPS 시뮬레이션 · 조이스틱으로 이동";
                case LocationMode.Starting:
                    return "GPS 연결 중...";
                case LocationMode.Gps:
                    return $"GPS 수신 중 (정확도 {location.HorizontalAccuracy:0}m)";
                default:
                    return "위치 서비스를 사용할 수 없어요. 설정에서 위치 권한을 켜 주세요";
            }
        }

        private string GetHintText(SpotData capturable)
        {
            if (capturable != null)
            {
                return $"{capturable.displayName} 도착! 잡기 버튼을 눌러 보세요";
            }
            SpotData nearest = _game.FindNearestOpenSpot();
            if (nearest != null)
            {
                return $"가장 가까운 스팟: {nearest.displayName} {_game.DistanceTo(nearest):0}m";
            }
            return "모든 스팟이 재시도 대기 중이에요";
        }
    }
}
