using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Treasure
{
    public enum ScreenId
    {
        Map,
        Camera,
        Action,
        Puzzle,
        Collection,
        Shop
    }

    // 화면 전환, 팝업 스택, 뒤로가기를 관리한다
    public class ScreenRouter : MonoBehaviour
    {
        [SerializeField] private GameObject _mainHud;
        [SerializeField] private GameObject _cameraScreen;
        [SerializeField] private GameObject _actionScreen;
        [SerializeField] private GameObject _puzzleScreen;
        [SerializeField] private GameObject _collectionScreen;
        [SerializeField] private GameObject _shopScreen;
        [SerializeField] private ExitConfirmPopup _exitPopup;
        [SerializeField] private CaptureFlow _captureFlow;

        private readonly List<Popup> _openPopups = new();

        public ScreenId Current { get; private set; }

        // 지도 화면이고 팝업이 없을 때만 지도 조작(줌, 마커 탭)을 받는다
        public bool IsMapInteractive => Current == ScreenId.Map && _openPopups.Count == 0;

        private void Start()
        {
            Show(ScreenId.Map);
        }

        private void Update()
        {
            // Android 뒤로 키는 Escape로 들어온다
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Back();
            }
        }

        public void Show(ScreenId screen)
        {
            Current = screen;
            _mainHud.SetActive(screen == ScreenId.Map);
            _cameraScreen.SetActive(screen == ScreenId.Camera);
            _actionScreen.SetActive(screen == ScreenId.Action);
            _puzzleScreen.SetActive(screen == ScreenId.Puzzle);
            _collectionScreen.SetActive(screen == ScreenId.Collection);
            _shopScreen.SetActive(screen == ScreenId.Shop);
        }

        public void OpenPopup(Popup popup)
        {
            if (!_openPopups.Contains(popup))
            {
                _openPopups.Add(popup);
            }
            popup.gameObject.SetActive(true);
            popup.transform.SetAsLastSibling();
        }

        public void ClosePopup(Popup popup)
        {
            _openPopups.Remove(popup);
            popup.gameObject.SetActive(false);
        }

        public void Back()
        {
            if (_openPopups.Count > 0)
            {
                _openPopups[_openPopups.Count - 1].HandleBack();
                return;
            }

            switch (Current)
            {
                case ScreenId.Map:
                    _exitPopup.Show();
                    break;
                case ScreenId.Action:
                    // 액션 중 뒤로가기는 포기로 보고 실패 처리한다 (재도전 대기 시작)
                    _captureFlow.GiveUp();
                    break;
                default:
                    // 카메라 화면에서 나가면 패널티 없이 지도로 돌아간다
                    Show(ScreenId.Map);
                    break;
            }
        }
    }
}
