using UnityEngine;

namespace Treasure
{
    // 모바일 테스트용 디버그 설정. 앱을 다시 켜도 유지되도록 PlayerPrefs에 저장한다
    public static class DebugSettings
    {
        public const string SIMULATED_LOCATION_KEY = "debug_simulated_location";
        public const string HEADING_UP_KEY = "debug_heading_up";

        // 에디터에는 GPS가 없으므로 항상 시뮬레이션이다
        public static bool UseSimulatedLocation
        {
            get => Application.isEditor || PlayerPrefs.GetInt(SIMULATED_LOCATION_KEY, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(SIMULATED_LOCATION_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        // true면 지도가 휴대폰 방향을 따라 회전한다
        public static bool HeadingUp
        {
            get => PlayerPrefs.GetInt(HEADING_UP_KEY, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(HEADING_UP_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
