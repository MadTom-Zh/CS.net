using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MadTomDev.Data;

namespace MadTomDev.Data
{
    public class Block
    {
        private Block() { }
        public Block(byte dataHeight)
        {
            if (!CheckDataWidth(dataHeight, out Exception err))
                throw err;

            this.dataHeight = dataHeight;
            this.dataWidth = dataHeight;
            _dataLength = this.dataHeight * this.dataWidth;
            _fullLength = _dataLength + 2 * this.dataWidth + this.dataHeight;

            data = new byte[dataHeight][];
            for (int i = 0; i < dataHeight; i++)
                data[i] = new byte[dataHeight];
            ccR = new byte[dataHeight];
            ccC = new byte[dataHeight];
            ccA = new byte[dataHeight];
            dataIndexer = IndexerSet.GetInstance().GetDataIndexer(dataHeight);
        }
        public Block Clone()
        {
            return new Block()
            {
                _dataHeight = dataHeight,
                _dataWidth = dataWidth,
                _dataLength = dataLength,
                data = data.CloneBlock(),
                ccR = ccR.CloneBytes(),
                ccC = ccC.CloneBytes(),
                ccA = ccA.CloneBytes(),
                dataIndexer = dataIndexer,
            };
        }

        internal void Clear()
        {
            for (int r = 0, c, rv = data.Length, cv = data[0].Length; r < rv; r++)
            {
                for (c = 0; c < cv; c++)
                    data[r][c] = 0;
                ccR[r] = 0;
                ccC[r] = 0;
                ccA[r] = 0;
            }
            errRows.Clear();
            errCols.Clear();
            errAlts.Clear();
        }

        public static bool Equals(Block a, Block b)
        {
            return a == b;
        }
        public static bool operator ==(Block a, Block b)
        {
            if (b is null)
                return false;
            return a.data.ContentEquals(b.data)
                && a.ccR.ContentEquals(b.ccR)
                && a.ccC.ContentEquals(b.ccC)
                && a.ccA.ContentEquals(b.ccA);
        }


        public static bool operator !=(Block a, Block b)
        {
            if (b is null)
                return true;
            return !(a == b);
        }



        /// <summary>
        /// data body
        /// </summary>
        public byte[][] data;
        private int _dataHeight;
        public int dataHeight
        {
            private set => _dataHeight = value;
            get => _dataHeight;
        }
        private int _dataWidth;
        public int dataWidth
        {
            private set => _dataWidth = value;
            get => _dataWidth;
        }
        private int _dataLength;
        public int dataLength
        {
            get => _dataLength;
        }
        private int _fullLength;
        public int fullLength
        {
            get => _fullLength;
        }
        /// <summary>
        /// currecting code for rows
        /// </summary>
        public byte[] ccR;
        /// <summary>
        /// currecting code for cols
        /// </summary>
        public byte[] ccC;
        /// <summary>
        /// currecting code for alts
        /// </summary>
        public byte[] ccA;

        internal byte[] GetDataArray(int offset, int length)
        {
            byte[] result = new byte[dataLength];
            int i = 0, r = offset / _dataWidth, c = offset % _dataWidth;

            while (true)
            {
                result[i++] = data[r][c];
                if (i >= length)
                    break;
                if (++c >= _dataWidth)
                {
                    c = 0;
                    r++;
                }
            }

            return result;
        }

        public void GenerateCorrectingCode()
        {
            GenerateCorrectingCode(data, ref ccR, ref ccC, ref ccA);
        }

        private List<int> errRows = new List<int>();
        private List<int> errCols = new List<int>();
        private List<int> errAlts = new List<int>();
        public bool CheckBlock()
        {
            return CheckBlock(this, ref errRows, ref errCols, ref errAlts);
        }

        private DataIndexer dataIndexer;
        public bool TryCorrecting(bool checkFirst = false)
        {
            if (checkFirst)
                CheckBlock();

            bool haveERs = errRows.Count > 0;
            bool haveECs = errCols.Count > 0;
            bool haveEAs = errAlts.Count > 0;
            if (haveERs || haveECs || haveEAs)
            {
                int c, r, a;
                bool haveCur = false;
                byte trueValue;
                if (!haveECs && !haveEAs)
                {
                    // 只有行错误
                    foreach (int ir in errRows)
                        GenerateCorrectingCodeInRow(data, ir, ref ccR);
                    return true;
                }
                else if (!haveERs && !haveEAs)
                {
                    // 只有列错误
                    foreach (int ic in errCols)
                        GenerateCorrectingCodeInCol(data, ic, ref ccC);
                    return true;
                }
                else if (!haveERs && !haveECs)
                {
                    // 只有交错误
                    foreach (int ia in errAlts)
                        GenerateCorrectingCodeInAlt(data, ia, ref ccA);
                    return true;
                }
                else if (!haveERs)
                {
                    // 同时存在 列-交错
                    for (int i = 0, iv = errCols.Count, j, jv; i < iv; i++)
                    {
                        c = errCols[i];
                        for (j = 0, jv = errAlts.Count; j < jv; j++)
                        {
                            a = errAlts[j];
                            r = dataIndexer.GetR(c, a);
                            if (TryCorrectingData(this, dataIndexer, r, c, a, out trueValue) == 4)
                            {
                                GenerateCorrectingCodeInCol(data, c, ref ccC);
                                GenerateCorrectingCodeInAlt(data, a, ref ccA);
                                errCols.Remove(c);
                                errAlts.Remove(a);
                                haveCur = true;
                            }
                            if (haveCur)
                                break;
                        }
                        if (haveCur)
                            break;
                    }
                    if (haveCur)
                        return TryCorrecting();
                    else
                        return false;
                }
                else if (!haveECs)
                {
                    // 同时存在 行-交错
                    for (int i = 0, iv = errRows.Count, j, jv; i < iv; i++)
                    {
                        r = errRows[i];
                        for (j = 0, jv = errAlts.Count; j < jv; j++)
                        {
                            a = errAlts[j];
                            c = dataIndexer.GetC(r, a);
                            if (TryCorrectingData(this, dataIndexer, r, c, a, out trueValue) == 5)
                            {
                                GenerateCorrectingCodeInRow(data, r, ref ccR);
                                GenerateCorrectingCodeInAlt(data, a, ref ccA);
                                errRows.Remove(r);
                                errAlts.Remove(a);
                                haveCur = true;
                            }
                            if (haveCur)
                                break;
                        }
                        if (haveCur)
                            break;
                    }
                    if (haveCur)
                        return TryCorrecting();
                    else
                        return false;
                }
                else if (!haveEAs)
                {
                    // 同时存在 行-列
                    for (int i = 0, iv = errRows.Count, j, jv; i < iv; i++)
                    {
                        r = errRows[i];
                        for (j = 0, jv = errCols.Count; j < jv; j++)
                        {
                            c = errCols[j];
                            a = dataIndexer.GetAltStep(r, c) / _dataHeight;
                            if (TryCorrectingData(this, dataIndexer, r, c, a, out trueValue) == 6)
                            {
                                GenerateCorrectingCodeInRow(data, r, ref ccR);
                                GenerateCorrectingCodeInCol(data, c, ref ccC);
                                errRows.Remove(r);
                                errCols.Remove(c);
                                haveCur = true;
                            }
                            if (haveCur)
                                break;
                        }
                        if (haveCur)
                            break;
                    }
                    if (haveCur)
                        return TryCorrecting();
                    else
                        return false;
                }
                else
                {
                    // 行列交都有
                    for (int i = 0, iv = errCols.Count, j, jv; i < iv; i++)
                    {
                        c = errCols[i];
                        for (j = 0, jv = errRows.Count; j < jv; j++)
                        {
                            r = errRows[j];
                            a = dataIndexer.GetAltStep(r, c) / _dataHeight;
                            switch (TryCorrectingData(this, dataIndexer, r, c, a, out trueValue))
                            {
                                case 0:
                                    data[r][c] = trueValue;
                                    errCols.RemoveAt(i--); //iv--;
                                    errRows.RemoveAt(j--); //jv--;
                                    errAlts.Remove(a);
                                    haveCur = true;
                                    break;
                                case 1:
                                    data[r][c] = trueValue;
                                    errCols.RemoveAt(i--);
                                    errRows.RemoveAt(j--);
                                    haveCur = true;
                                    break;
                                case 2:
                                    data[r][c] = trueValue;
                                    errRows.RemoveAt(j--);
                                    errAlts.Remove(a);
                                    haveCur = true;
                                    break;
                                case 3:
                                    data[r][c] = trueValue;
                                    errCols.RemoveAt(i--);
                                    errAlts.Remove(a);
                                    haveCur = true;
                                    break;
                                //case 4:
                                //    GenerateCorrectingCodeInCol(data, c, ref ccC);
                                //    GenerateCorrectingCodeInAlt(data, a, ref ccA);
                                //    errCols.RemoveAt(i--);
                                //    errAlts.Remove(a);
                                //    haveCur = true;
                                //    break;
                                //case 5:
                                //    GenerateCorrectingCodeInRow(data, r, ref ccR);
                                //    GenerateCorrectingCodeInAlt(data, a, ref ccA);
                                //    errRows.RemoveAt(j--);
                                //    errAlts.Remove(a);
                                //    haveCur = true;
                                //    break;
                                //case 6:
                                //    GenerateCorrectingCodeInRow(data, r, ref ccR);
                                //    GenerateCorrectingCodeInCol(data, c, ref ccC);
                                //    errRows.RemoveAt(j--);
                                //    errCols.RemoveAt(i--);
                                //    haveCur = true;
                                //    break;
                                default:
                                    break;
                            }
                            if (haveCur)
                                break;
                        }
                        if (haveCur)
                            break;
                    }
                    if (haveCur)
                        return TryCorrecting();
                    else
                        return false;
                }
            }
            else
            {
                return true;
            }
        }





        #region static methods

        public static bool CheckDataWidth(byte dataHeight, out Exception err)
        {
            if (dataHeight < 2)
            {
                err = new ArgumentException("Data height must be at least 2.");
                return false;
            }
            else if (dataHeight > byte.MaxValue)
            {
                err = new ArgumentException("Data height must be equal or less than 255.");
                return false;
            }
            err = null;
            return true;
        }

        public static void GenerateCorrectingCode(byte[][] data, ref byte[] ccR, ref byte[] ccC, ref byte[] ccA)
        {
            int rv = data.Length;
            int cv = data[0].Length;
            for (int r = 0; r < rv; r++)
            {
                GenerateCorrectingCodeInRow(data, r, ref ccR);
            }
            for (int c = 0; c < cv; c++)
            {
                GenerateCorrectingCodeInCol(data, c, ref ccC);
            }
            for (int a = 0; a < cv; a++)
            {
                GenerateCorrectingCodeInAlt(data, a, ref ccA);
            }
        }

        public static void GenerateCorrectingCodeInRow(byte[][] data, int rowIdx, ref byte[] ccR)
        {
            int width = data[0].Length;
            byte cBase = data[rowIdx][0];
            for (int c = 1; c < width; c++)
            {
                cBase ^= data[rowIdx][c];
            }
            ccR[rowIdx] = (byte)cBase;
        }
        public static void GenerateCorrectingCodeInCol(byte[][] data, int colIdx, ref byte[] ccC)
        {
            int height = data.Length;
            byte cBase = data[0][colIdx];
            for (int r = 1; r < height; r++)
            {
                cBase ^= data[r][colIdx];
            }
            ccC[colIdx] = cBase;
        }
        public static void GenerateCorrectingCodeInAlt(byte[][] data, int altIdx, ref byte[] ccA)
        {
            int height = data.Length;
            int width = data[0].Length;

            int s = altIdx * height, c, r;
            int sv = s + height;

            GetDataPosition(height, width, s, out r, out c);
            int cBase = data[r][c];

            for (s++; s < sv; s++)
            {
                GetDataPosition(height, width, s, out r, out c);
                cBase ^= data[r][c];
            }
            ccA[altIdx] = (byte)cBase;

            //for (int ia = 0, i = 0, iv = rv * cv; i < iv; i++)
            //{
            //    r = i % rv;
            //    c = (cv + i - 1) % cv;
            //    if (r == 0)
            //    {
            //        cBase = data[r][c];
            //    }
            //    else
            //    {
            //        cBase ^= data[r][c];
            //    }
            //    if (r == rv - 1)
            //    {
            //        ccA[ia++] = (byte)cBase;
            //    }
            //}
        }

        /// <summary>
        /// 当三个维度的测试值均一致时，则纠错正确；
        /// </summary>
        /// <param name="block">数据主体</param>
        /// <param name="r">行索引</param>
        /// <param name="c">列索引</param>
        /// <param name="optA">交错索引，如果没有预先计算，请输入-1</param>
        /// <param name="trueValue">坐标处应有的正确数值</param>
        /// <returns>小于0-此值正确，或无匹配，0-三个维度均匹配正确，1-行-列匹配正确，2-行-交错匹配正确，3-列-交错匹配正确，4-仅行匹配正确，5-仅列匹配正确，6-仅交错匹配正确；</returns>
        public static int TryCorrectingData(Block block, DataIndexer dataIdxer,
            int r, int c, int optA, out byte trueValue)
        {
            // test value, row, col, alt
            int test1, test2, test3;

            #region cal data value, in row
            byte[][] data = block.data;
            int i = 0, cV = data[0].Length;
            if (c == 0)
            {
                test1 = data[r][1];
                i++;
            }
            else
            {
                test1 = data[r][0];
            }
            for (i++; i < cV; i++)
            {
                if (i == c)
                    continue;
                test1 ^= data[r][i];
            }
            test1 ^= block.ccR[r];
            #endregion

            #region cal data value, in col
            int rV = data.Length;
            if (r == 0)
            {
                i = 1;
                test2 = data[1][c];
            }
            else
            {
                i = 0;
                test2 = data[0][c];
            }
            for (i++; i < rV; i++)
            {
                if (i == r)
                    continue;
                test2 ^= data[i][c];
            }
            test2 ^= block.ccC[c];
            #endregion

            #region cal data value, in alt
            test3 = 0;
            if (optA < 0)
            {
                optA = dataIdxer.GetAltStep(r, c) / block._dataHeight;
            }
            int s = optA * block._dataHeight;
            int sV = s + block._dataHeight;
            int ir, ic;
            bool notInit = true;
            for (; s < sV; s++)
            {
                Block.GetDataPosition(rV, cV, s, out ir, out ic);
                if (ir == r && ic == c)
                {
                    continue;
                }
                else
                {
                    if (notInit)
                    {
                        test3 = data[ir][ic];
                        notInit = false;
                    }
                    else
                    {
                        test3 ^= data[ir][ic];
                    }
                }
            }
            test3 ^= block.ccA[optA];
            #endregion


            byte dataValue = data[r][c];
            if (dataValue == test1 && dataValue == test2 && dataValue == test3)
            {
                trueValue = 0;
                return -1;
            }
            if (test1 == test2 && test2 == test3)
            {
                trueValue = (byte)test1;
                return 0;
            }
            else if (test1 == test2)
            {
                trueValue = (byte)test1;
                return 1;
            }
            else if (test1 == test3)
            {
                trueValue = (byte)test1;
                return 2;
            }
            else if (test2 == test3)
            {
                trueValue = (byte)test2;
                return 3;
            }

            if (test1 == dataValue)
            {
                trueValue = dataValue;
                return 4;
            }
            else if (test2 == dataValue)
            {
                trueValue = dataValue;
                return 5;
            }
            else if (test3 == dataValue)
            {
                trueValue = dataValue;
                return 6;
            }

            trueValue = 0;
            return -1;
        }

        public static void GetDataPosition(int dataHeight, int dataWidth, int step, out int r, out int c)
        {
            c = (dataWidth - 1 + step - (step / dataHeight)) % dataWidth;
            r = step % dataHeight;
        }
        public static bool CheckBlock(Block block, ref List<int> eRows, ref List<int> eCols, ref List<int> eAlts)
        {
            return CheckBlock(block.data, block.ccR, block.ccC, block.ccA, ref eRows, ref eCols, ref eAlts);
        }
        public static bool CheckBlock(byte[][] data, byte[] ccR, byte[] ccC, byte[] ccA, ref List<int> eRows, ref List<int> eCols, ref List<int> eAlts)
        {
            eRows.Clear();
            eCols.Clear();
            eAlts.Clear();







            int rv = data.Length;
            int cv = data[0].Length;
            int r, c, cBase;
            for (r = 0; r < rv; r++)
            {
                cBase = data[r][0];
                for (c = 1; c < cv; c++)
                {
                    cBase ^= data[r][c];
                }
                if (ccR[r] != cBase)
                    eRows.Add(r);
            }
            for (c = 0; c < cv; c++)
            {
                cBase = data[0][c];
                for (r = 1; r < rv; r++)
                {
                    cBase ^= data[r][c];
                }
                if (ccC[c] != cBase)
                    eCols.Add(c);
            }

            for (int a = 0; a < cv; a++)
            {
                c = cv - a - 1;
                r = 0;
                cBase = data[r][c];
                for (r = 1; r < rv; r++)
                {
                    c = (cv - a - 1 + r) % cv;
                    cBase ^= data[r][c];
                }
                if (ccA[a] != cBase)
                    eAlts.Add(a);
            }


            return eRows.Count == 0 && eCols.Count == 0 && eAlts.Count == 0;
        }


        #endregion
    }
}
