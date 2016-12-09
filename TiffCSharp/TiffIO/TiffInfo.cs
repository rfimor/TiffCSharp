using System;
using System.Collections.Generic;
using System.Text;
using TiffCSharp;

namespace TiffCSharp.TiffIO
{
    /// <summary>
    /// This class is a piece of info, e.g., image width, image height, image compression, etc., which a TIFF file may store. 
    /// It contains the tag of the info, the name proviuded by the user, and the data in TIFFdata format. 
    /// </summary>
    public class TiffInfo : ICloneable
    {
        #region protected data

        protected ushort tag;
        protected string name;
        protected TiffData data;

        #endregion

        #region properties

        public ushort Tag { get { return tag; } set { tag = value; } }
        public string Name { get { return name; } set { name = value; } }
        public TiffData Data { get { return data; } }


        #endregion

        #region constructors

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a double data. Type ID is 12.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A double data to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, double data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Double, 1);
            this.data.setContent(new double[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a float data. Type ID is 11.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A float data to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, float data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Float, 1);
            this.data.setContent(new float[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 32-bit integer. Type ID is 9.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 32-bit integer to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, int data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.SignedLong, 1);
            this.data.setContent(new int[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 16-bit integer. Type ID is 8.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 16-bit integer to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, short data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.SignedShort, 1);
            this.data.setContent(new short[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 32-bit unsigned integer. Type ID is 4.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 32-bit unsigned integer to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, uint data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Long, 1);
            this.data.setContent(new uint[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 16-bit unsigned integer. Type ID is 3.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 16-bit unsigned integer to be boxed in the info.</param>
        public TiffInfo(ushort tag, string name, ushort data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Short, 1);
            this.data.setContent(new ushort[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a signed rational number. Type ID is 10.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="numerator">Numberator of the rational number.</param>
        /// <param name="denominator">Denominator of the rational number.</param>
        public TiffInfo(ushort tag, string name, int numerator, int denominator) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.SignedRational, 1);
            this.data.setContent(new int[2] { numerator, denominator });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains an unsigned rational number Type ID is 5.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="numerator">Numberator of the rational number.</param>
        /// <param name="denominator">Denominator of the rational number.</param>
        public TiffInfo(ushort tag, string name, uint numerator, uint denominator) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Rational, 1);
            this.data.setContent(new uint[2] { numerator, denominator });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a byte. Type ID is 1.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A byte to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, byte data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.Byte, 1);
            this.data.setContent(new byte[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a signed byte. Type ID is 6.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A signed byte to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, sbyte data) {
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = new TiffData(TiffData.TIFFdataType.SignedByte, 1);
            this.data.setContent(new sbyte[1] { data });
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a double array. Type ID is 12.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A double array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, double[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Double);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.Double);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a float array. Type ID is 11.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A double array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, float[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Float);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.Float);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 32-bit integer array. Type ID is 9.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 32-bit integer array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, int[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.SignedLong);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.SignedLong);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 16-bit integer array. Type ID is 8.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 16-bit integer array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, short[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.SignedShort);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.SignedShort);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 32-bit unsigned integer array. Type ID is 4.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 32-bit unsigned integer array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, uint[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Long);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.Long);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a 16-bit unsigned integer array. Type ID is 3.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A 16-bit unsigned integer array to be boxed into the info.</param>
        public TiffInfo(ushort tag, string name, ushort[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Short);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.Short);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a signed rational array. Type ID is 10.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="numerator">Numberators.</param>
        /// <param name="denominator">Denominators.</param>
        public TiffInfo(ushort tag, string name, int[] numerator, int[] denominator) {
            this.tag = tag;
            this.name = string.Copy(name);

            if (numerator == null || denominator == null) {
                this.data = new TiffData(TiffData.TIFFdataType.SignedRational);
                return;
            }

            int length = System.Math.Min(numerator.Length, denominator.Length);

            int[] temp = new int[length * 2];
            for (int i = 0; i < length; i++) {
                temp[i] = numerator[i];
                temp[i + 1] = denominator[i];
            }

            this.data = new TiffData(TiffData.TIFFdataType.SignedRational);
            this.data.setContent(temp);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains an unsigned rational array. Type ID is 5.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="numerator">Numberators.</param>
        /// <param name="denominator">Denominators.</param>
        public TiffInfo(ushort tag, string name, uint[] numerator, uint[] denominator) {
            this.tag = tag;
            this.name = string.Copy(name);

            if (numerator == null || denominator == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Rational);
                return;
            }

            int length = System.Math.Min(numerator.Length, denominator.Length);

            uint[] temp = new uint[length * 2];
            for (int i = 0; i < length; i++) {
                temp[2 * i] = numerator[i];
                temp[2 * i + 1] = denominator[i];
            }

            this.data = new TiffData(TiffData.TIFFdataType.Rational);
            this.data.setContent(temp);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a byte array. Type ID is 1.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A byte array tobe wrapped into the info.</param>
        public TiffInfo(ushort tag, string name, byte[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.Byte);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.Byte);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a signed byte array. Type ID is 6.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A byte array tobe wrapped into the info.</param>
        public TiffInfo(ushort tag, string name, sbyte[] data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null) {
                this.data = new TiffData(TiffData.TIFFdataType.SignedByte);
                return;
            }
            this.data = new TiffData(TiffData.TIFFdataType.SignedByte);
            this.data.setContent(data);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a string. Type ID is 2.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">A string tobe stored into the info.</param>
        public TiffInfo(ushort tag, string name, string data) {
            this.tag = tag;
            this.name = string.Copy(name);
            if (data == null || data.Length == 0) {
                this.data = new TiffData(TiffData.TIFFdataType.Ascii);
                return;
            }
            char[] temp1 = data.ToCharArray();
            char[] temp2 = new char[data.Length + 1];
            Array.Copy(temp1, temp2, data.Length);
            temp2[data.Length] = '\0';
            this.data = new TiffData(TiffData.TIFFdataType.Ascii);
            this.data.setContent(temp2);
        }

        /// <summary>
        /// Construct an instance of TIFFinfo that contains a given instance of TIFFdata.
        /// </summary>
        /// <param name="tag">Tag of info.</param>
        /// <param name="name">Name the info.</param>
        /// <param name="data">Data to be wrapped into the info.</param>
        internal TiffInfo(ushort tag, string name, TiffData data) {
            if (data == null) return;
            this.tag = tag;
            this.name = string.Copy(name);
            this.data = data.Clone() as TiffData;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Does deep copy.
        /// </summary>
        /// <returns>A deep copy of this instance.</returns>
        public object Clone() {
            TiffInfo temp = new TiffInfo(this.tag, this.name, this.data);
            return temp;
        }

        #endregion

    }
}
