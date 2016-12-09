using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    /// <summary>
    /// This is a class to read a TIFF image file.
    /// First open a file, then read the image data and info.
    /// Methods reading data and info may throw ReadFileExceptions.
    /// </summary>

    public class TiffReader : IDisposable
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public TiffReader()
        {
            BufferSize = 8192;
        }
        #endregion

        #region properties

        public string CurrentFilePath { get; protected set; }

        #endregion

        #region public methods

        /// <summary>
        /// Open a TIFF file for reading. May throw ReadFileException.
        /// </summary>
        /// <param name="filePath">File path string.</param>
        public virtual void open(string filePath)
        {
            IFDs = null;
            if (System.IO.File.Exists(filePath))
            {
                reader = new BinaryReaderByteOrder(System.IO.File.OpenRead(filePath));
            }
            else
            {
                reader = null;
                throw new ReadFileException("File doesn't exist");
            }

            bool isTIFF = false;
            try
            {
                isTIFF = checkTIFF();
            }
            catch (ReadFileException ex)
            {
                throw ex;
            }
            if (!isTIFF) throw new ReadFileException("File is not a valid TIFF");
            CurrentFilePath = filePath;
        }

        /// <summary>
        /// Get number of images in the TIFF file.
        /// </summary>
        /// <returns>Total number of images stored.</returns>
        public int imageNumber()
        {
            return IFDs.Length;
        }

        /// <summary>
        /// Get image size.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        public void getImageSize(int fid, out int width, out int height) 
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];

            try
            {
                width = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.ImageWidth).GetValue(0));
                height = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.ImageLength).GetValue(0));
            }
            catch
            {
                throw new ReadFileException("Unable to read image info: ");
            }

            if (width <= 0 || height <= 0) throw new ReadFileException("Invalid image info.");
        }

        /// <summary>
        /// Get compression type.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Compression type. Read TIFF documentation for details.</returns>
        public MyTiffCompression.CompressionMethod getCompressionTagNumber(int fid)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            Array ca = dataStruct.searchData(TiffInfoCollection.Compression);
            if (ca == null || ca.Length == 0) return MyTiffCompression.CompressionMethod.UNCOMPRESSED;
            return (MyTiffCompression.CompressionMethod)Convert.ToInt32(ca.GetValue(0));
        }

        /// <summary>
        /// Determine if differencing was used for compression
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Predictor. Read TIFF documentation for details.</returns>
        public bool getDifferencePredictor(int fid) {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            Array ca = dataStruct.searchData(TiffInfoCollection.DifferencingPredictor);
            if (ca == null || ca.Length == 0) return false;
            else return (MyTiffCompression.DifferencePredictor)Convert.ToInt32(ca.GetValue(0)) == MyTiffCompression.DifferencePredictor.Horizontal;
        }

        /// <summary>
        /// Get the image bit depth.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Image depth in number of bits.</returns>
        public int getNumBits(int fid)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            int nbits = 0;

            try
            {
                nbits = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.BitsPerSample).GetValue(0));
            }
            catch
            {
                throw new ReadFileException("Unable to read image info: ");
            }

            return nbits;
        }

        /// <summary>
        /// Determine if image data represent float point numbers.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>True if image data represent float-point numbers.</returns>
        public bool isDataFloatPoint(int fid) {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            ushort nbits = 0;

            try {
                nbits = Convert.ToUInt16(dataStruct.searchData(TiffInfoCollection.SampleFormat).GetValue(0));
            } catch {
                return false;
            }

            return nbits == 3;
        }

        /// <summary>
        /// Get photo metric interpretation.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Photo metric interpretation. Read TIFF documentation for details.</returns>
        public int getPhotometricInterpretation(int fid)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            int pmi = 0;

            try
            {
                pmi = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.PhotometricInterpretation).GetValue(0));
            }
            catch
            {
                return 1;
            }

            return pmi;
        }

        /// <summary>
        /// Get sample per pixel.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Number of samples per pixel. 1 for grayscale, 3 for RGB</returns>
        public int getSamplePerPixel(int fid)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            int spp = 0;

            try {
                spp = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.SamplesPerPixel).GetValue(0));
            } catch {
                throw new ReadFileException("Unable to read image info: ");
            }

            return spp;
        }

        /// <summary>
        /// Get planar configuration.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Planar configuration. Read TIFF documents</returns>
        public int getPlanarConfiguration(int fid)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            int pc = 0;

            try {
                pc = Convert.ToInt32(dataStruct.searchData(TiffInfoCollection.PlanarConfiguration).GetValue(0));
            } catch {
                return 1;
            }

            return pc;
        }

        /// <summary>
        /// Get colormap.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>Colormap for palette-color images</returns>
        public byte[][] getColormap(int fid) {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];
            ushort[] temp;
            byte[][] colormap;
            int nbits;

            try {
                Array t = dataStruct.searchData(TiffInfoCollection.BitsPerSample);
                if (t == null || t.Length == 0) return null;
                nbits = Convert.ToInt32(t.GetValue(0));
                nbits = 1 << nbits;
                temp = dataStruct.searchData(TiffInfoCollection.ColorMap) as ushort[];
                if (temp.Length != 3 * nbits) {
                    nbits = temp.Length / 3;
                    if (nbits != 256) throw new ReadFileException("Colormap bit number error.");
                }
                
                colormap = new byte[3][];
                for (int k = 0; k < 3; k++) colormap[k] = new byte[nbits];
                
                int highBits = 0;
                int lowBits = 0;
                for (int k = 1; k <= 3; k++) {
                    highBits += temp[nbits - k] >> 8;
                    lowBits += temp[nbits - k] & 0xff;
                }
                
                if (highBits > lowBits) {
                    for (int k = 0; k < 3; k++) {
                        for (int i = 0; i < nbits; i++) colormap[k][i] = Convert.ToByte(temp[k * nbits + i] >> 8);
                    }
                } else {
                    for (int k = 0; k < 3; k++) {
                        for (int i = 0; i < nbits; i++) colormap[k][i] = Convert.ToByte(temp[k * nbits + i] & 0xff);
                    }
                }
            } catch (Exception ex) {
                throw new ReadFileException("Unable to read image info: ", ex);
            }

            return colormap;
        }

        /// <summary>
        /// Read a RGB image data. May throw ReadFileException if not a valid RGB image.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <returns>Image data in row-first order.</returns>
        public virtual byte[][] getRGBImageData(int fid, out int width, out int height) {
            System.IO.Stream stream = reader.BaseStream;

            int nbits, pmi, spp, planar;
            uint[] stripPos, stripCount;
            uint rowCount;
            MyTiffCompression.CompressionMethod compid;
            bool diffTag;

            try {
                getImageSize(fid, out width, out height);
                nbits = getNumBits(fid);
                pmi = getPhotometricInterpretation(fid);
                getStrips(fid, out stripPos, out stripCount, out rowCount);
                compid = getCompressionTagNumber(fid);
                spp = getSamplePerPixel(fid);
                planar = getPlanarConfiguration(fid);
                diffTag = getDifferencePredictor(fid);
            } catch (ReadFileException ex) {
                throw ex;
            }

            if (width <= 0 || height <= 0 || stripCount.Length != stripPos.Length || stripPos.Length == 0) throw new ReadFileException("Invalid image info.");
            if (nbits != 8 || spp != 3) throw new ReadFileException("Can only read 8 bit RGB images.");
            if (pmi != 2) throw new ReadFileException("Not a RGB image.");
            if (planar != 1 && planar != 2) throw new ReadFileException("Invalid image. Planar configuration undefined.");

            byte[][] dataTemp = new byte[3][];
            int colorBytes = width * height;
            for (int k = 0; k < 3; k++) dataTemp[k] = new byte[colorBytes];

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 3 * 3;

            int leftRows = height;
            bool notCompressed = compid == MyTiffCompression.CompressionMethod.UNCOMPRESSED;

            int curr = 0;
            int nRows = 0;
            if (planar == 1) {
                for (int i = 0; i < stripPos.Length; i++) {
                    stream.Seek(stripPos[i], System.IO.SeekOrigin.Begin);
                    uint leftSize = stripCount[i];
                    while (leftSize > 0) {
                        uint readSize = leftSize;
                        if (readSize > BufferSize && notCompressed) readSize = (uint)BufferSize;

                        byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                        if (!notCompressed) {
                            nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows; 
                            dr = MyTiffCompression.decompress(dr, compid, nRows * width * 3);
                        }

                        for (int j = 0; j < dr.Length; j += 3) {
                            dataTemp[0][curr] = dr[j];
                            dataTemp[1][curr] = dr[j+1];
                            dataTemp[2][curr++] = dr[j+2];
                        }
                        leftSize -= readSize;
                        leftRows -= (int)rowCount;
                    }
                }
            } else {
                int k = 0;
                for (int i = 0; i < stripPos.Length; i++) {
                    stream.Seek(stripPos[i], System.IO.SeekOrigin.Begin);
                    uint leftSize = stripCount[i];
                    while (leftSize > 0) {
                        uint readSize = leftSize;
                        if (readSize > BufferSize && notCompressed) readSize = (uint)BufferSize;

                        byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                        if (!notCompressed) {
                            nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows; 
                            dr = MyTiffCompression.decompress(dr, compid, nRows * width);
                        }
                        
                        for (int j = 0; j < dr.Length; j++) {
                            dataTemp[k][curr++] = dr[j];
                            if (curr == colorBytes) {
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
                    reverseDifferencing(dataTemp[c], (int)width, (int)height);
                }
            } 

            return dataTemp;
        }

        /// <summary>
        /// Read a grayscale image data. May throw ReadFileException if not a valid grayscale image.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <returns>Image data in row-first order.</returns>
        public virtual Array getGrayscaleImageData(int fid, out int width, out int height)
        {
            System.IO.Stream stream = reader.BaseStream;

            int nbits;
            uint[] stripPos, stripCount;
            uint rowCount;
            MyTiffCompression.CompressionMethod compid;
            bool diffTag;
            bool isFloat;

            try {
                getImageSize(fid, out width, out height);
                nbits = getNumBits(fid);
                getStrips(fid, out stripPos, out stripCount, out rowCount);
                compid = getCompressionTagNumber(fid);
                diffTag = getDifferencePredictor(fid);
                isFloat = isDataFloatPoint(fid);
            } catch (ReadFileException ex) {
                throw ex;
            }

            if (width <= 0 || height <= 0 || stripCount.Length != stripPos.Length || stripPos.Length == 0) throw new ReadFileException("Invalid image info.");
            if (nbits != 8 && nbits != 16 && nbits != 32) throw new ReadFileException("Can only read 8/16/32 bit images.");

            Array dataTemp;
            if (nbits == 16) dataTemp = new ushort[width * height];
            else if (nbits == 8) dataTemp = new byte[width * height];
            else if (isFloat) dataTemp = new float[width * height];
            else dataTemp = new uint[width * height];

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;
            byte[] bigEndianTemp = new byte[4];

            int leftRows = height;
            bool notCompressed = compid == MyTiffCompression.CompressionMethod.UNCOMPRESSED;

            int curr = 0;
            int nRows = 0;
            for (int i = 0; i < stripPos.Length; i++) {
                stream.Seek(stripPos[i], System.IO.SeekOrigin.Begin);
                uint leftSize = stripCount[i];

                while (leftSize > 0) {
                    uint readSize = leftSize;
                    if (readSize > BufferSize && notCompressed) readSize = (uint)BufferSize;

                    byte[] dr = reader.ReadBytes(Convert.ToInt32(readSize));
                    if (!notCompressed) {
                        nRows = leftRows > (int)rowCount ? (int)rowCount : leftRows; 
                        dr = MyTiffCompression.decompress(dr, compid, nRows * width * (nbits / 8));
                    }
                    if (nbits == 8 || reader.IsLittleEndian) {
                        Buffer.BlockCopy(dr, 0, dataTemp, curr, dr.Length);
                        curr += dr.Length;
                    } else if (nbits == 16) {
                        for (int j = 0; j < dr.Length / 2; j++) {
                            bigEndianTemp[0] = dr[2 * j + 1];
                            bigEndianTemp[1] = dr[2 * j];
                            dataTemp.SetValue(BitConverter.ToUInt16(bigEndianTemp, 0), curr++);
                        }
                    } else {
                        for (int j = 0; j < dr.Length / 4; j++) {
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

            if (diffTag) reverseDifferencing2D(dataTemp as byte[], width, height);

            return dataTemp;
        }

        /// <summary>
        /// Read a palette-color image data. May throw ReadFileException.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <param name="colormap">colormap.</param>
        /// <returns>Image data in row-first order.</returns>
        public virtual Array getPaletteColorImageData(int fid, out int width, out int height, out byte[][] colormap) {
            try {
                colormap = getColormap(fid);
            } catch {
                colormap = new byte[3][];
                for (int k = 0; k < 3; k++) {
                    colormap[k] = new byte[256];
                    for (int i = 0; i < 256; i++) colormap[k][i] = Convert.ToByte(i);
                }
            }
            return getGrayscaleImageData(fid, out width, out height);
        }
        

        /// <summary>
        /// Read a piece of info.
        /// </summary>
        /// <param name="tag">Tag of the info to be read.</param>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>An instance of TIFFinfo that contains the info.</returns>
        public virtual TiffData getInfo(int tag, int fid)
        {
            if (fid < 0 || fid >= IFDs.Length) return null;
            TiffStruct fileData = IFDs[fid];

            TiffDirData dir = fileData.search(tag);
            if (dir == null) return null;
            if (dir.Offset > 0)
            {
                reader.BaseStream.Seek(dir.Offset, System.IO.SeekOrigin.Begin);
                dir.readData(reader);
            }
            return dir.Data;
        }

        /// <summary>
        /// Read all info.
        /// </summary>
        /// <param name="fid">Zero-based index of images.</param>
        /// <returns>An instance of TIFFinfoCollection that contains all info.</returns>
        public virtual TiffInfoCollection getAllInfo(int fid)
        {
            if (fid < 0 || fid >= IFDs.Length) return null;
            return IFDs[fid].toInfoCollection();
        }
        
        /// <summary>
        /// Close the reader.
        /// </summary>
        public virtual void close()
        {
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }

        /// <summary>
        /// Test if the reader is open.
        /// </summary>
        /// <returns>True if the reader opens a file. False otherwise.</returns>
        public bool isFileOpen()
        {
            return reader != null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() {
            close();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region protected methods

        protected void getStrips(int fid, out uint[] stripPos, out uint[] stripCount, out uint rowsCount)
        {
            if (fid >= IFDs.Length) throw new ReadFileException("Image # " + fid + " doesn't exist.");

            TiffStruct dataStruct = IFDs[fid];

            try
            {
                Array temp = dataStruct.searchData(TiffInfoCollection.StripOffsets);
                if (temp is uint[]) stripPos = temp as uint[];
                else {
                    stripPos = new uint[temp.Length];
                    for (int i = 0; i < temp.Length; i++) stripPos[i] = Convert.ToUInt32(temp.GetValue(i));
                }

                temp = dataStruct.searchData(TiffInfoCollection.StripByteCounts);
                if (temp is uint[]) stripCount = temp as uint[];
                else {
                    stripCount = new uint[temp.Length];
                    for (int i = 0; i < temp.Length; i++) stripCount[i] = Convert.ToUInt32(temp.GetValue(i));
                }
            }
            catch
            {
                throw new ReadFileException("Unable to read image info: ");
            }

            try {
                rowsCount = Convert.ToUInt32(dataStruct.searchData(TiffInfoCollection.RowsPerStrip).GetValue(0));
            } catch {
                int height, width;
                getImageSize(fid, out width, out height);
                rowsCount = Convert.ToUInt32(height / stripPos.Length);
            }
        }

        protected bool checkTIFF()
        {
            System.IO.Stream stream = reader.BaseStream;
            if (stream == null) return false;

            byte[] fileType = new byte[2];

            List<TiffStruct> IFDlist = new List<TiffStruct>();
            try
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.Read(fileType, 0, 2);
                if (fileType[1] == 'M' && fileType[0] == 'M') reader.IsLittleEndian = false;
                else if (fileType[1] == 'I' && fileType[0] == 'I') reader.IsLittleEndian = true; 
                else return false;
                ushort num = reader.ReadUInt16();
                if (num != 42) return false;
                long firstIFD = reader.ReadUInt32();
                if (firstIFD <= 0) return false;

                while (firstIFD > 0)
                {
                    stream.Seek(firstIFD, System.IO.SeekOrigin.Begin);
                    TiffStruct ifd = createNewStruct();
                    ifd.read(reader);
                    IFDlist.Add(ifd);
                    firstIFD = ifd.NextIFD;
                }
            }
            catch (Exception ex)
            {
                if (IFDlist.Count == 0)
                {
                    if (ex is ReadFileException) throw ex;
                    throw new ReadFileException("Unexpected error, " + ex.Message);
                }
            }
            finally
            {
                IFDs = IFDlist.ToArray();
            }
            return true;
        }

        internal virtual TiffStruct createNewStruct()
        {
            return new TiffStruct();
        }

        protected void reverseDifferencing2D(Array data, int width, int height) {
            if (data is byte[]) {
                var temp = data as byte[];
                reverseDifferencing(temp, (int)width, (int)height);
            } else if (data is ushort[]) {
                var temp = data as ushort[];
                reverseDifferencing(temp, (int)width, (int)height);
            } else if (data is float[]) {
                var temp = data as float[];
                reverseDifferencing(temp, (int)width, (int)height);
            }
        }

        #endregion

        #region private methods

        private void reverseDifferencing(byte[] data, int width, int height) {
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head; x < head + width - 1; x++) {
                        data[x + 1] += data[x];
                    }
                    head += width;
                }
            }
        }

        private void reverseDifferencing(ushort[] data, int width, int height) {
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head; x < head + width - 1; x++) {
                        data[x + 1] += data[x];
                    }
                    head += width;
                }
            }
        }

        private void reverseDifferencing(float[] data, int width, int height) {
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head; x < head + width - 1; x++) {
                        data[x + 1] += data[x];
                    }
                    head += width;
                }
            }
        }

        #endregion

        #region protected data

        protected BinaryReaderByteOrder reader = null;

        #endregion

        #region internal data

        internal TiffStruct[] IFDs;

        #endregion

        #region reader buffer

        protected const int MIN_BUFFER_SIZE = 2048;
        public uint BufferSize { get; set; }
        
        #endregion

    }
}
