using System;
using TMPro;
using UnityEngine;

namespace Treasure
{
    // 도감 카드 한 장. 아직 못 잡은 캐릭터는 실루엣으로 보인다
    public class CardView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button _button;
        [SerializeField] private UnityEngine.UI.Image _background;
        [SerializeField] private UnityEngine.UI.Image _portrait;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _spotText;

        private static readonly Color LOCKED_BACKGROUND = new Color32(0x2B, 0x3A, 0x55, 0xFF);
        private static readonly Color OWNED_BACKGROUND = new Color32(0x6F, 0x8F, 0xB8, 0xFF);

        public void Bind(CharacterData character, string spotName, bool owned, Action onClick)
        {
            _portrait.sprite = character.portrait;
            _portrait.color = owned ? Color.white : Color.black;
            _nameText.text = owned ? character.displayName : "???";
            _spotText.text = spotName;
            _background.color = owned ? OWNED_BACKGROUND : LOCKED_BACKGROUND;
            _button.interactable = owned;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}
