using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Treasure.EditorTools
{
    // 한글 TMP 폰트 에셋(NanumGothic SDF)을 만들고 TMP 기본 폰트로 지정한다
    internal static class FontSetup
    {
        public const string FONT_PATH = "Assets/Fonts/NanumGothic-Regular.ttf";
        public const string FONT_ASSET_PATH = "Assets/Fonts/NanumGothic SDF.asset";

        // 한글은 획이 많아 샘플링 크기를 크게, 패딩은 샘플링의 10%
        private const int SAMPLING_POINT_SIZE = 40;
        private const int ATLAS_PADDING = 4;
        private const int ATLAS_SIZE = 1024;

        public static TMP_FontAsset EnsureFontAsset()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
            if (font == null)
            {
                throw new InvalidOperationException(
                    "Assets/Fonts/NanumGothic-Regular.ttf가 없습니다. 계획 Step 0의 curl 명령으로 받으세요");
            }

            string[] settingsGuids = AssetDatabase.FindAssets("t:TMP_Settings");
            if (settingsGuids.Length == 0)
            {
                throw new InvalidOperationException("TMP Essential Resources를 먼저 임포트하세요");
            }

            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
            if (fontAsset == null)
            {
                fontAsset = CreateFontAsset(font);
            }

            TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(AssetDatabase.GUIDToAssetPath(settingsGuids[0]));
            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
            // 한글을 글자 단위가 아니라 띄어쓰기 단위로 줄바꿈한다
            serializedSettings.FindProperty("m_UseModernHangulLineBreakingRules").boolValue = true;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static TMP_FontAsset CreateFontAsset(Font font)
        {
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, SAMPLING_POINT_SIZE, ATLAS_PADDING,
                GlyphRenderMode.SDFAA, ATLAS_SIZE, ATLAS_SIZE, AtlasPopulationMode.Dynamic, true);
            fontAsset.name = "NanumGothic SDF";
            AssetDatabase.CreateAsset(fontAsset, FONT_ASSET_PATH);

            // 머티리얼과 아틀라스 텍스처는 폰트 에셋 안의 서브에셋으로 저장해야 유지된다
            fontAsset.material.name = "NanumGothic SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            fontAsset.atlasTextures[0].name = "NanumGothic SDF Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);

            SerializedObject serialized = new SerializedObject(fontAsset);
            // 에디터에서 채운 글리프가 빌드에 섞이지 않도록 빌드 시 동적 데이터를 비운다
            serialized.FindProperty("m_ClearDynamicDataOnBuild").boolValue = true;
            // 런타임 동적 글리프 생성에 원본 폰트가 필요하다
            SerializedProperty source = serialized.FindProperty("m_SourceFontFile");
            if (source != null && source.objectReferenceValue == null)
            {
                source.objectReferenceValue = font;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }
    }
}
