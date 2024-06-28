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

        public static Point? TryGetPoint(this object value, Point? defaultValue = null)
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
                bool firstRec = true;
                foreach (char c in testStr)
                {
                    if ((c >= '0' && c <= '9'))
                    {
                        if (firstRec)
                            xStr.Append(c);
                        if (!firstRec)
                            yStr.Append(c);
                    }
                    else
                    {
                        if (firstRec)
                        {
                            firstRec = false;
                        }
                        else
                        {
                            if (yStr.Length > 0)
                                break;
                        }
                    }
                }
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new Point(int.Parse(xStr.ToString()), int.Parse(yStr.ToString()));
                else
                    return defaultValue;
            }
            catch (Exception)
            { return defaultValue; }
        }
        public static Size? TryGetSize(this object value, Size? defaultValue = null)
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
                bool firstRec = true;
                foreach (char c in testStr)
                {
                    if ((c >= '0' && c <= '9'))
                    {
                        if (firstRec)
                            xStr.Append(c);
                        if (!firstRec)
                            yStr.Append(c);
                    }
                    else
                    {
                        if (firstRec)
                        {
                            firstRec = false;
                        }
                        else
                        {
                            if (yStr.Length > 0)
                                break;
                        }
                    }
                }
                if (xStr.Length > 0 && yStr.Length > 0)
                    return new Size(int.Parse(xStr.ToString()), int.Parse(yStr.ToString()));
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

        public static double Cal(string str)
        {
            Formula f = new Formula(str);
            return f.Calculate();
        }
        public static bool TryCal(string str, out double result)
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
            public double Calculate()
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
                    return 0.0;
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
                    return double.Parse(selfStr);
                }

                int FindPriorRootOP( )
                {                    
                    int countBra = 0;
                    char c;
                    int idxLv2 = -1;
                    for (int i = selfStr.Length-1; i>=0;--i)
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

