using TMPro;
using UnityEngine;

namespace Treasure
{
    // 최종 캐릭터 포획 후 보여 주는 클리어 화면
    public class ClearScreen : Popup
    {
        [SerializeField] private UnityEngine.UI.Image _portrait;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private UnityEngine.UI.Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Close);
        }

        public void Show(CharacterData character)
        {
            _portrait.sprite = character.portrait;
            _bodyText.text = $"모든 퍼즐 조각을 모으고 {character.displayName}까지 포획했어요!\n동성로 탐험을 완료했습니다.";
            Open();
        }
    }
}
