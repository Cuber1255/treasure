using UnityEngine;

namespace Treasure
{
    // 카메라 화면: 캐릭터가 화면 안을 떠다니고, 탭하거나 잡기 버튼을 누르면 추격 액션 시작
    public class CameraScreen : MonoBehaviour
    {
        [SerializeField] private CaptureFlow _captureFlow;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private UnityEngine.UI.Image _characterImage;
        [SerializeField] private TapTarget _characterTarget;
        [SerializeField] private UnityEngine.UI.Button _catchButton;

        // 떠다니는 연출 값 (캔버스 단위)
        private const float DRIFT_X = 280f;
        private const float DRIFT_Y = 140f;
        private const float BASE_Y = 120f;
        private const float PULSE = 0.05f;

        private float _time;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => _router.Back());
            _catchButton.onClick.AddListener(() => _captureFlow.StartAction());
            _characterTarget.Tapped += () => _captureFlow.StartAction();
        }

        public void Show(CharacterData character)
        {
            _characterImage.sprite = character.portrait;
            _time = 0f;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            RectTransform rect = _characterImage.rectTransform;
            rect.anchoredPosition = new Vector2(Mathf.Sin(0.8f * _time) * DRIFT_X,
                BASE_Y + Mathf.Sin(1.7f * _time) * DRIFT_Y);
            float scale = 1f + PULSE * Mathf.Sin(4f * _time);
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
