using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 모바일 테스트용 디버그 설정 패널
    internal static class DebugUIBuilder
    {
        private const float OPTION_FONT_SIZE = 30f;
        private const float ROW_HEIGHT = 110f;

        public static DebugPanel BuildDebugPanel(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("DebugPanel", parent, null);
            DebugPanel panel = root.gameObject.AddComponent<DebugPanel>();
            UIFactory.CreateDim(root);
            RectTransform card = UIFactory.CreateCard(root, new Vector2(960f, 1500f));

            UIFactory.CreateTopText("TitleText", card, "디버그 설정", 52f, UIFactory.TEXT_DARK, -40f, 70f, 40f, true);
            CreateOptionRow(card, "위치", -140f, "GpsButton", "실제 GPS", "SimulationButton", "시뮬레이션",
                out Button gps, out Button simulation);
            CreateOptionRow(card, "내 위치 따라가기", -280f, "FollowOnButton", "켜기", "FollowOffButton", "끄기",
                out Button followOn, out Button followOff);
            CreateOptionRow(card, "지도 회전", -420f, "NorthUpButton", "북쪽 고정", "HeadingUpButton", "방향 따라 회전",
                out Button northUp, out Button headingUp);

            Button resetCooldown = CreateActionButton(card, "ResetCooldownButton", "쿨다운 초기화", 50f);
            Button resetSave = CreateActionButton(card, "ResetSaveButton", "저장 초기화", 510f);

            TextMeshProUGUI info = UIFactory.CreateText("InfoText", card, "", 28f, UIFactory.TEXT_DARK, TextAlignmentOptions.TopLeft);
            UIFactory.Stretch(info.rectTransform, 50f, 700f, 50f, 200f);

            Button close = UIFactory.CreateButton("CloseButton", card, "닫기", 40f, UIFactory.BLUE, Color.white, false);
            UIFactory.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f),
                new Vector2(360f, 120f));

            UIFactory.SetRef(panel, "_router", refs.router);
            UIFactory.SetRef(panel, "_game", refs.game);
            UIFactory.SetRef(panel, "_mapView", refs.mapView);
            UIFactory.SetRef(panel, "_vectorMap", refs.vectorMap);
            UIFactory.SetRef(panel, "_gpsButton", gps);
            UIFactory.SetRef(panel, "_simulationButton", simulation);
            UIFactory.SetRef(panel, "_followOnButton", followOn);
            UIFactory.SetRef(panel, "_followOffButton", followOff);
            UIFactory.SetRef(panel, "_northUpButton", northUp);
            UIFactory.SetRef(panel, "_headingUpButton", headingUp);
            UIFactory.SetRef(panel, "_resetCooldownButton", resetCooldown);
            UIFactory.SetRef(panel, "_resetSaveButton", resetSave);
            UIFactory.SetRef(panel, "_closeButton", close);
            UIFactory.SetRef(panel, "_infoText", info);
            return panel;
        }

        // 왼쪽 라벨 + 두 개의 선택 버튼 한 줄
        private static void CreateOptionRow(Transform card, string label, float y, string leftName, string leftText,
            string rightName, string rightText, out Button left, out Button right)
        {
            TextMeshProUGUI title = UIFactory.CreateText(label + "Label", card, label, 34f, UIFactory.NAVY, TextAlignmentOptions.Left);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(50f, y), new Vector2(270f, ROW_HEIGHT));
            left = CreateOptionButton(card, leftName, leftText, 330f, y);
            right = CreateOptionButton(card, rightName, rightText, 630f, y);
        }

        private static Button CreateOptionButton(Transform card, string name, string text, float x, float y)
        {
            Button button = UIFactory.CreateButton(name, card, text, OPTION_FONT_SIZE, UIFactory.BLUE, Color.white, false);
            UIFactory.Place((RectTransform)button.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y),
                new Vector2(280f, ROW_HEIGHT));
            return button;
        }

        private static Button CreateActionButton(Transform card, string name, string text, float x)
        {
            Button button = UIFactory.CreateButton(name, card, text, OPTION_FONT_SIZE, UIFactory.GRAY, Color.white, false);
            UIFactory.Place((RectTransform)button.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -560f),
                new Vector2(400f, ROW_HEIGHT));
            return button;
        }
    }
}
