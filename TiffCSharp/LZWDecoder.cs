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

namespace SharpLZW
{
    public class LZWDecoder : LZW
    {
        public string Decode(byte[] output) {
            unchecked {
                StringBuilder sb = new StringBuilder();
                StringBuilder fkBuilder = new StringBuilder();

                int pos = 0;
                int bytePos = 0;
                int prevValue = -1;

                codeLen = 9;

                bool firstChar = false;
                int bitLength = output.Length * 8;

                byte[] word = new byte[2];

                int value = 0;

                while (pos < bitLength) {
                    if (pos + codeLen + 8 <= bitLength) {
                        int res = pos - 8 * bytePos;
                        int highMask = ((1 << res) - 1) << (8 - res);
                        int high = output[bytePos] & (0xFF - highMask);
                        int shift = codeLen - 8 + res;
                        high = high << shift;
                        if (shift < 8) {
                            value = high + (output[bytePos + 1] >> (8 - shift));
                        } else if (shift == 8) {
                            value = high + output[bytePos + 1];
                        } else {
                            int ush = shift - 8;
                            value = high + (output[bytePos + 1] << ush) + (output[bytePos + 2] >> (8 - ush));
                        }
                    }
                        /*
                    else if (i + codeLen <= output.Length)
                    {
                        int encodedLen = i + codeLen;
                        int trimBitsLen = output.Length - encodedLen;

                        w = output.Substring(i, codeLen - (8 - trimBitsLen)) + output.Substring(output.Length - (8 - trimBitsLen), (8 - trimBitsLen));
                    }
                         * */
                    else {
                        break;
                    }

                    pos += codeLen;
                    bytePos = pos / 8;

                    string key, prevKey;

                    if (value == CLEAR_CODE) {
                        firstChar = true;
                        initTable();
                        continue;
                    } else if (value == END_OF_STREAM) {
                        return sb.ToString();
                    }

                    key = value < table.Count ? table[value] : "";
                    if (firstChar) {
                        prevValue = value;
                        prevKey = key;
                        sb.Append(key);
                        firstChar = false;
                        continue;
                    } else {
                        prevKey = prevValue < table.Count ? table[prevValue] : "";
                    }

                    string finalKey;
                    fkBuilder.Clear();
                    fkBuilder.Append(prevKey);

                    if (string.IsNullOrEmpty(key)) {
                        //handles the situation cScSc
                        fkBuilder.Append(prevKey[0]);
                        finalKey = fkBuilder.ToString();
                        sb.Append(finalKey);
                    } else {
                        sb.Append(key);
                        fkBuilder.Append(key[0]);
                        finalKey = fkBuilder.ToString();
                    }

                    if (!dict.ContainsKey(finalKey)) {
                        table.Add(finalKey);
                        dict.Add(finalKey, Convert.ToUInt16(dict.Count));
                    }

                    if (table.Count == codeSize - 1 && table.Count < MAX_TAB_LENGTH) {
                        codeLen++;
                        codeSize *= 2;
                    }

                    prevValue = value;
                }

                return sb.ToString();
            }
        }
    }
}