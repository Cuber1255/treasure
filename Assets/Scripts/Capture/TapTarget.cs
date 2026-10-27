using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Treasure
{
    // 탭할 수 있는 캐릭터 이미지
    // 움직이는 대상이라 click 대신 누르는 순간(pointer down)에 반응한다 (click은 손을 뗄 때 위치를 따진다)
    public class TapTarget : MonoBehaviour, IPointerDownHandler
    {
        public event Action Tapped;

        public void OnPointerDown(PointerEventData eventData)
        {
            Tapped?.Invoke();
        }
    }
}
