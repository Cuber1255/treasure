using System;
using TMPro;
using UnityEngine;

namespace Treasure
{
    // 상점 아이템 한 줄
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _ownedText;
        [SerializeField] private UnityEngine.UI.Button _buyButton;
        [SerializeField] private TMP_Text _priceText;

        public void Bind(ShopItemData item, int owned, bool canAfford, Action onBuy)
        {
            _icon.color = item.iconColor;
            _nameText.text = item.displayName;
            _descriptionText.text = item.description;
            _ownedText.text = $"보유 {owned}개";
            _priceText.text = $"{item.price} 코인";
            _buyButton.interactable = canAfford;
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }
}
