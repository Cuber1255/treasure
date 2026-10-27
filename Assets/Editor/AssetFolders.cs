using UnityEditor;

namespace Treasure.EditorTools
{
    internal static class AssetFolders
    {
        // "Assets/A/B" 형태의 폴더를 AssetDatabase에 차례로 만든다 (.meta는 Unity가 생성)
        public static void Ensure(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }
            int slash = folderPath.LastIndexOf('/');
            string parent = folderPath.Substring(0, slash);
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, folderPath.Substring(slash + 1));
        }

        public static void EnsureParentOf(string assetPath)
        {
            Ensure(assetPath.Substring(0, assetPath.LastIndexOf('/')));
        }
    }
}
