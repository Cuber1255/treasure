using UnityEngine;

namespace Treasure
{
    // 모든 팝업의 공통 베이스. 열고 닫는 것은 ScreenRouter를 거쳐 팝업 스택에 반영한다
    public class Popup : MonoBehaviour
    {
        [SerializeField] protected ScreenRouter _router;

        // 뒤로가기 시 동작. 기본은 닫기
        public virtual void HandleBack()
        {
            _router.ClosePopup(this);
        }

        protected void Open()
        {
            _router.OpenPopup(this);
        }

        protected void Close()
        {
            _router.ClosePopup(this);
        }
    }
}
