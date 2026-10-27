using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Treasure.EditorTools
{
    // 화면 빌더들이 공유하는 씬 시스템 참조
    internal class SceneRefs
    {
        public GameConfig config;
        public GameManager game;
        public ScreenRouter router;
        public CaptureFlow captureFlow;
        public Camera camera;
        public MapView mapView;
        public LocationProvider location;
        public VectorMapLoader vectorMap;
    }

    // GameScene 전체(월드 + UI)를 코드로 만든다
    public static class GameSceneBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/GameScene.unity";

        private static readonly Color PLAYER_COLOR = UIFactory.Hex("#1E6FFF");
        private const string MAP_SHADER_PATH = "Assets/Art/Shaders/MapFlatColor.shader";
        // 기준 해상도 (세로 화면)
        private static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1080f, 1920f);

        [MenuItem("Treasure/게임 씬 다시 만들기")]
        private static void RebuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            bool confirmed = EditorUtility.DisplayDialog("게임 씬 다시 만들기",
                "GameScene을 새로 만듭니다. 씬에서 직접 수정한 내용은 사라집니다. (데이터·아트·프리팹은 없을 때만 생성)",
                "만들기", "취소");
            if (confirmed)
            {
                BuildAll();
            }
        }

        // 자동화 진입점 (대화상자 없음)
        public static void BuildAll()
        {
            // 새 씬을 열 때 사용하지 않는 에셋이 언로드되므로, 에셋은 반드시 새 씬을 연 뒤에 불러온다
            // (먼저 불러 둔 GameConfig 참조가 씬에 null로 저장되는 문제 방지)
            // 프리팹용 임시 오브젝트도 이 새 씬에서 만들어 기존 씬을 건드리지 않는다
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            FontSetup.EnsureFontAsset();
            PlaceholderArtGenerator.EnsureArt();
            GameConfig config = GameDataBuilder.EnsureAssets();
            PrefabBuilder.EnsurePrefabs();

            SceneRefs refs = BuildWorld(config);
            BuildUI(refs);

            AssetFolders.EnsureParentOf(SCENE_PATH);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            // SampleScene 파일은 남겨 두고 빌드 목록에서만 뺀다
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(SCENE_PATH, true) };
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.locationUsageDescription = "스팟 근처에 도착했는지 확인하기 위해 위치 정보를 사용합니다.";
            PlayerSettings.iOS.cameraUsageDescription = "카메라 화면에서 캐릭터를 찾기 위해 카메라를 사용합니다.";
        }

        // ---- 월드 ----

        private static SceneRefs BuildWorld(GameConfig config)
        {
            SceneRefs refs = new SceneRefs { config = config };
            refs.camera = CreateCamera();
            CreateGlobalLight();
            CreateEventSystem();

            GameObject systems = new GameObject("Systems");
            refs.game = systems.AddComponent<GameManager>();
            LocationProvider location = systems.AddComponent<LocationProvider>();
            refs.location = location;
            refs.router = systems.AddComponent<ScreenRouter>();
            refs.captureFlow = systems.AddComponent<CaptureFlow>();
            UIFactory.SetRef(refs.game, "_config", config);
            UIFactory.SetRef(refs.game, "_location", location);

            BuildMap(refs);
            return refs;
        }

        private static Camera CreateCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = VectorMapStyle.LAND_COLOR;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            go.AddComponent<UniversalAdditionalCameraData>();
            // 스팟 마커(콜라이더) 탭을 EventSystem으로 받으려면 필요하다
            go.AddComponent<Physics2DRaycaster>();
            return camera;
        }

        private static void CreateGlobalLight()
        {
            GameObject go = new GameObject("Global Light 2D");
            Light2D light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            SortingLayer[] layers = SortingLayer.layers;
            int[] ids = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                ids[i] = layers[i].id;
            }
            light.targetSortingLayers = ids;
        }

        private static void CreateEventSystem()
        {
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            // 에디터에서 추가하면 DefaultInputActions가 자동 할당된다
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static void BuildMap(SceneRefs refs)
        {
            GameObject map = new GameObject("Map");
            refs.mapView = map.AddComponent<MapView>();
            BuildVectorMap(map.transform, refs);

            GameObject spots = new GameObject("Spots");
            spots.transform.SetParent(map.transform, false);

            BuildPlayerMarker(map.transform);

            MapView view = refs.mapView;
            UIFactory.SetRef(view, "_game", refs.game);
            UIFactory.SetRef(view, "_router", refs.router);
            UIFactory.SetRef(view, "_camera", refs.camera);
            UIFactory.SetRef(view, "_spotMarkerPrefab",
                AssetDatabase.LoadAssetAtPath<SpotMarker>(PrefabBuilder.SPOT_MARKER_PATH));
            UIFactory.SetRef(view, "_spotRoot", spots.transform);
        }

        // 플레이어 점과 바라보는 방향 화살표 (화살표는 점 위쪽으로 튀어나온다)
        private static void BuildPlayerMarker(Transform map)
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(map, false);
            CreateCircleSprite("Outline", player.transform, Color.white, 1.3f, 19);
            CreateCircleSprite("Dot", player.transform, PLAYER_COLOR, 1f, 20);

            GameObject headingArrow = new GameObject("HeadingArrow");
            headingArrow.transform.SetParent(player.transform, false);
            Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderArtGenerator.HEADING_ARROW_PATH);
            CreateArrowSprite("ArrowOutline", headingArrow.transform, arrowSprite, Color.white, 0.75f, 19);
            CreateArrowSprite("ArrowFill", headingArrow.transform, arrowSprite, PLAYER_COLOR, 0.55f, 21);

            MapView view = map.GetComponent<MapView>();
            UIFactory.SetRef(view, "_playerMarker", player.transform);
            UIFactory.SetRef(view, "_playerArrow", headingArrow.transform);
        }

        private static void CreateArrowSprite(string name, Transform parent, Sprite sprite, Color color, float scale, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = order;
        }

        // 직접 그리는 벡터 지도 (OpenFreeMap 타일)
        private static void BuildVectorMap(Transform map, SceneRefs refs)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(MAP_SHADER_PATH);
            if (shader == null)
            {
                throw new InvalidOperationException($"{MAP_SHADER_PATH}이 없습니다");
            }

            GameObject go = new GameObject("VectorMap");
            go.transform.SetParent(map, false);
            refs.vectorMap = go.AddComponent<VectorMapLoader>();
            UIFactory.SetRef(refs.vectorMap, "_game", refs.game);
            UIFactory.SetRef(refs.vectorMap, "_camera", refs.camera);
            UIFactory.SetRef(refs.vectorMap, "_shader", shader);
        }

        private static void CreateCircleSprite(string name, Transform parent, Color color, float scale, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = UIFactory.CircleSprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = order;
        }

        // ---- UI ----

        private static void BuildUI(SceneRefs refs)
        {
            Transform safeArea = CreateCanvas();

            MainHud hud = HudBuilder.BuildMainHud(safeArea, refs);
            CameraScreen cameraScreen = CaptureUIBuilder.BuildCameraScreen(safeArea, refs);
            CaptureActionScreen actionScreen = CaptureUIBuilder.BuildActionScreen(safeArea, refs);
            PuzzleScreen puzzleScreen = MenuScreensBuilder.BuildPuzzleScreen(safeArea, refs);
            CollectionScreen collectionScreen = MenuScreensBuilder.BuildCollectionScreen(safeArea, refs);
            ShopScreen shopScreen = MenuScreensBuilder.BuildShopScreen(safeArea, refs);
            SpotInfoPopup spotInfo = HudBuilder.BuildSpotInfoPopup(safeArea, refs);
            CaptureResultPopup result = CaptureUIBuilder.BuildResultPopup(safeArea, refs);
            CardDetailPopup cardDetail = MenuScreensBuilder.BuildCardDetailPopup(safeArea, refs);
            ExitConfirmPopup exitPopup = HudBuilder.BuildExitConfirmPopup(safeArea, refs);
            ClearScreen clearScreen = CaptureUIBuilder.BuildClearScreen(safeArea, refs);
            DebugPanel debugPanel = DebugUIBuilder.BuildDebugPanel(safeArea, refs);

            // 지도 화면만 켜 둔다
            GameObject[] hidden =
            {
                cameraScreen.gameObject, actionScreen.gameObject, puzzleScreen.gameObject, collectionScreen.gameObject,
                shopScreen.gameObject, spotInfo.gameObject, result.gameObject, cardDetail.gameObject,
                exitPopup.gameObject, clearScreen.gameObject, debugPanel.gameObject
            };
            foreach (GameObject go in hidden)
            {
                go.SetActive(false);
            }

            UIFactory.SetRef(refs.router, "_mainHud", hud.gameObject);
            UIFactory.SetRef(refs.router, "_cameraScreen", cameraScreen.gameObject);
            UIFactory.SetRef(refs.router, "_actionScreen", actionScreen.gameObject);
            UIFactory.SetRef(refs.router, "_puzzleScreen", puzzleScreen.gameObject);
            UIFactory.SetRef(refs.router, "_collectionScreen", collectionScreen.gameObject);
            UIFactory.SetRef(refs.router, "_shopScreen", shopScreen.gameObject);
            UIFactory.SetRef(refs.router, "_exitPopup", exitPopup);
            UIFactory.SetRef(refs.router, "_captureFlow", refs.captureFlow);

            UIFactory.SetRef(refs.captureFlow, "_game", refs.game);
            UIFactory.SetRef(refs.captureFlow, "_router", refs.router);
            UIFactory.SetRef(refs.captureFlow, "_cameraScreen", cameraScreen);
            UIFactory.SetRef(refs.captureFlow, "_actionScreen", actionScreen);
            UIFactory.SetRef(refs.captureFlow, "_resultPopup", result);
            UIFactory.SetRef(refs.captureFlow, "_clearScreen", clearScreen);

            UIFactory.SetRef(refs.mapView, "_spotInfoPopup", spotInfo);
            UIFactory.SetRef(collectionScreen, "_detailPopup", cardDetail);
            UIFactory.SetRef(hud, "_debugPanel", debugPanel);
        }

        private static Transform CreateCanvas()
        {
            GameObject go = new GameObject("Canvas");
            go.layer = 5;
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = REFERENCE_RESOLUTION;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            RectTransform safeArea = UIFactory.CreateRect("SafeArea", go.transform);
            UIFactory.Stretch(safeArea, 0f, 0f, 0f, 0f);
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            return safeArea;
        }
    }
}
