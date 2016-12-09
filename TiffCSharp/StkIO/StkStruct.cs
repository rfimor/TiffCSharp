using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;
using TiffCSharp.TiffIO;

namespace TiffCSharp.StkIO
{
    class StkStruct : TiffStruct
    {
        protected SortedList<uint, TiffData[]> UIC1data;
        protected SortedList<ushort, TiffData[]> UIC4data;
        protected TiffData UIC2data;
        protected TiffData UIC3data;
        protected int numPlane;

        internal int NumPlane
        {
            get
            {
                return numPlane;
            }
        }

        #region constructors

        internal StkStruct()
        {
        }

        internal StkStruct(long nextIFD)
            :
            base(nextIFD)
        {

        }

        #endregion

        #region override methods
        
        internal override void read(System.IO.BinaryReader reader)
        {
            preRead(reader);

            if (dirArray == null) throw new ReadFileException("Not a TIFF file.");

            TiffDirData uic2 = search(StkInfoCollection.UIC2Tag);
            if (uic2 == null) throw new ReadFileException("Not a stack file.");
            TiffDirData uic3 = search(StkInfoCollection.UIC3Tag);
            if (uic3 == null) throw new ReadFileException("Not a stack file.");

            numPlane = uic3.Data.Count;
            if (numPlane <= 0) throw new ReadFileException("Not a stack file.");

            postRead(reader);
        }

        // Not all STK file info can be read : The STK documentation I used might be out of date?
        internal override void postRead(System.IO.BinaryReader reader)
        {
            System.IO.Stream stream = reader.BaseStream;
            List<TiffDirData> dirTemp = new List<TiffDirData>();

            TiffDirData dir = search(StkInfoCollection.UIC2Tag);
            if (dir != null)
            {
                if (dir.Offset < 0) throw new ReadFileException("Corrupted stack file.");

                try
                {
                    stream.Seek(dir.Offset, System.IO.SeekOrigin.Begin);
                    UIC2data = new TiffData(TiffData.TIFFdataType.Rational, 3 * numPlane);
                    UIC2data.read(reader);
                    dirTemp.Add(dir);
                }
                catch
                {
                    throw new ReadFileException("Corrupted stack file.");
                }
            }

            dir = search(StkInfoCollection.UIC3Tag);
            if (dir != null)
            {
                try
                {
                    stream.Seek(dir.Offset, System.IO.SeekOrigin.Begin);
                    UIC3data = new TiffData(TiffData.TIFFdataType.Rational, numPlane);
                    UIC3data.read(reader);
                    dirTemp.Add(dir);
                }
                catch
                {
                    UIC3data = null;
                }
            }

            dir = search(StkInfoCollection.UIC4Tag);
            if (dir != null)
            {
                try
                {
                    stream.Seek(dir.Offset, System.IO.SeekOrigin.Begin);
                    readUIC4tagData(reader);
                    dirTemp.Add(dir);
                }
                catch
                {
                    UIC4data = null;
                }
            }

            dir = search(StkInfoCollection.UIC1Tag);
            if (dir != null)
            {
                try
                {
                    stream.Seek(dir.Offset, System.IO.SeekOrigin.Begin);
                    readUIC1tagData(reader, dir.Data.Count);
                    dirTemp.Add(dir);
                }
                catch
                {
                    UIC1data = null;
                }
            }

            for (int i = 0; i < dirArray.Length; i++)
            {
                if (dirArray[i].Tag == StkInfoCollection.UIC1Tag)
                {
                    continue;
                }
                else if (dirArray[i].Tag == StkInfoCollection.UIC2Tag)
                {
                    continue;
                }
                else if (dirArray[i].Tag == StkInfoCollection.UIC3Tag)
                {
                    continue;
                }
                else if (dirArray[i].Tag == StkInfoCollection.UIC4Tag)
                {
                    continue;
                }
                else if (dirArray[i].Offset > 0)
                {
                    try
                    {
                        stream.Seek(dirArray[i].Offset, System.IO.SeekOrigin.Begin);
                        dirArray[i].readData(reader);
                    }
                    catch
                    {
                        continue;
                    }
                }
                dirTemp.Add(dirArray[i]);
            }
            dirArray = dirTemp.ToArray();
            this.sort();
        }

        internal override void write(System.IO.BinaryWriter writer, out long nextIfdPos) {
            System.IO.Stream stream = writer.BaseStream;

            nextIfdPos = -1;
            if (dirArray.Length <= 0) return;
            uint offset = Convert.ToUInt32(stream.Position + 2 + dirArray.Length * 12 + 4);
            long startOffset = offset;

            bool r1, r2, r3, r4;
            r1 = r2 = r3 = r4 = false;
            ushort dircnt = 0;

            for (int i = 0; i < dirArray.Length; i++) {
                switch (dirArray[i].Tag) {
                    case StkInfoCollection.UIC1Tag:
                        r1 = UIC1data != null && UIC1data.Count > 0;
                        dircnt++;
                        break;
                    case StkInfoCollection.UIC2Tag:
                        r2 = UIC2data.getContent().Length > 0;
                        dircnt++;
                        break;
                    case StkInfoCollection.UIC3Tag:
                        r3 = UIC3data.getContent().Length > 0;
                        dircnt++;
                        break;
                    case StkInfoCollection.UIC4Tag:
                        r4 = UIC4data != null && UIC4data.Count > 0;
                        dircnt++;
                        break;
                    default:
                        dircnt++;
                        break;
                }
            }
            writer.Write(dircnt);

            for (int i = 0; i < dirArray.Length; i++) {
                if (dirArray[i].Tag != StkInfoCollection.UIC1Tag && dirArray[i].Tag != StkInfoCollection.UIC2Tag && dirArray[i].Tag != StkInfoCollection.UIC3Tag && dirArray[i].Tag != StkInfoCollection.UIC4Tag)
                dirArray[i].write(writer, ref offset);
            }

            long currPosTemp = stream.Position;
            if (currPosTemp < startOffset) {
                byte[] space = new byte[startOffset - currPosTemp];
                writer.Write(space);
            } else if (currPosTemp > startOffset) {
                throw new WriteFileException("Offset calculation failed.");
            }

            for (int i = 0; i < dirArray.Length; i++) {
                if (dirArray[i].Tag == StkInfoCollection.UIC1Tag || dirArray[i].Tag == StkInfoCollection.UIC2Tag ||
                    dirArray[i].Tag == StkInfoCollection.UIC3Tag || dirArray[i].Tag == StkInfoCollection.UIC4Tag) continue;
                if (dirArray[i].Data.dataLength() > 4) dirArray[i].Data.write(writer);
            }


            stream.Seek(currPosTemp, System.IO.SeekOrigin.Begin);
            if (r1) {
                writer.Write(Convert.ToUInt16(StkInfoCollection.UIC1Tag));
                writer.Write(Convert.ToUInt16(4));
                long offtemp = stream.Position;

                stream.Seek(offset, System.IO.SeekOrigin.Begin);
                long[] uic1off = preWriteUIC1tagData(writer);
                long dataoff = postWriteUIC1tagData(writer, uic1off, ref offset);

                stream.Seek(offtemp, System.IO.SeekOrigin.Begin);
                writer.Write(uic1off.Length);
                writer.Write(Convert.ToUInt32(dataoff));
            }
            if (r2) {
                writer.Write(Convert.ToUInt16(StkInfoCollection.UIC2Tag));
                writer.Write(Convert.ToUInt16(5));
                writer.Write(numPlane);
                writer.Write(offset);
                long offtemp = stream.Position;

                stream.Seek(offset, System.IO.SeekOrigin.Begin);
                UIC2data.write(writer);
                offset = Convert.ToUInt32(stream.Position);

                stream.Seek(offtemp, System.IO.SeekOrigin.Begin);
            }
            if (r3) {
                writer.Write(Convert.ToUInt16(StkInfoCollection.UIC3Tag));
                writer.Write(Convert.ToUInt16(5));
                writer.Write(numPlane);
                writer.Write(offset);
                long offtemp = stream.Position;

                stream.Seek(offset, System.IO.SeekOrigin.Begin);
                UIC3data.write(writer);
                offset = Convert.ToUInt32(stream.Position);

                stream.Seek(offtemp, System.IO.SeekOrigin.Begin);
            }
            if (r4) {
                writer.Write(Convert.ToUInt16(StkInfoCollection.UIC4Tag));
                writer.Write(Convert.ToUInt16(4));
                writer.Write(numPlane);
                writer.Write(offset);
                long offtemp = stream.Position;

                stream.Seek(offset, System.IO.SeekOrigin.Begin);
                writeUIC4tagData(writer, ref offset);

                stream.Seek(offtemp, System.IO.SeekOrigin.Begin);
            }
            nextIfdPos = stream.Position;
            writer.Write(Convert.ToUInt32(nextIFD));
            stream.Seek(offset, System.IO.SeekOrigin.Begin);
        }

        #endregion

        #region privates methods taking care of UIC1 and UIC4

        private void readUIC1tagData(System.IO.BinaryReader reader, int count) {
            System.IO.Stream stream = reader.BaseStream;

            UIC1data = new SortedList<uint, TiffData[]>();
            for (int i = 0; i < count; i++) {
                uint id = reader.ReadUInt32();

                // ONE LONG
                    //New Lut??
                if (id == StkInfoCollection.AutoScale || id == StkInfoCollection.MinScale || id == StkInfoCollection.MaxScale || id == StkInfoCollection.SpatialCalibration
                    || id == StkInfoCollection.ThreshState || id == StkInfoCollection.ThreshStateRed || id == StkInfoCollection.ThreshStateBlue || id == StkInfoCollection.ThreshStateGreen
                    || id == StkInfoCollection.ThreshStateLo || id == StkInfoCollection.ThreshStateHi || id == StkInfoCollection.Zoom || id == StkInfoCollection.CurrentBuffer
                    || id == StkInfoCollection.GrayFit || id == StkInfoCollection.GrayPointCount || id == StkInfoCollection.WavelengthTag
                    || id == StkInfoCollection.RedAutoScaleInfo || id == StkInfoCollection.RedMinScaleInfo || id == StkInfoCollection.RedMaxScaleInfo 
                    || id == StkInfoCollection.GreenAutoScaleInfo || id == StkInfoCollection.GreenMinScaleInfo || id == StkInfoCollection.GreenMaxScaleInfo
                    || id == StkInfoCollection.BlueAutoScaleInfo || id == StkInfoCollection.BlueMinScaleInfo || id == StkInfoCollection.BlueMaxScaleInfo
                    || id == StkInfoCollection.GrayUnitName || id == StkInfoCollection.StandardLUT || id == StkInfoCollection.NewLUT
                    || id == 10) {
                    TiffData data = new TiffData(TiffData.TIFFdataType.Long, 1);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                }
                    // ONE RATIONAL
                else if (id == StkInfoCollection.XCalibration || id == StkInfoCollection.YCalibration || id == StkInfoCollection.CreateTime || id == StkInfoCollection.LastSavedTime
                    || id == StkInfoCollection.GrayX || id == StkInfoCollection.GrayY || id == StkInfoCollection.GrayMin || id == StkInfoCollection.GrayMax
                    || id == StkInfoCollection.AutoScaleLoInfo || id == StkInfoCollection.AutoScaleHiInfo
                    || id == StkInfoCollection.RedAutoScaleLoInfo || id == StkInfoCollection.RedAutoScaleHiInfo
                    || id == StkInfoCollection.GreenAutoScaleLoInfo || id == StkInfoCollection.GreenAutoScaleHiInfo
                    || id == StkInfoCollection.BlueAutoScaleHiInfo || id == StkInfoCollection.BlueAutoScaleLoInfo) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Rational, 1);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // 2*N LONG (N RATIONALs)
                else if (id == StkInfoCollection.AbsoluteZ) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Rational, numPlane);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // N LONGs
                else if (id == StkInfoCollection.AbsoluteZValid) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Long, numPlane);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // STRING
                  else if (id == StkInfoCollection.CalibrationUnits || id == StkInfoCollection.Name) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Ascii, reader.ReadInt32());
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // OFFSET OF A LONG
                  else if (id == StkInfoCollection.Gamma || id == StkInfoCollection.GammaRed || id == StkInfoCollection.GammaGreen || id == StkInfoCollection.GammaBlue) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Long, 1);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // RGB TABLE
                  else if (id == StkInfoCollection.UserLutTable) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Byte, 768);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // 4*N long (2*N RATIONALS)
                  else if (id == StkInfoCollection.StagePosition || id == StkInfoCollection.CameraChipOffset) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                    TiffData data = new TiffData(TiffData.TIFFdataType.Rational, 2 * numPlane);
                    data.read(reader);
                    UIC1data.Add(id, new TiffData[1] { data });
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
                    // IGNORED FOR NOW
                  else if (id == StkInfoCollection.CameraBin || id == StkInfoCollection.ImagePropertyEx || id == StkInfoCollection.OverlayPlaneColor) {
                    reader.ReadUInt32();
                } 
                // N STRINGs
                else if (id == StkInfoCollection.StageLabel) {
                    long offset = reader.ReadUInt32();
                    long currPos = stream.Position;
                    if (offset >= 0) {
                        try {
                            stream.Seek(offset, System.IO.SeekOrigin.Begin);
                            uint[] pos = new uint[numPlane];
                            TiffData[] data = new TiffData[numPlane];
                            stream.Seek(reader.ReadUInt32(), System.IO.SeekOrigin.Begin);
                            for (int x = 0; x < numPlane; x++) {
                                data[x] = new TiffData(TiffData.TIFFdataType.Ascii, reader.ReadInt32());
                            }
                            UIC1data.Add(id, data);
                        } catch {
                        } finally {
                            stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                        }
                    }
                }
                    // ignore undefined tags
                    else {
                    return;
                }
            }
        }

        private void readUIC4tagData(System.IO.BinaryReader reader)
        {
            System.IO.Stream stream = reader.BaseStream;

            UIC4data = new SortedList<ushort, TiffData[]>();
            ushort id = reader.ReadUInt16();

            while (id > 0)
            {
                // UIC4 2*(# OF PLANES) RATIONALS
                if (id == StkInfoCollection.StagePosition || id == StkInfoCollection.CameraChipOffset)
                {
                    TiffData[] data = new TiffData[numPlane];
                    for (int i = 0; i < numPlane; i++ )
                    {
                        data[i] = new TiffData(TiffData.TIFFdataType.Rational, 2);
                        data[i].read(reader);
                    }
                    UIC4data.Add(id, data);
                }
                // UIC4 (# OF PLANES) STRINGS
                else if (id == StkInfoCollection.StageLabel)
                {
                    TiffData[] data = new TiffData[numPlane];
                    for (int i = 0; i < numPlane; i++)
                    {
                        data[i] = new TiffData(TiffData.TIFFdataType.Ascii, reader.ReadInt32());
                        data[i].read(reader);
                    }
                    UIC4data.Add(id, data);
                }
                // UIC4 (# OF PLANES) RATIONALS
                else if (id == StkInfoCollection.AbsoluteZ || id == StkInfoCollection.CameraBin)
                {
                    TiffData[] data = new TiffData[numPlane];
                    for (int i = 0; i < numPlane; i++)
                    {
                        data[i] = new TiffData(TiffData.TIFFdataType.Rational, 1);
                        data[i].read(reader);
                    }
                    UIC4data.Add(id, data);
                }
                // UIC4 (# OF PLANES) LONGS
                else if (id == StkInfoCollection.AbsoluteZValid)
                {
                    TiffData[] data = new TiffData[numPlane];
                    for (int i = 0; i < numPlane; i++)
                    {
                        data[i] = new TiffData(TiffData.TIFFdataType.Long, 1);
                        data[i].read(reader);
                    }
                    UIC4data.Add(id, data);
                }
                // ignore any other tags
                else
                {
                    return;
                }
                id = reader.ReadUInt16();
            }
        }

        private long[] preWriteUIC1tagData(System.IO.BinaryWriter writer)
        {
            System.IO.Stream stream = writer.BaseStream;

            long[] offset = new long[UIC1data.Count];
            for (int i = 0; i < UIC1data.Count; i++)
            {
                uint id = UIC1data.Keys[i];

                // ONE LONG
                if (id == StkInfoCollection.AutoScale || id == StkInfoCollection.MinScale || id == StkInfoCollection.MaxScale || id == StkInfoCollection.SpatialCalibration
                    || id == StkInfoCollection.ThreshState || id == StkInfoCollection.ThreshStateRed || id == StkInfoCollection.ThreshStateBlue || id == StkInfoCollection.ThreshStateGreen
                    || id == StkInfoCollection.ThreshStateLo || id == StkInfoCollection.ThreshStateHi || id == StkInfoCollection.Zoom || id == StkInfoCollection.CurrentBuffer
                    || id == StkInfoCollection.GrayFit || id == StkInfoCollection.GrayPointCount || id == StkInfoCollection.WavelengthTag
                    || id == StkInfoCollection.RedAutoScaleInfo || id == StkInfoCollection.RedMinScaleInfo || id == StkInfoCollection.RedMaxScaleInfo
                    || id == StkInfoCollection.GreenAutoScaleInfo || id == StkInfoCollection.GreenMinScaleInfo || id == StkInfoCollection.GreenMaxScaleInfo
                    || id == StkInfoCollection.BlueAutoScaleInfo || id == StkInfoCollection.BlueMinScaleInfo || id == StkInfoCollection.BlueMaxScaleInfo
                    || id == StkInfoCollection.GrayUnitName || id == StkInfoCollection.StandardLUT || id == StkInfoCollection.NewLUT
                    || id == 10)
                {
                    //do nothing, write data later
                    offset[i] = -1;
                }
                    // ONE RATIONAL or 2*N LONG (N RATIONALs) or N Longs
                else if (id == StkInfoCollection.XCalibration || id == StkInfoCollection.YCalibration || id == StkInfoCollection.CreateTime || id == StkInfoCollection.LastSavedTime
                    || id == StkInfoCollection.GrayX || id == StkInfoCollection.GrayY || id == StkInfoCollection.GrayMin || id == StkInfoCollection.GrayMax
                    || id == StkInfoCollection.AutoScaleLoInfo || id == StkInfoCollection.AutoScaleHiInfo 
                    || id == StkInfoCollection.RedAutoScaleLoInfo || id == StkInfoCollection.RedAutoScaleHiInfo
                    || id == StkInfoCollection.GreenAutoScaleLoInfo || id == StkInfoCollection.GreenAutoScaleHiInfo
                    || id == StkInfoCollection.BlueAutoScaleHiInfo || id == StkInfoCollection.BlueAutoScaleLoInfo
                    || id == StkInfoCollection.AbsoluteZ || id == StkInfoCollection.AbsoluteZValid || id == StkInfoCollection.StagePosition || id == StkInfoCollection.CameraChipOffset)
                {
                    offset[i] = stream.Position;
                    TiffData data = UIC1data.Values[i][0];
                    data.write(writer);
                }
                // STRING
                else if (id == StkInfoCollection.CalibrationUnits || id == StkInfoCollection.Name)
                {
                    offset[i] = stream.Position;
                    TiffData data = UIC1data.Values[i][0];
                    writer.Write(data.Count);
                    data.write(writer);
                }
                // OFFSET OF A LONG
                else if (id == StkInfoCollection.Gamma || id == StkInfoCollection.GammaRed || id == StkInfoCollection.GammaGreen || id == StkInfoCollection.GammaBlue)
                {
                    offset[i] = stream.Position;
                    TiffData data = UIC1data.Values[i][0];
                    data.write(writer);
                }
                // RGB TABLE
                else if (id == StkInfoCollection.UserLutTable)
                {
                    offset[i] = stream.Position;
                    TiffData data = UIC1data.Values[i][0];
                    data.write(writer);
                }
                // IGNORED FOR NOW
                else if (id == StkInfoCollection.CameraBin || id == StkInfoCollection.ImagePropertyEx || id == StkInfoCollection.OverlayPlaneColor)
                {
                    offset[i] = -2;
                }
                    // N Strings
                else if (id == StkInfoCollection.StageLabel) {
                    offset[i] = stream.Position;
                    for (int j = 0; j < UIC1data.Values[i].Length; j++) {
                        TiffData data = UIC1data.Values[i][j];
                        writer.Write(data.Count);
                        data.write(writer);
                    }
                }
                    //IDs not known how to write
                else
                {
                    offset[i] = -3;
                }
            }
            return offset;
        }

        private long postWriteUIC1tagData(System.IO.BinaryWriter writer, long[] offset, ref uint endOff)
        {
            System.IO.Stream stream = writer.BaseStream;

            long initOff = stream.Position;
            for (int i = 0; i < UIC1data.Count; i++)
            {
                uint id = UIC1data.Keys[i];

                if (i >= offset.Length)
                {
                    endOff = Convert.ToUInt32(stream.Position);
                    return initOff;
                }

                if (offset[i] < -1) continue; 
                
                writer.Write(id);
                if (offset[i] > 0)
                {
                    writer.Write(Convert.ToUInt32(offset[i]));
                }
                else if (offset[i] == -1)
                {
                    //One LONG
                    writer.Write(Convert.ToInt32((UIC1data.Values[i][0].getContent().GetValue(0))));
                }
            }
            endOff = Convert.ToUInt32(stream.Position);
            return initOff;
        }

        private void writeUIC4tagData(System.IO.BinaryWriter writer, ref uint offset)
        {
            System.IO.Stream stream = writer.BaseStream;

            foreach (KeyValuePair<ushort, TiffData[]> pair in UIC4data)
            {
                ushort id = pair.Key;
                // UIC4 2*(# OF PLANES) RATIONALS
                if (id == StkInfoCollection.StagePosition || id == StkInfoCollection.CameraChipOffset)
                {
                    writer.Write(Convert.ToUInt16(id));
                    for (int j = 0; j < numPlane; j++)
                    {
                        (pair.Value)[j].write(writer);
                    }
                }
                // UIC4 (# OF PLANES) STRINGS
                else if (id == StkInfoCollection.StageLabel)
                {
                    writer.Write(Convert.ToUInt16(id));
                    for (int j = 0; j < numPlane; j++)
                    {
                        writer.Write((pair.Value)[j].Count);
                        (pair.Value)[j].write(writer);
                    }
                }
                // UIC4 (# OF PLANES) RATIONALS
                else if (id == StkInfoCollection.AbsoluteZ || id == StkInfoCollection.CameraBin)
                {
                    writer.Write(Convert.ToUInt16(id));
                    for (int j = 0; j < numPlane; j++)
                    {
                        (pair.Value)[j].write(writer);
                    }
                }
                // UIC4 (# OF PLANES) LONGS
                else if (id == StkInfoCollection.AbsoluteZValid)
                {
                    writer.Write(Convert.ToUInt16(id));
                    for (int j = 0; j < numPlane; j++)
                    {
                        (pair.Value)[j].write(writer);
                    }
                }
            }
            writer.Write(Convert.ToUInt16(0));
            offset = Convert.ToUInt32(stream.Position);
        }

        internal override void setFromInfoCollection(TiffInfoCollection info)
        {
            base.setFromInfoCollection(info);

            StkInfoCollection allInfo = info as StkInfoCollection;
            if (allInfo == null) return;

            List<TiffDirData> dirTemp = new List<TiffDirData>();

            numPlane = allInfo.NumberOfPlanes;

            uint[] uic2 = new uint[6 * numPlane];

            if (allInfo.validUIC2tag())
            {
                for (int i = 0; i < Math.Min(numPlane, allInfo.ZDistance.Length); i++)
                {
                    if (allInfo.ZDistance[i] != null) {
                        Array content = allInfo.ZDistance[i].Data.getContent();
                        if (content == null || content.Length <= 0) continue;
                        uint[] temp = (uint[])content;
                        uic2[6 * i] = temp[0];
                        uic2[6 * i + 1] = temp[1];
                    } else {
                        uic2[6 * i] = 0;
                        uic2[6 * i + 1] = 0;
                    }
                }
                for (int i = 0; i < Math.Min(numPlane, allInfo.CreationTime.Length); i++)
                {
                    if (allInfo.CreationTime[i] != null) {
                        Array content = allInfo.CreationTime[i].Data.getContent();
                        if (content == null || content.Length <= 0) continue;
                        uint[] temp = (uint[])content;
                        uic2[6 * i + 2] = temp[0];
                        uic2[6 * i + 3] = temp[1];
                    } else {
                        uic2[6 * i + 2] = 0;
                        uic2[6 * i + 3] = 0;
                    }
                }
                for (int i = 0; i < Math.Min(numPlane, allInfo.ModifiedTime.Length); i++)
                {
                    if (allInfo.ModifiedTime[i] != null) {
                        Array content = allInfo.ModifiedTime[i].Data.getContent();
                        if (content == null || content.Length <= 0) continue;
                        uint[] temp = (uint[])(content);
                        uic2[6 * i + 4] = temp[0];
                        uic2[6 * i + 5] = temp[1];
                    } else {
                        uic2[6 * i + 4] = 0;
                        uic2[6 * i + 5] = 0;
                    }
                }
            }

            UIC2data = new TiffData(TiffData.TIFFdataType.Rational);
            UIC2data.setContent(uic2);
            TiffDirData ddTemp = new TiffDirData();
            ddTemp.Tag = StkInfoCollection.UIC2Tag;
            ddTemp.Data = new TiffData(TiffData.TIFFdataType.Rational, numPlane);
            dirTemp.Add(ddTemp);


            if (allInfo.validUIC3tag())
            {
                uint[] uic3 = new uint[2 * numPlane];

                for (int i = 0; i < numPlane; i++)
                {
                    if (allInfo.Wavelength[i] != null) {
                        uint[] temp = (uint[])(allInfo.Wavelength[i].Data.getContent());
                        uic3[2 * i] = temp[0];
                        uic3[2 * i + 1] = temp[1];
                    } else {
                        uic3[2 * i] = 0;
                        uic3[2 * i + 1] = 0;
                    }

                }
                UIC3data = new TiffData(TiffData.TIFFdataType.Rational);
                UIC3data.setContent(uic3);
                ddTemp = new TiffDirData();
                ddTemp.Tag = StkInfoCollection.UIC3Tag;
                ddTemp.Data = new TiffData(TiffData.TIFFdataType.Rational, numPlane);
                dirTemp.Add(ddTemp);
            }

            UIC4data = new SortedList<ushort, TiffData[]>();
            if (allInfo.validUIC4tag())
            {
                ddTemp = new TiffDirData();
                ddTemp.Tag = StkInfoCollection.UIC4Tag;
                ddTemp.Data = new TiffData(TiffData.TIFFdataType.Byte, numPlane);
                dirTemp.Add(ddTemp);

                foreach (KeyValuePair<ushort, List<TiffInfo>> pair in allInfo.UIC4DataDeepCopy)
                {
                    if (pair.Value.Count != numPlane) continue;
                    TiffData[] temp = new TiffData[numPlane];
                    for (int j = 0; j < numPlane; j++)
                    {
                        temp[j] = (pair.Value)[j].Data;
                    }
                    UIC4data.Add(pair.Key, temp);
                }
            }

            UIC1data = new SortedList<uint, TiffData[]>();
            if (allInfo.validUIC1tag())
            {
                ddTemp = new TiffDirData();
                ddTemp.Tag = StkInfoCollection.UIC1Tag;
                SortedList<uint, List<TiffInfo>> uic1Copy = allInfo.UIC1DataDeepCopy;
                ddTemp.Data = new TiffData(TiffData.TIFFdataType.Byte, uic1Copy.Count);
                dirTemp.Add(ddTemp);

                foreach (KeyValuePair<uint, List<TiffInfo>> pair in uic1Copy)
                {
                    TiffData[] temp = new TiffData[pair.Value.Count];
                    for (int i = 0; i < pair.Value.Count; i++) {
                        temp[i] = pair.Value[i].Data;
                    }
                    UIC1data.Add(pair.Key, temp);
                }
            }

            TiffDirData[] newDir = new TiffDirData[dirArray.Length + dirTemp.Count];
            for (int i = 0; i < dirArray.Length; i++) newDir[i] = dirArray[i];
            for (int i = dirArray.Length; i < newDir.Length; i++) newDir[i] = dirTemp[i - dirArray.Length];
            dirArray = newDir;
            Array.Sort(dirArray);
        }

        internal override TiffInfoCollection toInfoCollection()
        {
            StkInfoCollection infoC = new StkInfoCollection(Convert.ToUInt32(numPlane), base.toInfoCollection());

            TiffDirData uic2test = search(StkInfoCollection.UIC2Tag);
            if (uic2test == null || UIC2data == null) return infoC;

            uint[] uicContent = (uint[])UIC2data.getContent();
            if (uicContent.Length < 6 * numPlane) return infoC;

            for (int i = 0; i < numPlane; i++)
            {
                infoC.setZdistance(uicContent[6 * i], uicContent[6 * i + 1], i);
                infoC.setCreationDatetime(uicContent[6 * i + 2], uicContent[6 * i + 3], i);
                infoC.setModifiedDatetime(uicContent[6 * i + 4], uicContent[6 * i + 5], i);
            }

            TiffDirData uic3test = search(StkInfoCollection.UIC3Tag);
            if (uic3test != null && UIC3data != null)
            {
                uicContent = (uint[])UIC3data.getContent();
                if (uicContent.Length < 2 * numPlane) return infoC;

                for (int i = 0; i < numPlane; i++)
                {
                    infoC.setWavelength(uicContent[2 * i], uicContent[2 * i + 1], i);
                }
            }

            TiffDirData uic4test = search(StkInfoCollection.UIC4Tag);
            if (uic4test != null && UIC4data != null && UIC4data.Count > 0)
            {
                foreach (KeyValuePair<ushort, TiffData[]> pair in UIC4data)
                {
                    if (pair.Value.Length != numPlane) continue;
                    TiffInfo[] temp = new TiffInfo[numPlane];
                    for (int j = 0; j < numPlane; j++)
                    {
                        temp[j] = new TiffInfo(StkInfoCollection.UIC4Tag, "UIC4", (pair.Value)[j]);
                    }
                    infoC.forceAddUIC4Data(pair.Key, temp);
                }
            }

            TiffDirData uic1test = search(StkInfoCollection.UIC1Tag);
            if (uic1test != null && UIC1data != null && UIC1data.Count > 0)
            {
                foreach (KeyValuePair<uint, TiffData[]> pair in UIC1data)
                {
                    TiffInfo[] uic1info = new TiffInfo[pair.Value.Length];
                    for (int i = 0; i < pair.Value.Length; i++) {
                        uic1info[i] = new TiffInfo(StkInfoCollection.UIC1Tag, "UIC1", pair.Value[i]);
                    }
                    infoC.forceAddUIC1Data(pair.Key, uic1info);
                }
            }

            return infoC;
        }

        #endregion
    }
}

