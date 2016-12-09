using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    class TiffStruct
    {
        internal TiffDirData[] dirArray = new TiffDirData[0];
        protected long nextIFD = 0;

        #region constructors
        internal TiffStruct()
        {
        }

        internal TiffStruct(long nextIFD)
        {
            this.nextIFD = nextIFD;
        }

        #endregion

        internal long NextIFD
        {
            get
            {
                return nextIFD;
            }
            set
            {
                nextIFD = value;
            }
        }

        protected void sort()
        {
            if (dirArray == null) return;
            Array.Sort(dirArray);
        }

        internal virtual TiffDirData search(int tag)
        {
            if (dirArray == null) return null;
            int index = 0;
            try
            {
                index = Array.BinarySearch(dirArray, tag);
            }
            catch
            {
                index = -1;
            }
            if (index < 0) return null;
            return dirArray[index];
        }

        internal virtual Array searchData(int tag)
        {
            TiffDirData dirData = search(tag);
            if (dirData == null) return Array.CreateInstance(typeof(object), 0);
            return dirData.Data.getContent();
        }

        internal virtual void read(System.IO.BinaryReader reader)
        {
            preRead(reader);
            postRead(reader);
        }

        internal virtual void preRead(System.IO.BinaryReader reader)
        {
            System.IO.Stream stream = reader.BaseStream;

            uint num = reader.ReadUInt16();
            if (num == 0) throw new ReadFileException("Not a TIFF file.");

            List<TiffDirData> dirTemp = new List<TiffDirData>();

            long currPos = stream.Position;
            for (int i = 0; i < num; i++)
            {
                dirTemp.Add(new TiffDirData());
                try
                {
                    dirTemp[dirTemp.Count-1].read(reader);
                }
                catch
                {
                    dirTemp.RemoveAt(dirTemp.Count - 1);
                }
                finally
                {
                    currPos += 12;
                    stream.Seek(currPos, System.IO.SeekOrigin.Begin);
                }
            }
            nextIFD = reader.ReadInt32();
            dirArray = dirTemp.ToArray();
            this.sort();
        }


        internal virtual void postRead(System.IO.BinaryReader reader)
        {
            System.IO.Stream stream = reader.BaseStream;

            List<TiffDirData> dirTemp = new List<TiffDirData>();
            for (int i = 0; i < dirArray.Length; i++)
            {
                if (dirArray[i].Offset > 0)
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

        // File size limit: 4GB! Becuase offset is 32bits unsigned integer.
        internal virtual void write(System.IO.BinaryWriter writer, out long nextIfdPos) {
            System.IO.Stream stream = writer.BaseStream;

            nextIfdPos = -1;
            if (dirArray.Length <= 0) return;
            uint offset = Convert.ToUInt32(stream.Position + 2 + dirArray.Length * 12 + 4);
            writer.Write(Convert.ToUInt16(dirArray.Length));

            for (int i = 0; i < dirArray.Length; i++) {
                dirArray[i].write(writer, ref offset);
            }
            nextIfdPos = stream.Position;
            writer.Write(Convert.ToUInt32(nextIFD));
            for (int i = 0; i < dirArray.Length; i++) {
                if (dirArray[i].Data.dataLength() > 4) dirArray[i].Data.write(writer);
            }
        }
        
        internal virtual void setFromInfoCollection(TiffInfoCollection allInfo)
        {
            List<TiffDirData> dirList = new List<TiffDirData>();
            foreach (KeyValuePair<ushort, TiffInfo> pair in allInfo.InfoCollection)
            {
                if (pair.Value.Data == null) continue;
                if (pair.Value.Data.Count == 0) continue;
                TiffDirData temp = new TiffDirData();
                temp.Tag = pair.Value.Tag;
                temp.Data = pair.Value.Data.Clone() as TiffData;
                dirList.Add(temp);
            }
            dirArray = dirList.ToArray();
            sort();
        }
        
        internal virtual TiffInfoCollection toInfoCollection()
        {
            TiffInfoCollection col = new TiffInfoCollection();

            for (int i = 0; i < dirArray.Length; i++)
            {
                col.add(new TiffInfo(dirArray[i].Tag, "", dirArray[i].Data));
            }

            return col;
        }

        internal bool isempty()
        {
            return dirArray == null;
        }

    }
}
