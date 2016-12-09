/*

	This is a simple implementation of the well-known LZW algorithm. 
    Copyright (C) 2011  Stamen Petrov <stamen.petrov@gmail.com>

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

*/


using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;

namespace SharpLZW
{
    public class LZWEncoder : LZW
    {
        protected List<byte> codeList;
        protected byte tail;
        protected int tailBits;

        public byte[] Encode(byte[] input, int offset, int length)
        {
            codeList = new List<byte>(input.Length);
            tail = 0;
            tailBits = 0;

            initTable();
            addCode(CLEAR_CODE);

            StringBuilder wBuilder = new StringBuilder();
            string w = "";

            bool contained = true;

            for (int i = offset; i < offset + length; i++)
            {
                if (!contained) {
                    wBuilder.Clear();
                    wBuilder.Append(w);
                }

                char next = (char)input[i];

                wBuilder.Append(next);
                string k = wBuilder.ToString();
                contained = dict.ContainsKey(k);

                if (contained) {
                    w = k;
                } else {
                    addCode(dict[w]);
                    dict.Add(k, Convert.ToUInt16(dict.Count));
                    w = next.ToString();
                    if (dict.Count == codeSize - 1 && codeSize == MAX_TAB_LENGTH) {
                        addCode(CLEAR_CODE);
                        initTable();
                    } 
                    else if (dict.Count == codeSize) {
                        codeLen++;
                        codeSize *= 2;
                    }
                }
            }

            addCode(dict[w]);
            addCode(END_OF_STREAM);
            codeList.Add(tail);

            return codeList.ToArray();
        }

        /// <summary>
        /// Add a code. For speed concerns, this function doesn't check if the to-be-added code is greater than MAX_TAB_LENGTH.
        /// </summary>
        /// <param name="code">code to be added</param>
        protected void addCode(ushort code) {
            unchecked {
                int resbit = codeLen - 8 + tailBits;
                int shiftDiv = 1 << resbit;
                byte high = (byte)(code / shiftDiv);
                int low = code % shiftDiv;

                tail += high;
                codeList.Add(tail);

                tail = 0;
                if (resbit <= 8) {
                    tailBits = resbit;
                    tail = (byte)(low << (8 - resbit));
                } else {
                    tailBits = resbit - 8;
                    shiftDiv = 1 << tailBits;
                    codeList.Add((byte)(low / shiftDiv));
                    tail = (byte)((low % shiftDiv) << (8 - tailBits));
                }
            }
        }
    }
}
