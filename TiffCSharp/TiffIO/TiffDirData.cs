using System;
using System.Collections.Generic;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    class TiffDirData : IComparable
    {
        protected ushort tag;
        protected long offset;
        protected TiffData data;

        public TiffData Data { get { return data; } set { data = value; } }
        public ushort Tag { get { return tag; } set { tag = value; } }
        public long Offset { get { return offset; } }

        public void read(System.IO.BinaryReader reader) {
            tag = reader.ReadUInt16();
            int typeID = reader.ReadUInt16();
            int count = reader.ReadInt32();

            TiffData.TIFFdataType tifType;
            if (Enum.IsDefined(typeof(TiffData.TIFFdataType), typeID)) tifType = (TiffData.TIFFdataType)typeID;
            else tifType = TiffData.TIFFdataType.Undefined;

            data = new TiffData(tifType, count);

            if (data.dataLength() <= 4) {
                readData(reader);
                offset = -1;
            } else {
                offset = reader.ReadUInt32();
            }
        }

        public void write(System.IO.BinaryWriter writer, ref uint currPos) {
            writer.Write(Convert.ToUInt16(tag));
            writer.Write(Convert.ToUInt16(data.TiffType));
            writer.Write(data.Count);

            uint length = (uint)(data.dataLength());
            if (length <= 4) {
                data.write(writer);
                for (int i = 0; i < 4 - (int)length; i++) writer.Write((byte)0);
            } else {
                writer.Write(currPos);
                currPos += length;
            }
        }

        public void readData(System.IO.BinaryReader reader) {
            if (data == null) return;
            data.read(reader);
        }

        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (obj is TiffDirData) return tag.CompareTo(((TiffDirData)obj).tag);
            if (obj is ushort) {
                return tag.CompareTo(obj);
            }
            if (obj is int) {
                int r = (int)obj;
                if (r > ushort.MaxValue) return -1;
                if (r < ushort.MinValue) return 1;
                return tag.CompareTo(Convert.ToUInt16(obj));
            }
            if (obj is uint) {
                uint r = (uint)obj;
                if (r > ushort.MaxValue) return -1;
                if (r < ushort.MinValue) return 1;
                return tag.CompareTo(Convert.ToUInt16(obj));
            }
            if (obj is short || obj is byte || obj is sbyte) {
                return tag.CompareTo(Convert.ToUInt16(obj));
            }
            throw new ArgumentException("object is not valid to compare");
        }
    }
}
