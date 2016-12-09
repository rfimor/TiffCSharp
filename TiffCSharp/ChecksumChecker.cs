using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffCSharp

{
    public abstract class ChecksumChecker
    {
        abstract public uint ComputeChecksum(byte[] data, int startOffset, int length);

        public uint SwitchByteOrder(uint x) {
            return ((x & 0xFF000000) >> 24) + ((x & 0xFF0000) >> 8) + ((x & 0xFF00) << 8) + ((x & 0xFF) << 24);
        }
    }
}
