using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Input;

namespace MouseClickAnalysis
{
    public class Class_MCDataAnalysts
    {
        public struct MCData
        {
            public MouseButton mBtn;
            public DateTime timePoint;
        }
        private List<MCData> mcDataList
            = new List<MCData>();

        #region base vars

        public static long MaxVoidTimeMS = 1000;

        private DateTime lastBtnDownTime_Left = DateTime.MinValue;
        private DateTime lastBtnDownTime_Middle = DateTime.MinValue;
        private DateTime lastBtnDownTime_Right = DateTime.MinValue;
        private DateTime lastBtnDownTime_XBtn1 = DateTime.MinValue;
        private DateTime lastBtnDownTime_XBtn2 = DateTime.MinValue;

        private DateTime lastActTime = DateTime.MinValue;

        // 0 ready to rec, 1 recording, 2 stoped
        public int mbRecState_Left = 0;
        public int mbRecState_Middle = 0;
        public int mbRecState_Right = 0;
        public int mbRecState_XBtn1 = 0;
        public int mbRecState_XBtn2 = 0;

        #endregion

        public Class_MCDataAnalysts()
        {
        }

        public void ClearAndReset()
        {
            mcDataList.Clear();

            lastBtnDownTime_Left = DateTime.MinValue;
            lastBtnDownTime_Middle = DateTime.MinValue;
            lastBtnDownTime_Right = DateTime.MinValue;
            lastBtnDownTime_XBtn1 = DateTime.MinValue;
            lastBtnDownTime_XBtn2 = DateTime.MinValue;

            lastActTime = DateTime.MinValue;

            mbRecState_Left = 0;
            mbRecState_Middle = 0;
            mbRecState_Right = 0;
            mbRecState_XBtn1 = 0;
            mbRecState_XBtn2 = 0;
        }
        public bool IsStoped
        {
            get
            {
                return (mbRecState_Left == 2
                    && mbRecState_Middle == 2
                    && mbRecState_Right == 2
                    && mbRecState_XBtn1 == 2
                    && mbRecState_XBtn2 == 2);
            }
        }

        private void _StopAndMakeAnalysisData()
        {
            _MakeAnalysisDataList();

            mbRecState_Left = 2;
            mbRecState_Middle = 2;
            mbRecState_Right = 2;
            mbRecState_XBtn1 = 2;
            mbRecState_XBtn2 = 2;

            if (AnalysisStoped != null)
            {
                AnalysisStoped(this, null);
            }
        }
        public event EventHandler AnalysisStoped;

        public class OneButtonRecStopedArgs : EventArgs
        {
            public OneButtonRecStopedArgs()
            {
            }
            public OneButtonRecStopedArgs(MouseButton targetBtn)
            {
                this.targetBtn = targetBtn;
            }
            public MouseButton targetBtn;
        }
        public event EventHandler<OneButtonRecStopedArgs> OneButtonRecStoped;

        public void CheckTimeVoid()
        {
            if (IsStoped == true)
            {
                return;
            }

            if (lastActTime != DateTime.MinValue)
            {
                DateTime now = DateTime.Now;
                TimeSpan ts = now.Subtract(lastActTime);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    _StopAndMakeAnalysisData();
                }
            }
        }
        public void MCDataRec(MouseButton mBtn)
        {
            #region if a btn record is stoped, return
            if (mbRecState_Left == 2 && mBtn == MouseButton.Left)
            {
                return;
            }
            if (mbRecState_Middle == 2 && mBtn == MouseButton.Middle)
            {
                return;
            }
            if (mbRecState_Right == 2 && mBtn == MouseButton.Right)
            {
                return;
            }
            if (mbRecState_XBtn1 == 2 && mBtn == MouseButton.XButton1)
            {
                return;
            }
            if (mbRecState_XBtn2 == 2 && mBtn == MouseButton.XButton2)
            {
                return;
            }
            #endregion

            DateTime now = DateTime.Now;
            MCData newMCD = new MCData()
            {
                mBtn = mBtn,
                timePoint = now,
            };

            #region check currect act time, stop a button?  return
            TimeSpan ts;
            switch (newMCD.mBtn)
            {
                case MouseButton.Left:
                    if (lastBtnDownTime_Left != DateTime.MinValue)
                    {
                        ts = now.Subtract(lastBtnDownTime_Left);
                        if (ts.TotalMilliseconds > MaxVoidTimeMS)
                        {
                            mbRecState_Left = 2;
                            if (OneButtonRecStoped != null)
                                OneButtonRecStoped(this, new OneButtonRecStopedArgs(newMCD.mBtn));
                            return;
                        }
                    }
                    lastBtnDownTime_Left = now;
                    break;
                case MouseButton.Middle:
                    if (lastBtnDownTime_Middle != DateTime.MinValue)
                    {
                        ts = now.Subtract(lastBtnDownTime_Middle);
                        if (ts.TotalMilliseconds > MaxVoidTimeMS)
                        {
                            mbRecState_Middle = 2;
                            if (OneButtonRecStoped != null)
                                OneButtonRecStoped(this, new OneButtonRecStopedArgs(newMCD.mBtn));
                            return;
                        }
                    }
                    lastBtnDownTime_Middle = now;
                    break;
                case MouseButton.Right:
                    if (lastBtnDownTime_Right != DateTime.MinValue)
                    {
                        ts = now.Subtract(lastBtnDownTime_Right);
                        if (ts.TotalMilliseconds > MaxVoidTimeMS)
                        {
                            mbRecState_Right = 2;
                            if (OneButtonRecStoped != null)
                                OneButtonRecStoped(this, new OneButtonRecStopedArgs(newMCD.mBtn));
                            return;
                        }
                    }
                    lastBtnDownTime_Right = now;
                    break;
                case MouseButton.XButton1:
                    if (lastBtnDownTime_XBtn1 != DateTime.MinValue)
                    {
                        ts = now.Subtract(lastBtnDownTime_XBtn1);
                        if (ts.TotalMilliseconds > MaxVoidTimeMS)
                        {
                            mbRecState_XBtn1 = 2;
                            if (OneButtonRecStoped != null)
                                OneButtonRecStoped(this, new OneButtonRecStopedArgs(newMCD.mBtn));
                            return;
                        }
                    }
                    lastBtnDownTime_XBtn1 = now;
                    break;
                case MouseButton.XButton2:
                    if (lastBtnDownTime_XBtn2 != DateTime.MinValue)
                    {
                        ts = now.Subtract(lastBtnDownTime_XBtn2);
                        if (ts.TotalMilliseconds > MaxVoidTimeMS)
                        {
                            mbRecState_XBtn2 = 2;
                            if (OneButtonRecStoped != null)
                                OneButtonRecStoped(this, new OneButtonRecStopedArgs(newMCD.mBtn));
                            return;
                        }
                    }
                    lastBtnDownTime_XBtn2 = now;
                    break;
            }

            if (lastActTime != DateTime.MinValue)
            {
                ts = newMCD.timePoint.Subtract(lastActTime);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    _StopAndMakeAnalysisData();
                }
            }
            lastActTime = now;

            if (mbRecState_Left != 2 && lastBtnDownTime_Left != DateTime.MinValue)
            {
                ts = now.Subtract(lastBtnDownTime_Left);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    mbRecState_Left = 2;
                    if (OneButtonRecStoped != null)
                        OneButtonRecStoped(this, new OneButtonRecStopedArgs(MouseButton.Left));
                    return;
                }
            }
            if (mbRecState_Middle != 2 && lastBtnDownTime_Middle != DateTime.MinValue)
            {
                ts = now.Subtract(lastBtnDownTime_Middle);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    mbRecState_Middle = 2;
                    if (OneButtonRecStoped != null)
                        OneButtonRecStoped(this, new OneButtonRecStopedArgs(MouseButton.Middle));
                    return;
                }
            }
            if (mbRecState_Right != 2 && lastBtnDownTime_Right != DateTime.MinValue)
            {
                ts = now.Subtract(lastBtnDownTime_Right);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    mbRecState_Right = 2;
                    if (OneButtonRecStoped != null)
                        OneButtonRecStoped(this, new OneButtonRecStopedArgs(MouseButton.Right));
                    return;
                }
            }
            if (mbRecState_XBtn1 != 2 && lastBtnDownTime_XBtn1 != DateTime.MinValue)
            {
                ts = now.Subtract(lastBtnDownTime_XBtn1);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    mbRecState_XBtn1 = 2;
                    if (OneButtonRecStoped != null)
                        OneButtonRecStoped(this, new OneButtonRecStopedArgs(MouseButton.XButton1));
                    return;
                }
            }
            if (mbRecState_XBtn2 != 2 && lastBtnDownTime_XBtn2 != DateTime.MinValue)
            {
                ts = now.Subtract(lastBtnDownTime_XBtn2);
                if (ts.TotalMilliseconds > MaxVoidTimeMS)
                {
                    mbRecState_XBtn2 = 2;
                    if (OneButtonRecStoped != null)
                        OneButtonRecStoped(this, new OneButtonRecStopedArgs(MouseButton.XButton2));
                    return;
                }
            }
            #endregion

            mcDataList.Add(newMCD);
        }


        public struct CountDataItem
        {
            public int timeLengthMS;
            public MouseButton mBtn;
            public int MBDownCount;
        }
        public List<CountDataItem> CountDataList
            = new List<CountDataItem>();
        private void _MakeAnalysisDataList()
        {
            CountDataList.Clear();

            #region make count list

            FindAllBtnNAddMCD(MouseButton.Left);
            FindAllBtnNAddMCD(MouseButton.Middle);
            FindAllBtnNAddMCD(MouseButton.Right);
            FindAllBtnNAddMCD(MouseButton.XButton1);
            FindAllBtnNAddMCD(MouseButton.XButton2);


            #endregion

            #region sort count list

            _SortCDList_ByMBtn();
            _SortCDList_WithinMBtnType(MouseButton.Left);
            _SortCDList_WithinMBtnType(MouseButton.Middle);
            _SortCDList_WithinMBtnType(MouseButton.Right);
            _SortCDList_WithinMBtnType(MouseButton.XButton1);
            _SortCDList_WithinMBtnType(MouseButton.XButton2);

            #endregion
        }
        private void FindAllBtnNAddMCD(MouseButton button)
        {
            MCData curMCD, preMCD = new MCData();
            bool first = true;
            int timeLength;
            int mcIdx, mcIdxMax = mcDataList.Count;
            for (mcIdx = 0; mcIdx < mcIdxMax; mcIdx++)
            {
                curMCD = mcDataList[mcIdx];
                if (curMCD.mBtn == button)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        timeLength
                            = (int)(curMCD.timePoint.Subtract(preMCD.timePoint)
                                    .TotalMilliseconds + 0.5);
                        _AddCDList(button, timeLength);
                    }
                    preMCD = curMCD;
                }

            }
        }
        private void _AddCDList(MouseButton button, int timeLength)
        {
            List<CountDataItem> foundItem
                = CountDataList.Where(a => a.mBtn == button && a.timeLengthMS == timeLength).ToList();

            if (foundItem.Count == 0)
            {
                CountDataList.Add(
                    new CountDataItem()
                    {
                        timeLengthMS   = timeLength,
                        mBtn = button,
                        MBDownCount = 1,
                    });
            }
            else
            {
                CountDataItem f = foundItem[0];
                f.MBDownCount++;
                foundItem[0] = f;
            }
        }
        private void _SortCDList_ByMBtn()
        {
            CountDataItem formerCDData, laterCDData;
            for (int i = CountDataList.Count - 1; i > 0; i--)
            {
                laterCDData = CountDataList[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    formerCDData = CountDataList[j];
                    if (_SortCDList_ByMBtn_btnBigger(laterCDData.mBtn, formerCDData.mBtn))
                    {
                        CountDataList[i] = formerCDData;
                        CountDataList[j] = laterCDData;
                    }
                }
            }

        }
        private bool _SortCDList_ByMBtn_btnBigger(MouseButton mbA, MouseButton mbB)
        {
            if (mbA == mbB) return false;

            if (mbA == MouseButton.Left) return true;
            else if (mbA == MouseButton.Middle)
            {
                if (mbB != MouseButton.Left)
                {
                    return true;
                }
            }
            else if (mbA == MouseButton.Right)
            {
                if (mbB != MouseButton.Left
                    && mbB != MouseButton.Middle
                    )
                {
                    return true;
                }
            }
            else if (mbA == MouseButton.XButton1)
            {
                if (mbB != MouseButton.Left
                    && mbB != MouseButton.Middle
                    && mbB != MouseButton.Right
                    )
                {
                    return true;
                }
            }
            else if (mbA == MouseButton.XButton2)
            {
                if (mbB != MouseButton.Left
                    && mbB != MouseButton.Middle
                    && mbB != MouseButton.Right
                    && mbB != MouseButton.XButton1
                    )
                {
                    return true;
                }
            }

            return false;
        }
        private void _SortCDList_WithinMBtnType(MouseButton mbType)
        {
            CountDataItem formerCDData, laterCDData;
            for (int i = CountDataList.Count - 1; i > 0; i--)
            {
                laterCDData = CountDataList[i];
                if (mbType != laterCDData.mBtn) break;
                for (int j = i - 1; j >= 0; j--)
                {
                    formerCDData = CountDataList[j];

                    if (mbType != formerCDData.mBtn) return;

                    if (_SortCDList_ByMBtn_btnBigger(laterCDData.mBtn, formerCDData.mBtn))
                    {
                        CountDataList[i] = formerCDData;
                        CountDataList[j] = laterCDData;
                    }
                }
            }
        }

        public List<CountDataItem> CopyAndGetCountDataList_ByMBtn(MouseButton mouseButton)
        {
            List<CountDataItem> result = new List<CountDataItem>();
            CountDataItem curCDItem;
            for (int i = 0; i < CountDataList.Count; i++)
            {
                curCDItem = CountDataList[i];
                if (curCDItem.mBtn != mouseButton) continue;
                else result.Add(new CountDataItem()
                {
                    timeLengthMS = curCDItem.timeLengthMS,
                    mBtn = curCDItem.mBtn,
                    MBDownCount = curCDItem.MBDownCount,
                });
            }
            return result;
        }
    }
}
