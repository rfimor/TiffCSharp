using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;
using TiffCSharp.TiffIO;

namespace TiffCSharp.StkIO
{
    /// <summary>
    /// Derived from TIFFinfoCollection. Stores info in UIC1tag, UIC2tag, UIC3tag, UIC4tag data.
    /// </summary>
    public class StkInfoCollection : TiffInfoCollection
    {
        #region constants

        public const ushort UIC1Tag = 33628;
        public const ushort UIC2Tag = 33629;
        public const ushort UIC3Tag = 33630;
        public const ushort UIC4Tag = 33631;

        public const ushort AutoScale = 0;
        public const ushort MinScale = 1;
        public const ushort MaxScale = 2;
        public const ushort SpatialCalibration = 3;
        public const ushort XCalibration = 4;
        public const ushort YCalibration = 5;
        public const ushort CalibrationUnits = 6;
        public const ushort Name = 7;
        public const ushort ThreshState = 8;
        public const ushort ThreshStateRed = 9;
        public const ushort ThreshStateGreen = 11;
        public const ushort ThreshStateBlue = 12;
        public const ushort ThreshStateLo = 13;
        public const ushort ThreshStateHi = 14;
        public const ushort Zoom = 15;
        public const ushort CreateTime = 16;
        public const ushort LastSavedTime = 17;
        public const ushort CurrentBuffer = 18;
        public const ushort GrayFit = 19;
        public const ushort GrayPointCount = 20;
        public const ushort GrayX = 21;
        public const ushort GrayY = 22;
        public const ushort GrayMin = 23;
        public const ushort GrayMax = 24;
        public const ushort GrayUnitName = 25;
        public const ushort StandardLUT = 26;
        public const ushort WavelengthTag = 27;
        public const ushort StagePosition = 28;
        public const ushort CameraChipOffset = 29;
        public const ushort OverlayMask = 30;
        public const ushort OverlayCompress = 31;
        public const ushort Overlay = 32;
        public const ushort SpecialOverlayMask = 33;
        public const ushort SpecialOverlayCompress = 34;
        public const ushort SpecialOverlay = 35;
        public const ushort ImageProperty = 36;
        public const ushort StageLabel = 37;
        public const ushort AutoScaleLoInfo = 38;
        public const ushort AutoScaleHiInfo = 39;
        public const ushort AbsoluteZ = 40;
        public const ushort AbsoluteZValid = 41;
        public const ushort Gamma = 42;
        public const ushort GammaRed = 43;
        public const ushort GammaGreen = 44;
        public const ushort GammaBlue = 45;
        public const ushort CameraBin = 46;
        public const ushort NewLUT = 47;
        public const ushort ImagePropertyEx = 48;
        public const ushort PlaneProperty = 49;
        public const ushort UserLutTable = 50;
        public const ushort RedAutoScaleInfo = 51;
        public const ushort RedAutoScaleLoInfo = 52;
        public const ushort RedAutoScaleHiInfo = 53;
        public const ushort RedMinScaleInfo = 54;
        public const ushort RedMaxScaleInfo = 55;
        public const ushort GreenAutoScaleInfo = 56;
        public const ushort GreenAutoScaleLoInfo = 57;
        public const ushort GreenAutoScaleHiInfo = 58;
        public const ushort GreenMinScaleInfo = 59;
        public const ushort GreenMaxScaleInfo = 60;
        public const ushort BlueAutoScaleInfo = 61;
        public const ushort BlueAutoScaleLoInfo = 62;
        public const ushort BlueAutoScaleHiInfo = 63;
        public const ushort BlueMinScaleInfo = 64;
        public const ushort BlueMaxScaleInfo = 65;
        public const ushort OverlayPlaneColor = 66;

        #endregion

        #region fields

        //use List<TiffInfo> because some STRING data are for N planes, can't be hold by one TiffData.
        //Note that STRING is the only type that a TiffData object can't hold more than one instance.
        protected SortedList<uint, List<TiffInfo>> UIC1 = new SortedList<uint,List<TiffInfo>>();
        //UIC4 is data for every plane by definitation
        protected SortedList<ushort, List<TiffInfo>> UIC4 = new SortedList<ushort, List<TiffInfo>>(); 
        protected List<TiffInfo> zDistance = new List<TiffInfo>();
        protected List<TiffInfo> creationTime = new List<TiffInfo>();
        protected List<TiffInfo> modifiedTime = new List<TiffInfo>();
        protected List<TiffInfo> wavelength = new List<TiffInfo>();
        protected int numPlane = 0;

        #endregion

        #region public methods

        /// <summary>
        /// Add a plane. No meaningful UIC information for this plane.
        /// </summary>
        /// <param name="zDistance">Z distance of the plane added</param>
        /// <param name="wavelength">Wavelength of the plane added</param>
        public void addPlane() {
            numPlane++;
            zDistance.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
            creationTime.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
            modifiedTime.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
            wavelength.Add(new TiffInfo(StkInfoCollection.UIC3Tag, "", (uint)0, (uint)1));
            foreach (List<TiffInfo> v in UIC4.Values) {
                v.Add(v[v.Count - 1]);
            }
        }

        /// <summary>
        /// Remove the plane specifed by the index
        /// </summary>
        /// <param name="index">Zero-based index of the plane to be removed</param>
        public void removePlane(int index) {
            if (index < 0 || index >= numPlane) return;
            numPlane--;
            zDistance.RemoveAt(index);
            creationTime.RemoveAt(index);
            modifiedTime.RemoveAt(index);
            wavelength.RemoveAt(index);
            foreach (List<TiffInfo> v in UIC4.Values) {
                v.RemoveAt(index);
            }
        }

        /// <summary>
        /// Set the creation time of the given plane.
        /// </summary>
        /// <param name="dt">Local DateTime.</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setCreationDatetime(DateTime dt, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            long julian, millisecond;
            JulianDatetime.getSTKdatetime(dt, out julian, out millisecond);
            creationTime[planeNum] = new TiffInfo(StkInfoCollection.UIC2Tag, "Creation time", Convert.ToUInt32(julian), Convert.ToUInt32(millisecond));
        }

        /// <summary>
        /// Set the creation time of the given plane.
        /// </summary>
        /// <param name="julian">Julian Date</param>
        /// <param name="millisecond">Milliseconds</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setCreationDatetime(uint julian, uint millisecond, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            creationTime[planeNum] = new TiffInfo(StkInfoCollection.UIC2Tag, "Creation time", julian, millisecond);
        }

        /// <summary>
        /// Get the creation time of a certain plane. This info is stored in UIC2Tag data.
        /// </summary>
        /// <param name="planeNum">Zero-based index of the plane</param>
        /// <param name="local">Local DateTime.</param>
        /// <returns>True if this info is available. False otherwise.</returns>
        public bool getCreationTime(int planeNum, out DateTime local) {
            local = new DateTime();
            if (planeNum < 0 || planeNum >= creationTime.Count) return false;
            Array content = creationTime[planeNum].Data.getContent();
            if (content == null || content.Length <= 0) return false;
            long julian = Convert.ToInt64(content.GetValue(0));
            long millsec = Convert.ToInt64(content.GetValue(1));
            return JulianDatetime.getLocalTime(julian, millsec, out local);
        }

        /// <summary>
        /// Get the modification time of a certain plane.This info is stored in UIC2Tag data.
        /// </summary>
        /// <param name="planeNum">Zero-based index of the plane</param>
        /// <param name="local">Local DateTime.</param>
        /// <returns>True if this info is available. False otherwise.</returns>
        public bool getModifiedTime(int planeNum, out DateTime local) {
            local = new DateTime();
            if (planeNum < 0 || planeNum >= modifiedTime.Count) return false;
            Array content = modifiedTime[planeNum].Data.getContent();
            if (content == null || content.Length <= 0) return false;
            long julian = Convert.ToInt64(content.GetValue(0));
            long millsec = Convert.ToInt64(content.GetValue(1));
            return JulianDatetime.getLocalTime(julian, millsec, out local);
        }

        /// <summary>
        /// Set the modification time of the given plane.
        /// </summary>
        /// <param name="dt">Local DateTime.</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setModifiedDatetime(DateTime dt, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            long julian, millisecond;
            JulianDatetime.getSTKdatetime(dt, out julian, out millisecond);
            modifiedTime[planeNum] = new TiffInfo(StkInfoCollection.UIC2Tag, "Modified time", Convert.ToUInt32(julian), Convert.ToUInt32(millisecond));
        }

        /// <summary>
        /// Set the midification time of the given plane.
        /// </summary>
        /// <param name="julian">Julian Date</param>
        /// <param name="millisecond">Milliseconds</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setModifiedDatetime(uint julian, uint millisecond, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            modifiedTime[planeNum] = new TiffInfo(StkInfoCollection.UIC2Tag, "Modified time", julian, millisecond);
        }
        
        /// <summary>
        /// Get the wavelength of a certain plane.This info is stored in UIC3Tag data.
        /// </summary>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        /// <param name="num">Numerator of wavelength.</param>
        /// <param name="den">Denominator of wavelength.</param>
        /// <returns>True if this info is available. False otherwise.</returns>
        public bool getWavelength(int planeNum, out uint num, out uint den) {
            num = den = 0;
            if (wavelength == null) return false;
            if (planeNum < 0 || planeNum >= wavelength.Count) return false;
            Array content = wavelength[planeNum].Data.getContent();
            if (content == null || content.Length <= 0) return false;
            num = (uint)(content.GetValue(0));
            den = (uint)(content.GetValue(1));
            return true;
        }

        /// <summary>
        /// Set the wavelength of a certain plane.
        /// </summary>
        /// <param name="num">Numerator of wavelength.</param>
        /// <param name="den">Denominator of wavelength.</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setWavelength(uint num, uint den, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            wavelength[planeNum] = new TiffInfo(StkInfoCollection.UIC3Tag, "Wavelength", num, den);
        }

        /// <summary>
        /// Get the Z-distance of a certain plane.This info is stored in UIC2Tag data.
        /// </summary>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        /// <param name="num">Numerator of z-distance.</param>
        /// <param name="den">Denominator of z-distance</param>
        /// <returns>True if this info is available. False otherwise.</returns>
        public bool getZdistance(int planeNum, out uint num, out uint den) {
            num = den = 0;
            if (zDistance == null) return false;
            if (planeNum < 0 || planeNum >= zDistance.Count) return false;
            Array content = zDistance[planeNum].Data.getContent();
            if (content == null || content.Length <= 0) return false;
            num = (uint)(content.GetValue(0));
            den = (uint)(content.GetValue(1));
            return true;
        }

        /// <summary>
        /// Set the z-distance of a certain plane.
        /// </summary>
        /// <param name="num">Numerator of z-distance.</param>
        /// <param name="den">Denominator of z-distance.</param>
        /// <param name="planeNum">Zero-based index of the plane.</param>
        public void setZdistance(uint num, uint den, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            zDistance[planeNum] = new TiffInfo(StkInfoCollection.UIC2Tag, "ZDistance", num, den);
        }

        /// <summary>
        /// Test if UIC2tag data is valid.
        /// </summary>
        /// <returns>True if UIC2tag data exists and is in legal format. False otherwise</returns>
        public bool validUIC2tag() {
            if (zDistance.Count != numPlane || creationTime.Count != numPlane || modifiedTime.Count != numPlane) return false;
            for (int i = 0; i < numPlane; i++) {
                if (zDistance[i] == null || zDistance[i].Data.getContent().Length == 0) return false;
                if (creationTime[i] == null || creationTime[i].Data.getContent().Length == 0) return false;
                if (modifiedTime[i] == null || modifiedTime[i].Data.getContent().Length == 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Test if UIC3tag data is valid.
        /// </summary>
        /// <returns>True if UIC3tag data exists and is in legal format. False otherwise</returns>
        public bool validUIC3tag() {
            if (wavelength.Count != numPlane) return false;
            for (int i = 0; i < numPlane; i++) if (wavelength[i] == null || wavelength[i].Data.getContent().Length == 0) return false;
            return true;
        }

        /// <summary>
        /// Test if UIC4tag data is valid.
        /// </summary>
        /// <returns>True if UIC4tag data exists and is in legal format. False otherwise</returns>
        public bool validUIC4tag() {
            if (UIC4 == null) return false;
            if (UIC4.Count == 0) return false;
            foreach (ushort i in UIC4.Keys) {
                if (UIC4[i].Count < numPlane) return false;
                for (int j = 0; j < numPlane; j++) {
                    if (UIC4[i][j] == null || UIC4[i][j].Data.getContent().Length == 0) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test if UIC1tag data is valid.
        /// </summary>
        /// <returns>True if UIC1tag data exists and is in legal format. False otherwise</returns>
        public bool validUIC1tag() {
            if (UIC1 == null) return false;
            if (UIC1.Count == 0) return false;
            foreach (uint i in UIC1.Keys) {
                if (UIC1[i] == null || UIC1[i].Count == 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Forcely add absolute Z info to UIC1
        /// </summary>
        /// <param name="num">Array of the numerators of Z position.</param>
        /// <param name="denom">Array of the denominators of Z position.</param>
        /// <returns>True if successful.</returns>
        public void setAbsoluteZUIC1(uint[] num, uint[] denom) {
            if (num.Length != numPlane || denom.Length != numPlane) throw new Exception("Array lenght mismatch.");
            TiffInfo z = new TiffInfo(UIC1Tag, "Absolute Z", num, denom);
            forceAddUIC1Data(AbsoluteZ, new TiffInfo[1] { z });
            uint[] valid = new uint[numPlane];
            for (int i = 0; i < numPlane; i++) valid[i] = 1;
            TiffInfo v = new TiffInfo(UIC1Tag, "Absolute Z Validity", valid);
            forceAddUIC1Data(AbsoluteZValid, new TiffInfo[1] { v });
        }

        /// <summary>
        /// Get absolute Z info from UIC1, if exists
        /// </summary>
        /// <param name="num">Outout numerators of the abolsute Z positions</param>
        /// <param name="denom">Outout denominators of the abolsute Z positions</param>
        public void getAbsoluteZUIC1(out uint[] num, out uint[] denom) {
            List<TiffInfo> z, v;
            if (!UIC1.TryGetValue(AbsoluteZ, out z) || z.Count != 2 * numPlane) throw new Exception("No absolute Z info in UIC1.");
            if (!UIC1.TryGetValue(AbsoluteZValid, out v) || v.Count != numPlane) throw new Exception("Invalid absolute Z info in UIC1.");
            num = new uint[numPlane];
            denom = new uint[numPlane];
            var content = z[0].Data.getContent();
            var valid = v[0].Data.getContent();
            for (int i = 0; i < numPlane; i++) {
                if (Convert.ToUInt32(valid.GetValue(i)) == 0) throw new Exception("Invalid absolute Z info in UIC1.");
                num[i] = Convert.ToUInt32(content.GetValue(2 * i));
                denom[i] = Convert.ToUInt32(content.GetValue(2 * i + 1));
            }
        }

        /// <summary>
        /// Forcely add absolute Z info to UIC4
        /// </summary>
        /// <param name="num">Array of the numerators of Z position.</param>
        /// <param name="denom">Array of the denominators of Z position.</param>
        /// <returns>True if successful.</returns>
        public bool setAbsoluteZUIC4(uint[] num, uint[] denom) {
            if (num.Length != numPlane || denom.Length != numPlane) throw new Exception("Array lenght mismatch.");
            TiffInfo[] z = new TiffInfo[numPlane];
            TiffInfo[] v = new TiffInfo[numPlane];
            uint[] valid = new uint[numPlane];
            for (int i = 0; i < numPlane; i++) {
                valid[i] = 1;
                z[i] = new TiffInfo(UIC4Tag, "Absolute Z", num[i], denom[i]);
                v[i] = new TiffInfo(UIC4Tag, "Absolute Z Validity", valid[i]);
            }
            forceAddUIC4Data(AbsoluteZ, z);
            forceAddUIC4Data(AbsoluteZValid, v); 
            return true;
        }

        /// <summary>
        /// Get absolute Z info from UIC4, if exists
        /// </summary>
        /// <param name="num">Outout numerators of the abolsute Z positions</param>
        /// <param name="denom">Outout denominators of the abolsute Z positions</param>
        public void getAbsoluteZUIC4(out uint[] num, out uint[] denom) {
            List<TiffInfo> z, v;
            if (!UIC4.TryGetValue(AbsoluteZ, out z) || z.Count != numPlane) throw new Exception("No absolute Z info in UIC1.");
            if (!UIC4.TryGetValue(AbsoluteZValid, out v) || v.Count != numPlane) throw new Exception("Invalid absolute Z info in UIC1.");
            num = new uint[numPlane];
            denom = new uint[numPlane];
            for (int i = 0; i < numPlane; i++) {
                if (Convert.ToUInt32(v[i].Data.getContent().GetValue(0)) == 0) throw new Exception("Invalid absolute Z info in UIC1.");
                var content = z[i].Data.getContent();
                num[i] = Convert.ToUInt32(content.GetValue(0));
                denom[i] = Convert.ToUInt32(content.GetValue(1));
            }
        }
        
        /// <summary>
        /// Set calibration in X-direction.
        /// </summary>
        /// <param name="numerator">Numerator of the calibration value.</param>
        /// <param name="denominator">Denominator of the calibration value.</param>
        public void setXCalibration(uint numerator, uint denominator) {
            UIC1.Remove(StkInfoCollection.SpatialCalibration);
            var c = new TiffInfo(StkInfoCollection.UIC1Tag, "Calibration On/Off", (uint)1);
            var l = new List<TiffInfo>();
            l.Add(c);
            UIC1.Add(StkInfoCollection.SpatialCalibration, l);
            UIC1.Remove(StkInfoCollection.XCalibration);
            c = new TiffInfo(StkInfoCollection.UIC1Tag, "X Calibration", numerator, denominator);
            l = new List<TiffInfo>();
            l.Add(c);
            UIC1.Add(StkInfoCollection.XCalibration, l);
        }

        /// <summary>
        /// Set calibration in Y-direction.
        /// </summary>
        /// <param name="numerator">Numerator of the calibration value.</param>
        /// <param name="denominator">Denominator of the calibration value.</param>
        public void setYCalibration(uint numerator, uint denominator) {
            UIC1.Remove(StkInfoCollection.SpatialCalibration);
            var c = new TiffInfo(StkInfoCollection.UIC1Tag, "Calibration On/Off", (uint)1);
            var l = new List<TiffInfo>();
            l.Add(c);
            UIC1.Add(StkInfoCollection.SpatialCalibration, l);
            UIC1.Remove(StkInfoCollection.YCalibration);
            c = new TiffInfo(StkInfoCollection.UIC1Tag, "Y Calibration", numerator, denominator);
            l = new List<TiffInfo>();
            l.Add(c);
            UIC1.Add(StkInfoCollection.YCalibration, l);
        }

        /// <summary>
        /// Set the unit of calibration.
        /// </summary>
        /// <param name="unit">A string representing the unit.</param>
        public void setCalibrationUnit(string unit) {
            UIC1.Remove(StkInfoCollection.CalibrationUnits);
            var c = new TiffInfo(StkInfoCollection.UIC1Tag, "Calibration Unit", unit);
            var l = new List<TiffInfo>();
            l.Add(c);
            UIC1.Add(StkInfoCollection.CalibrationUnits, l);
        }

        /// <summary>
        /// Get calibration in X-direction.
        /// </summary>
        /// <param name="numerator">Numerator of the calibration value.</param>
        /// <param name="denominator">Denominator of the calibration value.</param>
        /// <returns>True if X-calibration value exists. False otherwise</returns>
        public bool getXCalibration(out uint numerator, out uint denominator) {
            numerator = denominator = 0;
            if (UIC1 == null) return false;

            List<TiffInfo> dataInfo;
            if (!UIC1.TryGetValue(StkInfoCollection.SpatialCalibration, out dataInfo) || dataInfo == null || dataInfo.Count == 0) return false;
            Array content = dataInfo[0].Data.getContent();
            if (content.Length == 0) return false;
            try {
                if ((uint)content.GetValue(0) == 0) return false;
            } catch {
                return false;
            }

            if (!UIC1.TryGetValue(StkInfoCollection.XCalibration, out dataInfo) || dataInfo == null || dataInfo.Count == 0) return false;
            content = dataInfo[0].Data.getContent();
            if (content.Length < 2) return false;

            try {
                uint[] xc = (uint[])content;
                numerator = xc[0];
                denominator = xc[1];
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Get calibration in Y-direction.
        /// </summary>
        /// <param name="numerator">Numerator of the calibration value.</param>
        /// <param name="denominator">Denominator of the calibration value.</param>
        /// <returns>True if Y-calibration value exists. False otherwise</returns>
        public bool getYCalibration(out uint numerator, out uint denominator) {
            numerator = denominator = 0;
            if (UIC1 == null) return false;

            List<TiffInfo> dataInfo;
            if (!UIC1.TryGetValue(StkInfoCollection.SpatialCalibration, out dataInfo) || dataInfo == null || dataInfo.Count == 0) return false;
            Array content = dataInfo[0].Data.getContent();
            if (content.Length == 0) return false;
            try {
                if ((uint)content.GetValue(0) == 0) return false;
            } catch {
                return false;
            }

            if (!UIC1.TryGetValue(StkInfoCollection.YCalibration, out dataInfo) || dataInfo == null || dataInfo.Count == 0) return false;
            content = dataInfo[0].Data.getContent();
            if (content.Length < 2) return false;

            try {
                uint[] xc = (uint[])content;
                numerator = xc[0];
                denominator = xc[1];
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Get the calibration unit.
        /// </summary>
        /// <param name="unit">String representing the unit.</param>
        /// <returns>True if this info exists. False otherwise.</returns>
        public bool getCalibrationUnit(out string unit) {
            unit = "";
            if (UIC1 == null) return false;
            int ixc = UIC1.IndexOfKey(StkInfoCollection.CalibrationUnits);
            if (ixc < 0) return false;
            if (UIC1.Values[ixc].Count == 0) return false;
            if (UIC1.Values[ixc][0].Data.getContent().Length < 1) return false;
            unit = new string((char[])(UIC1.Values[ixc][0].Data.getContent()));
            return true;
        }

        /// <summary>
        /// Forcely add TIFFinfo to UIC1 data
        /// </summary>
        /// <param name="key">UIC1 key</param>
        /// <param name="info">UIC1 info value</param>
        public void forceAddUIC1Data(uint key, IEnumerable<TiffInfo> info) {
            if (info == null) return;
            UIC1.Remove(key);
            UIC1.Add(key, new List<TiffInfo>(info));
        }

        /// <summary>
        /// Forcely add TIFFinfo to UIC4 data
        /// </summary>
        /// <param name="key">UIC4 key</param>
        /// <param name="info">UIC4 info value</param>
        public void forceAddUIC4Data(ushort key, TiffInfo[] info) {
            if (info == null || info.Length != numPlane) return;
            UIC4.Remove(key);
            UIC4.Add(key, new List<TiffInfo>(info));
        }

        /// <summary>
        /// Set UIC4 data for a specific plane
        /// </summary>
        /// <param name="key">UIC4 key</param>
        /// <param name="info">Tiffinfo</param>
        /// <param name="planeNum">Zero-indexed plane number</param>
        public void setUIC4Data(ushort key, TiffInfo info, int planeNum) {
            if (planeNum < 0 || planeNum >= numPlane) return;
            try {
                UIC4[key][planeNum] = info;
            } catch 
            { 
            }
        }

        /// <summary>
        /// Add TIFFinfo to UIC1 data, if the specified key doesn't exist
        /// </summary>
        /// <param name="key">UIC1 key</param>
        /// <param name="info">UIC1 info value</param>
        public void addUIC1Data(uint key, IEnumerable<TiffInfo> info) {
            if (info == null) return;
            if (UIC1.ContainsKey(key)) return;
            UIC1.Add(key, new List<TiffInfo>(info));
        }

        /// <summary>
        /// Add TIFFinfo to UIC4 data, if the specified key doesn't exist
        /// </summary>
        /// <param name="key">UIC4 key</param>
        /// <param name="info">UIC4 info value</param>
        public void addUIC4Data(ushort key, TiffInfo[] info) {
            if (info == null || info.Length != numPlane) return;
            if (UIC4.ContainsKey(key)) return;
            UIC4.Add(key, new List<TiffInfo>(info));
        }
        
        /// <summary>
        /// Overrided. Converting all the info to a string.
        /// </summary>
        /// <returns>All info in a string.</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            sw.WriteLine(base.ToString());

            for (int i = 0; i < zDistance.Count; i++) {
                uint num, den;
                bool l = getZdistance(i, out num, out den);
                if (l) sw.WriteLine("Z distance on plane #" + i + ": " + num + "/" + den);
            }

            sw.WriteLine();
            for (int i = 0; i < numPlane; i++) {
                DateTime dt;
                bool l = getCreationTime(i, out dt);
                if (l) sw.WriteLine("Creation time of plane #" + i + ": " + dt);
            }

            sw.WriteLine();
            for (int i = 0; i < numPlane; i++) {
                DateTime dt;
                bool l = getModifiedTime(i, out dt);
                if (l) sw.WriteLine("Modified time of plane #" + i + ": " + dt);
            }

            sw.WriteLine();
            for (int i = 0; i < zDistance.Count; i++) {
                uint num, den;
                bool l = getWavelength(i, out num, out den);
                if (l) sw.WriteLine("Wavelength on plane #" + i + ": " + num + "/" + den);
            }

            sw.WriteLine();
            if (UIC1 != null) {
                for (int i = 0; i < UIC1.Count; i++) {
                    sw.WriteLine("STK tag: " + UIC1.Keys[i]);
                    for (int j = 0; j < UIC1.Values[i].Count; j++) sw.WriteLine((UIC1.Values[i])[j].Data.ToString());
                }
            }

            sw.WriteLine();
            if (UIC4 != null) {
                for (int i = 0; i < UIC4.Count; i++) {
                    sw.WriteLine("STK tag: " + UIC4.Keys[i]);
                    for (int j = 0; j < UIC4.Values[i].Count; j++) sw.WriteLine((UIC4.Values[i])[j].Data.ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Return a deep copy
        /// </summary>
        /// <returns>A deep copy</returns>
        public override object Clone() {
            return new StkInfoCollection((uint)numPlane, this);
        }

        #endregion

        #region properties

        public int NumberOfPlanes { get { return numPlane; } }
        internal SortedList<uint, List<TiffInfo>> UIC1DataDeepCopy {
            get {
                SortedList<uint, List<TiffInfo>> result = new SortedList<uint, List<TiffInfo>>();
                foreach (KeyValuePair<uint, List<TiffInfo>> pair in UIC1) {
                    result.Add(pair.Key, new List<TiffInfo>());
                    for (int i = 0; i < pair.Value.Count; i++) {
                        result[pair.Key].Add(pair.Value[i].Clone() as TiffInfo);
                    }
                }
                return result;
            }
        }
        internal SortedList<ushort, List<TiffInfo>> UIC4DataDeepCopy { 
            get {
                SortedList<ushort, List<TiffInfo>> result = new SortedList<ushort, List<TiffInfo>>();
                foreach (KeyValuePair<ushort, List<TiffInfo>> pair in UIC4) {
                    result.Add(pair.Key, new List<TiffInfo>());
                    for (int i = 0; i < pair.Value.Count; i++) {
                        result[pair.Key].Add(pair.Value[i].Clone() as TiffInfo);
                    }
                }
                return result;
            } 
        }
        public TiffInfo[] ZDistance { get { return zDistance.ToArray(); } }
        public TiffInfo[] CreationTime { get { return creationTime.ToArray(); } }
        public TiffInfo[] ModifiedTime { get { return modifiedTime.ToArray(); } }
        public TiffInfo[] Wavelength { get { return wavelength.ToArray(); } }

        #endregion

        #region constructors

        public StkInfoCollection()
            : base() {
        }

        public StkInfoCollection(TiffInfoCollection tifInfo)
            : base() {
            if (tifInfo is StkInfoCollection) {
                var stkInfo = tifInfo as StkInfoCollection;
                createInstance((uint)stkInfo.NumberOfPlanes, stkInfo);
            } else {
                allInfo = tifInfo.InfoCollection;
                createUICtagInfo();
            }
        }

        public StkInfoCollection(uint numPlane)
            : base() {
            this.numPlane = Convert.ToInt32(numPlane);
            createUICtagInfo();
        }

        public StkInfoCollection(uint numPlane, TiffInfoCollection tifInfo)
            : base() {
            createInstance(numPlane, tifInfo);
        }

        protected void createUICtagInfo() {
            for (uint i = 0; i < this.numPlane; i++) {
                zDistance.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
                creationTime.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
                modifiedTime.Add(new TiffInfo(StkInfoCollection.UIC2Tag, "", (uint)0, (uint)1));
                wavelength.Add(new TiffInfo(StkInfoCollection.UIC3Tag, "", (uint)0, (uint)1));
            }

            uint[] num, denom;
            num = new uint[numPlane];
            denom = new uint[numPlane];
            for (int i = 0; i < numPlane; i++) denom[i] = 1;
            setAbsoluteZUIC4(num, denom);
            setAbsoluteZUIC1(num, denom);
        }

        protected void createInstance(uint numPlane, TiffInfoCollection tifInfo) {
            allInfo = tifInfo.InfoCollection;
            this.numPlane = Convert.ToInt32(numPlane);
            StkInfoCollection stkInfo = null;
            if (tifInfo is StkInfoCollection) {
                stkInfo = tifInfo as StkInfoCollection;
            }
            if (stkInfo != null && stkInfo.NumberOfPlanes == numPlane) {
                for (uint i = 0; i < this.numPlane; i++) {
                    zDistance.Add(stkInfo.ZDistance[i].Clone() as TiffInfo);
                    creationTime.Add(stkInfo.CreationTime[i].Clone() as TiffInfo);
                    modifiedTime.Add(stkInfo.ModifiedTime[i].Clone() as TiffInfo);
                    wavelength.Add(stkInfo.Wavelength[i].Clone() as TiffInfo);
                }
                UIC1 = stkInfo.UIC1DataDeepCopy;
                UIC4 = stkInfo.UIC4DataDeepCopy;
                uint[] num, denom;
                if (!UIC4.ContainsKey(AbsoluteZ)) {
                    num = new uint[numPlane];
                    denom = new uint[numPlane];
                    for (int i = 0; i < numPlane; i++) denom[i] = 1;
                    setAbsoluteZUIC4(num, denom);
                    setAbsoluteZUIC1(num, denom);
                } else {
                    try {
                        getAbsoluteZUIC4(out num, out denom);
                        setAbsoluteZUIC1(num, denom);
                    } catch {
                        num = new uint[numPlane];
                        denom = new uint[numPlane];
                        for (int i = 0; i < numPlane; i++) denom[i] = 1;
                        setAbsoluteZUIC4(num, denom);
                        setAbsoluteZUIC1(num, denom);
                    }
                }
            } else {
                createUICtagInfo();
            }
        }

        #endregion
    }
}
