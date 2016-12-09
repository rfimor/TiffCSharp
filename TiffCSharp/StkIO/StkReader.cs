using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;
using TiffCSharp.TiffIO;

namespace TiffCSharp.StkIO
{
    /// <summary>
    /// Derived from TIFFreader. Can read multiple images (with same size, depth, etc) as planes.
    /// </summary>
    public class StkReader : TiffReader
    {
        public StkReader() : base() { }

        #region fields

        protected int nbits;
        protected bool isFloat;
        protected uint[] stripPos;
        protected uint[] stripCount;
        protected uint rowCount;
        protected MyTiffCompression.CompressionMethod compid;
        protected int width;
        protected int height;
        protected int nPlane;
        protected int pmi;
        protected long spaceStrip;
        protected bool isPrepared;
        protected uint totBytes;
        protected int planarConfig;
        protected bool diffTag;

        #endregion

        protected void prepareReading() {
            isPrepared = false;
            System.IO.Stream stream = reader.BaseStream;

            try {
                getImageSize(0, out width, out height);
                nbits = getNumBits(0);
                pmi = getPhotometricInterpretation(0);
                getStrips(0, out stripPos, out stripCount, out rowCount);
                compid = getCompressionTagNumber(0);
                planarConfig = getPlanarConfiguration(0);
                diffTag = getDifferencePredictor(0);
                isFloat = isDataFloatPoint(0);
            } catch (ReadFileException ex) {
                throw ex;
            } catch (Exception ex) {
                throw new ReadFileException("Unabled to read TIF parameters", ex);
            }

            if (width <= 0 || height <= 0 || stripCount.Length != stripPos.Length || stripPos.Length == 0) throw new ReadFileException("Invalid image info.");
            if (nbits != 8 && nbits != 16 && nbits != 32) throw new ReadFileException("Can only read 8/16/32 bit images.");
            if (pmi != 0 && pmi != 1 && pmi != 2 && pmi != 3) throw new ReadFileException("Not a valid TIFF image.");

            if (nbits == 16) totBytes = Convert.ToUInt32(width * height * 2);
            else if (nbits == 32) totBytes = Convert.ToUInt32(width * height * 4);
            else totBytes = Convert.ToUInt32(width * height);
           
            spaceStrip = stripPos[stripPos.Length - 1] + stripCount[stripPos.Length - 1] - stripPos[0];
            isPrepared = true;
        }

        public int ImageWidth { get { return width; } }
        public int ImageHeight { get { return height; } }
        public MyTiffCompression.CompressionMethod CompressionID { get { return compid; } }
        public int DataDepth { get { return nbits; } }
        public int NumberOfPlanes { get { return nPlane; } }
        public int PhotometricInterpretation { get { return pmi; } }
        public bool IsReadyToRead { get { return isPrepared; } }

        /// <summary>
        /// Open a STK file for reading. May throw ReadFileException.
        /// </summary>
        /// <param name="filePath">File path string.</param>
        public override void open(string filePath) {
            isPrepared = false;
            base.open(filePath);
            nPlane = ((StkStruct)IFDs[0]).NumPlane;
            prepareReading();
        }

        /// <summary>
        /// Read image data of a certain plane to a 1D array. May throw ReadFileException.
        /// </summary>
        /// <param name="fid">Zero-based index of the plabe to read.</param>
        /// <returns>Image data</returns>
        public Array getGrayscaleImageDataPlane(int fid)
        {
            if (!isPrepared) prepareReading();
            if (!isPrepared) return null;

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;
            byte[] bigEndianTemp = new byte[2];

            System.IO.Stream stream = reader.BaseStream;

            Array dataTemp;
            if (nbits == 16) dataTemp = new ushort[totBytes / 2];
            else if (nbits == 8) dataTemp = new byte[totBytes];
            else if (isFloat) dataTemp = new float[totBytes / 4];
            else dataTemp = new uint[totBytes / 4];

            int leftRows = height; 

            int curr = 0;
            bool uncompressed = compid == MyTiffCompression.CompressionMethod.UNCOMPRESSED;
            int nStrip = uncompressed ? stripPos.Length : stripPos.Length / nPlane;
            int nRows = 0;
            for (int i = 0; i < nStrip; i++) {
                uint currPos = uncompressed ? Convert.ToUInt32(stripPos[i] + fid * spaceStrip) : stripPos[fid * nStrip + i];
                stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                uint leftSize = uncompressed ? stripCount[i] : stripCount[fid * nStrip + i];

                while (leftSize > 0) {
                    uint readSize = leftSize;
                    if (readSize > BufferSize && uncompressed) readSize = (uint)BufferSize;

                    byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                    if (!uncompressed) {
                        nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows;
                        dr = MyTiffCompression.decompress(dr, compid, nRows * width * (nbits / 8));
                    }
                    if (nbits == 8 || reader.IsLittleEndian) {
                        Buffer.BlockCopy(dr, 0, dataTemp, curr, dr.Length);
                        curr += dr.Length;
                    } else if (nbits == 16) {
                        for (int j = 0; j < readSize / 2; j++) {
                            bigEndianTemp[0] = dr[2 * j + 1];
                            bigEndianTemp[1] = dr[2 * j];
                            dataTemp.SetValue(BitConverter.ToUInt16(bigEndianTemp, 0), curr++);
                        }
                    } else {
                        for (int j = 0; j < readSize / 4; j++) {
                            bigEndianTemp[0] = dr[4 * j + 3];
                            bigEndianTemp[1] = dr[4 * j + 2];
                            bigEndianTemp[2] = dr[4 * j + 1];
                            bigEndianTemp[3] = dr[4 * j];
                            if (isFloat) dataTemp.SetValue(BitConverter.ToSingle(bigEndianTemp, 0), curr++);
                            else dataTemp.SetValue(BitConverter.ToUInt32(bigEndianTemp, 0), curr++);
                        }
                    }
                    leftSize -= readSize;
                    leftRows -= nRows;
                }
            }

            if (diffTag) reverseDifferencing2D(dataTemp, width, height);

            return dataTemp;
        }

        /// <summary>
        /// Read image data of a certain plane to a 3D array (3*size). May throw ReadFileException.
        /// </summary>
        /// <param name="fid">Zero-based index of the plabe to read.</param>
        /// <returns>Image data. 3 (RGB) * size</returns>
        public byte[][] getRGBimageDataPlane(int fid)
        {
            if (!isPrepared) prepareReading();
            if (!isPrepared) return null;

            System.IO.Stream stream = reader.BaseStream;
            byte[][] dataTemp = new byte[3][];
            for (int k = 0; k < 3; k++ ) dataTemp[k] = new byte[totBytes];

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 3 * 3;

            int leftRows = height;
            int curr = 0;
            bool uncompressed = compid == MyTiffCompression.CompressionMethod.UNCOMPRESSED;
            int nStrip = uncompressed ? stripPos.Length : stripPos.Length / nPlane;
            int nRows = 0;
            if (planarConfig == 1) {
                for (int i = 0; i < nStrip; i++) {
                    uint currPos = uncompressed ? Convert.ToUInt32(stripPos[i] + fid * spaceStrip) : stripPos[fid * nStrip + i];
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                    uint leftSize = uncompressed ? stripCount[i] : stripCount[fid * nStrip + i];
                    while (leftSize > 0) {
                        uint readSize = leftSize;
                        if (readSize > BufferSize && uncompressed) readSize = (uint)BufferSize;

                        byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                        if (!uncompressed) {
                            nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows; 
                            dr = MyTiffCompression.decompress(dr, compid, nRows * width * 3);
                        }

                        for (int j = 0; j < dr.Length; j += 3) {
                            dataTemp[0][curr] = dr[j];
                            dataTemp[1][curr] = dr[j + 1];
                            dataTemp[2][curr++] = dr[j + 2];
                        }
                        leftSize -= readSize;
                        leftRows -= nRows;
                    }
                }
            } else {
                int k = 0;
                for (int i = 0; i < nStrip; i++) {
                    uint currPos = uncompressed ? Convert.ToUInt32(stripPos[i] + fid * spaceStrip) : stripPos[fid * nStrip + i];
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                    uint leftSize = uncompressed ? stripCount[i] : stripCount[fid * nStrip + i];
                    while (leftSize > 0) {
                        uint readSize = leftSize;
                        if (readSize > BufferSize && uncompressed) readSize = (uint)BufferSize;

                        byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                        if (!uncompressed) {
                            nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows;
                            dr = MyTiffCompression.decompress(dr, compid, nRows * width);
                        }
                        
                        for (int j = 0; j < readSize; j++) {
                            dataTemp[k][curr++] = dr[j];
                            if (curr == totBytes) {
                                k++;
                                curr = 0;
                            }
                        }
                        leftSize -= readSize;
                        leftRows -= nRows;
                    }
                }
            }

            if (diffTag) {
                for (int c = 0; c < 3; c++) {
                    reverseDifferencing2D(dataTemp[c], (int)width, (int)height);
                }
            }
            
            return dataTemp;
        }

        internal override TiffStruct createNewStruct()
        {
            return new StkStruct();
        }
    }
}
