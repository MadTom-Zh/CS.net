using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public static class Extensions
    {
        public static byte[] CloneBytes(this byte[] ori)
        {
            byte[] clone = new byte[ori.Length];
            for (int i = 0, iv = ori.Length; i < iv; i++)
                clone[i] = ori[i];
            return clone;
        }
        public static bool ContentEquals(this byte[] a, byte[] b)
        {
            if (a == null && b == null)
                return true;
            else if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;
            for (int i = 0, iv = a.Length; i < iv; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
        public static void XOR(this byte[] a, byte[] b)
        {
            if (a == null)
                return;
            for (int i = 0, iv = a.Length; i < iv; i++)
            {
                a[i] ^= b[i];
            }
        }
        public static byte[][] CloneBlock(this byte[][] ori)
        {
            byte[][] clone = new byte[ori.Length][];
            for (int i = 0, iv = ori.Length; i < iv; i++)
                clone[i] = ori[i].CloneBytes();
            return clone;
        }
        public static bool ContentEquals(this byte[][] a, byte[][] b)
        {
            if (a == null && b == null)
                return true;
            else if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;
            for (int i = 0, iv = a.Length; i < iv; i++)
            {
                if (!a[i].ContentEquals(b[i]))
                    return false;
            }
            return true;
        }
        public static void XOR(this byte[][] a, byte[][] b)
        {
            if (a == null)
                return;
            for (int i = 0, iv = a.Length; i < iv; i++)
            {
                a[i].XOR(b[i]);
            }
        }
    }
    //public class NonStaticExtensions
    //{
    //}
}
