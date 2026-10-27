using System.Collections;
using UnityEngine;

namespace Treasure
{
    // 기기 후면 카메라 영상을 배경으로 보여 준다. 카메라가 없으면 기본 배경색과 안내 문구
    public class CameraFeed : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.RawImage _image;
        [SerializeField] private GameObject _unavailableLabel;

        private const int REQUEST_WIDTH = 1280;
        private const int REQUEST_HEIGHT = 720;
        private const int REQUEST_FPS = 30;
        private static readonly Color FALLBACK_COLOR = new Color32(0x2B, 0x3A, 0x55, 0xFF);
        // 권한 응답이 이 시간보다 늦어지면 안내 문구를 먼저 보여 준다 (응답이 오면 카메라를 켠다)
        private const float AUTH_NOTICE_SECONDS = 3f;

        private WebCamTexture _webCam;

        private void OnEnable()
        {
            StartCoroutine(StartCamera());
        }

        private IEnumerator StartCamera()
        {
            // 권한 응답을 기다리는 동안에도 빈 흰 화면 대신 기본 배경을 보여 준다
            ShowFallback();
            _unavailableLabel.SetActive(false);

            AsyncOperation authorization = Application.RequestUserAuthorization(UserAuthorization.WebCam);
            float authWaited = 0f;
            while (!authorization.isDone)
            {
                authWaited += Time.unscaledDeltaTime;
                if (authWaited > AUTH_NOTICE_SECONDS)
                {
                    _unavailableLabel.SetActive(true);
                }
                yield return null;
            }
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                float waited = 0f;
                while (waited < 10f && !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                {
                    waited += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
#endif
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam) || WebCamTexture.devices.Length == 0)
            {
                ShowFallback();
                yield break;
            }

            WebCamDevice[] devices = WebCamTexture.devices;
            string deviceName = devices[0].name;
            foreach (WebCamDevice device in devices)
            {
                if (!device.isFrontFacing)
                {
                    deviceName = device.name;
                    break;
                }
            }

            _webCam = new WebCamTexture(deviceName, REQUEST_WIDTH, REQUEST_HEIGHT, REQUEST_FPS);
            _webCam.Play();
            _image.texture = _webCam;
            _image.color = Color.white;
            _unavailableLabel.SetActive(false);
        }

        private void ShowFallback()
        {
            _image.texture = null;
            _image.color = FALLBACK_COLOR;
            _image.rectTransform.localEulerAngles = Vector3.zero;
            _image.rectTransform.sizeDelta = ((RectTransform)_image.rectTransform.parent).rect.size;
            _unavailableLabel.SetActive(true);
        }

        private void Update()
        {
            // 카메라 해상도는 첫 프레임이 들어와야 정확해진다
            if (_webCam == null || _webCam.width <= 16)
            {
                return;
            }

            RectTransform rect = _image.rectTransform;
            int rotation = _webCam.videoRotationAngle;
            rect.localEulerAngles = new Vector3(0f, 0f, -rotation);
            _image.uvRect = _webCam.videoVerticallyMirrored ? new Rect(0f, 1f, 1f, -1f) : new Rect(0f, 0f, 1f, 1f);
            rect.sizeDelta = CalculateEnvelopeSize(rotation);
        }

        // 부모 영역을 빈틈없이 덮는 크기 (회전 전 축 기준)
        private Vector2 CalculateEnvelopeSize(int rotation)
        {
            Vector2 parentSize = ((RectTransform)_image.rectTransform.parent).rect.size;
            bool rotated = rotation == 90 || rotation == 270;
            float textureAspect = (float)_webCam.width / _webCam.height;
            float shownAspect = rotated ? 1f / textureAspect : textureAspect;

            // 화면에 보이는 가로/세로
            float shownWidth = parentSize.x;
            float shownHeight = shownWidth / shownAspect;
            if (shownHeight < parentSize.y)
            {
                shownHeight = parentSize.y;
                shownWidth = shownHeight * shownAspect;
            }
            return rotated ? new Vector2(shownHeight, shownWidth) : new Vector2(shownWidth, shownHeight);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (_webCam != null)
            {
                _webCam.Stop();
                Destroy(_webCam);
                _webCam = null;
            }
            _image.texture = null;
        }
    }
}
