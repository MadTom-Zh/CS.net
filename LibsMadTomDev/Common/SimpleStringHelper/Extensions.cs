using System;

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace MadTomDev.Common
{
    public static partial class Extensions
    {
        public static string ToHexString(this ulong value, bool fillZero = true)
        {
            string result = value.ToString("X");
            if (fillZero)
            {
                int len = sizeof(ulong) * 2;
                if (result.Length < len)
                {
                    len -= result.Length;
                    while (len-- > 0)
                    {
                        result = "0" + result;
                    }
                }
            }
            return result;
        }
        public static string ToHexString(this long value, bool fillZero = true)
        {
            string result = Convert.ToString(value, 16);
            if (fillZero)
            {
                int len = sizeof(long) * 2;
                if (result.Length < len)
                {
                    len -= result.Length;
                    while (len-- > 0)
                    {
                        result = "0" + result;
                    }
                }
            }
            return result;
        }
        public static string ToHexString(this byte value, bool fillZero = true)
        {
            string result = Convert.ToString(value, 16);
            if (fillZero)
            {
                int len = sizeof(byte) * 2;
                if (result.Length < len)
                {
                    len -= result.Length;
                    while (len-- > 0)
                    {
                        result = "0" + result;
                    }
                }
            }
            return result;
        }
        public static string ToHexString(this byte[] data, string spliter = null)
        {
            StringBuilder strBdr = new StringBuilder();
            if (data != null)
            {
                for (int i = 0, iv = data.Length; i < iv; i++)
                {
                    strBdr.Append(data[i].ToHexString());
                    if (spliter != null) strBdr.Append(spliter);
                }
                if (strBdr.Length > 0
                    && spliter != null && spliter.Length > 0)
                {
                    strBdr.Remove(strBdr.Length - spliter.Length, spliter.Length);
                }
            }
            return strBdr.ToString();
        }

        public static string FromBinToHexString(this string binValue)
        { return string.Format("{0:x}", Convert.ToInt32(binValue, 2)); }
        public static int FromHexToInt(this string hexStr)
        {            return  Convert.ToInt32(hexStr,16);        }
        public static string FromHexToIntString(this Int32 hexValue)
        { return Convert.ToString(hexValue, 10); }
        public static long FromBinToInt(this string binValue)
        { return Convert.ToInt64(binValue, 2); }
        public static string FromHexToBinString(this Int32 hexValue)
        { return Convert.ToString(hexValue, 2); }
        public static string ToBinString(this long value, bool fillZero = true)
        {
            string result = Convert.ToString(value, 2);
            if (fillZero)
            {
                int len = sizeof(long) * 8;
                if (result.Length < len)
                {
                    len -= result.Length;
                    while (len-- > 0)
                    {
                        result = "0" + result;
                    }
                }
            }
            return result;
        }
        public static string ToBinString(this byte value, bool fillZero = true)
        {
            string result = Convert.ToString(value, 2);
            if (fillZero)
            {
                int len = sizeof(byte) * 8;
                if (result.Length < len)
                {
                    len -= result.Length;
                    while (len-- > 0)
                    {
                        result = "0" + result;
                    }
                }
            }
            return result;
        }
        public static string ToBinString(this byte[] value, string spliter = null)
        {
            StringBuilder result = new StringBuilder();
            foreach (byte b in value)
            {
                result.Append(b.ToBinString());
                if (spliter != null)
                {
                    result.Append(spliter);
                }
            }
            if (result.Length > 0
                && spliter != null && spliter.Length > 0)
            {
                result.Remove(result.Length - spliter.Length, spliter.Length);
            }
            return result.ToString();
        }

        public static string ToARGBHexString(this Color color)
        {
            byte[] data = new byte[4] { color.A, color.R, color.G, color.B, };
            return data.ToHexString();
        }
        public static Color FromARGBHexStringToColor(this string hexStr)
        {
            byte a = byte.Parse(hexStr.Substring(0, 2), NumberStyles.HexNumber);
            byte r = byte.Parse(hexStr.Substring(2, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hexStr.Substring(4, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hexStr.Substring(6, 2), NumberStyles.HexNumber);
            return Color.FromArgb(a, r, g, b);
        }

        public static string ToShortString(this FileAttributes fileAttributes)
        {
            StringBuilder result = new StringBuilder();

            if (fileAttributes.HasFlag(FileAttributes.Archive))
                result.Append("A");
            if (fileAttributes.HasFlag(FileAttributes.Compressed))
                result.Append("C");
            if (fileAttributes.HasFlag(FileAttributes.Device))
                result.Append("Dev");
            if (fileAttributes.HasFlag(FileAttributes.Directory))
                result.Append("Dir");
            if (fileAttributes.HasFlag(FileAttributes.Encrypted))
                result.Append("E");
            if (fileAttributes.HasFlag(FileAttributes.Hidden))
                result.Append("H");
            if (fileAttributes.HasFlag(FileAttributes.IntegrityStream))
                result.Append("I");
            if (fileAttributes.HasFlag(FileAttributes.Normal))
                result.Append("Nor");
            if (fileAttributes.HasFlag(FileAttributes.NoScrubData))
                result.Append("Nsd");
            if (fileAttributes.HasFlag(FileAttributes.NotContentIndexed))
                result.Append("Nci");
            if (fileAttributes.HasFlag(FileAttributes.Offline))
                result.Append("O");
            if (fileAttributes.HasFlag(FileAttributes.ReadOnly))
                result.Append("Ro");
            if (fileAttributes.HasFlag(FileAttributes.ReparsePoint))
                result.Append("Rp");
            if (fileAttributes.HasFlag(FileAttributes.SparseFile))
                result.Append("Sf");
            if (fileAttributes.HasFlag(FileAttributes.System))
                result.Append("Sys");
            if (fileAttributes.HasFlag(FileAttributes.Temporary))
                result.Append("T");

            return result.ToString();
        }
        public static string ToShortString7(this FileAttributes fileAttributes)
        {
            StringBuilder result = new StringBuilder();

            if (fileAttributes.HasFlag(FileAttributes.Archive)) result.Append("A");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.Compressed)) result.Append("C");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.Directory)) result.Append("D");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.Encrypted)) result.Append("E");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.Hidden)) result.Append("H");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.ReadOnly)) result.Append("R");
            else result.Append("-");
            if (fileAttributes.HasFlag(FileAttributes.System)) result.Append("S");
            else result.Append("-");

            return result.ToString();
        }

        /// <summary>
        /// format as yyyy-MM-dd HH:mm:ss
        /// if msLength is greator than 0, then the millisecond will be added.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="msLength">0 - 7</param>
        /// <returns></returns>
        public static string ToStringZhStandard(this DateTime dt, int msLength = 0)
        {
            if (msLength > 7)
                msLength = 7;
            if (msLength > 0)
            {
                string f = "";
                for (int i = 0; i < msLength; i++)
                    f += "f";

                return dt.ToString("yyyy-MM-dd HH:mm:ss." + f);
            }
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool TryToPoint(this string str, out Point pt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    pt = new Point();
                    return false;
                }
                string[] parts = str.Split(',');
                pt = new Point(
                    int.Parse(parts[0]),
                    int.Parse(parts[1]));
                return true;
            }
            catch (Exception)
            {
                pt = new Point();
                return false;
            }
        }
        public static string ToString(this Point pt)
        { return $"{pt.X}, {pt.Y}"; }

        public static bool TryToSize(string str, out Size sz)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    sz = Size.Empty;
                    return false;
                }
                string[] parts = str.Split(',');
                sz = new Size(
                    int.Parse(parts[0]),
                    int.Parse(parts[1]));
                return true;
            }
            catch (Exception)
            {
                sz = Size.Empty;
                return false;
            }
        }
        public static string ToString(this Size s)
        { return $"{s.Width}, {s.Height}"; }


        public static bool TryToColor(this string str, out Color clr)
        {
            bool noPass = false;
            if (string.IsNullOrEmpty(str))
            {
                noPass = true;
            }

            string[] argb = str.Split(',');
            byte a = 0, r = 0, g = 0, b = 0;
            if (noPass || !byte.TryParse(argb[0], out a))
                noPass = true;
            if (noPass || !byte.TryParse(argb[1], out r))
                noPass = true;
            if (noPass || !byte.TryParse(argb[2], out g))
                noPass = true;
            if (noPass || !byte.TryParse(argb[3], out b))
                noPass = true;

            if (!noPass)
            {
                clr = Color.FromArgb(a, r, g, b);
                return true;
            }
            else
            {
                clr = new Color();
                return false;
            }
        }
        public static string ToString(this Color clr)
        {
            return $"{clr.A},{clr.R},{clr.G},{clr.B}";
        }
    }
}
