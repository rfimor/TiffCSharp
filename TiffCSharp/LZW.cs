using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLZW
{
    public class LZW
    {
        protected const ushort CLEAR_CODE = 256;
        protected const ushort END_OF_STREAM = 257;

        protected const int MAX_TAB_LENGTH = 4096;

        protected List<string> table = new List<string>(MAX_TAB_LENGTH);
        protected Dictionary<string, ushort> dict = new Dictionary<string, ushort>(MAX_TAB_LENGTH);

        protected int codeLen;
        protected int codeSize;

        protected void initTable() {
            codeLen = 9;
            codeSize = 512;
            table.Clear();
            dict.Clear();
            for (ushort i = 0; i < 258; i++) {
                table.Add(new string((char)i, 1));
                dict.Add(table[i], i);
            }
        }
    }
}
