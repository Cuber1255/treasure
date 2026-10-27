using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 지도 HUD, 스팟 정보 팝업, 종료 확인 팝업
    internal static class HudBuilder
    {
        private const float MENU_BUTTON_SIZE = 200f;
        private const float MENU_BUTTON_Y = 140f;

        public static MainHud BuildMainHud(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("MainHud", parent, null);
            MainHud hud = root.gameObject.AddComponent<MainHud>();

            Button back = UIFactory.CreateBackButton(root);
            Color pill = UIFactory.WithAlpha(UIFactory.NAVY, 0.9f);
            TMP_Text coinText = UIFactory.CreatePill(root, "CoinPill", "CoinText", new Vector2(-40f, -40f), new Vector2(320f, 100f), 40f, pill);
            TMP_Text puzzleText = UIFactory.CreatePill(root, "PuzzlePill", "PuzzleText", new Vector2(-40f, -156f), new Vector2(320f, 90f), 36f, pill);

            Image status = UIFactory.CreateImage("StatusPanel", root, UIFactory.WithAlpha(Color.white, 0.85f), false, UIFactory.RoundedSprite);
            UIFactory.StretchTop(status.rectTransform, -270f, 170f, 40f);
            TextMeshProUGUI gpsText = UIFactory.CreateText("GpsText", status.transform, "", 28f, UIFactory.TEXT_DARK, TextAlignmentOptions.Center);
            SetHalf(gpsText.rectTransform, true);
            TextMeshProUGUI hintText = UIFactory.CreateText("HintText", status.transform, "", 32f, UIFactory.TEXT_DARK, TextAlignmentOptions.Center, true);
            SetHalf(hintText.rectTransform, false);

            Button catchButton = CreateMenuButton(root, "CatchButton", "잡기", UIFactory.AMBER, 135f);
            Button puzzleButton = CreateMenuButton(root, "PuzzleButton", "지도", UIFactory.BLUE, 405f);
            Button collectionButton = CreateMenuButton(root, "CollectionButton", "도감", UIFactory.BLUE, 675f);
            Button shopButton = CreateMenuButton(root, "ShopButton", "상점", UIFactory.BLUE, 945f);

            CreateAttribution(root);
            BuildMapControls(root, hud, refs);

            UIFactory.SetRef(hud, "_game", refs.game);
            UIFactory.SetRef(hud, "_router", refs.router);
            UIFactory.SetRef(hud, "_captureFlow", refs.captureFlow);
            UIFactory.SetRef(hud, "_backButton", back);
            UIFactory.SetRef(hud, "_coinText", coinText);
            UIFactory.SetRef(hud, "_puzzleText", puzzleText);
            UIFactory.SetRef(hud, "_gpsText", gpsText);
            UIFactory.SetRef(hud, "_hintText", hintText);
            UIFactory.SetRef(hud, "_catchButton", catchButton);
            UIFactory.SetRef(hud, "_puzzleButton", puzzleButton);
            UIFactory.SetRef(hud, "_collectionButton", collectionButton);
            UIFactory.SetRef(hud, "_shopButton", shopButton);
            return hud;
        }

        // 디버그 설정 버튼, 내 위치 버튼, 시뮬레이션 조이스틱
        // 하단 버튼은 y 40~240, 출처 표기는 300~344, 조이스틱은 360~580이라 서로 겹치지 않는다
        private static void BuildMapControls(RectTransform root, MainHud hud, SceneRefs refs)
        {
            Button debug = UIFactory.CreateButton("DebugButton", root, "설정", 30f, UIFactory.GRAY, Color.white, true);
            UIFactory.Place((RectTransform)debug.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, -40f),
                new Vector2(120f, 120f));

            Button recenter = UIFactory.CreateButton("RecenterButton", root, "내 위치", 30f, Color.white, UIFactory.NAVY, true);
            UIFactory.Place((RectTransform)recenter.transform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-110f, 450f),
                new Vector2(150f, 150f));

            Image joystickBackground = UIFactory.CreateImage("Joystick", root, UIFactory.WithAlpha(UIFactory.NAVY, 0.35f), true,
                UIFactory.CircleSprite);
            UIFactory.Place(joystickBackground.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(170f, 470f),
                new Vector2(220f, 220f));
            VirtualJoystick joystick = joystickBackground.gameObject.AddComponent<VirtualJoystick>();
            Image knob = UIFactory.CreateImage("Knob", joystickBackground.transform, UIFactory.WithAlpha(Color.white, 0.9f), false,
                UIFactory.CircleSprite);
            UIFactory.Place(knob.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));

            UIFactory.SetRef(joystick, "_location", refs.location);
            UIFactory.SetRef(joystick, "_background", joystickBackground.rectTransform);
            UIFactory.SetRef(joystick, "_knob", knob.rectTransform);

            UIFactory.SetRef(hud, "_mapView", refs.mapView);
            UIFactory.SetRef(hud, "_debugButton", debug);
            UIFactory.SetRef(hud, "_recenterButton", recenter);
            UIFactory.SetRef(hud, "_joystick", joystick);
        }

        // 상태 패널의 위쪽/아래쪽 절반
        private static void SetHalf(RectTransform rect, bool top)
        {
            rect.anchorMin = new Vector2(0f, top ? 0.5f : 0f);
            rect.anchorMax = new Vector2(1f, top ? 1f : 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(20f, 0f);
            rect.offsetMax = new Vector2(-20f, 0f);
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Color color, float x)
        {
            Button button = UIFactory.CreateButton(name, parent, label, 44f, color, Color.white, true);
            UIFactory.Place((RectTransform)button.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(x, MENU_BUTTON_Y),
                new Vector2(MENU_BUTTON_SIZE, MENU_BUTTON_SIZE));
            return button;
        }

        // 지도 데이터 출처 표기 (OpenFreeMap 이용 조건)
        private static void CreateAttribution(Transform parent)
        {
            Image background = UIFactory.CreateImage("Attribution", parent, UIFactory.WithAlpha(Color.white, 0.75f), false);
            UIFactory.Place(background.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f, 300f), new Vector2(700f, 44f));
            TextMeshProUGUI text = UIFactory.CreateText("Text", background.transform, "OpenFreeMap © OpenMapTiles Data from OpenStreetMap", 22f,
                UIFactory.TEXT_DARK, TextAlignmentOptions.Right);
            UIFactory.Stretch(text.rectTransform, 10f, 0f, 12f, 0f);
        }

        public static SpotInfoPopup BuildSpotInfoPopup(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("SpotInfoPopup", parent, null);
            SpotInfoPopup popup = root.gameObject.AddComponent<SpotInfoPopup>();
            UIFactory.CreateDim(root);
            RectTransform card = UIFactory.CreateCard(root, new Vector2(920f, 980f));

            TextMeshProUGUI title = UIFactory.CreateTopText("TitleText", card, "", 56f, UIFactory.TEXT_DARK, -50f, 80f, 50f, true);
            TextMeshProUGUI status = UIFactory.CreateTopText("StatusText", card, "", 34f, UIFactory.NAVY, -150f, 100f, 50f);
            TextMeshProUGUI pieces = UIFactory.CreateTopText("PiecesText", card, "", 32f, UIFactory.TEXT_DARK, -250f, 60f, 50f);
            TextMeshProUGUI description = UIFactory.CreateText("DescriptionText", card, "", 32f, UIFactory.TEXT_DARK, TextAlignmentOptions.TopLeft);
            UIFactory.Stretch(description.rectTransform, 50f, 320f, 50f, 200f);

            Button teleport = UIFactory.CreateButton("TeleportButton", card, "여기로 이동(테스트)", 30f, UIFactory.GRAY, Color.white, false);
            UIFactory.Place((RectTransform)teleport.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, 50f), new Vector2(360f, 120f));
            Button close = UIFactory.CreateButton("CloseButton", card, "닫기", 40f, UIFactory.BLUE, Color.white, false);
            UIFactory.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 50f), new Vector2(360f, 120f));

            UIFactory.SetRef(popup, "_router", refs.router);
            UIFactory.SetRef(popup, "_game", refs.game);
            UIFactory.SetRef(popup, "_titleText", title);
            UIFactory.SetRef(popup, "_statusText", status);
            UIFactory.SetRef(popup, "_piecesText", pieces);
            UIFactory.SetRef(popup, "_descriptionText", description);
            UIFactory.SetRef(popup, "_closeButton", close);
            UIFactory.SetRef(popup, "_teleportButton", teleport);
            return popup;
        }

        public static ExitConfirmPopup BuildExitConfirmPopup(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("ExitConfirmPopup", parent, null);
            ExitConfirmPopup popup = root.gameObject.AddComponent<ExitConfirmPopup>();
            UIFactory.CreateDim(root);
            RectTransform card = UIFactory.CreateCard(root, new Vector2(820f, 520f));

            UIFactory.CreateTopText("MessageText", card, "게임을 종료할까요?", 48f, UIFactory.NAVY, -120f, 100f, 40f, true);
            Button yes = UIFactory.CreateButton("YesButton", card, "종료", 40f, UIFactory.BLUE, Color.white, false);
            UIFactory.Place((RectTransform)yes.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 60f), new Vector2(320f, 120f));
            Button no = UIFactory.CreateButton("NoButton", card, "계속하기", 40f, UIFactory.AMBER, Color.white, false);
            UIFactory.Place((RectTransform)no.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 60f), new Vector2(320f, 120f));

            UIFactory.SetRef(popup, "_router", refs.router);
            UIFactory.SetRef(popup, "_yesButton", yes);
            UIFactory.SetRef(popup, "_noButton", no);
            return popup;
        }
    }
}
