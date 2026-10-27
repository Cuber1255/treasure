using System;
using TMPro;
using UnityEngine;

namespace Treasure
{
    // 탭 추격 액션 화면: 제한시간 안에 움직이는 캐릭터를 정해진 횟수만큼 탭하면 포획
    public class CaptureActionScreen : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private RectTransform _playArea;
        [SerializeField] private UnityEngine.UI.Image _characterImage;
        [SerializeField] private TapTarget _characterTarget;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private RectTransform _timeFill;
        [SerializeField] private TMP_Text _gaugeText;
        [SerializeField] private RectTransform _gaugeFill;
        // config.shopItems와 같은 순서·개수
        [SerializeField] private UnityEngine.UI.Button[] _itemButtons;
        [SerializeField] private TMP_Text[] _itemLabels;

        // 캐릭터가 PlayArea 가장자리에서 떨어져 있어야 하는 여백 (캔버스 단위)
        private const float EDGE_MARGIN = 140f;
        private const float ARRIVE_DISTANCE = 1f;
        private static readonly Color SLOW_TINT = new Color(0.7f, 0.85f, 1f);

        private TapChaseGame _chase;
        private float _timeLimit;
        private Action<bool> _onFinished;
        private float _moveSpeed;
        private Vector2 _target;
        private bool _running;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => _router.Back());
            _characterTarget.Tapped += OnCharacterTapped;
            for (int i = 0; i < _itemButtons.Length; i++)
            {
                int index = i;
                _itemButtons[i].onClick.AddListener(() => UseItem(index));
            }
        }

        public void Begin(CharacterData character, Action<bool> onFinished)
        {
            _chase = new TapChaseGame(character.tapsToCapture, character.timeLimitSeconds);
            _timeLimit = character.timeLimitSeconds;
            _onFinished = onFinished;
            _characterImage.sprite = character.portrait;
            _characterImage.rectTransform.anchoredPosition = Vector2.zero;
            _moveSpeed = character.moveSpeed;
            PickNewTarget();
            _running = true;
            RefreshDisplay();
        }

        public void GiveUp()
        {
            if (_running)
            {
                Finish(false);
            }
        }

        private void Update()
        {
            if (!_running)
            {
                return;
            }

            _chase.Tick(Time.deltaTime);
            MoveCharacter();
            RefreshDisplay();

            if (_chase.IsCaptured)
            {
                Finish(true);
            }
            else if (_chase.IsFailed)
            {
                Finish(false);
            }
        }

        private void MoveCharacter()
        {
            RectTransform rect = _characterImage.rectTransform;
            float step = _moveSpeed * _chase.SpeedMultiplier * Time.deltaTime;
            rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, _target, step);
            if (Vector2.Distance(rect.anchoredPosition, _target) < ARRIVE_DISTANCE)
            {
                PickNewTarget();
            }
            _characterImage.color = _chase.SpeedMultiplier < 1f ? SLOW_TINT : Color.white;
        }

        // 캐릭터는 PlayArea 중앙 앵커 기준으로 움직인다
        private void PickNewTarget()
        {
            Vector2 half = _playArea.rect.size * 0.5f;
            float rangeX = Mathf.Max(0f, half.x - EDGE_MARGIN);
            float rangeY = Mathf.Max(0f, half.y - EDGE_MARGIN);
            _target = new Vector2(UnityEngine.Random.Range(-rangeX, rangeX), UnityEngine.Random.Range(-rangeY, rangeY));
        }

        private void RefreshDisplay()
        {
            _timerText.text = $"남은 시간 {_chase.TimeRemaining:0.0}초";
            SetFill(_timeFill, _timeLimit > 0f ? _chase.TimeRemaining / _timeLimit : 0f);
            _gaugeText.text = $"포획 게이지 {_chase.Taps} / {_chase.TapsRequired}";
            SetFill(_gaugeFill, _chase.Progress01);

            ShopItemData[] items = _game.Config.shopItems;
            for (int i = 0; i < _itemButtons.Length && i < items.Length; i++)
            {
                int count = _game.Progress.GetItemCount(items[i].itemId);
                _itemLabels[i].text = $"{items[i].displayName}\n{count}개";
                _itemButtons[i].interactable = count > 0 && _running;
            }
        }

        private static void SetFill(RectTransform fill, float ratio)
        {
            Vector2 anchorMax = fill.anchorMax;
            anchorMax.x = Mathf.Clamp01(ratio);
            fill.anchorMax = anchorMax;
        }

        private void OnCharacterTapped()
        {
            if (!_running)
            {
                return;
            }
            _chase.RegisterHit();
            PickNewTarget();
        }

        private void UseItem(int index)
        {
            if (!_running)
            {
                return;
            }
            ShopItemData item = _game.Config.shopItems[index];
            if (!_game.Progress.TryUseItem(item.itemId))
            {
                return;
            }

            switch (item.effect)
            {
                case ItemEffect.AddTime:
                    _chase.AddTime(item.effectValue);
                    break;
                case ItemEffect.SlowDown:
                    _chase.ApplySlow(item.effectValue, item.durationSeconds);
                    break;
                case ItemEffect.FillGauge:
                    _chase.FillGauge(item.effectValue);
                    break;
            }
            RefreshDisplay();
        }

        private void Finish(bool captured)
        {
            _running = false;
            RefreshDisplay();
            _onFinished?.Invoke(captured);
        }
    }
}
