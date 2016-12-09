using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using SharpLZW;
using TiffCSharp.TiffIO;

namespace TiffCSharp
{
    public class MyTiffCompression
    {
        public enum CompressionMethod
        {
            UNCOMPRESSED = 1,
            CCITT1D = 2, //CCITT modified Huffman RLE
            CCITTFAX3 = 3,
            CCITTFAX4 = 4,
            LZW = 5,
            OLDJPEG = 6,
            JPEG = 7,
            ADOBE_DEFLATE = 8, //Adobe Deflate
            CCITTRLEW = 32771, //CCITT RLE
            PACKBITS = 32773,
            DEFLATE = 32946, //Deflate
            JP2000 = 34712
        }

        public enum DifferencePredictor
        {
            None = 1,
            Horizontal = 2
        }
        
        public static ushort getCompressionTag(TiffInfoCollection info) {
            try {
                ushort[] temp = info.getOneInfoData(TiffInfoCollection.Compression) as ushort[];
                return temp[0];
            } catch {
                return 1;
            }
        }

        public static DifferencePredictor getDifferencingPredictor(TiffInfoCollection info) {
            try {
                ushort[] temp = info.getOneInfoData(TiffInfoCollection.DifferencingPredictor) as ushort[];
                return (DifferencePredictor)(temp[0]);
            } catch {
                return DifferencePredictor.None;
            }
        }

        public static void setHorizontalDifferencing(TiffInfoCollection info, bool isHorizontalDifference) {
            if (isHorizontalDifference) info.forceAdd(new TiffInfo(TiffInfoCollection.DifferencingPredictor, "Predictor", (ushort)2));
            else info.forceAdd(new TiffInfo(TiffInfoCollection.DifferencingPredictor, "Predictor", (ushort)1));
        }
        
        public static void setCompressionTag(TiffIO.TiffInfoCollection info, CompressionMethod compID, CompressionLevel compressLevel) {
            if (compressLevel == CompressionLevel.NoCompression) info.forceAdd(new TiffInfo(TiffInfoCollection.Compression, "Compression", (ushort)CompressionMethod.UNCOMPRESSED));
            else info.forceAdd(new TiffInfo(TiffInfoCollection.Compression, "Compression", Convert.ToUInt16(compID)));
        }

        public static byte[] decompress(byte[] compressed, CompressionMethod compId, int expectedByteCount) {
            switch (compId) {
                case CompressionMethod.UNCOMPRESSED:
                    return compressed;
                case CompressionMethod.LZW:
                    var lzwDecoder = new LZWDecoder();
                    string decompressed = lzwDecoder.Decode(compressed);
                    byte[] bytes = new byte[expectedByteCount];
                    for (int i = 0; i < bytes.Length; i++) {
                        bytes[i] = (byte)decompressed[i];
                    }
                    return bytes;
                case CompressionMethod.DEFLATE:
                case CompressionMethod.ADOBE_DEFLATE:
                    return deflateDecompress(compressed, expectedByteCount);
                case CompressionMethod.PACKBITS:
                    return packbitsDecompress(compressed, expectedByteCount);
                default:
                    throw new ReadFileException("Unsupported comnpression");
            }
        }

        public static int compress(byte[] origData, int offset, int length, BinaryWriter writer, CompressionMethod compId, CompressionLevel compressLevel) {
            if (compressLevel == CompressionLevel.NoCompression) {
                writer.Write(origData);
                return origData.Length;
            }

            switch (compId) {
                case CompressionMethod.UNCOMPRESSED:
                    writer.Write(origData);
                    return origData.Length;
                case CompressionMethod.LZW:
                    var lzwEncoder = new LZWEncoder();
                    byte[] compressed = lzwEncoder.Encode(origData, offset, length);
                    writer.Write(compressed);
                    return compressed.Length;
                case CompressionMethod.DEFLATE:
                case CompressionMethod.ADOBE_DEFLATE:
                    writer.Write(deflateHeader(compressLevel));
                    compressed = deflateCompress(origData, offset, length, compressLevel);
                    writer.Write(compressed);
                    var crcChecker = new Adler32();
                    writer.Write(crcChecker.SwitchByteOrder(crcChecker.ComputeChecksum(origData, offset, length)));
                    return compressed.Length + 6;
                default:
                    throw new ReadFileException("Unsupported comnpression");
            }
        }

        public static byte[] deflateDecompress(byte[] compressed, int expectedByteCount, bool hasHeader = true) {
            byte[] dfArray = new byte[expectedByteCount];
            using (MemoryStream cs = new MemoryStream(compressed)) {
                if (hasHeader) cs.Seek(2, SeekOrigin.Begin);
                else cs.Seek(0, SeekOrigin.Begin);
                using (DeflateStream dfStream = new DeflateStream(cs, CompressionMode.Decompress)) {
                    dfStream.Read(dfArray, 0, expectedByteCount);
                    return dfArray;
                }
            }
        }

        public static byte[] deflateCompress(byte[] orig, int offset, int length, CompressionLevel compressLevel) {
            using (MemoryStream os = new MemoryStream(orig, offset, length)) {
                os.Seek(0, SeekOrigin.Begin);
                using (MemoryStream ist = new MemoryStream()) {
                    using (DeflateStream dfStream = new DeflateStream(ist, compressLevel)) {
                        os.CopyTo(dfStream);
                        dfStream.Close();
                        return ist.ToArray();
                    }
                }
            }
        }

        public static byte[] packbitsDecompress(byte[] compressed, int expectedByteCount) {
            byte[] output = new byte[expectedByteCount];
            int curr = 0;
            int cnt = 0;
            while (cnt < expectedByteCount) {
                byte code = compressed[curr++];
                int header = (sbyte)code;
                if (header >= 0) {
                    header++;
                    if (header > expectedByteCount - cnt) {
                        header = expectedByteCount - cnt;
                    }
                    Buffer.BlockCopy(compressed, curr, output, cnt, header);
                    curr += header;
                    cnt += header;
                } else if (header < 0 && header > -128) {
                    header = 1 - header;
                    byte next = compressed[curr++];
                    for (int i = 0; i < header && cnt < expectedByteCount; i++) {
                        output[cnt++] = next;
                    }
                }
            }
            return output;
        }

        public static ushort deflateHeader(CompressionLevel level) {
            switch (level) {
                case CompressionLevel.Fastest:
                    return (ushort)0x1C78;
                case CompressionLevel.Optimal:
                    return (ushort)0x9C78;
                default:
                    return (ushort)0x0178;
            }
        }
    }
}
