using TMPro;
using UnityEngine;

namespace Treasure
{
    // 도감 카드 상세: 캐릭터 스토리와 스팟 이야기
    public class CardDetailPopup : Popup
    {
        [SerializeField] private UnityEngine.UI.Image _portrait;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _spotText;
        [SerializeField] private TMP_Text _storyText;
        [SerializeField] private UnityEngine.UI.Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
        }

        public void Show(CharacterData character, SpotData spot)
        {
            _portrait.sprite = character.portrait;
            _nameText.text = character.displayName;
            _spotText.text = spot.displayName;
            // 기획서 차별점: 캐릭터와 함께 스팟 스토리도 보여 준다
            _storyText.text = character.story + "\n\n" + spot.description;
            Open();
        }
    }
}
