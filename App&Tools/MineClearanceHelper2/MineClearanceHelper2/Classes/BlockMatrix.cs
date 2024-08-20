using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MadTomDev.App.Ctrls;
using MadTomDev.Common;

namespace MadTomDev.App.Classes
{
    public class BlockMatrix
    {
        public class OneBlockData
        {
            public Point position;
            public enum Status
            {
                Ground = 0,

                MineSignal_Lv1 = 1,
                MineSignal_Lv2 = 2,
                MineSignal_Lv3 = 3,
                MineSignal_Lv4 = 4,
                MineSignal_Lv5 = 5,
                MineSignal_Lv6 = 6,
                MineSignal_Lv7 = 7,
                MineSignal_Lv8 = 8,

                Deteced_Save = 9,
                Deteced_Mine = 10,
                Deteced_Unknow = 11,
            }
            private Status _Status;
            public Status Statu
            {
                get { return _Status; }
            }
            public void SetStatus(Status newStatus, bool backSetToUI = true)
            {
                _Status = newStatus;
                if (backSetToUI && uiControl != null)
                {
                    uiControl.SetDetectStatus(newStatus);
                }
            }

            public bool IsSignal
            {
                get
                {
                    int signal = (int)_Status;
                    return 1 <= signal && signal <= 8;
                }
            }
            public int StatuInt
            {
                get => (int)_Status;
            }

            public OneBlock? uiControl;

            public OneBlockData(int x, int y, OneBlock? ui = null)
            {
                position.X = x;
                position.Y = y;
                uiControl = ui;
            }
            public OneBlockData(Point position, OneBlock? ui = null)
            {
                this.position = position;
                uiControl = ui;
            }

            public OneBlockData Clone(bool withUi = true)
            {
                OneBlock? cloneUi = withUi ? uiControl?.Clone() : null;
                OneBlockData clone = new OneBlockData(position, cloneUi)
                { _Status = _Status, };
                if (cloneUi != null)
                {
                    cloneUi.blockData = clone;
                }
                return clone;
            }
        }
        public List<OneBlockData> allBlocksList
            = new List<OneBlockData>();
        public Hashtable allBlocksHashTable_keyPosies
            = new Hashtable();
        public Hashtable allBlocksHashTable_keyUI
            = new Hashtable();

        public void AddItem(OneBlockData block)
        {
            allBlocksList.Add(block);
            allBlocksHashTable_keyPosies.Add(block.position, block);
            allBlocksHashTable_keyUI.Add(block.uiControl, block);

            block.uiControl.Call_Detect += uiControl_Call_Detect;
            block.uiControl.Call_Dispose += uiControl_Call_Dispose;
        }

        public event EventHandler Call_GridViewRemoveChild;
        void uiControl_Call_Detect(object sender, EventArgs e)
        {
            OneBlockData target = this.GetBlock((OneBlock)sender);
            DetectNMark(target.position);
        }
        void uiControl_Call_Dispose(object sender, EventArgs e)
        {
            OneBlockData target = GetBlock((OneBlock)sender);
            if (Call_GridViewRemoveChild != null)
            {
                Call_GridViewRemoveChild(target.uiControl, e);
            }
            allBlocksHashTable_keyPosies.Remove(target.position);
            allBlocksHashTable_keyUI.Remove(target.uiControl);
            allBlocksList.Remove(target);
            DetectNMark(target.position);
        }
        public event EventHandler Flag_Mines;
        public event EventHandler Flag_Clears;
        public event EventHandler Flag_Unknows;
        public void DetectNMark(Point position)
        {
            OneBlockData? curItem = GetBlock(position);
            if (curItem == null
                || curItem.uiControl == null
                || !curItem.IsSignal)
            {
                return;
            }
            int centerSignal = (int)curItem.uiControl.DetectStatus;

            // check if all mines, or unknow
            _CountAround(position.X, position.Y, out CounterAround counter);

            bool markMines = _AutoSetAround_toMines(position.X, position.Y, ref counter);
            bool markSaves = false;
            bool markUnknow = false;
            if (!markMines)
            {
                markSaves = _AutoSetAround_toSaves(position.X, position.Y, ref counter);
            }
            if (!markSaves)
            {
                markUnknow = _AutoSetAround_toUnknows(position.X, position.Y, ref counter);
            }

            if (markMines)
            {
                Flag_Mines?.Invoke(this, new EventArgs());
            }
            else if (markUnknow)
            {
                Flag_Unknows?.Invoke(this, new EventArgs());
            }
            else if (markSaves)
            {
                Flag_Clears?.Invoke(this, new EventArgs());
            }
            else // !markMines && !markUnknow && !markSaves
            {
                if (counter.unknows > 0)
                {
                    // nothing changed, try test unknow field
                    TestUnknowFieldNMark(position.X, position.Y);
                }
                else
                {
                    // do nothing, for now
                }
            }
        }


        #region count around
        public struct CounterAround
        {
            public int allBlocks;
            public int saves;
            public int mines;
            public int unknows;
        }
        private void _CountAround(int x, int y, out CounterAround counter)
        {
            counter.allBlocks = 0;
            counter.saves = 0;
            counter.mines = 0;
            counter.unknows = 0;
            foreach (Point p in GetPointsAround(x, y))
            {
                _CountAround_Add(GetBlock(p.X, p.Y), ref counter);
            }
        }
        private void _CountAround(Dictionary<Point, OneBlockData> field, int x, int y, out CounterAround counter)
        {
            counter.allBlocks = 0;
            counter.saves = 0;
            counter.mines = 0;
            counter.unknows = 0;
            foreach (Point p in GetPointsAround(x, y))
            {
                _CountAround_Add(_GetBlock(field, p.X, p.Y), ref counter);
            }
        }
        private void _CountAround_Add(OneBlockData? block, ref CounterAround counter)
        {
            if (block == null)
            {
                return;
            }
            int curSignal = (int)block.Statu;
            if (curSignal == 0)
            {
                counter.allBlocks++;
            }
            if (curSignal == 11)
            {
                counter.allBlocks++;
                counter.unknows++;
            }
            else if (curSignal == 9)
            {
                counter.allBlocks++;
                counter.saves++;
            }
            else if (curSignal == 10)
            {
                counter.allBlocks++;
                counter.mines++;
            }
        }
        #endregion


        #region auto set around
        private bool _AutoSetAround_toMines(int x, int y, ref CounterAround counter)
        {
            OneBlockData? curBlock = GetBlock(x, y);
            if (curBlock == null || !curBlock.IsSignal)
            {
                return false;
            }
            int signal = (int)curBlock.Statu;

            bool marked = false;
            if (counter.allBlocks <= signal
                || counter.allBlocks - counter.saves <= signal)
            {
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (_AutoSetBlock_to(p.X, p.Y, OneBlockData.Status.Deteced_Mine))
                    {
                        counter.mines++;
                        marked = true;
                    }
                }
            }
            return marked;
        }
        private bool _AutoSetAround_toMines(ref Dictionary<Point, OneBlockData> field, int x, int y, ref CounterAround counter)
        {
            OneBlockData? curBlock = _GetBlock(field, x, y);
            if (curBlock == null || !curBlock.IsSignal)
            {
                return false;
            }
            int signal = (int)curBlock.Statu;

            bool marked = false;
            if (counter.allBlocks <= signal
                || counter.allBlocks - counter.saves <= signal - counter.saves)
            {
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (_AutoSetBlock_to(ref field, p.X, p.Y, OneBlockData.Status.Deteced_Mine))
                    {
                        counter.mines++;
                        marked = true;
                    }
                }
            }
            return marked;
        }
        private bool _AutoSetBlock_to(int x, int y, OneBlockData.Status status)
        {
            OneBlockData? block = GetBlock(x, y);
            if (block == null)
            {
                return false;
            }
            switch (block.Statu)
            {
                case OneBlockData.Status.Ground:
                case OneBlockData.Status.Deteced_Unknow:
                    block.SetStatus(status);
                    return true;
            }
            return false;
        }
        private bool _AutoSetBlock_to(ref Dictionary<Point, OneBlockData> field, int x, int y, OneBlockData.Status status)
        {
            OneBlockData? block = _GetBlock(field, x, y);
            if (block == null)
            {
                return false;
            }
            switch (block.Statu)
            {
                case OneBlockData.Status.Ground:
                case OneBlockData.Status.Deteced_Unknow:
                    block.SetStatus(status);
                    return true;
            }
            return false;
        }
        private bool _AutoSetAround_toSaves(int x, int y, ref CounterAround counter)
        {
            OneBlockData? curBlock = GetBlock(x, y);
            if (curBlock == null || !curBlock.IsSignal)
            {
                return false;
            }
            int signal = (int)curBlock.Statu;

            bool marked = false;
            if (signal <= counter.mines)
            {
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (_AutoSetBlock_to(p.X, p.Y, OneBlockData.Status.Deteced_Save))
                    {
                        counter.saves++;
                        marked = true;
                    }
                }
            }
            return marked;
        }
        private bool _AutoSetAround_toSaves(ref Dictionary<Point, OneBlockData> field, int x, int y, ref CounterAround counter)
        {
            OneBlockData? curBlock = _GetBlock(field, x, y);
            if (curBlock == null || !curBlock.IsSignal)
            {
                return false;
            }
            int signal = (int)curBlock.Statu;

            bool marked = false;
            if (signal <= counter.mines)
            {
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (_AutoSetBlock_to(ref field, p.X, p.Y, OneBlockData.Status.Deteced_Save))
                    {
                        counter.saves++;
                        marked = true;
                    }
                }
            }
            return marked;
        }

        private bool _AutoSetAround_toUnknows(int x, int y, ref CounterAround counter)
        {
            OneBlockData? curBlock = GetBlock(x, y);
            if (curBlock == null || !curBlock.IsSignal)
            {
                return false;
            }

            bool marked = false;
            foreach (Point p in GetPointsAround(x, y))
            {
                if (_SetBlankBlock_toUnknow(p.X, p.Y)) { counter.unknows++; marked = true; }
            }

            return marked;
        }
        private bool _SetBlankBlock_toUnknow(int x, int y)
        {
            OneBlockData? block = GetBlock(x, y);
            if (block == null)
            {
                return false;
            }
            if (block.Statu == OneBlockData.Status.Ground)
            {
                block.SetStatus(OneBlockData.Status.Deteced_Unknow);
                return true;
            }
            return false;
        }
        #endregion


        private void TestUnknowFieldNMark(int x, int y)
        {
            // get unknow fields(maybe not just 1)
            List<Point> addedUnknowPosi = new List<Point>();
            List<Dictionary<Point, OneBlockData>> fields = GetUnknowFields(x, y);
            List<Dictionary<Point, OneBlockData>> testResults = new List<Dictionary<Point, OneBlockData>>();
            OneBlockData? curItem;
            bool markedMine = false, markedSave = false;
            foreach (Dictionary<Point, OneBlockData> field in fields)
            {
                // copy N test
                testResults.Clear();
                TestField(field);

                // 2024 0819
                // check all passed results, if all same, then it's pass
                List<Point> unknowBlockPosiList = GetUnknowBlockKeyList(field);
                //if (CheckResults(ref testResults, ref unknowBlockPosiList))
                //{
                //    foreach (Point p in unknowBlockPosiList)
                //    {
                //        OneBlockData? block = GetBlock(p);
                //        if (block == null)
                //        {
                //            continue;
                //        }
                //        OneBlockData.Status? newStatu = testResults[0][p]?.Statu;
                //        if (newStatu != null)
                //        {
                //            block.SetStatus((OneBlockData.Status)newStatu);
                //            if (newStatu == OneBlockData.Status.Deteced_Mine)
                //            {
                //                markedMine = true;
                //            }
                //            else if (newStatu == OneBlockData.Status.Deteced_Save)
                //            {
                //                markedSave = true;
                //            }
                //        }
                //    }
                //}


                // 2024 08 20
                // mark definite flags
                if (testResults.Count > 0)
                {
                    foreach (Point p in unknowBlockPosiList)
                    {
                        OneBlockData.Status curStatus = OneBlockData.Status.Ground;
                        bool definite = true;
                        foreach (Dictionary<Point, OneBlockData> r in testResults)
                        {
                            if (curStatus == OneBlockData.Status.Ground)
                            {
                                curStatus = r[p].Statu;
                            }
                            if (curStatus != r[p].Statu)
                            {
                                definite = false;
                                break;
                            }
                        }
                        if (definite)
                        {
                            OneBlockData? block = GetBlock(p);
                            if (block == null)
                            {
                                continue;
                            }
                            if (curStatus == OneBlockData.Status.Deteced_Mine)
                            {
                                block.SetStatus(curStatus);
                                markedMine = true;
                            }
                            else if (curStatus == OneBlockData.Status.Deteced_Save)
                            {
                                block.SetStatus(curStatus);
                                markedSave = true;
                            }
                        }
                    }
                }

            }

            // info
            if (markedMine)
            {
                Flag_Mines?.Invoke(this, new EventArgs());
            }
            else if (markedSave)
            {
                Flag_Clears?.Invoke(this, new EventArgs());
            }
            else
            {
                Flag_Unknows?.Invoke(this, new EventArgs());
            }


            #region get unknow-blocks fields
            List<Dictionary<Point, OneBlockData>> GetUnknowFields(int x, int y)
            {
                List<Dictionary<Point, OneBlockData>> result
                    = new List<Dictionary<Point, OneBlockData>>();
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (TryGetAddUnknowField(ref result, p.X, p.Y) == false)
                    {
                        result.Clear();
                        return result;
                    }
                }
                return result;
            }
            bool? TryGetAddUnknowField(ref List<Dictionary<Point, OneBlockData>> fieldList, int x, int y)
            {
                OneBlockData? curItem = GetBlock(x, y);
                if (curItem == null)
                {
                    return null;
                }
                if (curItem.Statu == BlockMatrix.OneBlockData.Status.Ground)
                {
                    return false;
                }
                foreach (Dictionary<Point, OneBlockData> f in fieldList)
                {
                    if (f.ContainsValue(curItem))
                    {
                        return null;
                    }
                }

                addedUnknowPosi.Clear();
                Dictionary<Point, OneBlockData> newField = new Dictionary<Point, OneBlockData>();
                newField.Add(curItem.position, curItem);
                if (TryGetAddUnknowField_AddAroundloop(ref newField, x, y, 2) == true)
                {
                    foreach (Dictionary<Point, OneBlockData> f in fieldList)
                    {
                        foreach (Point p in newField.Keys)
                        {
                            if (f.ContainsKey(p))
                            {
                                return null;
                            }
                        }
                    }
                    fieldList.Add(newField);
                    return true;
                }
                return false;
            }
            bool? TryGetAddUnknowField_AddAroundloop(ref Dictionary<Point, OneBlockData> field, int x, int y, int radius)
            {
                foreach (Point p in GetPointsAround(x, y))
                {
                    if (TryGetAddUnknowField_AddAroundloop_one(ref field, p.X, p.Y) == false)
                    {
                        return false;
                    }
                    if (addedUnknowPosi.Contains(p))
                    {
                        continue;
                    }
                    if (radius <= 1)
                    {
                        continue;
                    }
                    OneBlockData? testItem = GetBlock(p.X, p.Y);
                    if (testItem != null)
                    {
                        addedUnknowPosi.Add(p);
                        if (testItem.Statu == OneBlockData.Status.Deteced_Unknow)
                        {
                            if (TryGetAddUnknowField_AddAroundloop(ref field, p.X, p.Y, 2) == false)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (TryGetAddUnknowField_AddAroundloop(ref field, p.X, p.Y, radius - 1) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            bool? TryGetAddUnknowField_AddAroundloop_one(ref Dictionary<Point, OneBlockData> field, int x, int y)
            {
                OneBlockData? curItem = GetBlock(x, y);
                if (curItem == null
                    || field.ContainsKey(curItem.position))
                {
                    return null;
                }
                if (curItem.Statu == OneBlockData.Status.Ground)
                {
                    return false;
                }

                field.Add(curItem.position, curItem);
                return true;
            }
            #endregion

            #region clone data
            List<Point> GetUnknowBlockKeyList(Dictionary<Point, OneBlockData> field)
            {
                List<Point> result = new List<Point>();
                foreach (OneBlockData ob in field.Values)
                {
                    if (ob.Statu == BlockMatrix.OneBlockData.Status.Deteced_Unknow)
                    {
                        result.Add(ob.position);
                    }
                }
                return result;
            }
            Dictionary<Point, OneBlockData> CloneField(Dictionary<Point, OneBlockData> sourceField)
            {
                Dictionary<Point, OneBlockData> clone = new Dictionary<Point, OneBlockData>();
                foreach (Point p in sourceField.Keys)
                {
                    clone.Add(p, sourceField[p].Clone(false));
                }
                return clone;
            }
            #endregion

            #region test field

            void TestField(Dictionary<Point, OneBlockData> testField)
            {
                List<Point> unknowPosiList = GetUnknowBlockKeyList(testField);
                if (unknowPosiList.Count == 0)
                {
                    return;
                }
                Point startPt = unknowPosiList[0];
                Dictionary<Point, OneBlockData> cloneField = CloneField(testField);
                OneBlockData? startBlock = _GetBlock(cloneField, startPt);
                if (startBlock != null)
                {
                    startBlock.SetStatus(OneBlockData.Status.Deteced_Mine, false);
                    List<Point> cloneUPList = unknowPosiList.ToList();
                    if (TestField_aroundloop(ref cloneField, startPt.X, startPt.Y, ref cloneUPList) == true)
                    {
                        testResults.Add(cloneField);
                    }
                }

                cloneField = CloneField(testField);
                startBlock = _GetBlock(cloneField, startPt);
                if (startBlock != null)
                {
                    startBlock.SetStatus(OneBlockData.Status.Deteced_Save, false);
                    List<Point> cloneUPList = unknowPosiList.ToList();
                    if (TestField_aroundloop(ref cloneField, startPt.X, startPt.Y, ref cloneUPList) == true)
                    {
                        testResults.Add(cloneField);
                    }
                }
            }
            bool? TestField_aroundloop(
                ref Dictionary<Point, OneBlockData> testField,
                int x, int y,
                ref List<Point> unknowPosiList)
            {
                Point curPosi = new Point(x, y);
                if (!unknowPosiList.Contains(curPosi))
                {
                    return null;
                }
                unknowPosiList.Remove(curPosi);

                foreach (Point p in GetPointsAround(x, y))
                {
                    OneBlockData? testBlock = _GetBlock(testField, p.X, p.Y);
                    if (testBlock == null)
                    {
                        continue;
                    }

                    if (testBlock.IsSignal)
                    {

                        int signal = testBlock.StatuInt;
                        // check signal;
                        _CountAround(testField, p.X, p.Y, out CounterAround counter);
                        if (signal < counter.mines
                            || counter.allBlocks - counter.saves < signal)
                        {
                            return false;
                        }
                        // is signal ok, try set block around;
                        if (signal <= counter.mines)
                        {
                            _AutoSetAround_toSaves(ref testField, p.X, p.Y, ref counter);
                            if (unknowPosiList.Count == 0)
                            {
                                return true;
                            }
                        }
                        else if (signal >= counter.allBlocks - counter.saves)
                        {
                            _AutoSetAround_toMines(ref testField, p.X, p.Y, ref counter);
                            if (unknowPosiList.Count == 0)
                            {
                                return true;
                            }
                        }
                        else if (counter.unknows > 0)
                        {
                            // clone and try set/test mines

                            // get unknow-block posi around
                            List<Point> curUPList = GetPosiAround(
                                testField,
                                p.X, p.Y,
                                OneBlockData.Status.Deteced_Unknow);


                            Point startPt = curUPList[0];
                            OneBlockData? startBlock = _GetBlock(testField, startPt);
                            if (startBlock != null)
                            {
                                startBlock.SetStatus(OneBlockData.Status.Deteced_Mine, false);
                                List<Point> cloneUPList = unknowPosiList.ToList();
                                if (TestField_aroundloop(ref testField, startPt.X, startPt.Y, ref cloneUPList) == true)
                                {
                                    testResults.Add(testField);
                                }
                            }

                            startPt = curUPList[0];
                            Dictionary<Point, OneBlockData> cloneField = CloneField(testField);
                            startBlock = _GetBlock(cloneField, startPt);
                            if (startBlock != null)
                            {
                                startBlock.SetStatus(OneBlockData.Status.Deteced_Save, false);
                                List<Point> cloneUPList = unknowPosiList.ToList();
                                if (TestField_aroundloop(ref cloneField, startPt.X, startPt.Y, ref cloneUPList) == true)
                                {
                                    testResults.Add(cloneField);
                                }
                            }

                        }

                    }
                }
                if (unknowPosiList.Count > 0)
                {
                    Point startPt = unknowPosiList[0];
                    OneBlockData? startBlock = _GetBlock(testField, startPt);
                    if (startBlock != null)
                    {
                        startBlock.SetStatus(OneBlockData.Status.Deteced_Mine, false);
                        List<Point> cloneUPList = unknowPosiList.ToList();
                        if (TestField_aroundloop(ref testField, startPt.X, startPt.Y, ref cloneUPList) == true)
                        {
                            testResults.Add(testField);
                        }
                    }

                    startPt = unknowPosiList[0];
                    Dictionary<Point, OneBlockData> cloneField = CloneField(testField);
                    startBlock = _GetBlock(cloneField, startPt);
                    if (startBlock != null)
                    {
                        startBlock.SetStatus(OneBlockData.Status.Deteced_Save, false);
                        List<Point> cloneUPList = unknowPosiList.ToList();
                        if (TestField_aroundloop(ref cloneField, startPt.X, startPt.Y, ref cloneUPList) == true)
                        {
                            testResults.Add(cloneField);
                        }
                    }
                }
                return null;
            }

            #endregion

            #region compair results
            bool CheckResults(ref List<Dictionary<Point, OneBlockData>> testResults, ref List<Point> posis)
            {
                if (testResults.Count == 0)
                {
                    return false;
                }
                Dictionary<Point, OneBlockData>? firstTR = null;
                foreach (Dictionary<Point, OneBlockData> r in testResults)
                {
                    if (firstTR == null)
                    {
                        firstTR = r;
                        continue;
                    }

                    foreach (Point p in posis)
                    {
                        if (firstTR[p].Statu != r[p].Statu)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            #endregion
        }

        public List<Point> GetPointsAround(int x, int y)
        {
            List<Point> result = new List<Point>();
            result.Add(new Point(x - 1, y - 1));
            result.Add(new Point(x, y - 1));
            result.Add(new Point(x + 1, y - 1));
            result.Add(new Point(x - 1, y));
            result.Add(new Point(x + 1, y));
            result.Add(new Point(x - 1, y + 1));
            result.Add(new Point(x, y + 1));
            result.Add(new Point(x + 1, y + 1));
            return result;
        }
        public List<Point> GetPosiAround(Dictionary<Point, OneBlockData> field, int x, int y, OneBlockData.Status status)
        {
            List<Point> result = new List<Point>();
            OneBlockData? testBlock = _GetBlock(field, x, y);
            if (testBlock == null)
            {
                return result;
            }
            foreach (Point p in GetPointsAround(x, y))
            {
                testBlock = _GetBlock(field, p);
                if (testBlock == null)
                {
                    continue;
                }
                if (testBlock.Statu == status)
                {
                    result.Add(p);
                }
            }
            return result;
        }

        public OneBlockData? GetBlock(int listIdx)
        {
            if (listIdx >= allBlocksList.Count)
            {
                return null;
            }
            else
            {
                return allBlocksList[listIdx];
            }
        }
        public OneBlockData? GetBlock(OneBlock ui)
        {
            OneBlockData? result = (OneBlockData?)allBlocksHashTable_keyUI[ui];
            return result;
        }
        public OneBlockData? GetBlock(int x, int y)
        {
            Point pt = new Point(x, y);
            OneBlockData? result = (OneBlockData?)allBlocksHashTable_keyPosies[pt];
            return result;
        }
        private OneBlockData? _GetBlock(Dictionary<Point, OneBlockData> field, Point posi)
        {
            if (field.ContainsKey(posi))
            {
                return field[posi];
            }
            return null;
        }
        private OneBlockData? _GetBlock(Dictionary<Point, OneBlockData> field, int x, int y)
        {
            Point pt = new Point(x, y);
            return _GetBlock(field, pt);
        }
        public OneBlockData? GetBlock(Point position)
        {
            OneBlockData? result = (OneBlockData?)allBlocksHashTable_keyPosies[position];
            return result;
        }
    }
}
