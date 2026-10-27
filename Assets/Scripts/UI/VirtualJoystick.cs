using UnityEngine;
using UnityEngine.EventSystems;

namespace Treasure
{
    // 위치 시뮬레이션용 화면 조이스틱. 원 안에서 민 만큼(0~1) 이동 속도가 정해진다
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private LocationProvider _location;
        // 피벗이 중앙인 원형 배경
        [SerializeField] private RectTransform _background;
        // 앵커가 중앙인 손잡이
        [SerializeField] private RectTransform _knob;

        private Vector2 _input;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_background, eventData.position,
                    eventData.pressEventCamera, out Vector2 local))
            {
                return;
            }
            float radius = _background.rect.width * 0.5f;
            Vector2 clamped = Vector2.ClampMagnitude(local, radius);
            _knob.anchoredPosition = clamped;
            _input = clamped / radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetInput();
        }

        private void Update()
        {
            _location.SetSimulatedMoveInput(_input);
        }

        // 숨겨질 때 입력이 남아 계속 걸어가지 않도록 초기화한다
        private void OnDisable()
        {
            ResetInput();
            _location.SetSimulatedMoveInput(Vector2.zero);
        }

        private void ResetInput()
        {
            _input = Vector2.zero;
            _knob.anchoredPosition = Vector2.zero;
        }
    }
}
