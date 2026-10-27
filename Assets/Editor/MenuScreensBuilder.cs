using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 퍼즐, 도감, 카드 상세, 상점 화면
    internal static class MenuScreensBuilder
    {
        private static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TOP_CENTER = new Vector2(0.5f, 1f);
        private static readonly Vector2 BOTTOM_CENTER = new Vector2(0.5f, 0f);
        private static readonly Vector2 PILL_POSITION = new Vector2(-40f, -40f);
        private static readonly Vector2 PILL_SIZE = new Vector2(300f, 100f);

        // 스크롤 목록 영역 (위/아래/좌우 여백)
        private const float LIST_TOP = 200f;
        private const float LIST_BOTTOM = 40f;
        private const float LIST_SIDE = 40f;

        // 제목 영역 좌우 여백
        private const float TITLE_LEFT = 180f;
        private const float TITLE_RIGHT = 360f;

        public static PuzzleScreen BuildPuzzleScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("PuzzleScreen", parent, UIFactory.PALE);
            PuzzleScreen screen = root.gameObject.AddComponent<PuzzleScreen>();

            Button back = UIFactory.CreateBackButton(root);
            CreateTitle(root, "동성로 지도 퍼즐");
            TMP_Text completion = UIFactory.CreatePill(root, "CompletionPill", "CompletionText", PILL_POSITION, PILL_SIZE, 36f, UIFactory.NAVY);

            Image grid = UIFactory.CreateImage("Grid", root, UIFactory.NAVY, false);
            UIFactory.Place(grid.rectTransform, CENTER, CENTER, new Vector2(0f, 60f), new Vector2(960f, 960f));
            GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(236f, 236f);
            layout.spacing = new Vector2(4f, 4f);
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = refs.config.puzzleColumns;
            layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            layout.startAxis = GridLayoutGroup.Axis.Horizontal;

            int total = refs.config.puzzleColumns * refs.config.puzzleRows;
            RawImage[] pieces = new RawImage[total];
            TMP_Text[] labels = new TMP_Text[total];
            for (int i = 0; i < total; i++)
            {
                RectTransform cell = UIFactory.CreateRect($"Piece{i:00}", grid.transform);
                pieces[i] = cell.gameObject.AddComponent<RawImage>();
                pieces[i].raycastTarget = false;
                labels[i] = UIFactory.CreateText("Label", cell, "?", 90f, UIFactory.LIGHT, TextAlignmentOptions.Center, true);
                UIFactory.Stretch(labels[i].rectTransform, 0f, 0f, 0f, 0f);
            }

            TextMeshProUGUI hint = UIFactory.CreateText("HintText", root, "", 34f, UIFactory.NAVY, TextAlignmentOptions.Center);
            UIFactory.StretchBottom(hint.rectTransform, 160f, 160f, 60f);

            UIFactory.SetRef(screen, "_game", refs.game);
            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_backButton", back);
            UIFactory.SetRef(screen, "_completionText", completion);
            UIFactory.SetRef(screen, "_hintText", hint);
            UIFactory.SetRefs(screen, "_pieceImages", pieces);
            UIFactory.SetRefs(screen, "_pieceLabels", labels);
            return screen;
        }

        // 뒤로 버튼(왼쪽, 끝 x=160)과 정보 알약(오른쪽, 시작 x=740) 사이의 화면 제목
        private static void CreateTitle(Transform parent, string title)
        {
            TextMeshProUGUI text = UIFactory.CreateTopText("TitleText", parent, title, 56f, UIFactory.NAVY, -60f, 80f, 0f, true);
            RectTransform rect = text.rectTransform;
            rect.offsetMin = new Vector2(TITLE_LEFT, rect.offsetMin.y);
            rect.offsetMax = new Vector2(-TITLE_RIGHT, rect.offsetMax.y);
        }

        public static CollectionScreen BuildCollectionScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("CollectionScreen", parent, UIFactory.PALE);
            CollectionScreen screen = root.gameObject.AddComponent<CollectionScreen>();

            Button back = UIFactory.CreateBackButton(root);
            CreateTitle(root, "도감");
            TMP_Text count = UIFactory.CreatePill(root, "CountPill", "CountText", PILL_POSITION, PILL_SIZE, 36f, UIFactory.NAVY);

            RectTransform content = UIFactory.CreateScrollView(root, LIST_TOP, LIST_BOTTOM, LIST_SIDE);
            GridLayoutGroup layout = content.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(480f, 640f);
            layout.spacing = new Vector2(40f, 40f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            layout.childAlignment = TextAnchor.UpperCenter;

            UIFactory.SetRef(screen, "_game", refs.game);
            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_backButton", back);
            UIFactory.SetRef(screen, "_countText", count);
            UIFactory.SetRef(screen, "_content", content);
            UIFactory.SetRef(screen, "_cardPrefab", AssetDatabase.LoadAssetAtPath<CardView>(PrefabBuilder.CARD_VIEW_PATH));
            return screen;
        }

        public static CardDetailPopup BuildCardDetailPopup(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("CardDetailPopup", parent, null);
            CardDetailPopup popup = root.gameObject.AddComponent<CardDetailPopup>();
            UIFactory.CreateDim(root);
            RectTransform card = UIFactory.CreateCard(root, new Vector2(920f, 1400f));

            Image portrait = UIFactory.CreateImage("Portrait", card, Color.white, false);
            portrait.preserveAspect = true;
            UIFactory.Place(portrait.rectTransform, TOP_CENTER, TOP_CENTER, new Vector2(0f, -50f), new Vector2(480f, 480f));
            TextMeshProUGUI nameText = UIFactory.CreateTopText("NameText", card, "", 56f, UIFactory.TEXT_DARK, -560f, 80f, 50f, true);
            TextMeshProUGUI spotText = UIFactory.CreateTopText("SpotText", card, "", 34f, UIFactory.BLUE, -640f, 60f, 50f);
            TextMeshProUGUI story = UIFactory.CreateText("StoryText", card, "", 32f, UIFactory.TEXT_DARK, TextAlignmentOptions.TopLeft);
            UIFactory.Stretch(story.rectTransform, 50f, 720f, 50f, 200f);
            Button close = UIFactory.CreateButton("CloseButton", card, "닫기", 40f, UIFactory.BLUE, Color.white, false);
            UIFactory.Place((RectTransform)close.transform, BOTTOM_CENTER, BOTTOM_CENTER, new Vector2(0f, 50f), new Vector2(360f, 120f));

            UIFactory.SetRef(popup, "_router", refs.router);
            UIFactory.SetRef(popup, "_portrait", portrait);
            UIFactory.SetRef(popup, "_nameText", nameText);
            UIFactory.SetRef(popup, "_spotText", spotText);
            UIFactory.SetRef(popup, "_storyText", story);
            UIFactory.SetRef(popup, "_closeButton", close);
            return popup;
        }

        public static ShopScreen BuildShopScreen(Transform parent, SceneRefs refs)
        {
            RectTransform root = UIFactory.CreateScreenRoot("ShopScreen", parent, UIFactory.PALE);
            ShopScreen screen = root.gameObject.AddComponent<ShopScreen>();

            Button back = UIFactory.CreateBackButton(root);
            CreateTitle(root, "상점");
            TMP_Text coin = UIFactory.CreatePill(root, "CoinPill", "CoinText", PILL_POSITION, PILL_SIZE, 36f, UIFactory.NAVY);

            RectTransform content = UIFactory.CreateScrollView(root, LIST_TOP, LIST_BOTTOM, LIST_SIDE);
            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 24f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            UIFactory.SetRef(screen, "_game", refs.game);
            UIFactory.SetRef(screen, "_router", refs.router);
            UIFactory.SetRef(screen, "_backButton", back);
            UIFactory.SetRef(screen, "_coinText", coin);
            UIFactory.SetRef(screen, "_content", content);
            UIFactory.SetRef(screen, "_itemPrefab", AssetDatabase.LoadAssetAtPath<ShopItemView>(PrefabBuilder.SHOP_ITEM_VIEW_PATH));
            return screen;
        }
    }
}
