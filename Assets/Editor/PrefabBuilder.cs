using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 스팟 마커, 도감 카드, 상점 아이템 프리팹을 만든다 (없을 때만)
    internal static class PrefabBuilder
    {
        public const string SPOT_MARKER_PATH = "Assets/Prefabs/Environment/SpotMarker.prefab";
        public const string CARD_VIEW_PATH = "Assets/Prefabs/UI/CardView.prefab";
        public const string SHOP_ITEM_VIEW_PATH = "Assets/Prefabs/UI/ShopItemView.prefab";

        public static void EnsurePrefabs()
        {
            EnsurePrefab(SPOT_MARKER_PATH, BuildSpotMarker);
            EnsurePrefab(CARD_VIEW_PATH, BuildCardView);
            EnsurePrefab(SHOP_ITEM_VIEW_PATH, BuildShopItemView);
        }

        private static void EnsurePrefab(string path, System.Func<GameObject> build)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                return;
            }
            AssetFolders.EnsureParentOf(path);
            GameObject root = build();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static GameObject BuildSpotMarker()
        {
            GameObject root = new GameObject("SpotMarker");
            SpotMarker marker = root.AddComponent<SpotMarker>();

            SpriteRenderer range = CreateSprite("Range", root.transform, 0);
            SpriteRenderer icon = CreateSprite("Icon", root.transform, 11);
            CircleCollider2D collider = icon.gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            SpriteRenderer outline = CreateSprite("Outline", icon.transform, 10);
            outline.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

            UIFactory.SetRef(marker, "_icon", icon);
            UIFactory.SetRef(marker, "_range", range);
            return root;
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = UIFactory.CircleSprite;
            spriteRenderer.sortingOrder = order;
            return spriteRenderer;
        }

        private static GameObject BuildCardView()
        {
            RectTransform root = UIFactory.CreateRect("CardView", null);
            root.sizeDelta = new Vector2(480f, 640f);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = UIFactory.RoundedSprite;
            background.type = Image.Type.Sliced;
            background.color = UIFactory.BLUE;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            // 배경색은 Bind에서 정하므로 비활성 상태에도 색을 바꾸지 않는다
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.disabledColor = Color.white;
            button.colors = colors;

            Image portrait = UIFactory.CreateImage("Portrait", root, Color.white, false);
            portrait.preserveAspect = true;
            UIFactory.Place(portrait.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f),
                new Vector2(380f, 380f));
            TextMeshProUGUI nameText = UIFactory.CreateTopText("NameText", root, "???", 44f, Color.white, -450f, 70f, 20f, true);
            TextMeshProUGUI spotText = UIFactory.CreateTopText("SpotText", root, "", 30f, UIFactory.PALE, -530f, 60f, 20f);

            CardView view = root.gameObject.AddComponent<CardView>();
            UIFactory.SetRef(view, "_button", button);
            UIFactory.SetRef(view, "_background", background);
            UIFactory.SetRef(view, "_portrait", portrait);
            UIFactory.SetRef(view, "_nameText", nameText);
            UIFactory.SetRef(view, "_spotText", spotText);
            return root.gameObject;
        }

        private static GameObject BuildShopItemView()
        {
            RectTransform root = UIFactory.CreateRect("ShopItemView", null);
            root.sizeDelta = new Vector2(1000f, 240f);
            Image background = root.gameObject.AddComponent<Image>();
            background.sprite = UIFactory.RoundedSprite;
            background.type = Image.Type.Sliced;
            background.color = UIFactory.BLUE;
            background.raycastTarget = false;
            LayoutElement layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 240f;

            Image icon = UIFactory.CreateImage("Icon", root, Color.white, false, UIFactory.CircleSprite);
            UIFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(110f, 0f),
                new Vector2(150f, 150f));

            TextMeshProUGUI nameText = CreateRowText("NameText", root, 44f, Color.white, -20f, 60f, true);
            TextMeshProUGUI descriptionText = CreateRowText("DescriptionText", root, 30f, UIFactory.PALE, -85f, 80f, false);
            TextMeshProUGUI ownedText = CreateRowText("OwnedText", root, 28f, Color.white, -170f, 50f, false);

            Button buyButton = UIFactory.CreateButton("BuyButton", root, "", 36f, UIFactory.AMBER, UIFactory.NAVY, false);
            UIFactory.Place((RectTransform)buyButton.transform, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-170f, 0f), new Vector2(280f, 130f));
            TMP_Text priceText = buyButton.GetComponentInChildren<TMP_Text>();
            priceText.name = "PriceText";

            ShopItemView view = root.gameObject.AddComponent<ShopItemView>();
            UIFactory.SetRef(view, "_icon", icon);
            UIFactory.SetRef(view, "_nameText", nameText);
            UIFactory.SetRef(view, "_descriptionText", descriptionText);
            UIFactory.SetRef(view, "_ownedText", ownedText);
            UIFactory.SetRef(view, "_buyButton", buyButton);
            UIFactory.SetRef(view, "_priceText", priceText);
            return root.gameObject;
        }

        // 아이콘 오른쪽(x 210)부터 구매 버튼 왼쪽까지의 왼쪽 정렬 텍스트 한 줄
        private static TextMeshProUGUI CreateRowText(string name, Transform parent, float size, Color color, float y,
            float height, bool bold)
        {
            TextMeshProUGUI text = UIFactory.CreateText(name, parent, "", size, color, TextAlignmentOptions.Left, bold);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(210f, y - height);
            rect.offsetMax = new Vector2(-330f, y);
            return text;
        }
    }
}
