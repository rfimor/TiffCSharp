using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;
using System.IO.Compression;
using System.ComponentModel;

namespace TiffCSharp.TiffIO
{
    /// <summary>
    /// This is a class to write a TIFF image file. "MM" type images are not supported.
    /// </summary>
    public class TiffWriter: IDisposable
    {
        #region events

        public delegate void PlaneWrittenHandler(object sender, ProgressChangedEventArgs e);
        public event PlaneWrittenHandler PlaneWritten;

        #endregion

        #region Constructor
        public TiffWriter() {
            BufferSize = 8192;
            CompressMethod = MyTiffCompression.CompressionMethod.DEFLATE;
            CompressLevel = CompressionLevel.Optimal;
            HorizontalDifferencing = false;
        }
        #endregion

        #region properties

        public string CurrentFilePath { get; protected set; }
        public virtual MyTiffCompression.CompressionMethod CompressMethod { get { return compressionMethod; } set { compressionMethod = value; } }
        public virtual CompressionLevel CompressLevel { get; set; }

        private bool horizontalDifferencing = false;
        public bool HorizontalDifferencing {
            get {
                return CompressMethod != MyTiffCompression.CompressionMethod.UNCOMPRESSED && horizontalDifferencing;
            }
            set {
                horizontalDifferencing = value && CompressMethod != MyTiffCompression.CompressionMethod.UNCOMPRESSED;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Find data depth (number of bits) and if the data is float-point.
        /// </summary>
        /// <param name="dataType">Image data type.</param>
        /// <param name="numberOfBits">Number of bits.</param>
        /// <param name="isFloat">True is data is float-point numbers.</param>
        public static void getDataTypeInfo(ImageDataType dataType, out uint numberOfBits, out bool isFloat) {
            switch(dataType) {
                case ImageDataType.Byte:
                    numberOfBits = 8;
                    isFloat = false;
                    break;
                case ImageDataType.Float:
                    numberOfBits = 32;
                    isFloat = true;
                    break;
                case ImageDataType.UInt32:
                    numberOfBits = 32;
                    isFloat = false;
                    break;
                case ImageDataType.RGB:
                    numberOfBits = 8;
                    isFloat = false;
                    break;
                case ImageDataType.Short:
                    numberOfBits = 16;
                    isFloat = false;
                    break;
                default:
                    throw new Exception("Unsupported image data type");
            }
        }

        /// <summary>
        /// Get image data type from image data array.
        /// </summary>
        /// <param name="data">Image data array.</param>
        /// <returns>Image data type.</returns>
        public static ImageDataType getArrayDataType(Array data) {
            if (data is ushort[]) {
                return ImageDataType.Short;
            } else if (data is byte[]) {
                return ImageDataType.Byte;
            } else if (data is float[]) {
                return ImageDataType.Float;
            } else if (data is uint[]) {
                return ImageDataType.UInt32;
            } else if (data is byte[][]) {
                return ImageDataType.RGB;
            } else {
                throw new WriteFileException("Can only write 8/16/32 bit data, unsigned integer or single-precision float-point.");
            }
        }

        /// <summary>
        /// Open a TIFF file to write. 
        /// </summary>
        /// <param name="filePath">File path string.</param>
        public virtual void open(string filePath) {
            try {
                writer = new System.IO.BinaryWriter(System.IO.File.Create(filePath));
                CurrentFilePath = filePath;
            } catch (Exception ex) {
                throw new WriteFileException(ex.Message);
            }
        }

        /// <summary>
        /// Close the writer.
        /// </summary>
        public virtual void close()
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }

        /// <summary>
        /// Test if the writer opens any file.
        /// </summary>
        /// <returns>True if the writer opens a file. False otherwise.</returns>
        public bool isFileOpen()
        {
            return writer != null;
        }

        /// <summary>
        /// Write a single grayscle image with default TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public virtual void writeSingleGrayscaleImage(Array data, int width, int height) {
            TiffInfoCollection info = new TiffInfoCollection();
            info.setImageSize((uint)width, (uint)height);
            writeSingleGrayscaleImage(data, info);
        }

        /// <summary>
        /// Write a single RGB image with default TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public virtual void writeSingleRGBImage(byte[][] data, int width, int height) {
            TiffInfoCollection info = new TiffInfoCollection();
            info.setImageSize((uint)width, (uint)height);
            writeSingleRGBImage(data, info);
        }

        /// <summary>
        /// Write a single palette-color image with default TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="colormap">Colormap</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public virtual void writeSinglePaletteColorImage(Array data, byte[][] colormap, int width, int height)
        {
            TiffInfoCollection info = new TiffInfoCollection();
            info.setImageSize((uint)width, (uint)height);
            writeSinglePaletteColorImage(data, colormap, info);
        }
        
        /// <summary>
        /// Write a single grayscle image with given TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="info">Tiff info collection</param>
        public virtual void writeSingleGrayscaleImage(Array data, int width, int height, TiffInfoCollection info) {
            if (info == null) throw new WriteFileException("Info collection is NULL");
            info.setImageSize((uint)width, (uint)height);
            writeSingleGrayscaleImage(data, info);
        }

        /// <summary>
        /// Write a single palette-color image with given TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="colormap">Colormap</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="info">TIFF info</param>
        public virtual void writeSinglePaletteColorImage(Array data, byte[][] colormap, int width, int height, TiffInfoCollection info) {
            info.setImageSize((uint)width, (uint)height);
            writeSinglePaletteColorImage(data, colormap, info);
        }

        /// <summary>
        /// Write a single RGB image with given TIFF info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Row-major image data</param>
        /// <param name="colormap">Colormap</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="info">TIFF info</param>
        public virtual void writeSingleRGBImage(byte[][] data, int width, int height, TiffInfoCollection info) {
            info.setImageSize((uint)width, (uint)height);
            writeSingleRGBImage(data, info);
        }
        
        /// <summary>
        /// Write a single grayscle image. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in row-first order.</param>
        /// <param name="allInfo">Collection of all info.</param>
        public virtual void writeSingleGrayscaleImage(Array data, TiffInfoCollection allInfo) {
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");
            prepareWriteFile();
            addGrayScalePlane(data, allInfo);
        }

        /// <summary>
        /// Write a single palette color image. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in row-first order.</param>
        /// <param name="allInfo">Collection of all info.</param>
        public virtual void writeSinglePaletteColorImage(Array data, byte[][] colormap, TiffInfoCollection allInfo) {
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");
            addColormapToTiffInfo(allInfo, colormap);
            writeSinglePaletteColorImage(data, allInfo);
        }
        
        /// <summary>
        /// Write a single palette color image. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in row-first order.</param>
        /// <param name="allInfo">Collection of all info.</param>
        public virtual void writeSinglePaletteColorImage(Array data, TiffInfoCollection allInfo)
        {
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");
            prepareWriteFile();
            addPaletteColorPlane(data, allInfo);
        }
        
        /// <summary>
        /// Write a single RGB color image. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in row-first order.</param>
        /// <param name="allInfo">Collection of all info.</param>
        public virtual void writeSingleRGBImage(byte[][] data, TiffInfoCollection allInfo) {
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");
            prepareWriteFile();
            addRGBplane(data, allInfo);
        }

        /// <summary>
        /// Prepare to write a multi-image TIFF file
        /// </summary>
        public void prepareWriteFile() {
            System.IO.Stream stream = writer.BaseStream;
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            writer.Write((byte)73);
            writer.Write((byte)73);
            writer.Write((ushort)42);
            nextIFDpos = stream.Position;
            writer.Write((uint)0);
        }

        /// <summary>
        /// Add the next grayscale image
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="allInfo">Tiff info</param>
        public void addGrayScalePlane(Array data, TiffInfoCollection allInfo) {
            System.IO.Stream stream = writer.BaseStream;

            allInfo.add(new TiffInfo(TiffInfoCollection.PhotometricInterpretation, "", (ushort)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.XResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.YResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.ResolutionUnit, "", (ushort)1));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.SamplesPerPixel, "", (ushort)1));
            MyTiffCompression.setCompressionTag(allInfo, CompressMethod, CompressLevel);
            MyTiffCompression.setHorizontalDifferencing(allInfo, HorizontalDifferencing);

            uint nBits = 0;
            ushort[][] data16bit = null;
            byte[][] data8bit = null;
            float[][] dataFloat = null;
            uint[][] data32bit = null;
            dealWithBits(data, allInfo, out nBits, out data16bit, out data8bit, out data32bit, out dataFloat);

            uint width, height;
            allInfo.getImageSize(out width, out height);
            uint byteCounts = width * height * nBits / 8;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", toWordBoundary()));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", byteCounts));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", height));

            int[] missing = allInfo.missingInfoGrayscale();
            if (missing.Length > 0) {
                String msg = "Missing tags: ";
                for (int i = 0; i < missing.Length; i++) msg += missing[i] + " ";
                throw new WriteFileException(msg);
            }

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;

            if (nBits == 8) writeGrayScaleImageDataStrips(data8bit, byteCounts, width, height, allInfo);
            else if (nBits == 16) writeGrayScaleImageDataStrips(data16bit, byteCounts, width, height, allInfo);
            else if (dataFloat != null) writeGrayScaleImageDataStrips(dataFloat, byteCounts, width, height, allInfo);
            else writeGrayScaleImageDataStrips(data32bit, byteCounts, width, height, allInfo);

            finalizeImageWriting(allInfo, new TiffStruct());
        }

        /// <summary>
        /// Add the next palette color image
        /// </summary>
        /// <param name="data">Image data</param>
        /// <param name="allInfo">Tiff info</param>
        public void addPaletteColorPlane(Array data, TiffInfoCollection allInfo) {
            System.IO.Stream stream = writer.BaseStream;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PhotometricInterpretation, "", (ushort)3));
            allInfo.add(new TiffInfo(TiffInfoCollection.XResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.YResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.ResolutionUnit, "", (ushort)1));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.SamplesPerPixel, "", (ushort)1));
            MyTiffCompression.setCompressionTag(allInfo, CompressMethod, CompressLevel);
            MyTiffCompression.setHorizontalDifferencing(allInfo, HorizontalDifferencing);

            uint nBits = 0;
            ushort[][] data16bit = null;
            byte[][] data8bit = null;
            float[][] dataFloat = null;
            uint[][] data32bit = null;
            dealWithBits(data, allInfo, out nBits, out data16bit, out data8bit, out data32bit, out dataFloat);

            Array info = allInfo.getOneInfoData(TiffInfoCollection.ColorMap);
            if (info == null) throw new WriteFileException("Invalid colormap.");

            uint width, height;
            allInfo.getImageSize(out width, out height);
            uint byteCounts = width * height * nBits / 8;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", toWordBoundary()));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", byteCounts));

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", height));

            int[] missing = allInfo.missingInfoPaletteColor();
            if (missing.Length > 0) {
                String msg = "Missing tags: ";
                for (int i = 0; i < missing.Length; i++) msg += missing[i] + " ";
                throw new WriteFileException(msg);
            }

            if (nBits == 8) writeGrayScaleImageDataStrips(data8bit, byteCounts, width, height, allInfo);
            else if (nBits == 16) writeGrayScaleImageDataStrips(data16bit, byteCounts, width, height, allInfo);
            else if (dataFloat != null) writeGrayScaleImageDataStrips(dataFloat, byteCounts, width, height, allInfo);
            else writeGrayScaleImageDataStrips(data32bit, byteCounts, width, height, allInfo);

            finalizeImageWriting(allInfo, new TiffStruct());
        }

        /// <summary>
        /// Add the next RGB image
        /// </summary>
        /// <param name="data">Image data</param>
        /// <param name="allInfo">TIFF info</param>
        public void addRGBplane(byte[][] data, TiffInfoCollection allInfo) {
            System.IO.Stream stream = writer.BaseStream;

            allInfo.add(new TiffInfo(TiffInfoCollection.PhotometricInterpretation, "", (ushort)2));
            allInfo.add(new TiffInfo(TiffInfoCollection.XResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.YResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.ResolutionUnit, "", (ushort)1));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.SamplesPerPixel, "", (ushort)3));

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PlanarConfiguration, "", (ushort)1));
            MyTiffCompression.setCompressionTag(allInfo, CompressMethod, CompressLevel);
            MyTiffCompression.setHorizontalDifferencing(allInfo, HorizontalDifferencing);

            ushort[] nbits = new ushort[] { (ushort)8, (ushort)8, (ushort)8 };
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", nbits));

            uint width, height;
            allInfo.getImageSize(out width, out height);
            uint colorBytes = width * height;
            uint totOrigByteCounts = colorBytes * 3;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", toWordBoundary()));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", totOrigByteCounts));

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", height));

            int[] missing = allInfo.missingInfoRGB();
            if (missing.Length > 0) {
                String msg = "Missing tags: ";
                for (int i = 0; i < missing.Length; i++) msg += missing[i] + " ";
                throw new WriteFileException(msg);
            }

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            writeRGBImageDataStrips(new byte[1][][] { data }, totOrigByteCounts, width, height, allInfo);

            finalizeImageWriting(allInfo, new TiffStruct());
        }

        /// <summary>
        /// Check if a jagged array is valid image data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static bool checkJaggedArray(ushort[][] data, out int height, out int width)
        {
            height = data.Length;
            width = 0;
            if (height == 0) return false;
            width = data[0].Length;

            for (int i = 1; i < height; i++)
            {
                if (data[i].Length != width) return false;
            }
            return true;
        }

        public static void addColormapToTiffInfo(TiffInfoCollection allInfo, byte[][] colormap) {
            if (colormap.Length < 3) throw new WriteFileException("Corrupted colormap.");
            int mlen = colormap[0].Length;
            if (colormap[1].Length != mlen || colormap[2].Length != mlen) {
                throw new WriteFileException("Corrupted colormap.");
            }

            ushort[] maptemp = new ushort[3 * mlen];
            for (int k = 0; k < 3; k++) {
                for (int i = 0; i < mlen; i++) {
                    maptemp[k * mlen + i] = Convert.ToUInt16(colormap[k][i] * 256);
                }
            }

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.ColorMap, "Color Map", maptemp));
        }

        public void Dispose() {
            close();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region protected

        protected long nextIFDpos;
        protected System.IO.BinaryWriter writer = null;

        protected MyTiffCompression.CompressionMethod compressionMethod;

        protected void writeGrayScaleImageDataStrips(Array[] data, uint origByteCntPerPlane, uint width, uint height, TiffInfoCollection allInfo) {
            byte[] buffer;
            System.IO.Stream stream = writer.BaseStream;

            if (CompressMethod == MyTiffCompression.CompressionMethod.UNCOMPRESSED) {
                buffer = new byte[BufferSize];
                for (int pn = 0; pn < data.Length; pn++) {
                    int curr = 0;
                    uint leftSize = origByteCntPerPlane;
                    int writeSize = (int)BufferSize;
                    while (leftSize > 0) {
                        if (writeSize > leftSize) writeSize = (int)leftSize;
                        Buffer.BlockCopy(data[pn], curr, buffer, 0, writeSize);
                        curr += writeSize;
                        writer.Write(buffer, 0, writeSize);
                        leftSize -= (uint)writeSize;
                    }
                    if (PlaneWritten != null) PlaneWritten(this, new ProgressChangedEventArgs(Convert.ToInt32(100.0 * pn / data.Length), null));
                }
            } else {
                uint nRows;
                int nStrips;
                defineStripsGrayscale(origByteCntPerPlane, height, 2, out nRows, out nStrips);

                uint[] stripBytes, stripPos;
                stripBytes = new uint[nStrips * data.Length];
                stripPos = new uint[nStrips * data.Length];

                int writeSize = Convert.ToInt32(origByteCntPerPlane / height * nRows);
                buffer = new byte[writeSize];
                int cnt = 0;

                for (int pn = 0; pn < data.Length; pn++) {
                    Array dataTemp = data[pn];
                    if (HorizontalDifferencing) dataTemp = differencing2D(data[pn], (int)width, (int)height);

                    int curr = 0;
                    uint leftSize = origByteCntPerPlane;
                    while (leftSize > 0) {
                        int currWriteSize = writeSize;
                        if (currWriteSize > leftSize) currWriteSize = (int)leftSize;
                        Buffer.BlockCopy(dataTemp, curr, buffer, 0, currWriteSize);
                        curr += currWriteSize;
                        stripPos[cnt] = (uint)stream.Position;
                        stripBytes[cnt++] = Convert.ToUInt32(MyTiffCompression.compress(buffer, 0, currWriteSize, writer, CompressMethod, CompressLevel));
                        leftSize -= (uint)currWriteSize;
                    }
                    if (PlaneWritten != null) PlaneWritten(this, new ProgressChangedEventArgs(Convert.ToInt32(100.0 * pn / data.Length), null));
                }

                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", stripPos));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", stripBytes));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", nRows));
            }
        }

        protected void writeRGBImageDataStrips(byte[][][] data, uint origByteCntPerPlane, uint width, uint height, TiffInfoCollection allInfo) {
            byte[] buffer;
            System.IO.Stream stream = writer.BaseStream;

            if (CompressMethod == MyTiffCompression.CompressionMethod.UNCOMPRESSED) {
                buffer = new byte[BufferSize];
                for (int pn = 0; pn < data.Length; pn++) {
                    int k = 0;
                    int curr = 0;
                    uint leftSize = origByteCntPerPlane;
                    uint writeSize = BufferSize;
                    while (leftSize > 0) {
                        if (writeSize > leftSize) writeSize = leftSize;
                        for (int i = 0; i < writeSize; i++) {
                            buffer[i] = data[pn][k++][curr / 3];
                            curr++;
                            if (k == 3) k = 0;
                        }
                        writer.Write(buffer, 0, Convert.ToInt32(writeSize));
                        leftSize -= writeSize;
                    }
                    if (PlaneWritten != null) PlaneWritten(this, new ProgressChangedEventArgs(Convert.ToInt32(100.0 * pn / data.Length), null));
                }
            } else {
                uint nRows;
                int nStrips;
                defineStripsGrayscale(origByteCntPerPlane, height, 3, out nRows, out nStrips);

                uint[] stripBytes, stripPos;
                stripBytes = new uint[nStrips * data.Length];
                stripPos = new uint[nStrips * data.Length];
                int writeSize = Convert.ToInt32(origByteCntPerPlane / height * nRows);
                buffer = new byte[writeSize];
                int cnt = 0;

                for (int pn = 0; pn < data.Length; pn++) {
                    byte[][] dataTemp = new byte[3][];
                    for (int c = 0; c < 3; c++) {
                        dataTemp[c] = data[pn][c];
                        if (HorizontalDifferencing) {
                            dataTemp[c] = differencing(data[pn][c], (int)width, (int)height) as byte[];
                        }
                    } 
                    
                    int curr = 0;
                    int k = 0;
                    uint leftSize = origByteCntPerPlane;
                    while (leftSize > 0) {
                        int currWriteSize = writeSize;
                        if (currWriteSize > leftSize) currWriteSize = (int)leftSize;
                        for (int i = 0; i < currWriteSize; i++) {
                            buffer[i] = dataTemp[k++][curr / 3];
                            curr++;
                            if (k == 3) k = 0;
                        }
                        stripPos[cnt] = (uint)stream.Position;
                        stripBytes[cnt++] = Convert.ToUInt32(MyTiffCompression.compress(buffer, 0, currWriteSize, writer, CompressMethod, CompressLevel));
                        leftSize -= (uint)currWriteSize;
                    }
                    if (PlaneWritten != null) PlaneWritten(this, new ProgressChangedEventArgs(Convert.ToInt32(100.0 * pn / data.Length), null));
                }

                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", stripPos));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", stripBytes));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", nRows));
            }
        }

        protected void dealWithBits(Array data, TiffInfoCollection allInfo, out uint nBits, out ushort[][] data16bit, out byte[][] data8bit, out uint[][] data32bit, out float[][] dataFloat) {
            data16bit = null;
            data8bit = null;
            data32bit = null;
            dataFloat = null;
            if (data is ushort[]) {
                nBits = 16;
                data16bit = new ushort[1][] { data as ushort[] };
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)16));
                allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.UnsignedInteger);
            } else if (data is byte[]) {
                nBits = 8;
                data8bit = new byte[1][] { data as byte[] };
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)8));
                allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.UnsignedInteger);
            } else if (data is float[]) {
                nBits = 32;
                dataFloat = new float[1][] { data as float[] };
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)32));
                allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.FloatPoint);
            } else if (data is uint[]) {
                nBits = 32;
                data32bit = new uint[1][] { data as uint[] };
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)32));
                allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.UnsignedInteger);
            } else {
                throw new WriteFileException("Can only write 8/16/32 bit data, unsigned integer or single-precision float-point.");
            }
        }

        internal virtual void finalizeImageWriting(TiffInfoCollection allInfo, TiffStruct fileData) {
            System.IO.Stream stream = writer.BaseStream;
            uint tagPos = toWordBoundary();
            stream.Seek(nextIFDpos, System.IO.SeekOrigin.Begin);
            writer.Write(tagPos);

            stream.Seek(tagPos, System.IO.SeekOrigin.Begin);
            fileData.setFromInfoCollection(allInfo);
            fileData.NextIFD = 0;
            fileData.write(writer, out nextIFDpos);
        }

        protected void defineStripsGrayscale(uint origByteCntPerPlane, uint height, uint rowsPerBuffer, out uint nRows, out int nStrips) {
            uint widthByte = origByteCntPerPlane / height;
            nRows = rowsPerBuffer * BufferSize / widthByte;
            if (nRows == 0) nRows = 1;
            else if (nRows > height) nRows = height;
            nStrips = Convert.ToInt32(Math.Ceiling((double)height / nRows));
        }

        protected Array differencing2D(Array data, int width, int height) {
            if (data is byte[]) {
                var temp = data as byte[];
                return differencing(temp, (int)width, (int)height);
            } else if (data is ushort[]) {
                var temp = data as ushort[];
                return differencing(temp, (int)width, (int)height);
            } else if (data is float[]) {
                var temp = data as float[];
                return differencing(temp, (int)width, (int)height);
            } else return null;
        }

        #endregion

        #region private methods
        
        private byte[] differencing(byte[] data, int width, int height) {
            byte[] dataCopy = new byte[data.Length];
            Array.Copy(data, dataCopy, data.Length);
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head + width - 1; x > head; x--) {
                        dataCopy[x] -= dataCopy[x - 1];
                    }
                    head += width;
                }
            }
            return dataCopy;
        }

        private ushort[] differencing(ushort[] data, int width, int height) {
            ushort[] dataCopy = new ushort[data.Length];
            Array.Copy(data, dataCopy, data.Length); 
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head + width - 1; x > head; x--) {
                        dataCopy[x] -= dataCopy[x - 1];
                    }
                    head += width;
                }
            }
            return dataCopy;
        }

        private float[] differencing(float[] data, int width, int height) {
            float[] dataCopy = new float[data.Length];
            Array.Copy(data, dataCopy, data.Length);
            unchecked {
                int head = 0;
                for (int y = 0; y < height; y++) {
                    for (int x = head + width - 1; x > head; x--) {
                        dataCopy[x] -= dataCopy[x - 1];
                    }
                    head += width;
                }
            }
            return dataCopy;
        }
        
        private uint toWordBoundary() {
            long pos = writer.BaseStream.Position;
            while (pos % 2 != 0) {
                writer.Write((byte)0);
                pos++;
            }
            return (uint)pos;
        }

        #endregion

        #region writer buffer

        protected const int MIN_BUFFER_SIZE = 4096;
        public uint BufferSize { get; set; }

        #endregion
    }
}
