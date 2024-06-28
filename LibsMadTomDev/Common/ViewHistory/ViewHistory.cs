using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace MadTomDev.Common
{
    public class ViewHistory
    {
        public struct Item
        {
            public ViewHistory parent;
            public BitmapSource image;
            public string label;
            public string fullName;
            public string Name
            {
                get
                {
                    if (fullName.Contains(parent.PathSeparator))
                        return fullName.Substring(fullName.LastIndexOf(parent.PathSeparator) + 1);
                    else
                        return fullName;
                }
            }
            public object tag;
        }

        public string PathSeparator { set; get; }
        public bool IgnoreDuplicatedReport { set; get; }


        private Item[] _viewList;
        private int indCur, indStr, indEnd;
        public ViewHistory(int length, string PathSeparator, bool ignoreDuplicatedReport = true)
        {
            this.PathSeparator = PathSeparator;
            IgnoreDuplicatedReport = ignoreDuplicatedReport;
            _viewList = new Item[length];
            indStr = indCur = -1;
            indEnd = 0;
        }
        public int MaxLength
        { get => _viewList.Length; }
        public int Count
        {
            get
            {
                if (indStr < 0)
                {
                    return 0;
                }
                else if (indEnd < indStr)
                {
                    return _viewList.Length - indEnd + indStr + 1;
                }
                else
                {
                    return indEnd - indStr + 1;
                }
            }
        }

        public bool CanGetCurt
        {
            get
            {
                return (indCur >= 0);
            }
        }
        public bool CanGetNext
        {
            get
            {
                return (indCur >= 0 && indCur != indEnd);
            }
        }
        public bool CanGetPrev
        {
            get
            {
                return (indCur >= 0 && indCur != indStr);
            }
        }

        public delegate void HistoryWritenDelegate(ViewHistory sender);
        public event HistoryWritenDelegate HistoryWriten;
        public delegate void HistoryModifiedDelegate(ViewHistory sender, List<Item> modifiedEntrys);
        public event HistoryModifiedDelegate HistoryModified;
        public void Write_History(string fullName, BitmapSource image = null, object tag = null, string label = null)
        {
            if (IgnoreDuplicatedReport)
            {
                if (indCur >= 0 && _viewList[indCur].fullName == fullName)
                    return;
            }
            indCur = indEnd = _indRollForward(indCur);
            Item rec = _viewList[indCur];
            rec.parent = this;
            rec.fullName = fullName;
            if (label != null)
                rec.label = label;
            else
                rec.label = rec.Name;
            rec.image = image;
            rec.tag = tag;

            _viewList[indCur] = rec;
            if (indEnd == indStr)
            {
                indStr = _indRollForward(indStr);
            }
            if (indStr == -1) indStr = 0;
            HistoryWriten?.Invoke(this);
        }
        public int Modify_History(string fullName, bool newImg, BitmapSource image, bool newTag, object tag, bool newLabel, string label)
        {
            if (!newImg && !newTag && !newLabel)
                return 0;

            int result = 0;
            List<Item> modifiedEntries = new List<Item>();
            Item rec;
            for (int i = 0, iv = _viewList.Length; i < iv; i++)
            {
                rec = _viewList[i];
                if (rec.fullName == fullName)
                {
                    if (newImg) rec.image = image;
                    if (newTag) rec.tag = tag;
                    if (newLabel) rec.label = label;
                    _viewList[i] = rec;
                    modifiedEntries.Add(rec);
                    result++;
                }
            }
            HistoryModified?.Invoke(this, modifiedEntries);
            return result;
        }
        private int _indRollForward(int point)
        {
            point++;
            if (point >= _viewList.Length) return 0;
            else return point;
        }
        private int _indRollBackward(int point)
        {
            point--;
            if (point < 0) return (_viewList.Length - 1);
            else return (point);
        }

        /// <summary>
        /// 清空所有记录
        /// </summary>
        public void Clear()
        {
            indStr = indCur = -1;
            indEnd = 0;
        }

        /// <summary>
        /// 读取所有记录，时间从老到新；
        /// </summary>
        /// <returns></returns>
        public List<Item> Read_All()
        {
            int itemsCount = this.Count;
            List<Item> result = new List<Item>();
            if (indStr < 0)
                return result;
            int pointer = indStr;
            while (itemsCount > 0)
            {
                result.Add(_viewList[pointer]);
                pointer = _indRollForward(pointer);
                itemsCount--;
            }
            return result;
        }
        /// <summary>
        /// 读取过去内容，第一个离当前最近
        /// </summary>
        /// <returns></returns>
        public List<Item> Read_History()
        {
            List<Item> result = new List<Item>();
            int startPoint = indCur;
            int endPoint = indStr;
            while (startPoint != endPoint)
            {
                startPoint = _indRollBackward(startPoint);
                result.Add(_viewList[startPoint]);
            }
            return result;
        }

        /// <summary>
        /// 读取当前条目
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Item Read()
        {
            if (!CanGetCurt)
            {
                throw new Exception("Can not read current record!");
            }
            return _viewList[indCur];
        }

        /// <summary>
        /// 读取未来内容，第一个离当前最近
        /// </summary>
        /// <returns></returns>
        public List<Item> Read_Future()
        {
            List<Item> result = new List<Item>();
            int pointer = indCur;
            while (pointer != indEnd)
            {
                pointer = _indRollForward(pointer);
                result.Add(_viewList[pointer]);
            }
            return result;
        }

        public delegate void EntryGottenDelegate(ViewHistory sender, Item entry);
        public event EntryGottenDelegate EntryGotten;
        /// <summary>
        /// 获取条目，按偏移量，获取后，当前指针会定位到获取的位置；
        /// 当偏移量超过允许最大值时，抛出异常；
        /// </summary>
        /// <param name="offset">当大于0时，获取未来条目，小于0时获取历史</param>
        /// <returns></returns>
        public Item Get_Entry(int offset)
        {
            int pointer = indCur;
            bool isOverFlow = false;
            while (offset != 0)
            {
                if (offset > 0)
                {
                    if (pointer == indEnd)
                    {
                        isOverFlow = true;
                        break;
                    }
                    pointer = _indRollForward(pointer);
                    offset--;
                }
                else
                {
                    if (pointer == indStr)
                    {
                        isOverFlow = true;
                        break;
                    }
                    pointer = _indRollBackward(pointer);
                    offset++;
                }
            }
            if (isOverFlow)
            {
                throw new Exception("Offset overflow for [" + offset + "] steps!");
            }
            indCur = pointer;
            Item entry = _viewList[pointer];
            EntryGotten?.Invoke(this, entry);
            return entry;
        }



        /// <summary>
        /// 获取条目，按完整名称(完整路径)，获取后，当前指针会定位到获取的位置；
        /// 当找不到条目时，返回null；
        /// </summary>
        /// <param name="fullName">要搜索的条目的完整名称</param>
        /// <returns></returns>
        public Item Get_Entry(string fullName)
        {
            Item entry = _viewList[indCur];
            if (entry.fullName != fullName)
            {
                entry = new Item() { fullName = null, };
                int ptBack, ptFore;
                ptBack = ptFore = indCur;
                bool notEndBack = true, notEndFore = true;
                while (notEndBack || notEndFore)
                {
                    if (ptBack == indStr)
                        notEndBack = false;
                    if (ptFore == indEnd)
                        notEndFore = false;

                    if (notEndBack)
                    {
                        ptBack = _indRollBackward(ptBack);
                        entry = _viewList[ptBack];
                        if (entry.fullName == fullName)
                        {
                            indCur = ptBack;
                            break;
                        }
                    }
                    if (notEndBack)
                    {
                        ptFore = _indRollForward(ptFore);
                        entry = _viewList[ptFore];
                        if (entry.fullName == fullName)
                        {
                            indCur = ptFore;
                            break;
                        }
                    }
                }
            }

            // else current item is the result;

            if (entry.fullName != null)
            {
                EntryGotten?.Invoke(this, entry);
            }
            return entry;
        }

        public List<Item> Find_Historis(string fullName)
        {
            List<Item> result = new List<Item>();
            foreach (Item i in _viewList)
                if (i.fullName == fullName)
                    result.Add(i);
            return result;
        }

    }
}
