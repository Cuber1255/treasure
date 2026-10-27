using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Treasure
{
    // 지도 위 스팟 표시. 아이콘은 줌에 맞춰 화면 크기를 유지하고, 반경 원은 실제 미터 크기다
    public class SpotMarker : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private SpriteRenderer _range;

        // 기준 줌에서 아이콘 지름 (미터)
        private const float ICON_SIZE_METERS = 24f;
        private const float FINAL_ICON_SCALE = 1.5f;
        private const float RANGE_ALPHA = 0.18f;

        private static readonly Color CAPTURABLE_COLOR = new Color32(0xFF, 0xB0, 0x20, 0xFF);
        private static readonly Color OUT_OF_RANGE_COLOR = new Color32(0x3D, 0x7B, 0xD9, 0xFF);
        private static readonly Color INACTIVE_COLOR = new Color32(0x9A, 0xA3, 0xAD, 0xFF);
        private static readonly Color LOCKED_COLOR = new Color32(0x44, 0x4C, 0x57, 0xFF);

        public SpotData Spot { get; private set; }

        private Action<SpotData> _onClicked;

        public void Setup(SpotData spot, Vector2 localPosition, Action<SpotData> onClicked)
        {
            Spot = spot;
            _onClicked = onClicked;
            name = $"Spot_{spot.spotId}";
            transform.localPosition = localPosition;
            float diameter = spot.captureRadiusMeters * 2f;
            _range.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        public void Refresh(SpotState state, float zoomScale)
        {
            float size = ICON_SIZE_METERS * (Spot.isFinalSpot ? FINAL_ICON_SCALE : 1f) * zoomScale;
            _icon.transform.localScale = new Vector3(size, size, 1f);

            Color color = GetStateColor(state);
            _icon.color = color;

            bool hideRange = state == SpotState.Locked || state == SpotState.Cleared;
            color.a = hideRange ? 0f : RANGE_ALPHA;
            _range.color = color;
        }

        private static Color GetStateColor(SpotState state)
        {
            switch (state)
            {
                case SpotState.Capturable:
                    return CAPTURABLE_COLOR;
                case SpotState.OutOfRange:
                    return OUT_OF_RANGE_COLOR;
                case SpotState.Locked:
                    return LOCKED_COLOR;
                default:
                    return INACTIVE_COLOR;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(Spot);
        }
    }
}
