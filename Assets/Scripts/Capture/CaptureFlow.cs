using UnityEngine;

namespace Treasure
{
    // 잡기 → 카메라 → 탭 추격 → 결과로 이어지는 포획 흐름
    public class CaptureFlow : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private CameraScreen _cameraScreen;
        [SerializeField] private CaptureActionScreen _actionScreen;
        [SerializeField] private CaptureResultPopup _resultPopup;
        [SerializeField] private ClearScreen _clearScreen;

        // 포획 대상 스팟은 Begin 시점에 고정한다 (액션 중 반경을 벗어나도 유지)
        private SpotData _currentSpot;

        public void Begin()
        {
            SpotData spot = _game.FindCapturableSpot();
            if (spot == null)
            {
                return;
            }
            _currentSpot = spot;
            _router.Show(ScreenId.Camera);
            _cameraScreen.Show(spot.character);
        }

        public void StartAction()
        {
            if (_currentSpot == null)
            {
                return;
            }
            _router.Show(ScreenId.Action);
            _actionScreen.Begin(_currentSpot.character, OnActionFinished);
        }

        public void GiveUp()
        {
            _actionScreen.GiveUp();
        }

        private void OnActionFinished(bool captured)
        {
            SpotData spot = _currentSpot;
            CharacterData character = spot.character;
            _currentSpot = null;

            if (captured)
            {
                CaptureReward reward = _game.CompleteCapture(spot);
                if (reward.isFinal)
                {
                    _router.Show(ScreenId.Map);
                    _clearScreen.Show(character);
                    return;
                }
                _resultPopup.ShowSuccess(character, reward, _game.Progress.CollectedPieceCount, _game.TotalPieces,
                    () => _router.Show(ScreenId.Map));
                return;
            }

            _game.FailCapture(spot);
            int minutes = Mathf.CeilToInt(_game.Config.failCooldownSeconds / 60f);
            _resultPopup.ShowFail(character, minutes, () => _router.Show(ScreenId.Map));
        }
    }
}
