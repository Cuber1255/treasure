using TMPro;
using UnityEngine;

namespace Treasure
{
    // 상점 화면: 코인으로 포획 보조 아이템을 산다
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private ScreenRouter _router;
        [SerializeField] private UnityEngine.UI.Button _backButton;
        [SerializeField] private TMP_Text _coinText;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ShopItemView _itemPrefab;

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
            PlayerProgress progress = _game.Progress;
            _coinText.text = $"코인 {progress.Coins}";

            // Destroy는 프레임 끝에 처리되므로 먼저 떼어 내서 레이아웃에 남지 않게 한다
            for (int i = _content.childCount - 1; i >= 0; i--)
            {
                Transform child = _content.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            foreach (ShopItemData item in _game.Config.shopItems)
            {
                ShopItemView view = Instantiate(_itemPrefab, _content);
                view.Bind(item, progress.GetItemCount(item.itemId), progress.Coins >= item.price, () => Buy(item));
            }
        }

        private void Buy(ShopItemData item)
        {
            if (_game.Progress.TrySpendCoins(item.price))
            {
                _game.Progress.AddItem(item.itemId);
            }
        }
    }
}
