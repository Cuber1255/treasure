using UnityEngine;

namespace Treasure
{
    // 지도 화면에서 뒤로가기 시 종료 확인
    public class ExitConfirmPopup : Popup
    {
        [SerializeField] private UnityEngine.UI.Button _yesButton;
        [SerializeField] private UnityEngine.UI.Button _noButton;

        private void Awake()
        {
            _yesButton.onClick.AddListener(Quit);
            _noButton.onClick.AddListener(Close);
        }

        public void Show()
        {
            Open();
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
