using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    /// <summary>
    /// This class is a collection of all TIFFinfo that a TIFF file may store.
    /// </summary>
    public class TiffInfoCollection : ICloneable
    {
        #region constant definitions

        public const ushort NewSubfileType = 254;
        public const ushort SubfileType = 255;
        public const ushort ImageWidth = 256;
        public const ushort ImageLength = 257;
        public const ushort BitsPerSample = 258;
        public const ushort Compression = 259;
        public const ushort PhotometricInterpretation = 262;
        public const ushort Threshholding = 263;
        public const ushort CellWidth = 264;
        public const ushort CellLength = 265;
        public const ushort FillOrder = 266;
        public const ushort DocumentName = 269;
        public const ushort ImageDescription = 270;
        public const ushort Make = 271;
        public const ushort Model = 272;
        public const ushort StripOffsets = 273;
        public const ushort Orientation = 274;
        public const ushort SamplesPerPixel = 277;
        public const ushort RowsPerStrip = 278;
        public const ushort StripByteCounts = 279;
        public const ushort MinSampleValue = 280;
        public const ushort MaxSampleValue = 281;
        public const ushort XResolution = 282;
        public const ushort YResolution = 283;
        public const ushort PlanarConfiguration = 284;
        public const ushort PageName = 285;
        public const ushort XPosition = 286;
        public const ushort YPosition = 287;
        public const ushort FreeOffsets = 288;
        public const ushort FreeByteCounts = 289;
        public const ushort GrayResponseUnit = 290;
        public const ushort GrayResponseCurve = 291;
        public const ushort T4Options = 292;
        public const ushort T6Options = 293;
        public const ushort ResolutionUnit = 296;
        public const ushort PageNumber = 297;
        public const ushort TransferFunction = 301;
        public const ushort Software = 305;
        public const ushort DateTime = 306;
        public const ushort Artist = 315;
        public const ushort HostComputer = 316;
        public const ushort DifferencingPredictor = 317;
        public const ushort WhitePoint = 318;
        public const ushort PrimaryChromaticities = 319;
        public const ushort ColorMap = 320;
        public const ushort HalftoneHints = 321;
        public const ushort TileWidth = 322;
        public const ushort TileLength = 323;
        public const ushort TileOffsets = 324;
        public const ushort TileByteCounts = 325;
        public const ushort InkSet = 332;
        public const ushort InkNames = 333;
        public const ushort NumberOfInks = 334;
        public const ushort DotRange = 336;
        public const ushort TargetPrinter = 337;
        public const ushort ExtraSamples = 338;
        public const ushort SampleFormat = 339;
        public const ushort SMinSampleValue = 340;
        public const ushort SMaxSampleValue = 341;
        public const ushort TransferRange = 342;
        public const ushort JPEGProc = 512;
        public const ushort JPEGInterchangeFormat = 513;
        public const ushort JPEGInterchangeFormatLngth = 514;
        public const ushort JPEGRestartInterval = 515;
        public const ushort JPEGLosslessPredictors = 517;
        public const ushort JPEGPointTransforms = 518;
        public const ushort JPEGQTables = 519;
        public const ushort JPEGDCTables = 520;
        public const ushort JPEGACTables = 521;
        public const ushort YCbCrCoefficients = 529;
        public const ushort YCbCrSubSampling = 530;
        public const ushort YCbCrPositioning = 531;
        public const ushort ReferenceBlackWhite = 532;
        public const ushort Copyright = 33432;

        //Customized tags
        public const ushort ImageCorrected = 65535;
        public const ushort ScanMode = 65534;
        public const ushort OpticalResulutionXY = 65533;
        public const ushort PixelClockFrequency = 65532;
        public const ushort LineClockFrequency = 65531;

        #endregion

        #region 

        public enum SampleFormatType
        {
            UnsignedInteger,
            FloatPoint,
            Undefined
        }

        #endregion

        #region public methods

        /// <summary>
        /// Set sample format.
        /// </summary>
        /// <param name="type">Sample format to be set.</param>
        public void setSampleFormat(SampleFormatType type) {
            ushort ns = 4;
            switch (type) {
                case SampleFormatType.UnsignedInteger:
                    ns = 1;
                    break;
                case SampleFormatType.FloatPoint:
                    ns = 3;
                    break;
                default:
                    ns = 4;
                    break;
            }
            forceAdd(new TiffInfo(TiffInfoCollection.SampleFormat, "Sample Format", ns));
        }
        
        /// <summary>
        /// Get sample format.
        /// </summary>
        /// <returns>The sample format of image data.</returns>
        public SampleFormatType getSampleFormat() {
            ushort ns;
            try {
                ns = Convert.ToUInt16(getOneInfoData(TiffInfoCollection.SampleFormat).GetValue(0));
            } catch {
                ns = 4;
            }
            switch (ns) {
                case 1:
                    return SampleFormatType.UnsignedInteger;
                case 3:
                    return SampleFormatType.FloatPoint;
                default:
                    return SampleFormatType.Undefined;
            }
        }

        /// <summary>
        /// Add a piece of new info to the collection. If the info already exists, does nothing.
        /// </summary>
        /// <param name="info">New info to be added.</param>
        /// <returns>True if the info was added. False if the same info already exists.</returns>
        public bool add(TiffInfo info) {
            if (allInfo.ContainsKey(info.Tag)) return false;
            allInfo.Add(info.Tag, info);
            return true;
        }

        /// <summary>
        /// Add a piece of new info to the collection.If the info already exists, overwrite.
        /// </summary>
        /// <param name="info">New info to be added.</param>
        public void forceAdd(TiffInfo info) {
            this.remove(info.Tag);
            allInfo.Add(info.Tag, info);
        }

        /// <summary>
        /// Remove info with the given tag.
        /// </summary>
        /// <param name="tag">Tag of the info.</param>
        /// <returns>True if the info with that tag exists and removed. False otherwise.</returns>
        public bool remove(ushort tag) {
            return allInfo.Remove(tag);
        }

        /// <summary>
        /// Get a piece of info.
        /// </summary>
        /// <param name="tag">Tag of that info.</param>
        /// <returns>Info to be retrieved. Null if nonexistent.</returns>
        public TiffInfo getOneInfo(ushort tag) {
            int index = allInfo.IndexOfKey(tag);
            if (index < 0) {
                return null;
            } else {
                return allInfo.Values[index];
            }
        }

        /// <summary>
        /// Directly get the data of the info with the given tag.
        /// </summary>
        /// <param name="tag">Tag of the info.</param>
        /// <returns>Content of the info data. Empty array if nonexistent.</returns>
        public Array getOneInfoData(ushort tag) {
            TiffInfo info = getOneInfo(tag);
            if (info == null) return Array.CreateInstance(typeof(object), 0);
            return info.Data.getContent();
        }

        /// <summary>
        /// Output an array of all info.
        /// </summary>
        /// <returns>An array of all info.</returns>
        public TiffInfo[] toInfoArray() {
            TiffInfo[] array = new TiffInfo[allInfo.Count];
            for (int i = 0; i < array.Length; i++) {
                array[i] = allInfo.Values[i].Clone() as TiffInfo;
            }
            return array;
        }

        /// <summary>
        /// Test if the current collection contains minimum requirement of a grayscale image.
        /// </summary>
        /// <returns>An array contains missing info tags. Empty if none.</returns>
        public int[] missingInfoGrayscale() {
            List<int> temp = new List<int>();
            if (getOneInfo(ImageWidth) == null) temp.Add(ImageWidth);
            if (getOneInfo(ImageLength) == null) temp.Add(ImageLength);
            if (getOneInfo(BitsPerSample) == null) temp.Add(BitsPerSample);
            if (getOneInfo(Compression) == null) temp.Add(Compression);
            if (getOneInfo(PhotometricInterpretation) == null) temp.Add(PhotometricInterpretation);
            if (getOneInfo(StripOffsets) == null) temp.Add(StripOffsets);
            if (getOneInfo(RowsPerStrip) == null) temp.Add(RowsPerStrip);
            if (getOneInfo(StripByteCounts) == null) temp.Add(StripByteCounts);
            if (getOneInfo(XResolution) == null) temp.Add(XResolution);
            if (getOneInfo(YResolution) == null) temp.Add(YResolution);
            if (getOneInfo(ResolutionUnit) == null) temp.Add(ResolutionUnit);
            return temp.ToArray();
        }

        /// <summary>
        /// Test if the current collection contains minimum requirement of a RGB image.
        /// </summary>
        /// <returns>An array contains missing info tags. Empty if none.</returns>
        public int[] missingInfoRGB() {
            List<int> temp = new List<int>(missingInfoGrayscale());
            if (getOneInfo(PlanarConfiguration) == null) temp.Add(PlanarConfiguration);
            return temp.ToArray();
        }

        /// <summary>
        /// Test if the current collection contains minimum requirement of a palette-color image.
        /// </summary>
        /// <returns>An array contains missing info tags. Empty if none.</returns>
        public int[] missingInfoPaletteColor() {
            List<int> temp = new List<int>(missingInfoGrayscale());
            if (getOneInfo(ColorMap) == null) temp.Add(ColorMap);
            return temp.ToArray();
        }

        /// <summary>
        /// Set the image description (annotation) of the image.
        /// </summary>
        /// <param name="annotation">Annotation to be written in.</param>
        public void setAnnotation(string annotation) {
            forceAdd(new TiffInfo(TiffInfoCollection.ImageDescription, "Description", annotation));
        }

        /// <summary>
        /// Get the image description (annotation) of the image.
        /// </summary>
        /// <returns>Annotation string.</returns>
        public string getAnnotation() {
            Array textArray = getOneInfoData(TiffInfoCollection.ImageDescription);
            if (textArray == null || textArray.Length == 0) return "";
            string text = new string((char[])textArray);
            if (!string.IsNullOrEmpty(text)) return text.Trim((char)0);
            return "";
        }

        /// <summary>
        /// Write the current date and time to the info collection
        /// </summary>
        public void setCurrentDateTime() {
            forceAdd(new TiffInfo(TiffInfoCollection.DateTime, "CurrentDateTime", System.DateTime.Now.ToString("yyyy:MM:dd hh:mm:ss")));
        }

        /// <summary>
        /// Get the DateTime string
        /// </summary>
        /// <returns>Date time string in "yyyy:MM:dd hh:mm:ss" format</returns>
        public string getDateTimeString() {
            Array textArray = getOneInfoData(TiffInfoCollection.DateTime);
            if (textArray.Length > 0) return new string((char[])textArray);
            return "";
        }

        /// <summary>
        /// Number of info pieces.
        /// </summary>
        /// <returns>Number of info pieces.</returns>
        public int size() {
            return allInfo.Count;
        }

        /// <summary>
        /// Return Resolution(Calibration) in X direction
        /// </summary>
        /// <returns>Resolution in X: number of pixels per unit</returns>
        public double getXResolution() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.XResolution) as uint[];
                return (double)temp[0] / temp[1];
            } catch {
                return double.NaN;
            }
        }

        /// <summary>
        /// Set X resolution
        /// </summary>
        /// <param name="rx">X resolution: number of pixels per unit</param>
        public void setXResolution(double rx) {
            if (double.IsNaN(rx)) return;
            uint[] x = getURational3(rx);
            forceAdd(new TiffInfo(TiffInfoCollection.XResolution, "X Resolution", x[0], x[1]));
        }

        /// <summary>
        /// Set Y resolution
        /// </summary>
        /// <param name="ry">Y resolution: number of pixels per unit</param>
        public void setYResolution(double ry) {
            if (double.IsNaN(ry)) return;
            uint[] y = getURational3(ry);
            forceAdd(new TiffInfo(TiffInfoCollection.YResolution, "Y Resolution", y[0], y[1]));
        }

        /// <summary>
        /// Return Resolution(Calibration) in Y direction
        /// </summary>
        /// <returns>Resolution in Y: number of pixels per unit</returns>
        public double getYResolution() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.YResolution) as uint[];
                return (double)temp[0] / temp[1];
            } catch {
                return double.NaN;
            }
        }

        /// <summary>
        /// Return resolution unit.
        /// </summary>
        /// <returns>1: no unit (default). 2: inch. 3: cm. 6: micrometer. 9: nanometer.</returns>
        public uint getResolutionUnit() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.ResolutionUnit) as uint[];
                return temp[0];
            } catch {
                return 1;
            }
        }

        /// <summary>
        /// Set resolution unit
        /// </summary>
        /// <param name="r">1: no unit (default). 2: inch. 3: cm. 6: micrometer. 9: nanometer.</param>
        public void setResolutionUnit(uint r) {
            forceAdd(new TiffInfo(TiffInfoCollection.ResolutionUnit, "Resolution Unit", r));                        
        }

        /// <summary>
        /// Return optical resolution in XY
        /// </summary>
        /// <returns>Resolution in XY</returns>
        public double getOpticalResolutionXY() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.OpticalResulutionXY) as uint[];
                return (double)temp[0] / temp[1];
            } catch {
                return double.NaN;
            }
        }

        /// <summary>
        /// Return pixel clock frequency
        /// </summary>
        /// <returns>Pixel clock frequency in MHz</returns>
        public double getPixelClockFrequencyInMHz() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.PixelClockFrequency) as uint[];
                return 1.0 * temp[0] / temp[1];
            } catch (Exception ex) {
                throw new Exception("Pixel clock frequency not found", ex);
            }
        }

        /// <summary>
        /// Return line clock frequency
        /// </summary>
        /// <returns>Line clock frequency in KHz</returns>
        public double getLineClockFrequencyInKHz() {
            try {
                uint[] temp = getOneInfoData(TiffInfoCollection.LineClockFrequency) as uint[];
                return 1.0 * temp[0] / temp[1];
            } catch (Exception ex) {
                throw new Exception("Line clock frequency not found", ex);
            }
        }

        /// <summary>
        /// Set line clock frequency
        /// </summary>
        /// <param name="lineClockKHz"></param>
        public void setLineClockFrequencyInKHz(double lineClockKHz) {
            uint d = Convert.ToUInt32(lineClockKHz * 1000);
            forceAdd(new TiffInfo(TiffInfoCollection.LineClockFrequency, "Line Clock kHz", d, (uint)1000));
        }

        /// <summary>
        /// Set pixel clock frequency
        /// </summary>
        /// <param name="pxielClockMHz"></param>
        public void setPixelClockFrequencyInMHz(double pixelClockMHz) {
            uint d = Convert.ToUInt32(pixelClockMHz * 1e6);
            forceAdd(new TiffInfo(TiffInfoCollection.PixelClockFrequency, "Pixel Clock MHz", d, (uint)1000000));
        }
        
        /// <summary>
        /// Set optical resolution in XY
        /// </summary>
        /// <param name="rx">XY optical resolution</param>
        public void setOpticalResolutionXY(double rxy) {
            uint[] x = getURational3(rxy);
            forceAdd(new TiffInfo(TiffInfoCollection.OpticalResulutionXY, "Optical Resolution XY", x[0], x[1]));
        }
        
        /// <summary>
        /// Return scan mode
        /// </summary>
        /// <returns>scan mode</returns>
        public ushort getScanMode() {
            Array a = getOneInfoData(TiffInfoCollection.ScanMode);
            if (a == null || a.Length == 0) throw new Exception("Scan mode not found");
            return Convert.ToUInt16(a.GetValue(0));
        }

        /// <summary>
        /// Set scan mode info
        /// </summary>
        /// <param name="mode"></param>
        public void setScanMode(ushort mode) {
            forceAdd(new TiffInfo(TiffInfoCollection.ScanMode, "Scan mode", mode));
        }
        
        /// <summary>
        /// Return number of bytes per pixel. 1 for grey scale images, 3 for colored images.
        /// </summary>
        /// <returns>Number of bytes per pixel</returns>
        public ushort getSamplesPerPixel() {
            ushort ns;
            try {
                ns = Convert.ToUInt16(getOneInfoData(TiffInfoCollection.SamplesPerPixel).GetValue(0));
            } catch {
                ns = 1;
            }
            return ns;
        }

        /// <summary>
        /// Return colormap. Converting 16bit map to byte.
        /// </summary>
        /// <returns>Colormap in byte[3][2^Data Depth] format.</returns>
        public byte[][] getColormap() {            
            ushort[] temp = getOneInfoData(TiffInfoCollection.ColorMap) as ushort[];
            if (temp == null || temp.Length == 0) return null;
            int mlen = temp.Length / 3;
            byte[][] map = new byte[3][];
            for (int k = 0; k < 3; k++) {
                map[k] = new byte[mlen];
                for (int i = 0; i < mlen; i++) {
                    ushort x = temp[k * mlen + i];
                    if (x > byte.MaxValue) x = byte.MaxValue;
                    map[k][i] = Convert.ToByte(x);
                }
            }
            return map;
        }

        /// <summary>
        /// Set image size
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public void setImageSize(uint width, uint height) {
            forceAdd(new TiffInfo(TiffInfoCollection.ImageWidth, "Width", (uint)width));
            forceAdd(new TiffInfo(TiffInfoCollection.ImageLength, "Height", (uint)height));
        }

        /// <summary>
        /// Get image size
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public void getImageSize(out uint width, out uint height) {
            try {
                height = Convert.ToUInt32(getOneInfo(TiffInfoCollection.ImageLength).Data.getContent().GetValue(0));
            } catch {
                throw new WriteFileException("Image height info missing.");
            }
            try {
                width = Convert.ToUInt32(getOneInfo(TiffInfoCollection.ImageWidth).Data.getContent().GetValue(0));
            } catch {
                throw new WriteFileException("Image width info missing.");
            }
        }

        /// <summary>
        /// Overrided.
        /// </summary>
        /// <returns>String representing all info contained.</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);

            foreach (KeyValuePair<ushort, TiffInfo> info in allInfo) {
                sw.WriteLine("Tag: " + info.Key + "\t" + info.Value.Data.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        ///Return a deep copy of this object
        /// </summary>
        /// <returns>A deep copy</returns>
        public virtual object Clone() {
            TiffInfoCollection copy = new TiffInfoCollection();
            foreach (KeyValuePair<ushort, TiffInfo> pair in allInfo) {
                copy.allInfo.Add(pair.Key, pair.Value.Clone() as TiffInfo);
            }
            return copy;
        }

        #endregion

        protected SortedList<ushort, TiffInfo> allInfo;
        internal SortedList<ushort, TiffInfo> InfoCollection { get { return new SortedList<ushort, TiffInfo>(allInfo); } }

        public TiffInfoCollection() {
            allInfo = new SortedList<ushort, TiffInfo>();
        }

        #region helper

        private uint[] getURational3(double r) {
            uint[] temp = new uint[2] { 0, 0 };
            if (r == 0) return temp;
            if (r < 0) throw new Exception("Rational type has to be positive.");

            int multiple = 1;
            while (r < 1) {
                multiple *= 10;
                r *= 10;
            }
            uint pri = (uint)r;
            double res = r - pri;
            if (res < 0.01) {
                temp[0] = pri;
                temp[1] = (uint)multiple;
            } else {
                temp[0] = (uint)(r * 100);
                temp[1] = (uint)(100 * multiple);
            }

            return temp;
        }

        #endregion
    }
}
