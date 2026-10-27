using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

namespace Treasure
{
    // 지도 화면: 플레이어 따라가기/드래그 이동, 핀치/휠 줌, 방향 회전, 스팟 마커 관리
    public class MapView : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private Camera _camera;
        [SerializeField] private SpotInfoPopup _spotInfoPopup;
        [SerializeField] private SpotMarker _spotMarkerPrefab;
        [SerializeField] private Transform _spotRoot;
        [SerializeField] private Transform _playerMarker;
        // 플레이어가 바라보는 방향 화살표 (플레이어 마커의 자식)
        [SerializeField] private Transform _playerArrow;

        // 줌 = 카메라 orthographicSize (화면 세로 절반 길이, 미터)
        [SerializeField] private float _defaultZoom = 200f;
        [SerializeField] private float _minZoom = 40f;
        [SerializeField] private float _maxZoom = 1500f;
        // 아이콘 크기의 기준이 되는 줌
        [SerializeField] private float _referenceZoom = 200f;

        // 기준 줌에서 플레이어 점 지름 (미터)
        private const float PLAYER_SIZE_METERS = 14f;
        private const float WHEEL_ZOOM_IN = 0.9f;
        private const float WHEEL_ZOOM_OUT = 1.1f;
        private const float CAMERA_Z = -10f;
        // 탭과 구분하려고 이만큼(픽셀) 끌어야 드래그로 본다
        private const float PAN_THRESHOLD_PIXELS = 12f;
        // 플레이어 표시 위치의 지수 보간 속도. GPS가 1초 간격으로 점프하는 것을 부드럽게 한다
        private const float PLAYER_SMOOTHING = 6f;
        // 이보다 멀리 바뀌면 순간이동으로 보고 바로 맞춘다
        private const float SNAP_DISTANCE_METERS = 200f;
        private const float BEARING_SMOOTH_TIME = 0.15f;
        private const float ARROW_SMOOTH_TIME = 0.1f;

        private readonly List<SpotMarker> _markers = new();
        private readonly List<RaycastResult> _raycastResults = new();

        private Vector2 _cameraCenter;
        private Vector2 _displayPlayer;
        private bool _hasDisplayPlayer;
        private bool _headingUp;

        private float _bearing;
        private float _bearingVelocity;
        private float _arrowHeading;
        private float _arrowVelocity;

        private bool _pointerDown;
        private bool _isPanning;
        private bool _panBlocked;
        private Vector2 _pressPosition;
        private Vector2 _lastPointerPosition;

        public bool IsFollowing { get; private set; } = true;
        // 화면 위쪽이 가리키는 방위 (북 0, 시계방향)
        public float Bearing => _bearing;

        // true면 지도가 휴대폰 방향을 따라 회전한다. 매 프레임 PlayerPrefs를 읽지 않도록 캐시한다
        public bool HeadingUp
        {
            get => _headingUp;
            set
            {
                _headingUp = value;
                DebugSettings.HeadingUp = value;
            }
        }

        public void SetFollow(bool follow)
        {
            IsFollowing = follow;
        }

        private void Start()
        {
            EnhancedTouchSupport.Enable();
            GameConfig config = _game.Config;
            foreach (SpotData spot in config.spots)
            {
                SpotMarker marker = Instantiate(_spotMarkerPrefab, _spotRoot);
                Vector2 position = GeoUtil.ToLocalMeters(spot.latitude, spot.longitude,
                    config.originLatitude, config.originLongitude);
                marker.Setup(spot, position, OnMarkerClicked);
                _markers.Add(marker);
            }
            _camera.orthographicSize = _defaultZoom;
            _headingUp = DebugSettings.HeadingUp;
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            if (_router.IsMapInteractive)
            {
                HandleZoom();
                HandlePan();
            }
            else
            {
                ResetPan();
            }

            UpdateDisplayPlayer();
            if (IsFollowing)
            {
                _cameraCenter = _displayPlayer;
            }
            UpdateBearing();

            _camera.transform.position = new Vector3(_cameraCenter.x, _cameraCenter.y, CAMERA_Z);
            _camera.transform.rotation = Quaternion.Euler(0f, 0f, -_bearing);

            float zoomScale = _camera.orthographicSize / _referenceZoom;
            UpdatePlayerMarker(zoomScale);
            foreach (SpotMarker marker in _markers)
            {
                marker.Refresh(_game.GetSpotState(marker.Spot), zoomScale);
            }
        }

        // 스팟 판정은 GameManager가 원시 좌표로 하고, 여기서는 보이는 위치만 부드럽게 옮긴다
        private void UpdateDisplayPlayer()
        {
            GameConfig config = _game.Config;
            Vector2 target = GeoUtil.ToLocalMeters(_game.Location.Latitude, _game.Location.Longitude,
                config.originLatitude, config.originLongitude);
            if (!_hasDisplayPlayer || Vector2.Distance(_displayPlayer, target) > SNAP_DISTANCE_METERS)
            {
                _displayPlayer = target;
                _hasDisplayPlayer = true;
                return;
            }
            _displayPlayer = Vector2.Lerp(_displayPlayer, target, 1f - Mathf.Exp(-PLAYER_SMOOTHING * Time.deltaTime));
        }

        private void UpdateBearing()
        {
            LocationProvider location = _game.Location;
            float targetBearing = _headingUp && location.HasHeading ? location.HeadingDegrees : 0f;
            _bearing = Mathf.SmoothDampAngle(_bearing, targetBearing, ref _bearingVelocity, BEARING_SMOOTH_TIME);
            _bearing = Mathf.Repeat(_bearing, 360f);
            _arrowHeading = Mathf.SmoothDampAngle(_arrowHeading, location.HeadingDegrees, ref _arrowVelocity,
                ARROW_SMOOTH_TIME);
        }

        private void UpdatePlayerMarker(float zoomScale)
        {
            _playerMarker.localPosition = _displayPlayer;
            float playerSize = PLAYER_SIZE_METERS * zoomScale;
            _playerMarker.localScale = new Vector3(playerSize, playerSize, 1f);

            bool hasHeading = _game.Location.HasHeading;
            _playerArrow.gameObject.SetActive(hasHeading);
            // 월드 기준 회전이라 지도 회전 모드에서는 자동으로 화면 위를 가리킨다
            _playerArrow.localRotation = Quaternion.Euler(0f, 0f, -_arrowHeading);
        }

        private void HandleZoom()
        {
            float zoom = _camera.orthographicSize;

            var touches = EnhancedTouch.Touch.activeTouches;
            if (touches.Count == 2)
            {
                Vector2 a = touches[0].screenPosition;
                Vector2 b = touches[1].screenPosition;
                float previous = Vector2.Distance(a - touches[0].delta, b - touches[1].delta);
                float current = Vector2.Distance(a, b);
                if (current > 0.01f)
                {
                    // 손가락을 벌리면(current 증가) 확대 = orthographicSize 감소
                    zoom *= previous / current;
                }
            }

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll > 0f) zoom *= WHEEL_ZOOM_IN;
                else if (scroll < 0f) zoom *= WHEEL_ZOOM_OUT;
            }

            _camera.orthographicSize = Mathf.Clamp(zoom, _minZoom, _maxZoom);
        }

        // 한 손가락(또는 마우스) 드래그로 지도를 옮긴다. 옮기기 시작하면 따라가기가 꺼진다
        private void HandlePan()
        {
            if (!TryReadPointer(out bool pressed, out Vector2 position))
            {
                ResetPan();
                return;
            }

            if (!pressed)
            {
                _pointerDown = false;
                _isPanning = false;
                return;
            }

            if (!_pointerDown)
            {
                BeginPointer(position);
                return;
            }

            if (!_panBlocked)
            {
                DragPointer(position);
            }
            _lastPointerPosition = position;
        }

        // 터치 1개면 터치, 없으면 마우스. 터치 2개 이상(핀치)이면 false
        private bool TryReadPointer(out bool pressed, out Vector2 position)
        {
            pressed = false;
            position = Vector2.zero;
            var touches = EnhancedTouch.Touch.activeTouches;
            if (touches.Count >= 2)
            {
                return false;
            }
            if (touches.Count == 1)
            {
                EnhancedTouch.Touch touch = touches[0];
                position = touch.screenPosition;
                pressed = touch.phase != UnityEngine.InputSystem.TouchPhase.Ended
                    && touch.phase != UnityEngine.InputSystem.TouchPhase.Canceled;
                return true;
            }
            if (Mouse.current == null)
            {
                return true;
            }
            position = Mouse.current.position.ReadValue();
            pressed = Mouse.current.leftButton.isPressed;
            return true;
        }

        private void BeginPointer(Vector2 position)
        {
            _pressPosition = position;
            _lastPointerPosition = position;
            _pointerDown = true;
            _isPanning = false;

            // UI, 조이스틱, 스팟 마커 위에서 시작하면 지도를 옮기지 않는다.
            // IsPointerOverGameObject는 EventSystem 갱신 순서 때문에 첫 프레임에 틀릴 수 있어 직접 레이캐스트한다
            _raycastResults.Clear();
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, _raycastResults);
            }
            _panBlocked = _raycastResults.Count > 0;
        }

        private void DragPointer(Vector2 position)
        {
            if (!_isPanning && (position - _pressPosition).magnitude > PAN_THRESHOLD_PIXELS)
            {
                _isPanning = true;
                SetFollow(false);
            }
            if (!_isPanning)
            {
                return;
            }

            Vector2 deltaPixels = position - _lastPointerPosition;
            float metersPerPixel = 2f * _camera.orthographicSize / _camera.pixelHeight;
            // 화면 방향의 이동량을 지도 회전만큼 돌려 월드 방향으로 바꾼다
            Vector2 worldDelta = Quaternion.Euler(0f, 0f, -_bearing) * (deltaPixels * metersPerPixel);
            _cameraCenter -= worldDelta;
        }

        private void ResetPan()
        {
            _pointerDown = false;
            _isPanning = false;
        }

        private void OnMarkerClicked(SpotData spot)
        {
            if (_router.IsMapInteractive)
            {
                _spotInfoPopup.Show(spot);
            }
        }
    }
}
