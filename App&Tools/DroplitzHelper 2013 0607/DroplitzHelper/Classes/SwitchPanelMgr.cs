using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace DroplitzHelper.Classes
{
    public class SwitchPanelMgr
    {
        public SwitchPanelMgr()
        {
            this.SearchOneWayPass += SwitchPanelMgr_SearchOneWayPass;
            _tapCount = 1;
            _rowCount = 5;
            _MatrixInit();
        }
        private void _MatrixInit()
        {
            Switch6Ways dataItem;
            for (int colIdx = 0; colIdx < (_tapCount + 2); colIdx++)
            {
                for (int rowIdx = 0; rowIdx < (_rowCount + 2); rowIdx++)
                {
                    if (rowIdx == 0)
                    {
                        // end points
                        if (colIdx % 2 == 0)
                        {
                            dataItem = new Switch6Ways(Switch6Ways.Type.END);
                            dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                            switchMatrix[colIdx + "," + rowIdx] = dataItem;
                        }
                    }
                    else if (rowIdx == _rowCount + 1)
                    {
                        //start points
                        if (colIdx % 2 == 1)
                        {
                            dataItem = new Switch6Ways(Switch6Ways.Type.START);
                            dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                            switchMatrix[colIdx + "," + rowIdx] = dataItem;
                        }
                    }
                    else
                    {
                        //main body points
                        dataItem = new Switch6Ways(Switch6Ways.Type.None);
                        dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                        dataItem.ImChanged += dataItem_ImChanged;
                        switchMatrix[colIdx + "," + rowIdx] = dataItem;
                    }
                }
            }
            if (SizeChanged != null)
            {
                SizeChanged(this, new EventArgs());
            }
        }

        public void MatrixExpand(int tapIncre, int rowIncre)
        {
            if (_tapCount + tapIncre < 1) return;
            else if (_tapCount + tapIncre > TapCountMax) return;
            if (_rowCount + rowIncre < 1) return;
            else if (_rowCount + rowIncre > RowCountMax) return;

            Switch6Ways dataItem;
            while (Math.Abs(tapIncre) != 0)
            {
                if (tapIncre > 0)
                {
                    #region incr tap
                    int colIdx = (_tapCount * 2 + 1);
                    for (int i = 0; i < 2; i++)
                    {
                        for (int rowIdx = 0; rowIdx < (_rowCount + 2); rowIdx++)
                        {
                            if (rowIdx == 0)
                            {
                                // end points
                                if (colIdx % 2 == 0)
                                {
                                    dataItem = new Switch6Ways(Switch6Ways.Type.END);
                                    dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                                    switchMatrix[colIdx + "," + rowIdx] = dataItem;
                                }
                            }
                            else if (rowIdx == _rowCount + 1)
                            {
                                //start points
                                if (colIdx % 2 == 1)
                                {
                                    dataItem = new Switch6Ways(Switch6Ways.Type.START);
                                    dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                                    switchMatrix[colIdx + "," + rowIdx] = dataItem;
                                }
                            }
                            else
                            {
                                //main body points
                                dataItem = new Switch6Ways(Switch6Ways.Type.None);
                                dataItem.PosiInMatrix = new Point(colIdx, rowIdx);
                                dataItem.ImChanged += dataItem_ImChanged;
                                switchMatrix[colIdx + "," + rowIdx] = dataItem;
                            }
                        }
                        colIdx++;
                    }
                    _tapCount++;
                    #endregion
                }
                else
                {
                    #region reduc tap
                    int colIdx = _tapCount * 2;
                    string mKey;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int rowIdx = _rowCount + 1; rowIdx >= 0; rowIdx--)
                        {
                            mKey = colIdx + "," + rowIdx;
                            dataItem = (Switch6Ways)switchMatrix[mKey];
                            if (dataItem != null)
                            {
                                dataItem.Dispose();
                            }
                            switchMatrix[mKey] = null;
                        }
                        colIdx--;
                    }
                    _tapCount--;
                    #endregion
                }
                tapIncre += (tapIncre > 0) ? (-1) : (1);
            }

            string mKeyNew, mKeyOld;
            while (Math.Abs(rowIncre) != 0)
            {
                if (rowIncre > 0)
                {
                    #region incr row
                    // move starts
                    for (int colIdx = (_tapCount * 2 - 1); colIdx > 0; colIdx -= 2)
                    {
                        mKeyNew = colIdx + "," + (_rowCount + 2);
                        mKeyOld = colIdx + "," + (_rowCount + 1);
                        switchMatrix[mKeyNew] = switchMatrix[mKeyOld];
                        switchMatrix[mKeyOld] = null;
                    }
                    // incr row
                    for (int colIdx = (_tapCount * 2); colIdx >= 0; colIdx--)
                    {
                        mKeyOld = colIdx + "," + (_rowCount + 1);
                        dataItem = new Switch6Ways(Switch6Ways.Type.None);
                        dataItem.PosiInMatrix = new Point(colIdx, _rowCount + 1);
                        dataItem.ImChanged += dataItem_ImChanged;
                        switchMatrix[mKeyOld] = dataItem;
                    }
                    _rowCount++;
                    #endregion
                }
                else
                {
                    #region reduc row
                    // reduc row
                    for (int colIdx = (_tapCount * 2); colIdx >= 0; colIdx--)
                    {
                        mKeyOld = colIdx + "," + (_rowCount);
                        dataItem = (Switch6Ways)switchMatrix[mKeyOld];
                        if (dataItem != null)
                        {
                            dataItem.Dispose();
                            switchMatrix[mKeyOld] = null;
                        }
                    }
                    // move starts
                    for (int colIdx = (_tapCount * 2 - 1); colIdx > 0; colIdx -= 2)
                    {
                        mKeyNew = colIdx + "," + (_rowCount);
                        mKeyOld = colIdx + "," + (_rowCount + 1);
                        switchMatrix[mKeyNew] = switchMatrix[mKeyOld];
                        switchMatrix[mKeyOld] = null;
                    }
                    _rowCount--;
                    #endregion
                }
                rowIncre += (rowIncre > 0) ? (-1) : (1);
            }
            if (SizeChanged != null)
            {
                SizeChanged(this, new EventArgs());
            }
        }
        public event EventHandler SizeChanged;
        public void TryBroadcastSizeChanged()
        {
            if (SizeChanged != null)
            {
                SizeChanged(this, new EventArgs());
            }
        }

        private Hashtable switchMatrix = new Hashtable();
        public Switch6Ways GetSwitchItem(Point pt)
        {
            return GetSwitchItem(pt.X, pt.Y);
        }
        public Switch6Ways GetSwitchItem(int colIdx, int rowIdx)
        {
            Switch6Ways result
                = (Switch6Ways)switchMatrix[colIdx + "," + rowIdx];
            return result;
        }

        private int _tapCount, _rowCount;
        public static int TapCountMax = 10;
        public static int RowCountMax = 10;
        public int TapCount
        {
            get
            {
                return _tapCount;
            }
        }
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
        }

        public bool CheckWay(int switchColIdx, int switchRowIdx, int wayIdx)
        {
            Switch6Ways switch6w = GetSwitchItem(switchColIdx, switchRowIdx);
            if (switch6w.Ways[wayIdx] == false)
            {
                return false;
            }
            else
            {
                Switch6Ways curNeighbor = GetNeighbor(switchColIdx, switchRowIdx, wayIdx);
                if (curNeighbor == null)
                {
                    return false;
                }
                else
                {
                    return (curNeighbor.Ways[(wayIdx + 3) % 6] == true);
                }
            }
        }
        public static bool CheckWay(Switch6Ways s6w1, Switch6Ways s6wNeighbor)
        {
            if (CheckIsNeighbor(s6w1, s6wNeighbor) == false)
            {
                return false;
            }
            int neighborDirection = GetRelaIdx(s6w1.PosiInMatrix, s6wNeighbor.PosiInMatrix);
            if (s6w1.Ways[neighborDirection] == true)
            {
                if (s6wNeighbor.Ways[(neighborDirection + 3) % 6] == true)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CheckIsNeighbor(Switch6Ways s6w1, Switch6Ways s6w2)
        {
            if (s6w1 == null || s6w2 == null) return false;
            return CheckIsNeighbor(s6w1.PosiInMatrix, s6w2.PosiInMatrix);
        }
        public static bool CheckIsNeighbor(Point s6wPt1, Point s6wPt2)
        {
            if (s6wPt1.X == s6wPt2.X)
            {
                if (Math.Abs(s6wPt1.Y - s6wPt2.Y) == 1)
                {
                    return true;
                }
            }
            else if (Math.Abs(s6wPt1.X - s6wPt2.X) == 1)
            {
                if (s6wPt1.X % 2 == 0)
                {
                    if (s6wPt1.Y == s6wPt2.Y || (s6wPt1.Y - s6wPt2.Y == 1)) return true;
                }
                else
                {
                    if (s6wPt1.Y == s6wPt2.Y || (s6wPt1.Y - s6wPt2.Y == -1)) return true;
                }
            }
            return false;
        }
        public Switch6Ways GetNeighbor(Point curDataPosi, int wayIdx)
        {
            return GetNeighbor(curDataPosi.X, curDataPosi.Y, wayIdx);
        }
        public Switch6Ways GetNeighbor(int surSwitchColIdx, int surSwitchRowIdx, int wayIdx)
        {
            Point newPoint
                = GetRelaPosition(surSwitchColIdx, surSwitchRowIdx, wayIdx);
            Switch6Ways result = GetSwitchItem(newPoint.X, newPoint.Y);
            return result;
        }
        public static Point GetRelaPosition(Point surDataPosi, int wayIdx)
        {
            return GetRelaPosition(surDataPosi.X, surDataPosi.Y, wayIdx);
        }
        public static Point GetRelaPosition(int surColIdx, int surRowIdx, int wayIdx)
        {
            Point result = new Point();
            switch (wayIdx)
            {
                case 0:
                    result.X = surColIdx;
                    result.Y = surRowIdx + 1;
                    break;
                case 3:
                    result.X = surColIdx;
                    result.Y = surRowIdx - 1;
                    break;
                case 1:
                    result.X = surColIdx + 1;
                    if (surColIdx % 2 == 0) result.Y = surRowIdx;
                    else result.Y = surRowIdx + 1;
                    break;
                case 2:
                    result.X = surColIdx + 1;
                    if (surColIdx % 2 == 0) result.Y = surRowIdx - 1;
                    else result.Y = surRowIdx;
                    break;
                case 5:
                    result.X = surColIdx - 1;
                    if (surColIdx % 2 == 0) result.Y = surRowIdx;
                    else result.Y = surRowIdx + 1;
                    break;
                case 4:
                    result.X = surColIdx - 1;
                    if (surColIdx % 2 == 0) result.Y = surRowIdx - 1;
                    else result.Y = surRowIdx;
                    break;
                default:
                    throw new Exception("No Way Index defined like [" + wayIdx + "]");
                //break;
            }
            return result;
        }
        public static int GetRelaIdx(Point curPoint, Point tarPoint)
        {
            if (curPoint.X == tarPoint.X)
            {
                if (tarPoint.Y - curPoint.Y == 1) return 0;
                else if (tarPoint.Y - curPoint.Y == -1) return 3;
            }
            if (curPoint.X % 2 == 0)
            {
                if (tarPoint.Y == curPoint.Y)
                {
                    if (tarPoint.X - curPoint.X == 1) return 1;
                    else if (tarPoint.X - curPoint.X == -1) return 5;
                }
                else if (tarPoint.Y - curPoint.Y == -1)
                {
                    if (tarPoint.X - curPoint.X == 1) return 2;
                    else if (tarPoint.X - curPoint.X == -1) return 4;
                }
            }
            else
            {
                if (tarPoint.Y - curPoint.Y == 1)
                {
                    if (tarPoint.X - curPoint.X == 1) return 1;
                    else if (tarPoint.X - curPoint.X == -1) return 5;
                }
                else if (tarPoint.Y == curPoint.Y)
                {
                    if (tarPoint.X - curPoint.X == 1) return 2;
                    else if (tarPoint.X - curPoint.X == -1) return 4;
                }
            }
            return -1;
        }

        public class Point
        {
            public int X;
            public int Y;
            private DateTime hashData = DateTime.Now;
            public Point()
            {
                X = 0;
                Y = 0;
            }
            public Point(Point bluePrint)
            {
                X = bluePrint.X;
                Y = bluePrint.Y;
            }
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static Point operator +(Point p1, Point p2)
            {
                return new Point(p1.X + p2.X, p1.Y + p2.Y);
            }
            public static Point operator -(Point p1, Point p2)
            {
                return new Point(p1.X - p2.X, p1.Y - p2.Y);
            }
            public static bool operator ==(Point p1, Point p2)
            {
                int p1x = 0;
                int p1y = 0;
                bool isP1Null = false;
                try
                {
                    p1x = p1.X;
                    p1y = p1.Y;
                }
                catch (NullReferenceException)
                {
                    isP1Null = true;
                }
                int p2x = 0;
                int p2y = 0;
                bool isP2Null = false;
                try
                {
                    p2x = p2.X;
                    p2y = p2.Y;
                }
                catch (NullReferenceException)
                {
                    isP2Null = true;
                }
                if (isP1Null == isP2Null)
                {
                    if (isP1Null == true) return true;
                    else return (p1x == p2x && p1y == p2y);
                }
                else return false;
            }
            public static bool operator !=(Point p1, Point p2)
            {
                return (p1 == p2) == false;
            }
            public override bool Equals(object o)
            {
                return false;
                //Point source = (Point)o;
                //if (o == null) return false;
                //else return (source.X == this.X && source.Y == this.Y);
            }
            public override int GetHashCode()
            {
                return 0;
                //return (int)hashData.ToFileTimeUtc();
            }
        }
        public class BunchWays
        {
            public Point StartPoint
            {
                get
                {
                    if (PosibleWays.Count > 0) return PosibleWays[0].WayPoints[0];
                    else return null;
                }
            }
            public int MaxWayLength
            {
                get
                {
                    int result = 0;
                    foreach (CompleteWay cw in PosibleWays)
                    {
                        if (result < cw.WayLength)
                        {
                            result = cw.WayLength;
                        }
                    }
                    return result;
                }
            }
            public int MinWayLength
            {
                get
                {
                    int result = int.MaxValue;
                    foreach (CompleteWay cw in PosibleWays)
                    {
                        if (result > cw.WayLength)
                        {
                            result = cw.WayLength;
                        }
                    }
                    return result;
                }
            }

            public class CompleteWay
            {
                public List<Point> WayPoints = new List<Point>();
                public int WayLength
                {
                    get
                    {
                        return WayPoints.Count;
                    }
                }
                public CompleteWay Clone()
                {
                    CompleteWay result = new CompleteWay();
                    for (int i = 0; i < this.WayPoints.Count; i++)
                    {
                        result.WayPoints.Add(new Point(this.WayPoints[i]));
                    }
                    return result;
                }

                public bool ContainPoint(Point pt)
                {
                    foreach (Point curPt in WayPoints)
                    {
                        if (pt == curPt) return true;
                    }
                    return false;
                }

                public static bool operator ==(CompleteWay cw1, CompleteWay cw2)
                {
                    bool isCw1Null = false;
                    try
                    {
                        int tmp = cw1.WayLength;
                    }
                    catch (NullReferenceException)
                    {
                        isCw1Null = true;
                    }
                    bool isCw2Null = false;
                    try
                    {
                        int tmp = cw2.WayLength;
                    }
                    catch (NullReferenceException)
                    {
                        isCw2Null = true;
                    }
                    if (isCw1Null == isCw2Null)
                    {
                        if (isCw1Null == true) return true;
                        else
                        {
                            if (cw1.WayLength == cw2.WayLength)
                            {
                                for (int i = cw1.WayLength - 1; i >= 0; i--)
                                {
                                    if (cw1.WayPoints[i] != cw2.WayPoints[i])
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            }
                            return false;
                        }
                    }
                    else return false;
                }
                public static bool operator !=(CompleteWay cw1, CompleteWay cw2)
                {
                    return (cw1 == cw2) == false;
                }

                public override bool Equals(object o)
                {
                    CompleteWay target = (CompleteWay)o;
                    if (target == null) return false;
                    else return this == target;
                }
                public override int GetHashCode()
                {
                    return 0;
                }
            }
            public List<CompleteWay> PosibleWays = new List<CompleteWay>();
        }

        public List<BunchWays> AllWays = new List<BunchWays>();
        //public List<BunchWays.CompleteWay> FinalWays = new List<BunchWays.CompleteWay>();

        private bool ReCalculateWays_Working = false;
        private List<BunchWays.CompleteWay> _openWays
            = new List<BunchWays.CompleteWay>();
        public List<BunchWays.CompleteWay> OpenWays
        {
            get
            {
                return _openWays;
            }
        }

        void dataItem_ImChanged(object sender, EventArgs e)
        {
            if (ReCalculateWays_Working == true) return;

            Switch6Ways target = (Switch6Ways)sender;
            Point curPoint, prePoint, nxtPoint;
            bool wayBackOpen, wayForwOpen;

            #region maybe destroy a/some way(s)
            BunchWays.CompleteWay oneWay;
            for (int oneWayIdx = _openWays.Count - 1; oneWayIdx >= 0; oneWayIdx--)
            {
                oneWay = _openWays[oneWayIdx];
                for (int i = 0; i < oneWay.WayLength; i++)
                {
                    curPoint = oneWay.WayPoints[i];
                    if (target.PosiInMatrix == curPoint)
                    {
                        // point found
                        bool killWay = false;
                        if (i == 0 && (target.MyType == Switch6Ways.Type.END || target.MyType == Switch6Ways.Type.U) == false)
                        {
                            killWay = true;
                        }
                        else if (i == (oneWay.WayLength - 1) && (target.MyType == Switch6Ways.Type.START || target.MyType == Switch6Ways.Type.U) == false)
                        {
                            killWay = true;
                        }
                        else
                        {
                            prePoint = null;
                            nxtPoint = null;
                            wayBackOpen = false;
                            wayForwOpen = false;
                            if (i == 0)
                            {
                                nxtPoint = oneWay.WayPoints[i + 1];
                            }
                            else if (i == (oneWay.WayLength - 1))
                            {
                                prePoint = oneWay.WayPoints[i - 1];
                            }
                            else
                            {
                                prePoint = oneWay.WayPoints[i - 1];
                                nxtPoint = oneWay.WayPoints[i + 1];
                            }
                            if (prePoint != null)
                            {
                                if (SwitchPanelMgr.CheckWay(GetSwitchItem(curPoint.X, curPoint.Y), GetSwitchItem(prePoint.X, prePoint.Y)))
                                {
                                    wayBackOpen = true;
                                }
                            }
                            else wayBackOpen = true;
                            if (nxtPoint != null)
                            {
                                if (SwitchPanelMgr.CheckWay(GetSwitchItem(curPoint.X, curPoint.Y), GetSwitchItem(nxtPoint.X, nxtPoint.Y)))
                                {
                                    wayForwOpen = true;
                                }
                            }
                            else wayForwOpen = true;

                            if (wayBackOpen == false || wayForwOpen == false)
                            {
                                killWay = true;
                            }
                        }
                        if (killWay)
                        {
                            _openWays.Remove(oneWay);
                            SetUIOpenWay(oneWay, false, false);
                            continue;
                        }
                    }
                }
            }
            #endregion

            #region or may make a new way
            List<BunchWays.CompleteWay> newOpenWays
                = GetReadyWaysWithPoint(target.PosiInMatrix);
            BunchWays.CompleteWay orgOW;
            bool orgFound;
            foreach (BunchWays.CompleteWay openWay in newOpenWays)
            {
                orgFound = false;
                for (int i = _openWays.Count - 1; i >= 0; i--)
                {
                    orgOW = _openWays[i];
                    if (orgOW == openWay)
                    {
                        orgFound = true;
                        break;
                    }
                }
                if (orgFound == false)
                {
                    _openWays.Add(openWay);
                }
                SetUIOpenWay(openWay, true, false);
            }
            #endregion
        }
        private void SetUIOpenWay(BunchWays.CompleteWay way, bool isOpen, bool priWay)
        {
            Switch6Ways s6wData;
            foreach (Point pt in way.WayPoints)
            {
                s6wData = GetSwitchItem(pt.X, pt.Y);
                if (isOpen == true)
                {
                    if (s6wData.MyType == Switch6Ways.Type.U) s6wData.UI.ReSetUI_FrameColor_EndPoint();
                    else if (priWay == true) s6wData.UI.ReSetUI_FrameColor_InOpenWayPri();
                    else s6wData.UI.ReSetUI_FrameColor_InOpenWaySec();
                }
                else
                {
                    s6wData.UI.ReSetUI_FrameColor_Normal();
                }
            }
        }
        private List<BunchWays.CompleteWay> GetReadyWaysWithPoint(Point point)
        {
            List<BunchWays.CompleteWay> completeWays
                = new List<BunchWays.CompleteWay>();
            List<BunchWays.CompleteWay> waysWithEnds
                = new List<BunchWays.CompleteWay>();
            BunchWays.CompleteWay first = new BunchWays.CompleteWay();
            first.WayPoints.Add(point);
            CheckIfPointInOpenWays_FindWayDown(ref waysWithEnds, first);
            foreach (BunchWays.CompleteWay ow in waysWithEnds)
            {
                CheckIfPointInOpenWays_FindWayUp(ref completeWays, ow);
            }
            return completeWays;
        }
        private void CheckIfPointInOpenWays_FindWayDown(ref List<BunchWays.CompleteWay> waysWithEnds, BunchWays.CompleteWay curWay)
        {
            Point lowPoint = curWay.WayPoints[0];
            Switch6Ways lowPtData = GetSwitchItem(lowPoint);
            if (lowPtData.MyType == Switch6Ways.Type.END
                || lowPtData.MyType == Switch6Ways.Type.U)
            {
                if (lowPtData.Ways[5] == true
                    || lowPtData.Ways[0] == true
                    || lowPtData.Ways[1] == true)
                {
                    waysWithEnds.Add(curWay);
                    return;
                }
            }

            Switch6Ways nextPtData;
            BunchWays.CompleteWay newWay;
            if (lowPtData.Ways[2] == true)
            {
                nextPtData = GetNeighbor(lowPoint, 2);
                if (nextPtData != null && nextPtData.Ways[5] == true)
                {
                    newWay = curWay.Clone();
                    newWay.WayPoints.Insert(0, GetRelaPosition(lowPoint, 2));
                    CheckIfPointInOpenWays_FindWayDown(ref waysWithEnds, newWay);
                }
            }
            if (lowPtData.Ways[3] == true)
            {
                nextPtData = GetNeighbor(lowPoint, 3);
                if (nextPtData != null && nextPtData.Ways[0] == true)
                {
                    newWay = curWay.Clone();
                    newWay.WayPoints.Insert(0, GetRelaPosition(lowPoint, 3));
                    CheckIfPointInOpenWays_FindWayDown(ref waysWithEnds, newWay);
                }
            }
            if (lowPtData.Ways[4] == true)
            {
                nextPtData = GetNeighbor(lowPoint, 4);
                if (nextPtData != null && nextPtData.Ways[1] == true)
                {
                    newWay = curWay.Clone();
                    newWay.WayPoints.Insert(0, GetRelaPosition(lowPoint, 4));
                    CheckIfPointInOpenWays_FindWayDown(ref waysWithEnds, newWay);
                }
            }
        }
        private void CheckIfPointInOpenWays_FindWayUp(ref List<BunchWays.CompleteWay> completeWays, BunchWays.CompleteWay wayWithEnd)
        {
            Point highPoint = wayWithEnd.WayPoints[wayWithEnd.WayLength - 1];
            Switch6Ways highPtData = GetSwitchItem(highPoint);
            if (wayWithEnd.WayLength > 1
                && (highPtData.MyType == Switch6Ways.Type.START
                || highPtData.MyType == Switch6Ways.Type.U))
            {
                if (highPtData.Ways[2] == true
                    || highPtData.Ways[3] == true
                    || highPtData.Ways[4] == true)
                {
                    completeWays.Add(wayWithEnd);
                    return;
                }
            }

            Switch6Ways nextPtData;
            BunchWays.CompleteWay newWay;
            if (highPtData.Ways[5] == true)
            {
                nextPtData = GetNeighbor(highPoint, 5);
                if (nextPtData != null && nextPtData.Ways[2] == true)
                {
                    newWay = wayWithEnd.Clone();
                    newWay.WayPoints.Add(GetRelaPosition(highPoint, 5));
                    CheckIfPointInOpenWays_FindWayUp(ref completeWays, newWay);
                }
            }
            if (highPtData.Ways[0] == true)
            {
                nextPtData = GetNeighbor(highPoint, 0);
                if (nextPtData != null && nextPtData.Ways[3] == true)
                {
                    newWay = wayWithEnd.Clone();
                    newWay.WayPoints.Add(GetRelaPosition(highPoint, 0));
                    CheckIfPointInOpenWays_FindWayUp(ref completeWays, newWay);
                }
            }
            if (highPtData.Ways[1] == true)
            {
                nextPtData = GetNeighbor(highPoint, 1);
                if (nextPtData != null && nextPtData.Ways[4] == true)
                {
                    newWay = wayWithEnd.Clone();
                    newWay.WayPoints.Add(GetRelaPosition(highPoint, 1));
                    CheckIfPointInOpenWays_FindWayUp(ref completeWays, newWay);
                }
            }
        }

        public void KillAtPoint(int colIdx, int rowIdx)
        {
            List<BunchWays.CompleteWay> ways2kill
                = GetOpenWaysWithPoint(new Point(colIdx, rowIdx));
            BunchWays.CompleteWay cw;
            for (int i = _openWays.Count - 1; i >= 0; i--)
            {
                cw = _openWays[i];
                foreach (BunchWays.CompleteWay w2Kil in ways2kill)
                {
                    if (w2Kil == cw)
                    {
                        _openWays.RemoveAt(i);
                        break;
                    }
                }
            }
            List<Point> allPoints = new List<Point>();
            foreach (BunchWays.CompleteWay curCW in ways2kill)
            {
                allPoints.AddRange(curCW.WayPoints);
            }
            RemoveDuplicatedPoints(ref allPoints);

            Switch6Ways s6w;
            if (allPoints.Count == 0)
            {
                s6w = GetSwitchItem(colIdx, rowIdx);
                s6w.MyType = Switch6Ways.Type.None;
            }
            else
            {
                Point laterPt, forwerPt;
                for (int i = allPoints.Count - 1; i > 0; i--)
                {
                    laterPt = allPoints[i];
                    for (int j = i - 1; j >= 0; j--)
                    {
                        forwerPt = allPoints[j];
                        if (laterPt.Y < forwerPt.Y)
                        {
                            allPoints[j] = laterPt;
                            allPoints[i] = forwerPt;
                        }
                    }
                }
                for (int i = allPoints.Count - 1; i >= 0; i--)
                {
                    KillPointAndDrop(allPoints[i]);
                }
            }
        }
        private List<BunchWays.CompleteWay> GetOpenWaysWithPoint(Point point)
        {
            List<BunchWays.CompleteWay> result
                = new List<BunchWays.CompleteWay>();
            foreach (BunchWays.CompleteWay cw in _openWays)
            {
                if (cw.ContainPoint(point) == true)
                {
                    result.Add(cw);
                }
            }
            return result;
        }
        public static void RemoveDuplicatedPoints(ref List<Point> ptList)
        {
            Point forwPt, curPt;
            for (int i = 0; i < ptList.Count - 1; i++)
            {
                forwPt = ptList[i];
                for (int j = ptList.Count - 1; j > i; j--)
                {
                    curPt = ptList[j];
                    if (forwPt == curPt)
                    {
                        ptList.RemoveAt(j);
                    }
                }
            }
        }
        private void KillPointAndDrop(Point point)
        {
            Switch6Ways curS6w = GetSwitchItem(point);
            if (curS6w.MyType == Switch6Ways.Type.START
                || curS6w.MyType == Switch6Ways.Type.END)
            {
                return;
            }
            curS6w.Dispose();
            for (int rowIdx = point.Y + 1; rowIdx <= _rowCount; rowIdx++)
            {
                curS6w = GetSwitchItem(point.X, rowIdx);
                curS6w.PosiInMatrix = new Point(point.X, rowIdx - 1);
                switchMatrix[point.X + "," + (rowIdx - 1)] = null;
                switchMatrix[point.X + "," + (rowIdx - 1)] = curS6w;
                //curS6w.TryBroadcastImChanged();
            }
            curS6w = new Switch6Ways();
            curS6w.PosiInMatrix = new Point(point.X, _rowCount);
            curS6w.ImChanged += dataItem_ImChanged;
            switchMatrix[point.X + "," + _rowCount] = curS6w;
            //curS6w.TryBroadcastImChanged();
            TryBroadcastSizeChanged();
        }

        public void ReCalculateWays(bool maxLength)
        {
            ReCalculateWays_Working = true;
            AllWays.Clear();
            ReCalculateWays_scanPoints();

            // all ways collected!!
            AllWays_Sort(maxLength == false);

            WayChoosePlan bestPlan
                = AllWays_SetupPlan(null, 0);
            //WayChoosePlan curPlan;
            //for (int i = okWayPlans.Count - 1; i >= 0; i--)
            //{
            //    curPlan = okWayPlans[i];
            //    if ((maxLength == true && curPlan.CountWaysLength > bestPlan.CountWaysLength)
            //        || (maxLength == false && curPlan.CountWaysLength < bestPlan.CountWaysLength))
            //    {
            //        bestPlan = curPlan;
            //    }
            //}
            if (bestPlan != null)
            {
                ReCalculateWays_Set2Panel(bestPlan.finalWays);
            }
        }
        private void ReCalculateWays_Set2Panel(List<BunchWays.CompleteWay> ways)
        {
            if (ways == null || ways.Count == 0) return;
            BunchWays.CompleteWay curOpenWay;
            for (int i = _openWays.Count - 1; i >= 0; i--)
            {
                curOpenWay = _openWays[i];
                SetUIOpenWay(curOpenWay, false, false);
                _openWays.RemoveAt(i);
            }
            SetOpenWays(ways);
            ReCalculateWays_Working = false;
        }

        private void SetOpenWays(List<BunchWays.CompleteWay> openWays)
        {
            SwitchMap switchMap = new SwitchMap();
            foreach (BunchWays.CompleteWay oneWay in openWays)
            {
                switchMap.AddWay(oneWay);
            }
            switchMap.RemoveDuplicatedPositions();

            Point curPt;
            Switch6Ways s6w;
            bool[] switchMapItemData;
            bool allOpen;
            for (int mapPosiIdx = switchMap.allPosition.Count - 1; mapPosiIdx >= 0; mapPosiIdx--)
            {
                curPt = switchMap.allPosition[mapPosiIdx];
                switchMapItemData = (bool[])switchMap.SMap[curPt.X + "," + curPt.Y];
                s6w = GetSwitchItem(curPt);
                allOpen = false;
                for (int turnTimes = s6w.PosibleShapesCount; turnTimes > 1; turnTimes--)
                {
                    allOpen = true;
                    for (int i = 0; i < 6; i++)
                    {
                        if (switchMapItemData[i] == true)
                        {
                            if (s6w.Ways[i] == false)
                            {
                                allOpen = false;
                            }
                        }
                    }
                    if (allOpen == true)
                    {
                        break;
                    }
                    s6w.TurnWaysRight(1);
                }
            }

            // set color
            BunchWays.CompleteWay curOW;
            for (int i = openWays.Count - 1; i >= 0; i--)
            {
                curOW = openWays[i];
                _openWays.Add(curOW);
                SetUIOpenWay(curOW, true, (i == 0));
            }
        }
        private class SwitchMap
        {
            public List<Point> allPosition
                = new List<Point>();
            public Hashtable SMap
                = new Hashtable();

            public void AddWay(BunchWays.CompleteWay oneWay)
            {
                Point curPoint, prePoint, nxtPoint;
                bool[] switch6Data;
                for (int i = oneWay.WayPoints.Count - 1; i >= 0; i--)
                {
                    prePoint = null;
                    curPoint = oneWay.WayPoints[i];
                    nxtPoint = null;
                    if (i == 0)
                    {
                        nxtPoint = oneWay.WayPoints[i + 1];
                    }
                    else if (i == oneWay.WayPoints.Count - 1)
                    {
                        prePoint = oneWay.WayPoints[i - 1];
                    }
                    else
                    {
                        prePoint = oneWay.WayPoints[i - 1];
                        nxtPoint = oneWay.WayPoints[i + 1];
                    }
                    switch6Data = (bool[])SMap[curPoint.X + "," + curPoint.Y];
                    if (switch6Data == null)
                    {
                        switch6Data = new bool[6] { false, false, false, false, false, false };
                    }

                    if (prePoint != null)
                    {
                        switch6Data[GetRelaIdx(curPoint, prePoint)] = true;
                    }
                    if (nxtPoint != null)
                    {
                        switch6Data[GetRelaIdx(curPoint, nxtPoint)] = true;
                    }
                    SMap[curPoint.X + "," + curPoint.Y] = switch6Data;
                    allPosition.Add(curPoint);

                }
            }
            public void RemoveDuplicatedPositions()
            {
                Point forwPt, curPt;
                for (int i = 0; i < allPosition.Count - 1; i++)
                {
                    forwPt = allPosition[i];
                    for (int j = allPosition.Count - 1; j > i; j--)
                    {
                        curPt = allPosition[j];
                        if (forwPt == curPt)
                        {
                            allPosition.RemoveAt(j);
                        }
                    }
                }
            }
        }

        #region filter ways
        private void AllWays_Sort(bool isASC)
        {
            BunchWays surBW, curBW;
            for (int i = AllWays.Count - 1; i > 0; i--)
            {
                surBW = AllWays[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    curBW = AllWays[j];
                    if (isASC == true)
                    {
                        if (surBW.MaxWayLength < curBW.MaxWayLength)
                        {
                            AllWays[j] = surBW;
                            AllWays[i] = curBW;
                        }
                    }
                    else
                    {
                        if (surBW.MaxWayLength > curBW.MaxWayLength)
                        {
                            AllWays[j] = surBW;
                            AllWays[i] = curBW;
                        }
                    }
                }
            }

            BunchWays.CompleteWay surCW, curCW;
            for (int k = AllWays.Count - 1; k >= 0; k--)
            {
                surBW = AllWays[k];
                for (int i = surBW.PosibleWays.Count - 1; i > 0; i--)
                {
                    surCW = surBW.PosibleWays[i];
                    for (int j = i - 1; j >= 0; j--)
                    {
                        curCW = surBW.PosibleWays[j];
                        if (isASC == true)
                        {
                            if (surCW.WayLength < curCW.WayLength)
                            {
                                surBW.PosibleWays[j] = surCW;
                                surBW.PosibleWays[i] = curCW;
                            }
                        }
                        else
                        {
                            if (surCW.WayLength > curCW.WayLength)
                            {
                                surBW.PosibleWays[j] = surCW;
                                surBW.PosibleWays[i] = curCW;
                            }
                        }
                    }
                }
            }
        }
        private class JointPoints
        {
            public BunchWays.CompleteWay cw1;
            public int cw1PtIdx;
            public BunchWays.CompleteWay cw2;
            public int cw2PtIdx;

            public bool CheckCompatibility(SwitchPanelMgr spMgr)
            {
                bool[] comSwitchWays = new bool[6];
                #region get joint points com switch ways(could be 1 to 4)
                for (int i = 5; i >= 0; i--)
                {
                    comSwitchWays[i] = false;
                }
                if (cw1PtIdx == 0)
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw1.WayPoints[cw1PtIdx], cw1.WayPoints[cw1PtIdx + 1])] = true;
                }
                else if (cw1PtIdx == cw1.WayLength - 1)
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw1.WayPoints[cw1PtIdx], cw1.WayPoints[cw1PtIdx - 1])] = true;
                }
                else
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw1.WayPoints[cw1PtIdx], cw1.WayPoints[cw1PtIdx + 1])] = true;
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw1.WayPoints[cw1PtIdx], cw1.WayPoints[cw1PtIdx - 1])] = true;
                }
                if (cw2PtIdx == 0)
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw2.WayPoints[cw2PtIdx], cw2.WayPoints[cw2PtIdx + 1])] = true;
                }
                else if (cw2PtIdx == cw2.WayLength - 1)
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw2.WayPoints[cw2PtIdx], cw2.WayPoints[cw2PtIdx - 1])] = true;
                }
                else
                {
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw2.WayPoints[cw2PtIdx], cw2.WayPoints[cw2PtIdx + 1])] = true;
                    comSwitchWays[SwitchPanelMgr.GetRelaIdx(cw2.WayPoints[cw2PtIdx], cw2.WayPoints[cw2PtIdx - 1])] = true;
                }
                #endregion

                Point pt = cw1.WayPoints[cw1PtIdx];
                Switch6Ways s6wData = spMgr.GetSwitchItem(pt.X, pt.Y);
                bool waysGood;
                for (int i = s6wData.PosibleShapesCount; i > 0; i--)
                {
                    waysGood = true;
                    for (int i6 = 0; i6 < 6; i6++)
                    {
                        if (comSwitchWays[i6] == true)
                        {
                            if (s6wData.Ways[i6] == false)
                            {
                                waysGood = false;
                                break;
                            }
                        }
                    }
                    if (waysGood == true)
                    {
                        return true;
                    }
                    if (s6wData.MyType == Switch6Ways.Type.START
                        || s6wData.MyType == Switch6Ways.Type.END)
                    {
                        break;
                    }
                    s6wData.TurnWaysRight(1);
                }
                return false;
            }
        }
        public static bool CheckWaysCompatibility(SwitchPanelMgr spMgr, BunchWays.CompleteWay cw1, BunchWays.CompleteWay cw2)
        {
            // find joint points
            List<JointPoints> jointPointCCList = new List<JointPoints>();
            Point p1, p2;
            JointPoints jp;
            for (int i = cw1.WayLength - 1; i >= 0; i--)
            {
                p1 = cw1.WayPoints[i];
                for (int j = cw2.WayLength - 1; j >= 0; j--)
                {
                    p2 = cw2.WayPoints[j];
                    if (p1 == p2)
                    {
                        jp = new JointPoints();
                        jp.cw1 = cw1;
                        jp.cw2 = cw2;
                        jp.cw1PtIdx = i;
                        jp.cw2PtIdx = j;
                        jointPointCCList.Add(jp);
                    }
                }
            }

            if (jointPointCCList.Count == 0)
            {
                return true;
            }
            else
            {
                foreach (JointPoints jPt in jointPointCCList)
                {
                    if (jPt.CheckCompatibility(spMgr) == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private class WayChoosePlan
        {
            public List<BunchWays.CompleteWay> finalWays
                = null;
            private List<BunchWays.CompleteWay> _oriWays
                = new List<BunchWays.CompleteWay>();

            public WayChoosePlan Clone()
            {
                WayChoosePlan result = new WayChoosePlan();
                for (int i = 0; i < this._oriWays.Count; i++)
                {
                    result.AddWay(_oriWays[i]);
                }
                return result;
            }

            public void AddWay(BunchWays.CompleteWay way)
            {
                _oriWays.Add(way);
            }
            public bool CheckAllWaysCompatibility(SwitchPanelMgr spMgr)
            {
                for (int i = _oriWays.Count - 1; i > 0; i--)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (SwitchPanelMgr.CheckWaysCompatibility(spMgr, _oriWays[i], _oriWays[j]) == false)
                        {
                            return false;
                        }
                    }
                }
                finalWays = _oriWays;
                return true;
            }
            public int CountWaysLength
            {
                get
                {
                    int result = 0;
                    foreach (BunchWays.CompleteWay curWay in _oriWays)
                    {
                        result += curWay.WayLength;
                    }
                    return result;
                }
            }
        }
        private WayChoosePlan AllWays_SetupPlan(WayChoosePlan curPlan, int curBWIdx)
        {
            if (curBWIdx == AllWays.Count)
            {
                return curPlan;
            }
            BunchWays curBW = AllWays[curBWIdx];
            if (curPlan == null) curPlan = new WayChoosePlan();
            WayChoosePlan newWCP;
            foreach (BunchWays.CompleteWay cw in curBW.PosibleWays)
            {
                newWCP = curPlan.Clone();
                newWCP.AddWay(cw);
                if (newWCP.CheckAllWaysCompatibility(this))
                {
                    curPlan = newWCP;
                }
            }
            return AllWays_SetupPlan(curPlan, curBWIdx + 1);
        }

        #endregion

        #region search and collect all ways
        private void ReCalculateWays_scanPoints()
        {
            for (int rowIdx = 0; rowIdx < (RowCount + 1); rowIdx++)
            {
                for (int colIdx = (TapCount * 2); colIdx >= 0; colIdx--)
                {
                    ReCalculateWays_checkAndStart(colIdx, rowIdx);
                }
            }
        }
        private void ReCalculateWays_checkAndStart(int colIdx, int rowIdx)
        {
            Switch6Ways s6wData = GetSwitchItem(colIdx, rowIdx);
            if (s6wData == null) return;
            if ((s6wData.MyType == Switch6Ways.Type.END
                || s6wData.MyType == Switch6Ways.Type.U) == false)
            {
                return;
            }
            //BunchWays newBW = new BunchWays();
            //newBW.StartPoint = new Point(colIdx, rowIdx);
            ReCalculateWays_WaySearchStart(new Point(colIdx, rowIdx));
        }
        private void ReCalculateWays_WaySearchStart(Point startPoint)
        {
            BunchWays.CompleteWay oneWay = new BunchWays.CompleteWay();
            oneWay.WayPoints.Add(startPoint);

            Switch6Ways s6wData = GetSwitchItem(startPoint.X, startPoint.Y);
            for (int tryCount = s6wData.PosibleShapesCount; tryCount > 0; tryCount--)
            {
                if (s6wData.MyType == Switch6Ways.Type.END
                    || s6wData.MyType == Switch6Ways.Type.START)
                {
                    ReCalculateWays_WaySearch(oneWay.Clone());
                    return;
                }
                else
                {
                    s6wData.TurnWaysRight(1);
                    ReCalculateWays_WaySearch(oneWay.Clone());
                }
            }
        }
        private class SearchOneWayPassArgs : EventArgs
        {
            public BunchWays.CompleteWay oneWay;
            public SearchOneWayPassArgs(BunchWays.CompleteWay oneWay)
            {
                this.oneWay = oneWay;
            }
        }
        private event EventHandler<SearchOneWayPassArgs> SearchOneWayPass;
        void SwitchPanelMgr_SearchOneWayPass(object sender, SearchOneWayPassArgs e)
        {
            // one way OK
            bool found = false;
            if (AllWays.Count > 0)
            {
                Point firstPoint = e.oneWay.WayPoints[0];
                BunchWays curBW;
                for (int i = AllWays.Count - 1; i >= 0; i--)
                {
                    curBW = AllWays[i];
                    if (curBW.StartPoint == firstPoint)
                    {
                        found = true;
                        curBW.PosibleWays.Add(e.oneWay);
                        break;
                    }
                }
            }
            if (AllWays.Count == 0 || found == false)
            {
                BunchWays newBW = new BunchWays();
                newBW.PosibleWays.Add(e.oneWay);
                AllWays.Add(newBW);
            }
        }
        private void ReCalculateWays_WaySearch(BunchWays.CompleteWay oneWay)
        {
            if (oneWay.WayPoints.Count == 0) return;

            Point lastPoint = oneWay.WayPoints[oneWay.WayPoints.Count - 1];
            Switch6Ways lastPointData = GetSwitchItem(lastPoint.X, lastPoint.Y);
            if (lastPointData == null) return;

            if (oneWay.WayPoints.Count == 1)
            {
                // first point
                ReCalculateWays_WaySearch_3Ways(oneWay);
            }
            else
            {
                // SEC POINT or END POINT
                Point prePoint = oneWay.WayPoints[oneWay.WayPoints.Count - 2];
                Switch6Ways prePointData = GetSwitchItem(prePoint.X, prePoint.Y);
                int wayIdxLast2Pre = GetRelaIdx(lastPoint, prePoint);
                for (int tryCount = lastPointData.PosibleShapesCount; tryCount > 0; tryCount--)
                {
                    // get prePosi wayIdx
                    if ((lastPointData.MyType == Switch6Ways.Type.START
                        || lastPointData.MyType == Switch6Ways.Type.END
                        || lastPointData.MyType == Switch6Ways.Type.None) == false)
                    {
                        lastPointData.TurnWaysRight(1);
                    }
                    if (lastPointData.Ways[wayIdxLast2Pre] == true)
                    {
                        if (lastPointData.MyType == Switch6Ways.Type.U
                            || lastPointData.MyType == Switch6Ways.Type.START)
                        {
                            if (SearchOneWayPass != null)
                            {
                                SearchOneWayPass(this, new SearchOneWayPassArgs(oneWay));
                            }
                        }
                        else
                        {
                            ReCalculateWays_WaySearch_3Ways(oneWay);
                        }
                    }
                }
            }

        }
        private void ReCalculateWays_WaySearch_3Ways(BunchWays.CompleteWay surWay)
        {
            BunchWays.CompleteWay newWay;
            Point lastPoint = surWay.WayPoints[surWay.WayPoints.Count - 1];
            Switch6Ways lastPointData = GetSwitchItem(lastPoint.X, lastPoint.Y);
            if (lastPointData.Ways[5] == true)
            {
                newWay = surWay.Clone();
                newWay.WayPoints.Add(GetRelaPosition(lastPoint.X, lastPoint.Y, 5));
                ReCalculateWays_WaySearch(newWay);
            }
            if (lastPointData.Ways[0] == true)
            {
                newWay = surWay.Clone();
                newWay.WayPoints.Add(GetRelaPosition(lastPoint.X, lastPoint.Y, 0));
                ReCalculateWays_WaySearch(newWay);
            }
            if (lastPointData.Ways[1] == true)
            {
                newWay = surWay.Clone();
                newWay.WayPoints.Add(GetRelaPosition(lastPoint.X, lastPoint.Y, 1));
                ReCalculateWays_WaySearch(newWay);
            }
        }
        #endregion
    }
}
