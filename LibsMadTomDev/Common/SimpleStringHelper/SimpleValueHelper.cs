using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MadTomDev.Common
{
    public static partial class SimpleValueHelper
    {
		
        public enum ByteStringFormates
        {
            Bin, Dec, Oct, Hex,
        }
        public static byte TryGetByte(this object value, ByteStringFormates formate, byte defaultValue = 0)
        {
            if (value == null)
                return defaultValue;
            if (value is byte)
                return (byte)value;

            string strValue = value.ToString();

            byte result = defaultValue;
            switch (formate)
            {
                case ByteStringFormates.Bin:
                    ChopOrFillString(ref strValue, 8);
                    result = 0;
                    for (int i = 7; i >= 0; --i)
                    {
                        char c = strValue[i];
                        if (c == '0')
                        { }
                        else if (c == '1')
                        {
                            if (i == 7) result += 1;
                            else if (i == 6) result += 2;
                            else if (i == 5) result += 4;
                            else if (i == 4) result += 8;
                            else if (i == 3) result += 16;
                            else if (i == 2) result += 32;
                            else if (i == 1) result += 64;
                            else if (i == 0) result += 128;
                        }
                        else
                        {
                            result = defaultValue;
                            break;
                        }
                    }
                    break;
                case ByteStringFormates.Oct:
                    ChopOrFillString(ref strValue, 3);
                    result = 0;
                    for (int i = 2; i >= 0; --i)
                    {
                        char c = strValue[i];
                        if (c == '0')
                        { }
                        else if (c >= '1' && c <= '7')
                        {
                            int baseNum = int.Parse(c.ToString());
                            if (i == 2) result += (byte)baseNum;
                            else if (i == 1) result += (byte)(baseNum * 8);
                            else if (i == 0) result += (byte)(baseNum * 64);
                        }
                        else
                        {
                            result = defaultValue;
                            break;
                        }
                    }
                    break;
                case ByteStringFormates.Dec:
                    ChopOrFillString(ref strValue, 3);
                    if (int.TryParse(strValue, out int outInt))
                    {
                        if (outInt < 0 || outInt > 255)
                        {
                            result = defaultValue;
                        }
                        else
                        {
                            result = (byte)outInt;
                        }
                    }
                    else
                    {
                        result = defaultValue;
                    }
                    break;
                case ByteStringFormates.Hex:
                    ChopOrFillString(ref strValue, 2);
                    strValue = strValue.ToUpper();
                    for (int i = 1; i >= 0; --i)
                    {
                        char c = strValue[i];
                        if (c == '0')
                        { }
                        else if (c >= '1' && c <= '9')
                        {
                            int baseNum = int.Parse(c.ToString());
                            if (i == 1) result += (byte)baseNum;
                            else if (i == 0) result += (byte)(baseNum * 16);
                        }
                        else if (c >= 'A' && c <= 'F')
                        {
                            int baseNum = c - 'A' + 10;
                            if (i == 1) result += (byte)baseNum;
                            else if (i == 0) result += (byte)(baseNum * 16);
                        }
                        else
                        {
                            result = defaultValue;
                            break;
                        }
                    }
                    break;
            }

            return result;



            void ChopOrFillString(ref string str, int length, char filler = '0')
            {
                if (str.Length > length)
                {
                    str = str.Substring(str.Length - length, length);
                }
                else
                {
                    while (str.Length < length)
                    {
                        str = filler + str;
                    }
                }
            }
        }
        public static byte[] TryGetBytes(this object value, ByteStringFormates formate, byte defaultValue = 0)
        {
            if (value == null)
                return null;
            if (value is byte[])
                return (byte[])value;

            string[] parts = value.ToString().Split(new string[] {" ","\t","\\","/",",", ".", "-", "_", "|",":",";" },StringSplitOptions.RemoveEmptyEntries);
            List<byte> bytes = new List<byte>();
            for (int i=0,iv = parts.Count();i<iv;++i)
            {
                bytes.Add(parts[i].TryGetByte(formate,defaultValue));
            }
            return bytes.ToArray();
        }
		
		
        public static int TryGetInt(this object value, int defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            if (value is int)
                return (int)value;
            try
            {
                return int.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static long TryGetLong(this object value, long defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            if (value is long)
                return (long)value;
            try
            {
                return long.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static float TryGetFloat(this object value, float defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            if (value is float)
                return (float)value;
            try
            {
                return float.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static double TryGetDouble(this object value, double defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            if (value is double)
                return (double)value;
            try
            {
                return double.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static decimal TryGetDecimal(this object value, decimal defaultValue = -1)
        {
            if (value == null)
                return defaultValue;
            if (value is decimal)
                return (decimal)value;
            try
            {
                return decimal.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static DateTime TryGetDate(this object value)
        {
            if (value == null)
                return DateTime.MinValue;
            if (value is DateTime)
                return (DateTime)value;
            try
            {
                return DateTime.Parse(value.ToString());
            }
            catch (Exception)
            { return DateTime.MinValue; }
        }
        public static bool? TryGetBool(this object value, bool? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is bool)
                return (bool)value;
            try
            {
                return bool.Parse(value.ToString());
            }
            catch (Exception)
            { return defaultValue; }
        }

        public static System.Drawing.Point? TryGetDPoint(this object value, System.Drawing.Point? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is System.Drawing.Point)
                return (System.Drawing.Point)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new System.Drawing.Point(int.Parse(xStr.ToString()), int.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static System.Drawing.PointF? TryGetDPointF(this object value, System.Drawing.Point? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is System.Drawing.PointF)
                return (System.Drawing.PointF)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr, false);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new System.Drawing.PointF(float.Parse(xStr.ToString()), float.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static Point? TryGetWPoint(this object value, Point? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is Point)
                return (Point)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr, false);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new Point(double.Parse(xStr.ToString()), double.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        private static void GetStringPair(string source, out StringBuilder xStr, out StringBuilder yStr, bool isIntOrDouble = true)
        {
            xStr = new StringBuilder();
            yStr = new StringBuilder();
            int startIdx = 0;
            GetNumberStr(xStr, ref startIdx);
            GetNumberStr(yStr, ref startIdx);

            void GetNumberStr(StringBuilder strBdr, ref int startIdx)
            {
                bool hasNav = false;
                bool hasNum = false;
                bool hasDot = false;
                int i, iv;
                for (i = startIdx, iv = source.Length; i < iv; ++i)
                {
                    char c = source[i];
                    if ((c >= '0' && c <= '9'))
                    {
                        strBdr.Append(c);
                        hasNum = true;
                    }
                    else if (c == '-')
                    {
                        if (hasNum || hasNav)
                        {
                            break;
                        }
                        strBdr.Append(c);
                        hasNav = true;
                    }
                    else if (c == '.' && !isIntOrDouble && !hasDot)
                    {
                        if (hasDot)
                        {
                            break;
                        }
                        strBdr.Append(c);
                        hasDot = true;
                    }
                    else if (c != ' ')
                    {
                        break;
                    }
                }
                startIdx = i + 1;
            }
        }
        public static System.Drawing.Size? TryGetDSize(this object value, System.Drawing.Size? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is System.Drawing.Size)
                return (System.Drawing.Size)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new System.Drawing.Size(int.Parse(xStr.ToString()), int.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static System.Drawing.SizeF? TryGetDSizeF(this object value, System.Drawing.SizeF? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is System.Drawing.SizeF)
                return (System.Drawing.SizeF)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr, false);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new System.Drawing.SizeF(float.Parse(xStr.ToString()), float.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static Size? TryGetWSize(this object value, Size? defaultValue = null)
        {
            if (value == null)
                return defaultValue;
            if (value is Size)
                return (Size)value;
            try
            {
                string testStr = value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(testStr))
                    return defaultValue;

                StringBuilder xStr = new StringBuilder();
                StringBuilder yStr = new StringBuilder();
                GetStringPair(testStr, out xStr, out yStr, false);
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new Size(double.Parse(xStr.ToString()), double.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }


        /// <summary>
        /// try convert object-value to Guid;
        /// default value could only be Guid.Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Guid TryGetGuid(this object value)
        {
            if (value == null)
                return Guid.Empty;
            if (value is Guid)
                return (Guid)value;
            try
            {
                return Guid.Parse(value.ToString());
            }
            catch (Exception)
            { return Guid.Empty; }
        }

        public static Int64 Combine(Int32 a, Int32 b)
        {
            byte[] bytes = new byte[8];
            byte[] bytesInt = BitConverter.GetBytes(a);
            bytes[0] = bytesInt[0];
            bytes[1] = bytesInt[1];
            bytes[2] = bytesInt[2];
            bytes[3] = bytesInt[3];
            bytesInt = BitConverter.GetBytes(b);
            bytes[4] = bytesInt[0];
            bytes[5] = bytesInt[1];
            bytes[6] = bytesInt[2];
            bytes[7] = bytesInt[3];
            return BitConverter.ToInt64(bytes, 0);
        }
        public static UInt64 Combine(UInt32 a, UInt32 b)
        {
            byte[] bytes = new byte[8];
            byte[] bytesInt = BitConverter.GetBytes(a);
            bytes[0] = bytesInt[0];
            bytes[1] = bytesInt[1];
            bytes[2] = bytesInt[2];
            bytes[3] = bytesInt[3];
            bytesInt = BitConverter.GetBytes(b);
            bytes[4] = bytesInt[0];
            bytes[5] = bytesInt[1];
            bytes[6] = bytesInt[2];
            bytes[7] = bytesInt[3];
            return BitConverter.ToUInt64(bytes, 0);
        }
        public static void BreakDown(this Int64 value, out Int32 a, out Int32 b)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            a = BitConverter.ToInt32(bytes, 0);
            b = BitConverter.ToInt32(bytes, 4);
        }
        public static void BreakDown(this UInt64 value, out UInt32 a, out UInt32 b)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            a = BitConverter.ToUInt32(bytes, 0);
            b = BitConverter.ToUInt32(bytes, 4);
        }

        public static decimal Cal(string str)
        {
            Formula f = new Formula(str);
            return f.Calculate();
        }
        public static bool TryCal(string str, out decimal result)
        {
            try
            {
                Formula f = new Formula(str);
                result = f.Calculate();
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }
        private class Formula
        {
            private Formula left = null;
            private char op;
            private Formula right = null;

            private string selfStr;

            public Formula(string formula)
            {
                formula = formula.Replace(" ", "");
                formula = formula.Replace("[", "(");
                formula = formula.Replace("]", ")");
                formula = formula.Replace("{", "(");
                formula = formula.Replace("}", ")");
                formula = formula.Replace("（", "(");
                formula = formula.Replace("）", ")");
                formula = formula.Replace("【", "(");
                formula = formula.Replace("】", ")");
                while (formula.StartsWith('(') && formula.EndsWith(')'))
                {
                    formula = formula.Substring(1, formula.Length - 2);
                }
                selfStr = formula;
            }
            public Formula(string left, char op, string right)
            {
                this.left = new Formula(left);
                this.op = op;
                this.right = new Formula(right);
            }
            public Formula(Formula left, char op, Formula right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }
            public decimal Calculate()
            {
                if (left != null)
                {
                    switch (op)
                    {
                        case '+':
                            return left.Calculate() + right.Calculate();
                        case '-':
                            return left.Calculate() - right.Calculate();
                        case '*':
                            return left.Calculate() * right.Calculate();
                        case '/':
                            return left.Calculate() / right.Calculate();
                        case '%':
                            return left.Calculate() % right.Calculate();
                    }
                }
                if (string.IsNullOrWhiteSpace(selfStr))
                {
                    return 0;
                }



                int opIdx = FindPriorRootOP();
                if (opIdx > 0)
                {
                    left = new Formula(selfStr.Substring(0, opIdx));
                    op = selfStr[opIdx];
                    right = new Formula(selfStr.Substring(opIdx + 1));
                    return Calculate();
                }
                else
                {
                    return decimal.Parse(selfStr);
                }

                int FindPriorRootOP()
                {
                    int countBra = 0;
                    char c;
                    int idxLv2 = -1;
                    for (int i = selfStr.Length - 1; i >= 0; --i)
                    {
                        c = selfStr[i];
                        if (c == ')')
                        {
                            ++countBra;
                        }
                        else if (c == '(')
                        {
                            --countBra;
                        }
                        else if (countBra == 0)
                        {
                            if (c == '+' || c == '-')
                            {
                                return i;
                            }
                            if (c == '*' || c == '/')
                            {
                                idxLv2 = i;
                            }
                        }
                    }
                    return idxLv2;
                }
            }
        }
    }
}

