using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.App
{
    public class StaticMethods
    {
        public static bool SetBlockError(ref Data.Block block, int errCount, Random rand)
        {
            if (errCount < 1)
                return false;

            int maxIdx = block.dataHeight * block.dataWidth + 2 * block.dataWidth + block.dataHeight;
            if (errCount > maxIdx)
                return false;


            int test = 0;
            void SetRandValue(ref byte par, ref int testBuf, Random rand)
            {
                do
                {
                    testBuf = rand.Next(256);
                }
                while (par == testBuf);
                par = (byte)testBuf;
            }

            int r, c;
            List<int> errPosiList = new List<int>();
            if (errCount == maxIdx)
            {
                for (c = 0; c < block.dataWidth; c++)
                {
                    SetRandValue(ref block.ccC[c], ref test, rand);
                    SetRandValue(ref block.ccA[c], ref test, rand);
                }
                for (r = 0; r < block.dataHeight; r++)
                {
                    SetRandValue(ref block.ccR[r], ref test, rand);
                }
                for (r = 0; r < block.dataHeight; r++)
                {
                    for (c = 0; c < block.dataWidth; c++)
                    {
                        SetRandValue(ref block.data[r][c], ref test, rand);
                    }
                }
            }
            else
            {
                while (errPosiList.Count < errCount)
                {
                    do
                    {
                        test = rand.Next(maxIdx);
                    }
                    while (errPosiList.Contains(test));
                    errPosiList.Add(test);
                }
                int ccIdx;
                foreach (int i in errPosiList)
                {
                    new Data.BlockIndexer(block.dataHeight).GetPosition(i, out ccIdx, out c, out r);
                    switch (ccIdx)
                    {
                        case 0:
                            SetRandValue(ref block.ccR[r], ref test, rand);
                            break;
                        case 1:
                            SetRandValue(ref block.ccC[c], ref test, rand);
                            break;
                        case 2:
                            SetRandValue(ref block.ccA[c], ref test, rand);
                            break;
                        default:
                            SetRandValue(ref block.data[r][c], ref test, rand);
                            break;
                    }
                }
            }
            return true;
        }

        internal static void SetBlockDataRandom(ref byte[][] array, Random rand)
        {
            int dataHeight = array.Length;
            int dataWidth = array[0].Length;
            for (int r = 0, c; r < dataHeight; r++)
            {
                for (c = 0; c < dataWidth; c++)
                {
                    array[r][c] = (byte)rand.Next(256);
                }
            }
        }

        public static bool SetBandError(ref Data.Band band, int errCount, int maxErrLength, Random rand, out int maxErrMadeLength, out int errMadeCount)
        {
            maxErrMadeLength = 0;
            errMadeCount = 0;

            int maxLength = (int)band.MaxBandFullLength;
            if (errCount > maxLength)
                return false;
            //errCount = maxLength;
            if (maxErrLength > errCount)
                maxErrLength = errCount;

            byte[] buffer;
            if (maxErrLength == maxLength)
            {
                buffer = new byte[maxErrLength];
                rand.NextBytes(buffer);
                SetBandError_writeBuffer(ref band.bandBuffer, buffer, 0, maxErrLength);

                maxErrMadeLength = maxErrLength;
                errMadeCount = 1;
                return true;
            }

            int i, iv;
            List<int> dict = new List<int>();
            for (i = 0, iv = maxLength; i < iv; i++)
            {
                dict.Add(i);
            }

            int bufLength, counter = 0, errMade = 0;
            while (counter < errCount)
            {
                GetNextSection(out i, out bufLength);

                buffer = new byte[bufLength];
                rand.NextBytes(buffer);

                SetBandError_writeBuffer(
                    ref band.bandBuffer, buffer,
                    i,
                    bufLength);
                counter += bufLength;
                errMade++;
            }

            void GetNextSection(out int start, out int length)
            {
                start = rand.Next(dict.Count);
                length = rand.Next(Math.Min(maxErrLength, GetSectionLength(start)) + 1);
            }
            int GetSectionLength(int start)
            {
                int preV = dict[start];
                int result = 1;
                for (int i = start + 1, iv = dict.Count; i < iv; i++)
                {
                    if (preV + 1 == dict[i])
                    {
                        result++;
                        if (result >= maxErrLength)
                            break;
                    }
                    else break;
                }
                return result;
            }

            errMadeCount = errCount;
            return true;
        }
        private static void SetBandError_writeBuffer(ref byte[] buffer, byte[] data, int start, int length)
        {
            for (int i = 0, j = start, iv = length; i < iv; i++, j++)
            {
                buffer[j] = data[i];
            }
        }
    }
}
