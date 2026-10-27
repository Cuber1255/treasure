using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Treasure
{
    // 포획 성공/실패 결과 팝업
    public class CaptureResultPopup : Popup
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private UnityEngine.UI.Image _portrait;
        [SerializeField] private UnityEngine.UI.Button _confirmButton;

        private Action _onConfirm;

        private void Awake()
        {
            _confirmButton.onClick.AddListener(Confirm);
        }

        public void ShowSuccess(CharacterData character, CaptureReward reward, int collected, int total, Action onConfirm)
        {
            List<string> lines = new List<string>
            {
                $"{character.displayName} 포획!",
                $"+{reward.coins} 코인",
                reward.pieceIndex >= 0
                    ? $"퍼즐 조각 획득 ({collected}/{total})"
                    : "이 스팟의 퍼즐 조각은 이미 모두 모았어요"
            };
            if (reward.isNewCard)
            {
                lines.Add("도감에 새 카드가 등록됐어요");
            }
            Show(character, "포획 성공!", string.Join("\n", lines), Color.white, onConfirm);
        }

        public void ShowFail(CharacterData character, int minutes, Action onConfirm)
        {
            string body = $"{character.displayName} 포획 실패!\n{minutes}분 후 같은 스팟에서 다시 도전할 수 있어요.";
            // 도망간 느낌이 나도록 초상화를 반투명하게 한다
            Show(character, "도망갔어요...", body, new Color(1f, 1f, 1f, 0.4f), onConfirm);
        }

        private void Show(CharacterData character, string title, string body, Color portraitColor, Action onConfirm)
        {
            _titleText.text = title;
            _bodyText.text = body;
            _portrait.sprite = character.portrait;
            _portrait.color = portraitColor;
            _onConfirm = onConfirm;
            Open();
        }

        public override void HandleBack()
        {
            Confirm();
        }

        private void Confirm()
        {
            Close();
            Action callback = _onConfirm;
            _onConfirm = null;
            callback?.Invoke();
        }
    }
}
