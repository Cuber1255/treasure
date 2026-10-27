using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 빌더 공용 uGUI 생성 헬퍼와 팔레트
    internal static class UIFactory
    {
        public static readonly Color NAVY = Hex("#2B3A55");
        public static readonly Color BLUE = Hex("#6F8FB8");
        public static readonly Color LIGHT = Hex("#AFC8E8");
        public static readonly Color PALE = Hex("#E4EEF9");
        public static readonly Color AMBER = Hex("#FFB020");
        public static readonly Color GRAY = Hex("#9AA3AD");
        public static readonly Color TEXT_DARK = Hex("#1F2A3D");
        public static readonly Color DIM = new Color(0f, 0f, 0f, 0.55f);

        private const int UI_LAYER = 5;
        private const float BACK_BUTTON_SIZE = 120f;

        private static TMP_FontAsset _font;
        private static Sprite _roundedSprite;
        private static Sprite _circleSprite;

        public static TMP_FontAsset Font =>
            _font != null ? _font : _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontSetup.FONT_ASSET_PATH);

        public static Sprite RoundedSprite =>
            _roundedSprite != null ? _roundedSprite : _roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        public static Sprite CircleSprite =>
            _circleSprite != null ? _circleSprite : _circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderArtGenerator.CIRCLE_PATH);

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        // ---- RectTransform ----

        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = UI_LAYER;
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        // 한 점 앵커 배치
        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // 부모를 채우고 여백(left, top, right, bottom)을 둔다
        public static void Stretch(RectTransform rect, float left, float top, float right, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        // 상단 가로 전체: 위에서 y만큼 내려온 곳에 높이 height
        public static void StretchTop(RectTransform rect, float y, float height, float side)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-side * 2f, height);
        }

        // 하단 가로 전체: 아래에서 y만큼 올라간 곳에 높이 height
        public static void StretchBottom(RectTransform rect, float y, float height, float side)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-side * 2f, height);
        }

        // ---- 그래픽 ----

        public static Image CreateImage(string name, Transform parent, Color color, bool raycast, Sprite sprite = null)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            image.sprite = sprite;
            if (sprite != null && sprite == RoundedSprite)
            {
                image.type = Image.Type.Sliced;
            }
            return image;
        }

        public static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions align, bool bold = false)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        // 상단 가로 전체 텍스트 (가운데 정렬 기본)
        public static TextMeshProUGUI CreateTopText(string name, Transform parent, string text, float size, Color color,
            float y, float height, float side, bool bold = false)
        {
            TextMeshProUGUI label = CreateText(name, parent, text, size, color, TextAlignmentOptions.Center, bold);
            StretchTop(label.rectTransform, y, height, side);
            return label;
        }

        public static Button CreateButton(string name, Transform parent, string label, float fontSize, Color background,
            Color labelColor, bool circle, bool bold = true)
        {
            Image image = CreateImage(name, parent, Color.white, true, circle ? CircleSprite : RoundedSprite);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.colors = new ColorBlock
            {
                normalColor = background,
                highlightedColor = Scale(background, 1.1f),
                pressedColor = Scale(background, 0.85f),
                selectedColor = background,
                disabledColor = GRAY,
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            TextMeshProUGUI text = CreateText("Label", image.transform, label, fontSize, labelColor, TextAlignmentOptions.Center, bold);
            Stretch(text.rectTransform, 8f, 4f, 8f, 4f);
            return button;
        }

        private static Color Scale(Color color, float factor)
        {
            return new Color(Mathf.Clamp01(color.r * factor), Mathf.Clamp01(color.g * factor), Mathf.Clamp01(color.b * factor), color.a);
        }

        public static Button CreateBackButton(Transform parent)
        {
            Button button = CreateButton("BackButton", parent, "<", 64f, NAVY, Color.white, true);
            Place((RectTransform)button.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f),
                new Vector2(BACK_BUTTON_SIZE, BACK_BUTTON_SIZE));
            return button;
        }

        // 우상단 정보 표시 알약 (예: 코인 300)
        public static TextMeshProUGUI CreatePill(Transform parent, string name, string textName, Vector2 position,
            Vector2 size, float fontSize, Color background)
        {
            Image pill = CreateImage(name, parent, background, false, RoundedSprite);
            Place(pill.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), position, size);
            TextMeshProUGUI text = CreateText(textName, pill.transform, "", fontSize, Color.white, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, 10f, 0f, 10f, 0f);
            return text;
        }

        public static Image CreateDim(Transform parent)
        {
            Image dim = CreateImage("Dim", parent, DIM, true);
            Stretch(dim.rectTransform, 0f, 0f, 0f, 0f);
            return dim;
        }

        public static RectTransform CreateCard(Transform parent, Vector2 size)
        {
            Image card = CreateImage("Card", parent, PALE, false, RoundedSprite);
            Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            return card.rectTransform;
        }

        // 세로 스크롤 목록. 반환값은 항목을 넣을 Content
        public static RectTransform CreateScrollView(Transform parent, float top, float bottom, float side)
        {
            RectTransform root = CreateRect("ScrollView", parent);
            Stretch(root, side, top, side, bottom);
            ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();

            Image viewportImage = CreateImage("Viewport", root, Color.white, true);
            Stretch(viewportImage.rectTransform, 0f, 0f, 0f, 0f);
            Mask mask = viewportImage.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            RectTransform content = CreateRect("Content", viewportImage.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = content;
            scroll.viewport = viewportImage.rectTransform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;
            return content;
        }

        // 화면 루트: SafeArea를 채우는 RectTransform (배경 지정 시 터치를 막는 Image)
        public static RectTransform CreateScreenRoot(string name, Transform parent, Color? background)
        {
            RectTransform root = CreateRect(name, parent);
            Stretch(root, 0f, 0f, 0f, 0f);
            if (background.HasValue)
            {
                Image image = root.gameObject.AddComponent<Image>();
                image.color = background.Value;
                image.raycastTarget = true;
            }
            return root;
        }

        // ---- 직렬화 참조 연결 ----

        public static void SetRef(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = FindProperty(serialized, target, field);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetRefs(UnityEngine.Object target, string field, UnityEngine.Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = FindProperty(serialized, target, field);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // 이름이 틀리면 바로 예외를 던져 코드와 빌더의 필드 이름 불일치를 드러낸다
        private static SerializedProperty FindProperty(SerializedObject serialized, UnityEngine.Object target, string field)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name}.{field} 필드를 찾을 수 없습니다");
            }
            return property;
        }
    }
}
