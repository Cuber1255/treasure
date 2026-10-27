using System.Collections.Generic;
using System.Text;

namespace Treasure.Tests
{
    // 테스트용 벡터 타일(.pbf) 바이트를 만든다
    internal static class MvtTestWriter
    {
        private const int WIRE_VARINT = 0;
        private const int WIRE_LENGTH = 2;

        public static uint Command(int id, int count) => (uint)((count << 3) | (id & 7));

        public static uint ZigZag(int v) => (uint)((v << 1) ^ (v >> 31));

        public static byte[] Tile(params byte[][] layers)
        {
            var buffer = new List<byte>();
            foreach (byte[] layer in layers)
            {
                WriteBytes(buffer, 3, layer);
            }
            return buffer.ToArray();
        }

        public static byte[] Layer(string name, string[] keys, string[] values, params byte[][] features)
        {
            var buffer = new List<byte>();
            WriteKey(buffer, 15, WIRE_VARINT);
            WriteVarint(buffer, 2);
            WriteBytes(buffer, 1, Encoding.UTF8.GetBytes(name));
            foreach (byte[] feature in features)
            {
                WriteBytes(buffer, 2, feature);
            }
            foreach (string key in keys)
            {
                WriteBytes(buffer, 3, Encoding.UTF8.GetBytes(key));
            }
            foreach (string value in values)
            {
                var valueMessage = new List<byte>();
                WriteBytes(valueMessage, 1, Encoding.UTF8.GetBytes(value));
                WriteBytes(buffer, 4, valueMessage.ToArray());
            }
            WriteKey(buffer, 5, WIRE_VARINT);
            WriteVarint(buffer, 4096);
            return buffer.ToArray();
        }

        public static byte[] Feature(int type, uint[] tags, uint[] geometry)
        {
            var buffer = new List<byte>();
            if (tags.Length > 0)
            {
                WritePacked(buffer, 2, tags);
            }
            WriteKey(buffer, 3, WIRE_VARINT);
            WriteVarint(buffer, (ulong)type);
            WritePacked(buffer, 4, geometry);
            return buffer.ToArray();
        }

        // (x, y)에서 시작하는 한 변 size인 정사각형 링 (타일 좌표에서 양의 면적)
        public static uint[] Square(int x, int y, int size)
        {
            return new[]
            {
                Command(1, 1), ZigZag(x), ZigZag(y),
                Command(2, 3), ZigZag(size), ZigZag(0), ZigZag(0), ZigZag(size), ZigZag(-size), ZigZag(0),
                Command(7, 1)
            };
        }

        public static uint[] Line(int x0, int y0, int x1, int y1)
        {
            return new[]
            {
                Command(1, 1), ZigZag(x0), ZigZag(y0),
                Command(2, 1), ZigZag(x1 - x0), ZigZag(y1 - y0)
            };
        }

        private static void WriteVarint(List<byte> buffer, ulong value)
        {
            while (value >= 0x80)
            {
                buffer.Add((byte)(value | 0x80));
                value >>= 7;
            }
            buffer.Add((byte)value);
        }

        private static void WriteKey(List<byte> buffer, int field, int wireType)
        {
            WriteVarint(buffer, (ulong)((field << 3) | wireType));
        }

        private static void WriteBytes(List<byte> buffer, int field, byte[] bytes)
        {
            WriteKey(buffer, field, WIRE_LENGTH);
            WriteVarint(buffer, (ulong)bytes.Length);
            buffer.AddRange(bytes);
        }

        private static void WritePacked(List<byte> buffer, int field, uint[] values)
        {
            var packed = new List<byte>();
            foreach (uint value in values)
            {
                WriteVarint(packed, value);
            }
            WriteBytes(buffer, field, packed.ToArray());
        }
    }
}
