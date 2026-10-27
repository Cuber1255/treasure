using TMPro;
using UnityEngine;

namespace Treasure
{
    // 동성로 지도 퍼즐 화면: 모은 조각만 그림이 보인다
    public class PuzzleScreen : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private TMP_Text _completionText;
        [SerializeField] private TMP_Text _hintText;
        // 왼쪽 위부터 행 순서
        [SerializeField] private UnityEngine.UI.RawImage[] _pieceImages;
        [SerializeField] private TMP_Text[] _pieceLabels;

        private static readonly Color MISSING_COLOR = new Color32(0x2B, 0x3A, 0x55, 0xFF);

        private void Awake()
        {
            _backButton.onClick.AddListener(() => _router.Back());
        }

        private void OnEnable()
        {
            _game.Progress.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            _game.Progress.Changed -= Refresh;
        }

        private void Refresh()
        {
            GameConfig config = _game.Config;
            int columns = config.puzzleColumns;
            int rows = config.puzzleRows;
            for (int i = 0; i < _pieceImages.Length; i++)
            {
                bool owned = _game.Progress.HasPiece(i);
                UnityEngine.UI.RawImage image = _pieceImages[i];
                _pieceLabels[i].gameObject.SetActive(!owned);
                if (owned)
                {
                    int column = i % columns;
                    int row = i / columns;
                    // 텍스처 UV는 아래에서 위로 올라가므로 행을 뒤집는다
                    image.texture = config.puzzleImage;
                    image.uvRect = new Rect((float)column / columns, 1f - (float)(row + 1) / rows,
                        1f / columns, 1f / rows);
                    image.color = Color.white;
                }
                else
                {
                    image.texture = null;
                    image.color = MISSING_COLOR;
                }
            }

            int collected = _game.Progress.CollectedPieceCount;
            int total = _game.TotalPieces;
            _completionText.text = $"완성도 {collected * 100 / total}%";
            _hintText.text = _game.Progress.IsPuzzleComplete(total)
                ? "퍼즐 완성! 동성로 광장에 최종 캐릭터가 나타났어요"
                : "스팟에서 캐릭터를 잡으면 그 스팟의 조각을 얻어요";
        }
    }
}
