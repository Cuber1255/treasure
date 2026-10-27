using UnityEngine;

namespace Treasure.EditorTools
{
    // 빌더가 처음 데이터 에셋을 만들 때 쓰는 기본 콘텐츠 표
    // 에셋이 생긴 뒤에는 에셋 쪽 값을 수정한다 (빌더는 기존 에셋을 덮어쓰지 않는다)
    internal static class GameContent
    {
        // 동성로 광장 (지도 원점)
        public const double ORIGIN_LAT = 35.8697;
        public const double ORIGIN_LON = 128.5955;
        // 퍼즐 그림에 담는 범위: 원점 기준 ±800m 정사각형
        public const float PUZZLE_HALF_SIZE_METERS = 800f;
        public const float CAPTURE_RADIUS_METERS = 40f;

        internal struct SpotEntry
        {
            public string id;
            public string name;
            public double lat;
            public double lon;
            public int[] pieces;
            public string description;
            public bool isFinal;

            public SpotEntry(string id, string name, double lat, double lon, int[] pieces, string description, bool isFinal = false)
            {
                this.id = id;
                this.name = name;
                this.lat = lat;
                this.lon = lon;
                this.pieces = pieces;
                this.description = description;
                this.isFinal = isFinal;
            }
        }

        internal struct CharacterEntry
        {
            public string id;
            public string name;
            public string colorHex;
            public int taps;
            public float timeLimit;
            public float speed;
            public string story;

            public CharacterEntry(string id, string name, string colorHex, int taps, float timeLimit, float speed, string story)
            {
                this.id = id;
                this.name = name;
                this.colorHex = colorHex;
                this.taps = taps;
                this.timeLimit = timeLimit;
                this.speed = speed;
                this.story = story;
            }

            public Color Color
            {
                get
                {
                    ColorUtility.TryParseHtmlString(colorHex, out Color color);
                    return color;
                }
            }
        }

        internal struct ItemEntry
        {
            public string id;
            public string name;
            public int price;
            public ItemEffect effect;
            public float value;
            public float duration;
            public string colorHex;
            public string description;

            public ItemEntry(string id, string name, int price, ItemEffect effect, float value, float duration, string colorHex, string description)
            {
                this.id = id;
                this.name = name;
                this.price = price;
                this.effect = effect;
                this.value = value;
                this.duration = duration;
                this.colorHex = colorHex;
                this.description = description;
            }
        }

        // 좌표는 OSM/Nominatim 조회값 기준 근사치. 최종 스팟은 마지막
        public static readonly SpotEntry[] SPOTS =
        {
            new SpotEntry("Park228", "2·28 기념중앙공원", 35.86967, 128.59764, new[] { 0, 1 },
                "1960년 2·28 민주운동을 기념하는 공원. 동성로 한가운데에서 잠시 쉬어 가는 초록 쉼터다."),
            new SpotEntry("GukchaeBosangPark", "국채보상운동기념공원", 35.86870, 128.60196, new[] { 2, 3 },
                "1907년 시민들이 나랏빚을 갚자며 뜻을 모은 국채보상운동을 기념하는 공원. 달구벌대종이 있다."),
            new SpotEntry("JungangnoStation", "중앙로역", 35.87074, 128.59404, new[] { 4, 5 },
                "대구 도시철도 1호선 중앙로역. 동성로로 들어서는 북쪽 관문이다."),
            new SpotEntry("BanwoldangStation", "반월당역", 35.86551, 128.59341, new[] { 6, 7 },
                "도시철도 1·2호선이 만나는 환승역. 지하상가 메트로센터와 이어져 있다."),
            new SpotEntry("GyodongMarket", "교동시장", 35.87350, 128.59900, new[] { 8, 9 },
                "한국전쟁 이후 형성된 오래된 시장. 먹거리 골목과 귀금속 거리로 유명하다."),
            new SpotEntry("Yangnyeongsi", "대구약령시", 35.86830, 128.58952, new[] { 10, 11 },
                "360여 년 전통의 한약재 시장. 골목마다 약초 향이 가득하다."),
            new SpotEntry("SparkLand", "스파크랜드", 35.86870, 128.59875, new[] { 12, 13 },
                "건물 옥상에 대관람차가 있는 동성로의 랜드마크. 관람차에 오르면 동성로가 한눈에 보인다."),
            new SpotEntry("GyesanCathedral", "계산성당", 35.86794, 128.58784, new[] { 14, 15 },
                "1902년 세워진 고딕 양식 성당. 근대골목 투어의 중심지다."),
            new SpotEntry("DongseongnoSquare", "동성로 광장", 35.86970, 128.59550, new int[0],
                "옛 대구백화점 앞, 동성로의 한가운데. 버스킹과 약속의 장소다.", true),
        };

        // SPOTS와 같은 순서 (i번째 스팟의 캐릭터)
        public static readonly CharacterEntry[] CHARACTERS =
        {
            new CharacterEntry("Ipari", "이팔이", "#5BBF6A", 6, 15f, 350f,
                "공원 나무 그늘에서 낮잠 자는 걸 좋아하는 새싹 요정. 2·28의 함성을 기억하며 공원을 지킨다."),
            new CharacterEntry("Daejongi", "대종이", "#C9A227", 7, 15f, 400f,
                "달구벌대종 소리에 깨어난 종 모양 정령. 반짝이는 동전 모으기가 취미다."),
            new CharacterEntry("Chikchiki", "칙칙이", "#4A6FA5", 7, 15f, 450f,
                "지하철 소리를 흉내 내며 역 안을 쏘다니는 장난꾸러기. 열차 시간표를 전부 외우고 있다."),
            new CharacterEntry("Bandari", "반달이", "#F2C94C", 8, 15f, 450f,
                "반월당이라는 이름 속 반달에서 태어난 캐릭터. 밤이 깊어질수록 조금씩 둥글어진다."),
            new CharacterEntry("Napjagi", "납작이", "#E9A06B", 8, 15f, 500f,
                "대구 명물 납작만두를 닮은 먹보 캐릭터. 시장 골목 맛집을 전부 꿰고 있다."),
            new CharacterEntry("Yakchodori", "약초돌이", "#8D6E63", 8, 15f, 520f,
                "약초 바구니를 메고 다니는 꼬마 한의사. 지친 탐험가에게 몰래 쌍화차를 건넨다."),
            new CharacterEntry("Binggeuli", "빙글이", "#D65DB1", 9, 15f, 600f,
                "옥상 관람차를 타고 빙글빙글 도는 걸 멈추지 못하는 캐릭터. 높은 곳에서 동성로를 내려다본다."),
            new CharacterEntry("Stain", "스테인", "#7E57C2", 9, 15f, 600f,
                "성당 스테인드글라스에서 빠져나온 빛의 조각. 햇빛을 받으면 일곱 빛깔로 반짝인다."),
            new CharacterEntry("DongseongKing", "동성왕", "#E5484D", 15, 20f, 750f,
                "동성로 지도 퍼즐을 숨겨 둔 장본인. 모든 조각을 모은 탐험가 앞에만 나타난다."),
        };

        public static readonly ItemEntry[] ITEMS =
        {
            new ItemEntry("Hourglass", "모래시계", 100, ItemEffect.AddTime, 5f, 0f, "#4FC3F7", "포획 시간이 5초 늘어나요."),
            new ItemEntry("StickyTrap", "끈끈이", 120, ItemEffect.SlowDown, 0.4f, 5f, "#81C784", "5초 동안 캐릭터가 느려져요."),
            new ItemEntry("CaptureNet", "포획 그물", 200, ItemEffect.FillGauge, 0.5f, 0f, "#FFB74D", "포획 게이지를 절반 채워요."),
        };
    }
}
