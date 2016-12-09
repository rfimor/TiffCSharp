using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    /// <summary>
    /// This class provides accessiblity to the basic TIFF data piece.
    /// </summary>
    public class TiffData : ICloneable
    {
        #region Tiff Data type

        public enum TIFFdataType
        {
            Byte = 1,
            Ascii = 2,
            Short = 3,
            Long = 4,
            Rational = 5,
            SignedByte = 6,
            Undefined = 7,
            SignedShort = 8,
            SignedLong = 9,
            SignedRational = 10,
            Float = 11,
            Double = 12
        }

        #endregion

        #region field

        protected TIFFdataType type;
        protected int nBytes;
        protected int count;
        protected Array content;
        protected Type dataType;

        #endregion

        #region properties

        /// <summary>
        /// Count of the directory data. Read TIFF documentation for details.
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Type of the directory data. Read TIFF documentation for details.
        /// </summary>
        public TIFFdataType TiffType
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Number of bytes of this directory data. Read TIFF documentation for details.
        /// </summary>
        public int NumBytes
        {
            get
            {
                return nBytes;
            }
        }

        /// <summary>
        /// Based C# value type of the directory data.
        /// </summary>
        public Type DataType
        {
            get
            {
                return dataType;
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Construct an instance of empty TIFFdata given type id.
        /// </summary>
        /// <param name="id">Type id.</param>
        public TiffData(TIFFdataType type)
        {
            this.type = type;
            getDataTypeInfo(type, out nBytes, out dataType);
            content = Array.CreateInstance(dataType, 0);
        }

        /// <summary>
        /// Construct an instance of empty (to be filled with the setContent() method) TIFFdata given type id and counts of data.
        /// </summary>
        /// <param name="id">Type id</param>
        /// <param name="count">Counts of data.</param>
        public TiffData(TIFFdataType type, int count)
        {
            this.type = type;
            getDataTypeInfo(type, out nBytes, out dataType);
            this.count = count;
            content = Array.CreateInstance(dataType, 0);
        }

        #endregion

        #region methods

        /// <summary>
        /// Find number of byete and underlying C# data type
        /// </summary>
        /// <param name="type">TIFF data type</param>
        /// <param name="nBytes">Size of TIFF data type in bytes</param>
        /// <param name="dataType">Underlying C# data type</param>
        public static void getDataTypeInfo(TIFFdataType type, out int nBytes, out Type dataType) {
            switch (type) {
                case TIFFdataType.Byte:
                    nBytes = 1;
                    dataType = typeof(byte);
                    break;
                case TIFFdataType.Ascii:
                    nBytes = 1;
                    dataType = typeof(char);
                    break;
                case TIFFdataType.Short:
                    nBytes = 2;
                    dataType = typeof(ushort);
                    break;
                case TIFFdataType.Long:
                    nBytes = 4;
                    dataType = typeof(uint);
                    break;
                case TIFFdataType.Rational:
                    nBytes = 8;
                    dataType = typeof(uint);
                    break;
                case TIFFdataType.SignedByte:
                    nBytes = 1;
                    dataType = typeof(sbyte);
                    break;
                case TIFFdataType.SignedShort:
                    nBytes = 2;
                    dataType = typeof(short);
                    break;
                case TIFFdataType.SignedLong:
                    nBytes = 4;
                    dataType = typeof(int);
                    break;
                case TIFFdataType.SignedRational:
                    nBytes = 8;
                    dataType = typeof(int);
                    break;
                case TIFFdataType.Float:
                    nBytes = 4;
                    dataType = typeof(float);
                    break;
                case TIFFdataType.Double:
                    nBytes = 8;
                    dataType = typeof(double);
                    break;
                default:
                    type = TIFFdataType.Undefined;
                    nBytes = 1;
                    dataType = typeof(byte);
                    break;
            }
        }

        public void read(System.IO.BinaryReader reader)
        {
            if (count <= 0) return;
            switch (type)
            {
                case TIFFdataType.Byte:
                    content = new byte[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((byte[])content)[i] = reader.ReadByte();
                    } 
                    break;
                case TIFFdataType.Ascii:
                    content = new char[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((char[])content)[i] = (char)reader.ReadByte();
                    }                    
                    break;
                case TIFFdataType.Short:
                    content = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((ushort[])content)[i] = reader.ReadUInt16();
                    }
                    break;
                case TIFFdataType.Long:
                    content = new uint[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((uint[])content)[i] = reader.ReadUInt32();
                    } 
                    break;
                case TIFFdataType.Rational:
                    content = new uint[count * 2];
                    for (int i = 0; i < 2 * count; i++)
                    {
                        ((uint[])content)[i] = reader.ReadUInt32();
                    }
                    break;
                case TIFFdataType.SignedByte:
                    content = new sbyte[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((sbyte[])content)[i] = reader.ReadSByte();
                    }
                    break;
                case TIFFdataType.SignedShort:
                    content = new short[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((short[])content)[i] = reader.ReadInt16();
                    }
                    break;
                case TIFFdataType.SignedLong:
                    content = new int[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((int[])content)[i] = reader.ReadInt32();
                    }
                    break;
                case TIFFdataType.SignedRational:
                    content = new int[count * 2];
                    for (int i = 0; i < 2 * count; i++)
                    {
                        ((int[])content)[i] = reader.ReadInt32();
                    }
                    break;
                case TIFFdataType.Float:
                    content = new float[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((float[])content)[i] = reader.ReadSingle();
                    }
                    break;
                case TIFFdataType.Double:
                    content = new double[count];
                    for (int i = 0; i < count; i++)
                    {
                        ((double[])content)[i] = reader.ReadDouble();
                    }
                    break;
                default:
                    content = new Object[0];
                    break;

            }
        }

        public void write(System.IO.BinaryWriter writer)
        {
            if (count <= 0) return;
            if (type == TIFFdataType.Byte)
            {
                byte[] boxed = (byte[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.Ascii)
            {
                char[] boxed = (char[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write((byte)(boxed[i]));
                }
            }
            else if (type == TIFFdataType.Short)
            {
                ushort[] boxed = (ushort[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.Long)
            {
                uint[] boxed = (uint[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.Rational)
            {
                uint[] boxed = (uint[])content;
                for (int i = 0; i < 2 * count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.SignedByte)
            {
                sbyte[] boxed = (sbyte[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.SignedShort)
            {
                short[] boxed = (short[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.SignedLong)
            {
                int[] boxed = (int[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.SignedRational)
            {
                int[] boxed = (int[])content;
                for (int i = 0; i < 2 * count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.Float)
            {
                float[] boxed = (float[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
            else if (type == TIFFdataType.Double)
            {
                double[] boxed = (double[])content;
                for (int i = 0; i < count; i++)
                {
                    writer.Write(boxed[i]);
                }
            }
        }

        /// <summary>
        /// Set the content.
        /// </summary>
        /// <param name="data">An array storing the content. Must be the same type as the based data type of this TIFFdata.</param>
        /// <returns>True if data is not empty and the type matches.</returns>
        public bool setContent(Array data)
        {
            if (data == null)
            {
                count = 0;
                content = Array.CreateInstance(dataType, 0);
                return true;
            }

            if (!dataType.Equals(data.GetType().GetElementType())) return false;

            int length = data.Length;
            if (type == TIFFdataType.Rational || type == TIFFdataType.SignedRational)
            {
                count = length / 2;
                if (count == 0) return false;
                length = 2 * count;
            }
            else
            {
                count = length;
            }

            content = Array.CreateInstance(dataType, length);
            Array.Copy(data, content, length);
            return true;
        }

        /// <summary>
        /// Get the content of TIFFdata.
        /// </summary>
        /// <returns>An array containing the content. </returns>
        public Array getContent()
        {
            int length = content.Length;
            Array data = Array.CreateInstance(dataType, length);

            Array.Copy(content, 0, data, 0, length);
            return data;
        }

        /// <summary>
        /// Does deep copy.
        /// </summary>
        /// <returns>A deep copy of this instance.</returns>
        public object Clone()
        {
            TiffData temp = new TiffData(this.type, this.count);
            temp.setContent(content);
            return temp;
        }

        /// <summary>
        /// Get the size of content.
        /// </summary>
        /// <returns>Length of content in bytes.</returns>
        public int dataLength()
        {
            return nBytes * count;
        }

        /// <summary>
        /// Overrided. 
        /// </summary>
        /// <returns>A string representing this instance.</returns>
        public override string ToString()
        {
            string contentString = "";
            if (content == null || content.Length == 0) return contentString;

            if (type == TIFFdataType.Ascii)
            {
                contentString = new string((char[])content);
            }
            else if (type == TIFFdataType.Rational || type == TIFFdataType.SignedRational)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    contentString += content.GetValue(i).ToString() + "/" + content.GetValue(i + 1).ToString() + ", ";
                }
                contentString += content.GetValue(2 * count - 2).ToString() + "/" + content.GetValue(2 * count - 1).ToString();
            }
            else
            {
                for (int i = 0; i < count - 1; i++)
                {
                    contentString += content.GetValue(i).ToString() + ", ";
                }
                contentString += content.GetValue(count - 1).ToString();
            }
            return "type: " + type + "\t data: " + contentString;
        }

        #endregion
    }
}
