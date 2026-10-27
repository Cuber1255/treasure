using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 카메라 화면, 탭 추격 화면, 결과 팝업, 클리어 화면
    internal static class CaptureUIBuilder
    {
        private static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TOP_CENTER = new Vector2(0.5f, 1f);
        private static readonly Vector2 BOTTOM_CENTER = new Vector2(0.5f, 0f);

        public static CameraScreen BuildCameraScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("CameraScreen", parent, Color.black);
            root.gameObject.AddComponent<RectMask2D>();
            CameraScreen screen = root.gameObject.AddComponent<CameraScreen>();

            BuildCameraFeed(root);

            Image guide = UIFactory.CreateImage("GuideBar", root, new Color(0f, 0f, 0f, 0.45f), false);
            UIFactory.StretchTop(guide.rectTransform, -180f, 120f, 0f);
            TextMeshProUGUI guideText = UIFactory.CreateText("Text", guide.transform, "캐릭터를 탭해서 붙잡으세요!", 40f,
                Color.white, TextAlignmentOptions.Center, true);
            UIFactory.Stretch(guideText.rectTransform, 20f, 0f, 20f, 0f);

            Image character = UIFactory.CreateImage("CameraCharacter", root, Color.white, true);
            character.preserveAspect = true;
            UIFactory.Place(character.rectTransform, CENTER, CENTER, new Vector2(0f, 120f), new Vector2(360f, 360f));
            TapTarget target = character.gameObject.AddComponent<TapTarget>();

            Button back = UIFactory.CreateBackButton(root);
            Button catchButton = UIFactory.CreateButton("CameraCatchButton", root, "잡기", 44f, UIFactory.AMBER, Color.white, true);
            UIFactory.Place((RectTransform)catchButton.transform, BOTTOM_CENTER, BOTTOM_CENTER, new Vector2(0f, 80f), new Vector2(200f, 200f));

            UIFactory.SetRef(screen, "_captureFlow", refs.captureFlow);
            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_backButton", back);
            UIFactory.SetRef(screen, "_characterImage", character);
            UIFactory.SetRef(screen, "_characterTarget", target);
            UIFactory.SetRef(screen, "_catchButton", catchButton);
            return screen;
        }

        private static void BuildCameraFeed(Transform parent)
        {
            RectTransform rect = UIFactory.CreateRect("CameraFeed", parent);
            UIFactory.Place(rect, CENTER, CENTER, Vector2.zero, new Vector2(1080f, 1920f));
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.raycastTarget = false;
            CameraFeed feed = rect.gameObject.AddComponent<CameraFeed>();

            TextMeshProUGUI label = UIFactory.CreateText("UnavailableLabel", rect, "카메라를 사용할 수 없어 기본 배경으로 보여 줘요",
                34f, Color.white, TextAlignmentOptions.Center);
            UIFactory.Place(label.rectTransform, CENTER, CENTER, new Vector2(0f, -300f), new Vector2(900f, 120f));
            label.gameObject.SetActive(false);

            UIFactory.SetRef(feed, "_image", image);
            UIFactory.SetRef(feed, "_unavailableLabel", label.gameObject);
        }

        public static CaptureActionScreen BuildActionScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("ActionScreen", parent, UIFactory.NAVY);
            CaptureActionScreen screen = root.gameObject.AddComponent<CaptureActionScreen>();

            Button back = UIFactory.CreateBackButton(root);
            TextMeshProUGUI timerText = UIFactory.CreateTopText("TimerText", root, "", 44f, Color.white, -60f, 70f, 160f, true);
            RectTransform timeFill = CreateBar(root, "TimeBar", "TimeFill", -150f, 30f, UIFactory.LIGHT, 1f);
            TextMeshProUGUI gaugeText = UIFactory.CreateTopText("GaugeText", root, "", 40f, Color.white, -210f, 60f, 160f);
            RectTransform gaugeFill = CreateBar(root, "GaugeBar", "GaugeFill", -290f, 40f, UIFactory.AMBER, 0f);

            RectTransform playArea = UIFactory.CreateRect("PlayArea", root);
            UIFactory.Stretch(playArea, 60f, 380f, 60f, 300f);
            Image character = UIFactory.CreateImage("ActionCharacter", playArea, Color.white, true);
            character.preserveAspect = true;
            UIFactory.Place(character.rectTransform, CENTER, CENTER, Vector2.zero, new Vector2(280f, 280f));
            TapTarget target = character.gameObject.AddComponent<TapTarget>();

            int itemCount = refs.config.shopItems.Length;
            Button[] itemButtons = new Button[itemCount];
            TMP_Text[] itemLabels = new TMP_Text[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                itemButtons[i] = UIFactory.CreateButton($"ItemButton{i}", root, "", 32f, UIFactory.BLUE, Color.white, false, false);
                UIFactory.Place((RectTransform)itemButtons[i].transform, Vector2.zero, CENTER, new Vector2(190f + 350f * i, 125f),
                    new Vector2(320f, 170f));
                itemLabels[i] = itemButtons[i].GetComponentInChildren<TMP_Text>();
            }

            UIFactory.SetRef(screen, "_game", refs.game);
            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_backButton", back);
            UIFactory.SetRef(screen, "_playArea", playArea);
            UIFactory.SetRef(screen, "_characterImage", character);
            UIFactory.SetRef(screen, "_characterTarget", target);
            UIFactory.SetRef(screen, "_timerText", timerText);
            UIFactory.SetRef(screen, "_timeFill", timeFill);
            UIFactory.SetRef(screen, "_gaugeText", gaugeText);
            UIFactory.SetRef(screen, "_gaugeFill", gaugeFill);
            UIFactory.SetRefs(screen, "_itemButtons", itemButtons);
            UIFactory.SetRefs(screen, "_itemLabels", itemLabels);
            return screen;
        }

        // 가로 진행 막대. 채움 비율은 Fill의 anchorMax.x로 표현한다
        private static RectTransform CreateBar(Transform parent, string name, string fillName, float y, float height,
            Color fillColor, float initialRatio)
        {
            Image bar = UIFactory.CreateImage(name, parent, UIFactory.WithAlpha(Color.white, 0.25f), false);
            UIFactory.Place(bar.rectTransform, TOP_CENTER, CENTER, new Vector2(0f, y), new Vector2(900f, height));
            Image fill = UIFactory.CreateImage(fillName, bar.transform, fillColor, false);
            RectTransform rect = fill.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(initialRatio, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static CaptureResultPopup BuildResultPopup(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("CaptureResultPopup", parent, null);
            CaptureResultPopup popup = root.gameObject.AddComponent<CaptureResultPopup>();
            UIFactory.CreateDim(root);
            RectTransform card = UIFactory.CreateCard(root, new Vector2(880f, 940f));

            TextMeshProUGUI title = UIFactory.CreateTopText("TitleText", card, "", 56f, UIFactory.TEXT_DARK, -50f, 80f, 40f, true);
            Image portrait = UIFactory.CreateImage("Portrait", card, Color.white, false);
            portrait.preserveAspect = true;
            UIFactory.Place(portrait.rectTransform, TOP_CENTER, TOP_CENTER, new Vector2(0f, -150f), new Vector2(300f, 300f));
            TextMeshProUGUI body = UIFactory.CreateText("BodyText", card, "", 34f, UIFactory.TEXT_DARK, TextAlignmentOptions.Top);
            UIFactory.Stretch(body.rectTransform, 50f, 480f, 50f, 190f);
            Button confirm = UIFactory.CreateButton("ConfirmButton", card, "확인", 40f, UIFactory.AMBER, Color.white, false);
            UIFactory.Place((RectTransform)confirm.transform, BOTTOM_CENTER, BOTTOM_CENTER, new Vector2(0f, 50f), new Vector2(360f, 120f));

            UIFactory.SetRef(popup, "_router", refs.router);
            UIFactory.SetRef(popup, "_titleText", title);
            UIFactory.SetRef(popup, "_bodyText", body);
            UIFactory.SetRef(popup, "_portrait", portrait);
            UIFactory.SetRef(popup, "_confirmButton", confirm);
            return popup;
        }

        public static ClearScreen BuildClearScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("ClearScreen", parent, UIFactory.NAVY);
            ClearScreen screen = root.gameObject.AddComponent<ClearScreen>();

            UIFactory.CreateTopText("TitleText", root, "동성로 지도 완성!", 72f, Color.white, -300f, 110f, 40f, true);
            Image portrait = UIFactory.CreateImage("Portrait", root, Color.white, false);
            portrait.preserveAspect = true;
            UIFactory.Place(portrait.rectTransform, CENTER, CENTER, new Vector2(0f, 120f), new Vector2(420f, 420f));
            TextMeshProUGUI body = UIFactory.CreateText("BodyText", root, "", 38f, Color.white, TextAlignmentOptions.Center);
            UIFactory.Place(body.rectTransform, CENTER, CENTER, new Vector2(0f, -260f), new Vector2(900f, 260f));
            Button close = UIFactory.CreateButton("CloseButton", root, "지도로 돌아가기", 44f, UIFactory.AMBER, Color.white, false);
            UIFactory.Place((RectTransform)close.transform, BOTTOM_CENTER, BOTTOM_CENTER, new Vector2(0f, 160f), new Vector2(480f, 130f));

            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_portrait", portrait);
            UIFactory.SetRef(screen, "_bodyText", body);
            UIFactory.SetRef(screen, "_closeButton", close);
            return screen;
        }
    }
}
