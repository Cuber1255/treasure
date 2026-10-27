using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

namespace Treasure
{
    // Mapbox 벡터 타일(.pbf)을 레이어·피처 목록으로 읽는다. 필요한 필드만 직접 해석한다
    public static class MvtReader
    {
        private const int CMD_MOVE_TO = 1;
        private const int CMD_LINE_TO = 2;
        private const int CMD_CLOSE_PATH = 7;

        public static List<MvtLayer> Read(byte[] data)
        {
            // 서버가 gzip으로 보낸 바이트를 그대로 저장했을 수 있다
            if (data.Length >= 2 && data[0] == 0x1f && data[1] == 0x8b)
            {
                data = Decompress(data);
            }

            var layers = new List<MvtLayer>();
            var reader = new ProtoReader(data, 0, data.Length);
            while (reader.HasMore)
            {
                reader.ReadKey(out int field, out int wireType);
                if (field == 3 && wireType == 2)
                {
                    layers.Add(ReadLayer(reader.ReadMessage()));
                }
                else
                {
                    reader.Skip(wireType);
                }
            }
            return layers;
        }

        // 명령 정수 배열을 부분(MoveTo로 시작하는 점 목록)들로 푼다. 링의 닫는 점은 넣지 않는다
        public static List<Vector2Int[]> DecodeGeometry(uint[] geometry)
        {
            var parts = new List<Vector2Int[]>();
            var current = new List<Vector2Int>();
            int x = 0;
            int y = 0;
            int i = 0;
            while (i < geometry.Length)
            {
                uint command = geometry[i++];
                int id = (int)(command & 7);
                int count = (int)(command >> 3);
                if (id == CMD_CLOSE_PATH)
                {
                    continue;
                }
                if (id != CMD_MOVE_TO && id != CMD_LINE_TO)
                {
                    break;
                }
                for (int n = 0; n < count; n++)
                {
                    if (i + 1 >= geometry.Length)
                    {
                        // 인자가 모자란 잘린 도형
                        i = geometry.Length;
                        break;
                    }
                    x += ZigZag(geometry[i++]);
                    y += ZigZag(geometry[i++]);
                    if (id == CMD_MOVE_TO && current.Count > 0)
                    {
                        parts.Add(current.ToArray());
                        current.Clear();
                    }
                    current.Add(new Vector2Int(x, y));
                }
            }
            if (current.Count > 0)
            {
                parts.Add(current.ToArray());
            }
            return parts;
        }

        private static int ZigZag(uint value)
        {
            return (int)(value >> 1) ^ -(int)(value & 1);
        }

        private static byte[] Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }

        private static MvtLayer ReadLayer(ProtoReader reader)
        {
            var layer = new MvtLayer();
            while (reader.HasMore)
            {
                reader.ReadKey(out int field, out int wireType);
                switch (field)
                {
                    case 1: layer.Name = reader.ReadString(); break;
                    case 2: layer.Features.Add(ReadFeature(reader.ReadMessage())); break;
                    case 3: layer.Keys.Add(reader.ReadString()); break;
                    case 4: layer.Values.Add(ReadValue(reader.ReadMessage())); break;
                    case 5: layer.Extent = (int)reader.ReadVarint(); break;
                    default: reader.Skip(wireType); break;
                }
            }
            return layer;
        }

        private static MvtFeature ReadFeature(ProtoReader reader)
        {
            var feature = new MvtFeature();
            while (reader.HasMore)
            {
                reader.ReadKey(out int field, out int wireType);
                switch (field)
                {
                    case 2: feature.Tags = reader.ReadPackedUInt32(); break;
                    case 3: feature.Type = (int)reader.ReadVarint(); break;
                    case 4: feature.Geometry = reader.ReadPackedUInt32(); break;
                    default: reader.Skip(wireType); break;
                }
            }
            return feature;
        }

        // 값 메시지의 종류와 상관없이 문자열로 돌려준다 (분류에는 문자열 비교만 쓴다)
        private static string ReadValue(ProtoReader reader)
        {
            string value = "";
            while (reader.HasMore)
            {
                reader.ReadKey(out int field, out int wireType);
                switch (field)
                {
                    case 1: value = reader.ReadString(); break;
                    case 2: value = reader.ReadFloat().ToString(CultureInfo.InvariantCulture); break;
                    case 3: value = reader.ReadDouble().ToString(CultureInfo.InvariantCulture); break;
                    case 4: value = ((long)reader.ReadVarint()).ToString(CultureInfo.InvariantCulture); break;
                    case 5: value = reader.ReadVarint().ToString(CultureInfo.InvariantCulture); break;
                    case 6: value = DecodeZigZag64(reader.ReadVarint()).ToString(CultureInfo.InvariantCulture); break;
                    case 7: value = reader.ReadVarint() != 0 ? "true" : "false"; break;
                    default: reader.Skip(wireType); break;
                }
            }
            return value;
        }

        private static long DecodeZigZag64(ulong value)
        {
            return (long)(value >> 1) ^ -(long)(value & 1);
        }

        // protobuf 바이트 범위 하나를 앞에서부터 읽는다
        private class ProtoReader
        {
            private readonly byte[] _data;
            public int Position;
            public readonly int End;

            public ProtoReader(byte[] data, int start, int end)
            {
                _data = data;
                Position = start;
                End = end;
            }

            public bool HasMore => Position < End;

            public void ReadKey(out int field, out int wireType)
            {
                ulong key = ReadVarint();
                field = (int)(key >> 3);
                wireType = (int)(key & 7);
            }

            public ulong ReadVarint()
            {
                ulong result = 0;
                int shift = 0;
                while (true)
                {
                    if (Position >= End || shift > 63)
                    {
                        throw new InvalidDataException("varint가 잘렸습니다");
                    }
                    byte b = _data[Position++];
                    result |= (ulong)(b & 0x7f) << shift;
                    if ((b & 0x80) == 0)
                    {
                        return result;
                    }
                    shift += 7;
                }
            }

            // 길이 구분 필드를 하위 리더로 떼어 내고 그만큼 앞으로 간다
            public ProtoReader ReadMessage()
            {
                int length = ReadLength();
                var sub = new ProtoReader(_data, Position, Position + length);
                Position += length;
                return sub;
            }

            public string ReadString()
            {
                int length = ReadLength();
                string value = Encoding.UTF8.GetString(_data, Position, length);
                Position += length;
                return value;
            }

            public uint[] ReadPackedUInt32()
            {
                ProtoReader packed = ReadMessage();
                var values = new List<uint>();
                while (packed.HasMore)
                {
                    values.Add((uint)packed.ReadVarint());
                }
                return values.ToArray();
            }

            public float ReadFloat()
            {
                Require(4);
                float value = BitConverter.ToSingle(ReadLittleEndian(4), 0);
                return value;
            }

            public double ReadDouble()
            {
                Require(8);
                return BitConverter.ToDouble(ReadLittleEndian(8), 0);
            }

            public void Skip(int wireType)
            {
                switch (wireType)
                {
                    case 0: ReadVarint(); break;
                    case 1: Require(8); Position += 8; break;
                    case 2: int length = ReadLength(); Position += length; break;
                    case 5: Require(4); Position += 4; break;
                    default: throw new InvalidDataException($"알 수 없는 wire type {wireType}");
                }
            }

            private int ReadLength()
            {
                ulong length = ReadVarint();
                if (length > (ulong)(End - Position))
                {
                    throw new InvalidDataException("길이 구분 필드가 범위를 넘습니다");
                }
                return (int)length;
            }

            private void Require(int count)
            {
                if (End - Position < count)
                {
                    throw new InvalidDataException("고정 길이 필드가 잘렸습니다");
                }
            }

            // protobuf 고정 길이 숫자는 리틀엔디언이다
            private byte[] ReadLittleEndian(int count)
            {
                var bytes = new byte[count];
                Array.Copy(_data, Position, bytes, 0, count);
                Position += count;
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
                return bytes;
            }
        }
    }
}
