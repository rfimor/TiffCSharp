using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffCSharp
{
    public class CRC32 : ChecksumChecker
    {
        uint[] table;

        public override uint ComputeChecksum(byte[] bytes, int startOffset, int length) {
            uint crc = 0xffffffff;
            for (int i = startOffset; i < startOffset + length; i++) {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes, int startOffset, int length) {
            return BitConverter.GetBytes(ComputeChecksum(bytes, startOffset, length));
        }

        public CRC32() {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; i++) {
                temp = i;
                for (int j = 8; j > 0; j--) {
                    if ((temp & 1) == 1) {
                        temp = (uint)((temp >> 1) ^ poly);
                    } else {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }

    }
}

