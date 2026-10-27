using System.Collections.Generic;

namespace Treasure
{
    // 벡터 타일 레이어 하나 (키·값 목록은 피처 태그가 인덱스로 가리킨다)
    public class MvtLayer
    {
        public string Name;
        public int Extent = 4096;
        public readonly List<string> Keys = new();
        public readonly List<string> Values = new();
        public readonly List<MvtFeature> Features = new();

        // 태그 값. 없으면 null
        public string GetTag(MvtFeature feature, string key)
        {
            uint[] tags = feature.Tags;
            for (int i = 0; i + 1 < tags.Length; i += 2)
            {
                uint keyIndex = tags[i];
                if (keyIndex >= Keys.Count || Keys[(int)keyIndex] != key)
                {
                    continue;
                }
                uint valueIndex = tags[i + 1];
                return valueIndex < Values.Count ? Values[(int)valueIndex] : null;
            }
            return null;
        }
    }

    // 벡터 타일 피처 하나 (태그와 도형 명령)
    public class MvtFeature
    {
        public const int TYPE_LINE = 2;
        public const int TYPE_POLYGON = 3;

        public int Type;
        // (키 인덱스, 값 인덱스) 쌍
        public uint[] Tags = new uint[0];
        // 명령 정수 배열
        public uint[] Geometry = new uint[0];
    }
}
