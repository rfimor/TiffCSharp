using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;
using TiffCSharp.TiffIO;
using System.IO.Compression;

namespace TiffCSharp.StkIO
{
    /// <summary>
    /// Derived from TIFFwriter. Can write multiple images (with same size, depth, etc) as planes.
    /// </summary>
    public class StkWriter : TiffWriter
    {
        #region fields

        protected int width;
        protected int height;
        protected uint numberOfBits;
        protected int planeNum;
        protected uint stripOffset;
        protected bool isPrepared;
        protected int planeCounter;
        protected List<uint> stripOffsetsList = new List<uint>();
        protected List<uint> stripByteCounts = new List<uint>();
        protected uint numberOfRows;
        protected int numberOfStrips;
        protected bool isFloatPoint;
        protected uint byteCountsPerPlane;

        protected StkInfoCollection stkInfo;

        #endregion

        #region compression

        public override MyTiffCompression.CompressionMethod CompressMethod {
            get {
                return base.CompressMethod;
            }
            set {
                base.CompressMethod = value;
                if (compressionMethod == MyTiffCompression.CompressionMethod.LZW) HorizontalDifferencing = true;
                else HorizontalDifferencing = false;
            }
        }

        #endregion

        #region constructor

        public StkWriter()
            : base()
        {
            CompressMethod = MyTiffCompression.CompressionMethod.UNCOMPRESSED;
            HorizontalDifferencing = false;
        }

        #endregion

        #region protected methdos

        protected void prepareWriteStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType, int planeNum, ushort photometricInterpretation) {
            if (allInfo == null) return;
            stkInfo = allInfo;
            TiffWriter.getDataTypeInfo(dataType, out numberOfBits, out isFloatPoint);

            isPrepared = false;
            planeCounter = 0;
            if (width <= 0 || height <= 0 || planeNum <= 0) throw new WriteFileException("Invalid image parameters");
            this.width = width;
            this.height = height;
            this.planeNum = planeNum;

            System.IO.Stream stream = writer.BaseStream;
            prepareWriteFile();

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PhotometricInterpretation, "", photometricInterpretation));
            allInfo.add(new TiffInfo(TiffInfoCollection.XResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.YResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.ResolutionUnit, "", (ushort)1));

            if (isFloatPoint) allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.FloatPoint);
            else allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.UnsignedInteger);

            if (photometricInterpretation <= 1 || photometricInterpretation == 3) {
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)numberOfBits));
                byteCountsPerPlane = (uint)(width * height * numberOfBits / 8);
            } else if (photometricInterpretation == 2) {
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", new ushort[] { (ushort)8, (ushort)8, (ushort)8 }));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.SamplesPerPixel, "Sample per Pixel", (ushort)3));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PlanarConfiguration, "", (ushort)1));
                byteCountsPerPlane = (uint)(width * height * 3);
            } else {
                throw new Exception("Only support grayscale, palette-color, or RGB images.");
            }

            stripOffset = (uint)stream.Position;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.ImageWidth, "width", (uint)width));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.ImageLength, "height", (uint)height));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", stripOffset));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", byteCountsPerPlane));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", (uint)height));
            MyTiffCompression.setCompressionTag(allInfo, CompressMethod, CompressLevel);
            MyTiffCompression.setHorizontalDifferencing(allInfo, HorizontalDifferencing);

            int[] missing;
            if (photometricInterpretation <= 1) missing = allInfo.missingInfoGrayscale();
            else if (photometricInterpretation == 2) missing = allInfo.missingInfoRGB();
            else missing = allInfo.missingInfoPaletteColor();

            if (missing.Length > 0) {
                String msg = "Missing tags: ";
                for (int i = 0; i < missing.Length; i++) msg += missing[i] + " ";
                throw new WriteFileException(msg);
            }

            if (!allInfo.validUIC2tag()) throw new WriteFileException("Invalid UIC2 data");
            
            isPrepared = true;
        }
        
        protected void prepareWriteStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType, ushort photometricInterpretation)
        {
            if (allInfo == null) return;
            stkInfo = allInfo;
            TiffWriter.getDataTypeInfo(dataType, out numberOfBits, out isFloatPoint);

            isPrepared = false;
            planeCounter = 0;
            if (width <= 0 || height <= 0) throw new WriteFileException("Invalid image parameters");
            this.width = width;
            this.height = height;

            prepareWriteFile();
            System.IO.Stream stream = writer.BaseStream;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PhotometricInterpretation, "", photometricInterpretation));
            allInfo.add(new TiffInfo(TiffInfoCollection.XResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.YResolution, "", (uint)1, (uint)1));
            allInfo.add(new TiffInfo(TiffInfoCollection.ResolutionUnit, "", (ushort)1));

            if (isFloatPoint) allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.FloatPoint);
            else allInfo.setSampleFormat(TiffInfoCollection.SampleFormatType.UnsignedInteger); 
            
            uint nb = 0;
            if (photometricInterpretation <= 1 || photometricInterpretation == 3) {
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", (ushort)numberOfBits));
                byteCountsPerPlane = (uint)(width * height * numberOfBits / 8);
                nb = 2;
            } else if (photometricInterpretation == 2) {
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.BitsPerSample, "Bits per Sample", new ushort[] { (ushort)8, (ushort)8, (ushort)8 }));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.SamplesPerPixel, "Sample per Pixel", (ushort)3));
                allInfo.forceAdd(new TiffInfo(TiffInfoCollection.PlanarConfiguration, "", (ushort)1));
                byteCountsPerPlane = (uint)(width * height * 3);
                nb = 3;
            } else {
                throw new Exception("Only support grayscale, palette-color, or RGB images.");
            }

            stripOffset = (uint)stream.Position;

            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.ImageWidth, "width", (uint)width));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.ImageLength, "height", (uint)height));
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", stripOffset));
            stkInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", byteCountsPerPlane)); 
            allInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", (uint)height));
            MyTiffCompression.setCompressionTag(allInfo, CompressMethod, CompressLevel);
            MyTiffCompression.setHorizontalDifferencing(allInfo, HorizontalDifferencing);

            int[] missing;
            if (photometricInterpretation <= 1) missing = allInfo.missingInfoGrayscale();
            else if (photometricInterpretation == 2) missing = allInfo.missingInfoRGB();
            else missing = allInfo.missingInfoPaletteColor();

            if (missing.Length > 0) {
                String msg = "Missing tags: ";
                for (int i = 0; i < missing.Length; i++) msg += missing[i] + " ";
                throw new WriteFileException(msg);
            }

            if (!allInfo.validUIC2tag()) throw new WriteFileException("Invalid UIC2 data");

            defineStripsGrayscale(byteCountsPerPlane, (uint)height, nb, out numberOfRows, out numberOfStrips);
            stripOffsetsList.Clear();
            stripByteCounts.Clear();

            isPrepared = true;
        }

        protected uint findBitsFromData(Array data) {
            if (data is byte[]) return 8;
            else if (data is ushort[]) return 16;
            else if (data is float[]) return 32;
            else if (data is uint[]) return 32;
            else throw new WriteFileException("Unsupported data type");
        }

        protected bool isDataCompatible(Array data) {
            if (data == null || data.Length == 0) return false;
            if (isFloatPoint && !(data is float[])) return false;
            return findBitsFromData(data) == numberOfBits;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Open a STK file to write. 
        /// </summary>
        /// <param name="filePath">File path string.</param>
        public override void open(string filePath)
        {
            isPrepared = false;
            planeCounter = 0;
            base.open(filePath);
        }
        
        public void prepareWriteGrayscaleStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType, int planeNum) {
            prepareWriteStack(allInfo, width, height, dataType, planeNum, (ushort)1);
        }

        public void prepareWriteGrayscaleStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType) {
            prepareWriteStack(allInfo, width, height, dataType, (ushort)1);
        }

        public void prepareWritePaletteColorStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType, int planeNum, byte[][] colormap)
        {
            addColormapToTiffInfo(allInfo, colormap);
            prepareWriteStack(allInfo, width, height, dataType, planeNum, (ushort)3);
        }

        public void prepareWritePaletteColorStack(StkInfoCollection allInfo, int width, int height, ImageDataType dataType, byte[][] colormap) {
            addColormapToTiffInfo(allInfo, colormap);
            prepareWriteStack(allInfo, width, height, dataType, (ushort)3);
        }

        public void prepareWriteRGBstack(StkInfoCollection allInfo, int width, int height, int planeNum)
        {
            prepareWriteStack(allInfo, width, height, ImageDataType.Byte, planeNum, (ushort)2);
        }

        public void prepareWriteRGBstack(StkInfoCollection allInfo, int width, int height)
        {
            prepareWriteStack(allInfo, width, height, ImageDataType.Byte, (ushort)2);
        }
        
        /// <summary>
        /// Write a specific plane. This function should be called after calling "prepareWriteGrayscaleStack"
        /// </summary>
        /// <param name="data">Image data</param>
        /// <param name="planeID">ID of the plane to write</param>
        public void writeGrayscaleDataPlane(Array data) {
            if (!isDataCompatible(data)) throw new WriteFileException("Inconsistent data type");

            System.IO.Stream stream = writer.BaseStream;
            byte[] buffer;

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;

            if (CompressMethod == MyTiffCompression.CompressionMethod.UNCOMPRESSED) {
                buffer = new byte[BufferSize];

                int curr = 0;
                uint leftSize = byteCountsPerPlane;
                int writeSize = (int)BufferSize;
                while (leftSize > 0) {
                    if (writeSize > leftSize) writeSize = (int)leftSize;
                    Buffer.BlockCopy(data, curr, buffer, 0, writeSize);
                    curr += writeSize;
                    writer.Write(buffer, 0, writeSize);
                    leftSize -= (uint)writeSize;
                }
            } else {
                int writeSize = Convert.ToInt32(byteCountsPerPlane / height * numberOfRows);
                buffer = new byte[writeSize];
                uint leftSize = byteCountsPerPlane;

                if (HorizontalDifferencing) data = differencing2D(data, width, height);

                int curr = 0;
                while (leftSize > 0) {
                    int currWriteSize = writeSize;
                    if (currWriteSize > leftSize) currWriteSize = (int)leftSize;
                    Buffer.BlockCopy(data, curr, buffer, 0, currWriteSize);
                    curr += currWriteSize;
                    stripOffsetsList.Add((uint)stream.Position);
                    stripByteCounts.Add(Convert.ToUInt32(MyTiffCompression.compress(buffer, 0, currWriteSize, writer, CompressMethod, CompressLevel)));
                    leftSize -= (uint)currWriteSize;
                }
            }

            planeCounter++;
        }

        /// <summary>
        /// Write a specific plane. This function should be called after calling "prepareWriteRGBstack"
        /// </summary>
        /// <param name="data">Image data</param>
        /// <param name="planeID">ID of the plane to write</param>
        public void writeRGBdataPlane(byte[][] data) {
            System.IO.Stream stream = writer.BaseStream;
            byte[] buffer;

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;

            if (CompressMethod == MyTiffCompression.CompressionMethod.UNCOMPRESSED) {
                buffer = new byte[BufferSize];

                int k = 0;
                int curr = 0;
                uint leftSize = byteCountsPerPlane;
                uint writeSize = BufferSize;
                while (leftSize > 0) {
                    if (writeSize > leftSize) writeSize = leftSize;
                    for (int i = 0; i < writeSize; i++) {
                        buffer[i] = data[k++][curr / 3];
                        curr++;
                        if (k == 3) k = 0;
                    }
                    writer.Write(buffer, 0, Convert.ToInt32(writeSize));
                    leftSize -= writeSize;
                }
            } else {
                int writeSize = Convert.ToInt32(byteCountsPerPlane / height * numberOfRows);
                buffer = new byte[writeSize];
                uint leftSize = byteCountsPerPlane;

                byte[][] dataTemp = new byte[3][];
                for (int c = 0; c < 3; c++) {
                    dataTemp[c] = data[c];
                    if (HorizontalDifferencing) {
                        dataTemp[c] = differencing2D(data[c], width, height) as byte[];
                    }
                }

                int curr = 0;
                int k = 0;
                while (leftSize > 0) {
                    int currWriteSize = writeSize;
                    if (currWriteSize > leftSize) currWriteSize = (int)leftSize;
                    for (int i = 0; i < currWriteSize; i++) {
                        buffer[i] = dataTemp[k++][curr / 3];
                        curr++;
                        if (k == 3) k = 0;
                    }
                    stripOffsetsList.Add((uint)stream.Position);
                    stripByteCounts.Add(Convert.ToUInt32(MyTiffCompression.compress(buffer, 0, currWriteSize, writer, CompressMethod, CompressLevel)));
                    leftSize -= (uint)currWriteSize;
                }
            }

            planeCounter++;
        }

        /// <summary>
        /// Finalize the plane-by-plane writing process
        /// </summary>
        public void finalizeDataPlaneWriting() {
            planeNum = planeCounter;
            if (CompressMethod != MyTiffCompression.CompressionMethod.UNCOMPRESSED) {
                stkInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripOffsets, "strip Offsets", stripOffsetsList.ToArray()));
                stkInfo.forceAdd(new TiffInfo(TiffInfoCollection.StripByteCounts, "strip Byte Counts", stripByteCounts.ToArray()));
                stkInfo.forceAdd(new TiffInfo(TiffInfoCollection.RowsPerStrip, "Rows per strip", numberOfRows));
            }
            finalizeImageWriting(stkInfo, new StkStruct());
        }

        /// <summary>
        /// Write grayscale image data to a stack image. Use default info.
        /// </summary>
        /// <param name="data">Image data in plane number * image size format</param>
        public void writeGrayscaleStack(Array[] data, int width, int height) {
            if (data == null || data.Length == 0) throw new WriteFileException("Empty image data");
            writeGrayscaleStack(data, width, height, new StkInfoCollection((uint)data.Length));
        }

        /// <summary>
        /// Write RGB image data to a stack image. Use default info.
        /// </summary>
        /// <param name="data">Image data in plane number * 3 * RGB image size format</param>
        public void writeRGBstack(byte[][][] data, int width, int height)
        {
            if (data == null || data.Length == 0) throw new WriteFileException("Empty image data");
            writeRGBstack(data, width, height, new StkInfoCollection((uint)data.Length));
        }

        /// <summary>
        /// Write palette-color image data to a stack image. Use default info.
        /// </summary>
        /// <param name="data"></param>
        public void writePaletteColorStack(Array[] data, byte[][]colormap, int width, int height)
        {
            if (data == null || data.Length == 0) throw new WriteFileException("Empty image data");
            writePaletteColorStack(data, colormap, width, height, new StkInfoCollection((uint)data.Length));
        }
        
        /// <summary>
        /// Write grayscale image data to a stack image. Also test the availability of required info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in a 2D jagged array, with dimesion number of planes * (height * width).</param>
        /// <param name="allInfo">Info collection to be written in the stack file.</param>
        public void writeGrayscaleStack(Array[] data, int width, int height, StkInfoCollection allInfo) {
            if (data == null || data.Length == 0) throw new WriteFileException("Empty image data");
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");

            System.IO.Stream stream = writer.BaseStream;
            int planeNum = data.Length;
            if (planeNum != allInfo.NumberOfPlanes) {
                throw new WriteFileException("Incompatible image data and image info collection: Inconsistent plane number");
            }
            int size = width * height;

            for (int i = 0; i < planeNum; i++) {
                if (data[i] == null || data[i].Length != size) throw new WriteFileException("Invalid image data");
            }

            prepareWriteGrayscaleStack(allInfo, width, height, TiffWriter.getArrayDataType(data[0]), planeNum);
            if (!isPrepared) throw new WriteFileException("Failed to pre-write data");

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;
            
            writeGrayScaleImageDataStrips(data, byteCountsPerPlane, (uint)width, (uint)height, allInfo);
            finalizeImageWriting(allInfo, new StkStruct());
        }

        /// <summary>
        /// Write RGB image data to a stack image. Also test the availability of required info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in a 3D jagged array, with dimesion number of planes * 3 * (height * width).</param>
        /// <param name="allInfo">Info collection to be written in the stack file.</param>
        public void writeRGBstack(byte[][][] data, int width, int height, StkInfoCollection allInfo)
        {
            if (data == null) throw new WriteFileException("Invalid image data");
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");

            System.IO.Stream stream = writer.BaseStream;
            int planeNum = data.Length;
            if (planeNum != allInfo.NumberOfPlanes) {
                throw new WriteFileException("Incompatible image data and image info collection: Inconsistent plane number");
            }
            int size = width * height;

            for (int i = 0; i < planeNum; i++) {
                if (data[i] == null || data[i].Length != 3) throw new WriteFileException("Invalid image data");
                for (int k = 0; k < 3; k++) {
                    if (data[i][k].Length != size) throw new WriteFileException("Invalid image data");
                }
            }

            prepareWriteRGBstack(allInfo, width, height, planeNum);
            if (numberOfBits != 8) throw new WriteFileException("Can only write 8bit RGB image.");
            if (!isPrepared) throw new WriteFileException("Failed to pre-write data");

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;

            writeRGBImageDataStrips(data, byteCountsPerPlane, (uint)width, (uint)height, allInfo);
            finalizeImageWriting(allInfo, new StkStruct());
        }

        /// <summary>
        /// Write palette-color image data to a stack image. Also test the availability of required info. May throw WriteFileException.
        /// </summary>
        /// <param name="data">Image data in a 2D jagged array, with dimesion number of planes * (height * width).</param>
        /// <param name="allInfo">Info collection to be written in the stack file.</param>
        public void writePaletteColorStack(Array[] data, byte[][] colormap, int width, int height, StkInfoCollection allInfo)
        {
            if (data == null || data.Length == 0) throw new WriteFileException("Empty image data");
            if (allInfo == null) throw new WriteFileException("Info collection is NULL");

            System.IO.Stream stream = writer.BaseStream;
            int planeNum = data.Length;
            if (planeNum != allInfo.NumberOfPlanes) {
                throw new WriteFileException("Incompatible image data and image info collection: Inconsistent plane number");
            }
            int size = width * height;

            for (int i = 0; i < planeNum; i++) {
                if (data[i] == null || data[i].Length != size) throw new WriteFileException("Invalid image data");
            }

            prepareWritePaletteColorStack(allInfo, width, height, TiffWriter.getArrayDataType(data[0]), planeNum, colormap);
            if (!isPrepared) throw new WriteFileException("Failed to pre-write data");

            if (BufferSize < MIN_BUFFER_SIZE) BufferSize = MIN_BUFFER_SIZE;
            BufferSize = BufferSize / 4 * 4;

            writeGrayScaleImageDataStrips(data, byteCountsPerPlane, (uint)width, (uint)height, allInfo);
            finalizeImageWriting(allInfo, new StkStruct());
        }

        #endregion
    }
}
