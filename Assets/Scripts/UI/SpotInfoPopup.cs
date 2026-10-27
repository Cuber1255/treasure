using TMPro;
using UnityEngine;

namespace Treasure
{
    // 지도에서 스팟 마커를 탭하면 뜨는 정보 팝업
    public class SpotInfoPopup : Popup
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _piecesText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private UnityEngine.UI.Button _closeButton;
        // 개발 빌드에서만 보이는 순간 이동 버튼
        [SerializeField] private UnityEngine.UI.Button _teleportButton;

        private SpotData _spot;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
            _teleportButton.gameObject.SetActive(Debug.isDebugBuild);
            _teleportButton.onClick.AddListener(() => _game.Location.TeleportTo(_spot.latitude, _spot.longitude));
        }

        public void Show(SpotData spot)
        {
            _spot = spot;
            _titleText.text = spot.displayName;
            _descriptionText.text = spot.description;
            Open();
            Refresh();
        }

        private void Update()
        {
            if (_spot != null)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            _statusText.text = GetStatusText();
            _piecesText.text = GetPiecesText();
        }

        private string GetStatusText()
        {
            switch (_game.GetSpotState(_spot))
            {
                case SpotState.Capturable:
                    return "포획 가능! 잡기 버튼을 누르세요";
                case SpotState.OutOfRange:
                    return $"{_game.DistanceTo(_spot):0}m 떨어져 있어요 (반경 {_spot.captureRadiusMeters:0}m 안으로 들어가세요)";
                case SpotState.Cooldown:
                    int seconds = Mathf.CeilToInt(_game.GetCooldownRemaining(_spot));
                    return $"다시 도전까지 {seconds / 60}:{seconds % 60:00}";
                case SpotState.Locked:
                    return $"퍼즐 조각을 모두 모으면 열려요 ({_game.Progress.CollectedPieceCount}/{_game.TotalPieces})";
                default:
                    return "탐험 완료!";
            }
        }

        private string GetPiecesText()
        {
            if (_spot.isFinalSpot || _spot.puzzlePieceIndices == null)
            {
                return "";
            }
            int owned = 0;
            foreach (int index in _spot.puzzlePieceIndices)
            {
                if (_game.Progress.HasPiece(index))
                {
                    owned++;
                }
            }
            return $"이 스팟의 퍼즐 조각 {owned}/{_spot.puzzlePieceIndices.Length}";
        }
    }
}
