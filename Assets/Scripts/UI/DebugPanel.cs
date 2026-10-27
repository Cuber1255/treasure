using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure
{
    // 모바일 테스트용 디버그 설정 패널 (개발 빌드와 에디터에서만 열 수 있다)
    public class DebugPanel : Popup
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private MapView _mapView;
        [SerializeField] private VectorMapLoader _vectorMap;
        [SerializeField] private Button _gpsButton;
        [SerializeField] private Button _simulationButton;
        [SerializeField] private Button _followOnButton;
        [SerializeField] private Button _followOffButton;
        [SerializeField] private Button _northUpButton;
        [SerializeField] private Button _headingUpButton;
        [SerializeField] private Button _resetCooldownButton;
        [SerializeField] private Button _resetSaveButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _infoText;

        // 런타임에서는 에디터 UIFactory 색을 쓸 수 없어 따로 둔다
        private static readonly Color SELECTED_COLOR = new Color32(0xFF, 0xB0, 0x20, 0xFF);
        private static readonly Color NORMAL_COLOR = new Color32(0x6F, 0x8F, 0xB8, 0xFF);
        // 누른 색은 조금 어둡게
        private const float PRESSED_TINT = 0.85f;

        private void Awake()
        {
            _gpsButton.onClick.AddListener(() => _game.Location.SetUseSimulation(false));
            _simulationButton.onClick.AddListener(() => _game.Location.SetUseSimulation(true));
            _followOnButton.onClick.AddListener(() => _mapView.SetFollow(true));
            _followOffButton.onClick.AddListener(() => _mapView.SetFollow(false));
            _northUpButton.onClick.AddListener(() => _mapView.HeadingUp = false);
            _headingUpButton.onClick.AddListener(() => _mapView.HeadingUp = true);
            _resetCooldownButton.onClick.AddListener(() => _game.Progress.ClearCooldowns());
            _resetSaveButton.onClick.AddListener(ResetSave);
            _closeButton.onClick.AddListener(Close);
        }

        public void Show()
        {
            Open();
        }

        private void Update()
        {
            LocationProvider location = _game.Location;
            SetSelected(_gpsButton, !location.UseSimulation);
            SetSelected(_simulationButton, location.UseSimulation);
            SetSelected(_followOnButton, _mapView.IsFollowing);
            SetSelected(_followOffButton, !_mapView.IsFollowing);
            SetSelected(_northUpButton, !_mapView.HeadingUp);
            SetSelected(_headingUpButton, _mapView.HeadingUp);
            // 에디터에는 GPS가 없다
            _gpsButton.interactable = !Application.isEditor;
            _infoText.text = BuildInfoText(location);
        }

        private string BuildInfoText(LocationProvider location)
        {
            string accuracy = location.Mode == LocationMode.Gps ? $"{location.HorizontalAccuracy:0}m" : "-";
            string heading = "정보 없음";
            if (location.HasHeading)
            {
                string source = location.HeadingSource == HeadingSource.Compass
                    ? $"나침반 정확도 ±{location.HeadingAccuracy:0}°"
                    : "이동 방향";
                heading = $"{location.HeadingDegrees:0}° ({source})";
            }
            Camera mapCamera = Camera.main;
            float halfHeight = mapCamera != null ? mapCamera.orthographicSize : 0f;

            return $"위치 모드: {GetModeLabel(location.Mode)}\n"
                + $"위도 {location.Latitude:F6}, 경도 {location.Longitude:F6}\n"
                + $"GPS 정확도: {accuracy}\n"
                + $"방향: {heading}\n"
                + $"지도: {_vectorMap.StatusText}\n"
                + $"화면 세로 반경 {halfHeight:0}m · 회전 {_mapView.Bearing:0}°";
        }

        private static string GetModeLabel(LocationMode mode)
        {
            switch (mode)
            {
                case LocationMode.Simulated: return "시뮬레이션";
                case LocationMode.Starting: return "GPS 연결 중";
                case LocationMode.Gps: return "GPS 수신 중";
                default: return "위치 사용 불가";
            }
        }

        private static void SetSelected(Button button, bool selected)
        {
            Color color = selected ? SELECTED_COLOR : NORMAL_COLOR;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color;
            colors.selectedColor = color;
            colors.pressedColor = new Color(color.r * PRESSED_TINT, color.g * PRESSED_TINT, color.b * PRESSED_TINT, 1f);
            button.colors = colors;
        }

        // 저장을 지우고 씬을 다시 불러 처음 상태로 시작한다
        private static void ResetSave()
        {
            SaveSystem.Delete();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
