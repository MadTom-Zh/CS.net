using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadTomDev.Data
{
    public class DataIndexer
    {
        // 按部署获取位置，如高度为3的索引如下图：
        //                    ccR
        // ccA   a1  a2  a3
        // data  6   3   0    r1
        // data  1   7   4    r2
        // data  5   2   8    r3
        // ccC   c1  c2  c3

        private int dataHeight;
        //private bool isNotInited = true;
        //private void WaitInit()
        //{
        //    while (isNotInited)
        //    {
        //        Task.Delay(2).Wait();
        //    }
        //}
        public DataIndexer(int dataHeight)
        {
            // change Dictionary to ConcurrentDictionary
            this.dataHeight = dataHeight;
            dataPosiStepDict = new ConcurrentDictionary<int, ConcurrentDictionary<int, int>>();
            dataStepPosiDict = new ConcurrentDictionary<int, System.Drawing.Point>();
            int ir, ic;
            for (int s = 0, sv = dataHeight * dataHeight; s < sv; s++)
            {
                ir = s % dataHeight;
                ic = (dataHeight - 1 + s - s / dataHeight) % dataHeight;
                if (!dataPosiStepDict.ContainsKey(ir))
                {
                    dataPosiStepDict.TryAdd(ir, new ConcurrentDictionary<int, int>());
                }
                dataPosiStepDict[ir].TryAdd(ic, s);
                dataStepPosiDict.TryAdd(s, new System.Drawing.Point(ic, ir));
            }
            //isNotInited = false;
        }
        private ConcurrentDictionary<int, ConcurrentDictionary<int, int>> dataPosiStepDict = null;
        private ConcurrentDictionary<int, System.Drawing.Point> dataStepPosiDict = null;

        //private object locker = new object();
        public int GetAltStep(int r, int c)
        {
            //if (isNotInited)
            //    WaitInit();
            ConcurrentDictionary<int, int> d2;
            int result = -1;
            //lock (locker)
            //{
                if (dataPosiStepDict.ContainsKey(r))
                {
                    d2 = dataPosiStepDict[r];
                    if (d2.ContainsKey(c))
                    {
                        result = d2[c];
                    }
                }
            //}
            return result;
        }
        public int GetR(int c, int a)
        {
            //if (isNotInited)
            //    WaitInit();
            System.Drawing.Point pt;
            int result = -1;
            //lock (locker)
            //{
                for (int s = a * dataHeight, sV = s + dataHeight; s < sV; s++)
                {
                    pt = dataStepPosiDict[s];
                    if (pt.X == c)
                    {
                        result = pt.Y;
                        break;
                    }
                }
            //}
            return result;
        }
        public int GetC(int r, int a)
        {
            //if (isNotInited)
            //    WaitInit();
            System.Drawing.Point pt;
            int result = -1;
            //lock (locker)
            //{
                for (int s = a * dataHeight, sV = s + dataHeight; s < sV; s++)
                {
                    pt = dataStepPosiDict[s];
                    if (pt.Y == r)
                    {
                        result = pt.X;
                        break;
                    }
                }
            //}
            return result;
        }
    }
}
