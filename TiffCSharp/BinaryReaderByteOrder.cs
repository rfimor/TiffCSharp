using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TiffCSharp

{
    public class BinaryReaderByteOrder : BinaryReader
    {
        public bool IsLittleEndian { set; get; }
        byte[] cache = new byte[32];

        public BinaryReaderByteOrder(Stream stream, bool isLittleEndian = true) : base(stream) {
            IsLittleEndian = isLittleEndian;
        }

        public override UInt16 ReadUInt16() {
            if (IsLittleEndian) return base.ReadUInt16();
            byte[] data = base.ReadBytes(2);
            cache[0] = data[1];
            cache[1] = data[0];
            return BitConverter.ToUInt16(cache, 0);
        }

        public override UInt32 ReadUInt32() {
            if (IsLittleEndian) return base.ReadUInt32();
            byte[] data = base.ReadBytes(4);
            cache[0] = data[3];
            cache[1] = data[2];
            cache[2] = data[1];
            cache[3] = data[0];
            return BitConverter.ToUInt32(cache, 0);
        }

        public override Int16 ReadInt16() {
            if (IsLittleEndian) return base.ReadInt16();
            byte[] data = base.ReadBytes(2);
            cache[0] = data[1];
            cache[1] = data[0];
            return BitConverter.ToInt16(cache, 0);
        }

        public override int ReadInt32() {
            if (IsLittleEndian) return base.ReadInt32();
            byte[] data = base.ReadBytes(4);
            cache[0] = data[3];
            cache[1] = data[2];
            cache[2] = data[1];
            cache[3] = data[0];
            return BitConverter.ToInt32(cache, 0);
        }

        public override float ReadSingle() {
            if (IsLittleEndian) return base.ReadSingle();
            byte[] data = base.ReadBytes(4);
            cache[0] = data[3];
            cache[1] = data[2];
            cache[2] = data[1];
            cache[3] = data[0];
            return BitConverter.ToSingle(cache, 0);
        }

        public override double ReadDouble() {
            if (IsLittleEndian) return base.ReadDouble();
            byte[] data = base.ReadBytes(8);
            cache[0] = data[7];
            cache[1] = data[6];
            cache[2] = data[5];
            cache[3] = data[4];
            cache[4] = data[3];
            cache[5] = data[2];
            cache[6] = data[1];
            cache[7] = data[0];
            return BitConverter.ToDouble(cache, 0);
        }

    }
}
