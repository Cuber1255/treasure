using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace Treasure
{
    // 화면에 보이는 벡터 지도 타일(OpenFreeMap)을 받아 단색 메시로 그린다
    // 타일 해석·삼각분할은 백그라운드 스레드에서, Mesh 만들기만 메인 스레드에서 한다
    public class VectorMapLoader : MonoBehaviour
    {
        [SerializeField] private GameManager _game;
        [SerializeField] private Camera _camera;
        [SerializeField] private Shader _shader;

        // OpenMapTiles 최대 줌. 더 확대하면 같은 타일을 벡터로 키워 그린다
        private const int TILE_ZOOM = 14;
        private const int MAX_PARALLEL_LOADS = 2;
        private const int CACHE_DAYS = 30;
        private const int REQUEST_TIMEOUT_SECONDS = 20;
        private const float TILE_RETRY_SECONDS = 15f;
        private const float TEMPLATE_RETRY_SECONDS = 30f;
        private const string TEMPLATE_PREF_KEY = "vector_tile_url_template";
        private const string CACHE_FOLDER = "VectorTiles";

        [Serializable]
        private class TileJson
        {
            public string[] tiles;
        }

        private readonly Dictionary<long, GameObject> _loaded = new();
        private readonly HashSet<long> _loading = new();
        private readonly Dictionary<long, float> _retryAt = new();
        private readonly List<long> _evictKeys = new();
        private Material[] _materials;
        private string _urlTemplate;
        private bool _templateChecked;
        private int _failedCount;

        public string StatusText
        {
            get
            {
                string source = _urlTemplate != null ? "OpenFreeMap 벡터" : "OpenFreeMap 벡터(주소 확인 전 · 저장된 타일만)";
                return $"{source} · 타일 {_loaded.Count}개 · 받는 중 {_loading.Count}개 · 실패 {_failedCount}회";
            }
        }

        private void Start()
        {
            _materials = new Material[VectorMapStyle.SLOT_COUNT];
            for (int i = 0; i < _materials.Length; i++)
            {
                MapSlot slot = (MapSlot)i;
                _materials[i] = new Material(_shader) { name = $"VectorMap_{slot}", color = VectorMapStyle.GetColor(slot) };
            }
            StartCoroutine(ResolveUrlTemplate());
        }

        private void Update()
        {
            // 주소 확인 전에 요청하면 첫 타일들이 실패해 재시도 대기에 걸린다
            if (!_templateChecked)
            {
                return;
            }
            RectInt range = GetVisibleTileRange();
            RequestTiles(range);
            EvictTiles(range);
        }

        private void OnDestroy()
        {
            // 저장 초기화로 씬을 다시 불러올 때 런타임 메시·머티리얼이 남지 않게 한다
            foreach (long key in new List<long>(_loaded.Keys))
            {
                DestroyTile(key);
            }
            if (_materials == null)
            {
                return;
            }
            foreach (Material material in _materials)
            {
                Destroy(material);
            }
        }

        // 타일 주소의 버전 경로가 주기적으로 바뀌므로 실행할 때마다 TileJSON에서 다시 얻는다
        private IEnumerator ResolveUrlTemplate()
        {
            while (true)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(_game.Config.vectorTileJsonUrl))
                {
                    request.timeout = REQUEST_TIMEOUT_SECONDS;
                    yield return request.SendWebRequest();

                    string template = request.result == UnityWebRequest.Result.Success
                        ? ParseTemplate(request.downloadHandler.text)
                        : null;
                    if (template != null)
                    {
                        _urlTemplate = template;
                        PlayerPrefs.SetString(TEMPLATE_PREF_KEY, template);
                        PlayerPrefs.Save();
                        _templateChecked = true;
                        yield break;
                    }
                    Debug.LogWarning($"벡터 지도 주소 확인 실패: {request.error}");
                }

                // 오프라인이면 지난번에 저장한 주소를 쓴다 (캐시된 타일은 그대로 쓰고, 새 타일은 이 주소로 받는다)
                if (_urlTemplate == null && PlayerPrefs.HasKey(TEMPLATE_PREF_KEY))
                {
                    _urlTemplate = PlayerPrefs.GetString(TEMPLATE_PREF_KEY);
                }
                _templateChecked = true;
                if (_urlTemplate != null)
                {
                    yield break;
                }
                yield return new WaitForSecondsRealtime(TEMPLATE_RETRY_SECONDS);
            }
        }

        private static string ParseTemplate(string json)
        {
            try
            {
                TileJson tileJson = JsonUtility.FromJson<TileJson>(json);
                if (tileJson?.tiles != null && tileJson.tiles.Length > 0 && !string.IsNullOrEmpty(tileJson.tiles[0]))
                {
                    return tileJson.tiles[0];
                }
            }
            catch (ArgumentException)
            {
                // 형식이 틀린 응답은 실패로 본다
            }
            return null;
        }

        // xMax/yMax를 포함하는 타일 범위
        private RectInt GetVisibleTileRange()
        {
            GameConfig config = _game.Config;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            // 지도가 회전해도 화면을 덮도록 화면 대각선 절반(radius)만큼 사방으로 넓힌 정사각형 범위를 받는다
            float radius = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
            Vector2 center = _camera.transform.position;

            GeoUtil.FromLocalMeters(center + new Vector2(-radius, radius),
                config.originLatitude, config.originLongitude, out double northLat, out double westLon);
            GeoUtil.FromLocalMeters(center + new Vector2(radius, -radius),
                config.originLatitude, config.originLongitude, out double southLat, out double eastLon);

            int xMin = GeoUtil.LonToTileX(westLon, TILE_ZOOM);
            int xMax = GeoUtil.LonToTileX(eastLon, TILE_ZOOM);
            int yMin = GeoUtil.LatToTileY(northLat, TILE_ZOOM);
            int yMax = GeoUtil.LatToTileY(southLat, TILE_ZOOM);
            return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private void RequestTiles(RectInt range)
        {
            for (int x = range.xMin; x <= range.xMax; x++)
            {
                for (int y = range.yMin; y <= range.yMax; y++)
                {
                    long key = TileKey(x, y);
                    if (_loaded.ContainsKey(key) || _loading.Contains(key)
                        || (_retryAt.TryGetValue(key, out float retryAt) && retryAt > Time.unscaledTime))
                    {
                        continue;
                    }
                    if (_loading.Count >= MAX_PARALLEL_LOADS)
                    {
                        return;
                    }
                    _loading.Add(key);
                    StartCoroutine(LoadTile(x, y, key));
                }
            }
        }

        // 보이는 범위보다 사방 1타일 넓게 남겨 두고 나머지는 지운다
        private void EvictTiles(RectInt range)
        {
            _evictKeys.Clear();
            foreach (long key in _loaded.Keys)
            {
                if (!IsInKeepRange(key, range))
                {
                    _evictKeys.Add(key);
                }
            }
            foreach (long key in _evictKeys)
            {
                DestroyTile(key);
            }
        }

        private static bool IsInKeepRange(long key, RectInt range)
        {
            int x = (int)(key >> 32);
            int y = (int)(key & 0xffffffff);
            return x >= range.xMin - 1 && x <= range.xMax + 1 && y >= range.yMin - 1 && y <= range.yMax + 1;
        }

        private static long TileKey(int x, int y)
        {
            return ((long)x << 32) | (uint)y;
        }

        private IEnumerator LoadTile(int x, int y, long key)
        {
            string path = Path.Combine(Application.persistentDataPath, CACHE_FOLDER, $"{TILE_ZOOM}_{x}_{y}.pbf");
            if (!IsCacheFresh(path) && _urlTemplate != null)
            {
                yield return DownloadToCache(x, y, path);
            }
            // 다운로드에 실패해도 오래된 캐시가 있으면 그것을 쓴다
            if (!File.Exists(path))
            {
                FailTile(key);
                yield break;
            }
            yield return BuildTile(x, y, key, File.ReadAllBytes(path));
        }

        private IEnumerator BuildTile(int x, int y, long key, byte[] bytes)
        {
            GameConfig config = _game.Config;
            double originLat = config.originLatitude;
            double originLon = config.originLongitude;
            // 점 수만 개를 메인 스레드에서 해석·삼각분할하면 프레임이 끊긴다
            Task<MapMeshData[]> task = Task.Run(() => VectorTileMeshBuilder.Build(bytes, TILE_ZOOM, x, y, originLat, originLon));
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                Debug.LogWarning($"벡터 지도 타일 만들기 실패 ({TILE_ZOOM}/{x}/{y}): {task.Exception.GetBaseException().Message}");
                FailTile(key);
                yield break;
            }
            _loading.Remove(key);
            _retryAt.Remove(key);
            // 만드는 동안 화면에서 멀어졌으면 버린다
            if (!IsInKeepRange(key, GetVisibleTileRange()))
            {
                yield break;
            }
            _loaded[key] = CreateTileObject(x, y, task.Result);
        }

        private static bool IsCacheFresh(string path)
        {
            return File.Exists(path) && (DateTime.Now - File.GetLastWriteTime(path)).TotalDays < CACHE_DAYS;
        }

        private IEnumerator DownloadToCache(int x, int y, string path)
        {
            string url = _urlTemplate.Replace("{z}", TILE_ZOOM.ToString()).Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = REQUEST_TIMEOUT_SECONDS;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, request.downloadHandler.data);
            }
            else
            {
                Debug.LogWarning($"벡터 지도 타일 다운로드 실패 ({TILE_ZOOM}/{x}/{y}): {request.error}");
            }
        }

        private void FailTile(long key)
        {
            _loading.Remove(key);
            _retryAt[key] = Time.unscaledTime + TILE_RETRY_SECONDS;
            _failedCount++;
        }

        private GameObject CreateTileObject(int x, int y, MapMeshData[] meshes)
        {
            var root = new GameObject($"Tile_{x}_{y}");
            root.transform.SetParent(transform, false);
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].Vertices.Count > 0)
                {
                    CreateSlotObject(root.transform, (MapSlot)i, x, y, meshes[i]);
                }
            }
            return root;
        }

        private void CreateSlotObject(Transform root, MapSlot slot, int x, int y, MapMeshData data)
        {
            var mesh = new Mesh
            {
                name = $"{slot}_{x}_{y}",
                // UInt16 인덱스는 정점 65535개까지다
                indexFormat = data.Vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(data.Vertices);
            mesh.SetTriangles(data.Triangles, 0);

            var child = new GameObject(slot.ToString());
            child.transform.SetParent(root, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer meshRenderer = child.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _materials[(int)slot];
            meshRenderer.sortingOrder = VectorMapStyle.GetSortingOrder(slot);
        }

        private void DestroyTile(long key)
        {
            GameObject root = _loaded[key];
            _loaded.Remove(key);
            // 씬을 내릴 때는 루트가 먼저 파괴되어 있을 수 있다
            if (root == null)
            {
                return;
            }
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Destroy(filter.sharedMesh);
            }
            Destroy(root);
        }
    }
}
