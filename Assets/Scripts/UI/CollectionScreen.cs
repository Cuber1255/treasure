using TMPro;
using UnityEngine;

namespace Treasure
{
    // 도감 화면: 스팟 순서대로 캐릭터 카드를 보여 준다
    public class CollectionScreen : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private TMP_Text _countText;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private CardDetailPopup _detailPopup;

        private void Awake()
        {
            _backButton.onClick.AddListener(() => _router.Back());
        }

        private void OnEnable()
        {
            // 다시 열 때 카드가 중복되지 않도록 비우고 새로 만든다
            // Destroy는 프레임 끝에 처리되므로 먼저 떼어 내서 레이아웃에 남지 않게 한다
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Transform child = _content.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            int owned = 0;
            SpotData[] spots = _game.Config.spots;
            foreach (SpotData spot in spots)
            {
                CharacterData character = spot.character;
                bool hasCard = _game.Progress.HasCard(character.characterId);
                if (hasCard)
                {
                    owned++;
                }
                CardView card = Instantiate(_cardPrefab, _content);
                card.Bind(character, spot.displayName, hasCard, () => _detailPopup.Show(character, spot));
            }
            _countText.text = $"수집 {owned}/{spots.Length}";
        }
    }
}
