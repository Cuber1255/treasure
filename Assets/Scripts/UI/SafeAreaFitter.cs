using UnityEngine;

namespace Treasure
{
    // 노치/홈 인디케이터를 피하도록 RectTransform을 Screen.safeArea에 맞춘다
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void Update()
        {
            Rect area = Screen.safeArea;
            Vector2Int screen = new Vector2Int(Screen.width, Screen.height);
            if (area == _appliedArea && screen == _appliedScreen)
            {
                return;
            }
            Apply(area, screen);
        }

        private void Apply(Rect area, Vector2Int screen)
        {
            if (screen.x <= 0 || screen.y <= 0)
            {
                return;
            }
            _appliedArea = area;
            _appliedScreen = screen;

            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(area.xMin / screen.x, area.yMin / screen.y);
            rect.anchorMax = new Vector2(area.xMax / screen.x, area.yMax / screen.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
