using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Treasure
{
    // 현재 위치를 어디서 얻고 있는지
    public enum LocationMode
    {
        Simulated,
        Starting,
        Gps,
        Unavailable
    }

    // 바라보는 방향을 어디서 얻고 있는지
    public enum HeadingSource
    {
        None,
        Compass,
        Movement
    }

    // 플레이어 위치 제공. 기기에서는 GPS와 시뮬레이션을 바로 전환할 수 있고, 에디터는 항상 시뮬레이션
    public class LocationProvider : MonoBehaviour
    {
        // 에디터 테스트용 이동 속도 (미터/초). Shift를 누르면 5배
        [SerializeField] private float _simulatedSpeedMetersPerSecond = 20f;

        private const float SPRINT_MULTIPLIER = 5f;
        // GPS 갱신 기준: 원하는 정확도 5m, 2m 이상 움직이면 갱신
        private const float GPS_ACCURACY_METERS = 5f;
        private const float GPS_UPDATE_DISTANCE_METERS = 2f;
        private const float GPS_START_TIMEOUT_SECONDS = 20f;
        // 이보다 약한 입력은 방향으로 쓰지 않는다 (조이스틱 미세 떨림 무시)
        private const float MOVEMENT_HEADING_MIN_INPUT = 0.1f;

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public float HorizontalAccuracy { get; private set; }
        public LocationMode Mode { get; private set; } = LocationMode.Simulated;

        // 원시 방위(0~360, 북 0 시계방향)
        public float HeadingDegrees { get; private set; }
        public HeadingSource HeadingSource { get; private set; }
        public bool HasHeading => HeadingSource != HeadingSource.None;
        public float HeadingAccuracy { get; private set; }
        public bool UseSimulation => _useSimulation;

        private bool _gpsStarted;
        private bool _useSimulation;
        private bool _serviceRunning;
        private bool _serviceFailed;
        private Vector2 _joystickInput;
        private float _movementHeading;
        private bool _hasMovementHeading;

        public void Initialize(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        // 디버그용 순간 이동. GPS 값이 다음 프레임에 덮어쓰지 않도록 시뮬레이션으로 바꾼다
        public void TeleportTo(double lat, double lon)
        {
            SetUseSimulation(true);
            Latitude = lat;
            Longitude = lon;
        }

        // 화면 조이스틱 입력 (-1~1)
        public void SetSimulatedMoveInput(Vector2 input)
        {
            _joystickInput = Vector2.ClampMagnitude(input, 1f);
        }

        // 실제 GPS와 시뮬레이션을 바로 전환한다. 위치는 현재 값에서 이어간다
        public void SetUseSimulation(bool simulated)
        {
            if (Application.isEditor)
            {
                simulated = true;
            }
            DebugSettings.UseSimulatedLocation = simulated;
            _useSimulation = simulated;

            if (simulated)
            {
                Mode = LocationMode.Simulated;
            }
            else
            {
                Mode = _serviceRunning ? LocationMode.Gps
                    : (_serviceFailed ? LocationMode.Unavailable : LocationMode.Starting);
            }
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                // 에디터에서는 Input.location을 건드리지 않고 키보드/조이스틱으로 이동한다
                _useSimulation = true;
                Mode = LocationMode.Simulated;
                return;
            }
            _useSimulation = DebugSettings.UseSimulatedLocation;
            Mode = _useSimulation ? LocationMode.Simulated : LocationMode.Starting;
            // 나침반 trueHeading에 위치 서비스가 필요하고 위치 모드도 즉시 전환해야 하므로 모드와 상관없이 시작한다
            StartCoroutine(StartLocationService());
        }

        private IEnumerator StartLocationService()
        {
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
                float waited = 0f;
                while (waited < 10f && !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
#endif
            if (!Input.location.isEnabledByUser)
            {
                _serviceFailed = true;
                if (!_useSimulation)
                {
                    Mode = LocationMode.Unavailable;
                }
                yield break;
            }

            Input.location.Start(GPS_ACCURACY_METERS, GPS_UPDATE_DISTANCE_METERS);
            Input.compass.enabled = true;
            _gpsStarted = true;

            float elapsed = 0f;
            while (Input.location.status == LocationServiceStatus.Initializing && elapsed < GPS_START_TIMEOUT_SECONDS)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _serviceRunning = Input.location.status == LocationServiceStatus.Running;
            _serviceFailed = !_serviceRunning;
            if (!_useSimulation)
            {
                Mode = _serviceRunning ? LocationMode.Gps : LocationMode.Unavailable;
            }
        }

        private void Update()
        {
            if (Mode == LocationMode.Gps)
            {
                LocationInfo data = Input.location.lastData;
                Latitude = data.latitude;
                Longitude = data.longitude;
                HorizontalAccuracy = data.horizontalAccuracy;
            }
            else if (Mode == LocationMode.Simulated)
            {
                MoveSimulated();
            }
            UpdateHeading();
        }

        // 키보드와 조이스틱 입력을 합쳐 이동한다. 조이스틱을 반만 밀면 반 속도
        private void MoveSimulated()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 keys = Vector2.zero;
            bool sprint = false;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) keys.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) keys.y -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) keys.x += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) keys.x -= 1f;
                sprint = keyboard.shiftKey.isPressed;
            }

            Vector2 direction = Vector2.ClampMagnitude(keys.normalized + _joystickInput, 1f);
            if (direction == Vector2.zero)
            {
                return;
            }

            float speed = _simulatedSpeedMetersPerSecond * (sprint ? SPRINT_MULTIPLIER : 1f);
            double lat = Latitude;
            double lon = Longitude;
            GeoUtil.OffsetByMeters(ref lat, ref lon, direction * (speed * Time.deltaTime));
            Latitude = lat;
            Longitude = lon;

            if (direction.magnitude > MOVEMENT_HEADING_MIN_INPUT)
            {
                _movementHeading = GeoUtil.DirectionToHeading(direction);
                _hasMovementHeading = true;
            }
        }

        // 나침반이 있으면 나침반이 우선이다 (기기 시뮬레이션 중에도 휴대폰을 돌리면 화살표가 돈다)
        private void UpdateHeading()
        {
            if (_serviceRunning && Input.compass.enabled && Input.compass.timestamp > 0
                && Input.compass.headingAccuracy >= 0)
            {
                float trueHeading = Input.compass.trueHeading;
                HeadingDegrees = trueHeading >= 0 ? trueHeading : Input.compass.magneticHeading;
                HeadingAccuracy = Input.compass.headingAccuracy;
                HeadingSource = HeadingSource.Compass;
            }
            else if (Mode == LocationMode.Simulated && _hasMovementHeading)
            {
                HeadingDegrees = _movementHeading;
                HeadingSource = HeadingSource.Movement;
            }
            else
            {
                HeadingSource = HeadingSource.None;
            }
        }

        private void OnDestroy()
        {
            if (_gpsStarted)
            {
                Input.compass.enabled = false;
                Input.location.Stop();
            }
        }
    }
}
