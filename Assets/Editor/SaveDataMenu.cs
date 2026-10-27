using UnityEditor;

namespace Treasure.EditorTools
{
    internal static class SaveDataMenu
    {
        [MenuItem("Treasure/저장 데이터 초기화")]
        private static void ResetSaveData()
        {
            // 플레이 중에는 GameManager가 곧바로 다시 저장하므로 막는다
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("저장 데이터 초기화", "플레이 모드를 종료한 뒤 실행하세요", "확인");
                return;
            }
            SaveSystem.Delete();
        }
    }
}
