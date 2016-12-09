using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffCSharp
{
    public class Adler32 : ChecksumChecker
    {
        uint checksum;

        public Adler32() {
            checksum = 1;
        }

        protected void HashCore(byte[] array, int ibStart, int cbSize) {
            int n;
            uint s1 = checksum & 0xFFFF;
            uint s2 = checksum >> 16;

            while (cbSize > 0) {
                n = (3800 > cbSize) ? cbSize : 3800;
                cbSize -= n;

                while (--n >= 0) {
                    s1 = s1 + (uint)(array[ibStart++] & 0xFF);
                    s2 = s2 + s1;
                }

                s1 %= 65521;
                s2 %= 65521;
            }

            checksum = (s2 << 16) | s1;

            /*
            uint unSum1 = checksum & 0xFFFF;
            uint unSum2 = (checksum >> 16) & 0xFFFF;

            for (int i = ibStart; i < cbSize; i++) {
                unSum1 = (unSum1 + array[i]) % 65521;
                unSum2 = (unSum1 + unSum2) % 65521;
            }

            checksum = (unSum2 << 16) + unSum1;
             * */
        }

        public override uint ComputeChecksum(byte[] array, int startOffset, int length) {
            HashCore(array, startOffset, length);
            return checksum;
        }
    }
}
