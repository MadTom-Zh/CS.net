using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public class BlockIndexer
    {
        // 按部署获取位置，如高度为3的索引如下图：
        //                    ccR
        // ccA   10  5   0
        // data  15  11  6    1
        // data  2   16  12   7
        // data  8   3   17   13
        // ccC   14  9   4
        public BlockIndexer(int dataHeight)
        {
            DataHeight = dataHeight;
            Steps = dataHeight * dataHeight + 3 * dataHeight;
        }
        public int DataHeight { private set; get; }
        public int Steps { private set; get; }

        private int _CurStep = -1;
        public int CurStep
        {
            set
            {
                if (value < 0 || value >= Steps)
                    throw new IndexOutOfRangeException("Step must between 0 and (less than) steps");

                _CurStep = value;
            }
            get => _CurStep;
        }
        public bool CanGoNext
        {
            get => _CurStep + 1 < Steps;
        }
        public bool CanGoPrev
        {
            get => _CurStep - 1 >= 0;
        }
        public bool GetPosition(out int ccIdx, out int x, out int y)
        {
            bool result = GetPosition(_CurStep, out ccIdx, out x, out y);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newStep"></param>
        /// <param name="ccIdx"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool GetPosition(int newStep, out int ccIdx, out int x, out int y)
        {
            if (newStep < 0 || newStep >= Steps)
            {
                ccIdx = -1; x = -1; y = -1;
                return false;
            }
            CurStep = newStep;
            int fullHeight = DataHeight + 2;
            int fullWidth = DataHeight + 1;

            int div = newStep / fullHeight;
            if (div >= DataHeight)
            {
                x = newStep - fullHeight * div;
                y = (newStep + 1) % fullHeight;
            }
            else
            {
                x = (fullWidth - 2 + newStep + (fullWidth - 2) * div) % fullWidth;
                y = newStep % fullHeight;
            }





            if (x == DataHeight)
            {
                ccIdx = 0;
                x = -1; y--;
            }
            else if (y == 0)
            {
                ccIdx = 2;
                y = -1;
            }
            else if (y == fullHeight - 1)
            {
                ccIdx = 1;
                y = -1;
            }
            else
            {
                ccIdx = -1;
                y--;
            }
            return true;
        }
        public bool GetNextPosition(out int ccIdx, out int x, out int y)
        {
            if (CanGoNext)
            {
                bool result = GetPosition(_CurStep + 1, out ccIdx, out x, out y);
                return result;
            }
            ccIdx = -1; x = -1; y = -1;
            return false;
        }
        public bool GetPrevPosition(out int ccIdx, out int x, out int y)
        {
            if (CanGoPrev)
            {
                bool result = GetPosition(_CurStep - 1, out ccIdx, out x, out y);
                return result;
            }
            ccIdx = -1; x = -1; y = -1;
            return false;
        }

        #region static methods
        //public static bool GetPosition(int blockHeight, int curStep, out int ccIdx, out int x, out int y)
        //{
        //    bool result = new BlockIndexer(blockHeight).GetPosition(curStep, out ccIdx, out x, out y);
        //    return result;
        //}
        #endregion
    }
}
