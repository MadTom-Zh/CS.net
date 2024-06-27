using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace MadTomDev.App
{
    class DataBase : IDisposable
    {
        private string fileFullName_BlackUser = "BiliDm2.BU.txt";
        private string fileFullName_BlackKeyWord = "BiliDm2.BW.txt";
        private string fileFullName_BlackPinYin = "BiliDm2.BP.txt";
        private string fileFullName_Replacement = "BiliDm2.RP.txt";
        public DataBase()
        {
            // 2024 0408 将过滤信息保存在网盘里；
            //string baseDir = Common.Variables.IOPath.SettingDir;
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FilterDB");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }
            fileFullName_BlackUser = Path.Combine(baseDir, fileFullName_BlackUser);
            fileFullName_BlackKeyWord = Path.Combine(baseDir, fileFullName_BlackKeyWord);
            fileFullName_BlackPinYin = Path.Combine(baseDir, fileFullName_BlackPinYin);
            fileFullName_Replacement = Path.Combine(baseDir, fileFullName_Replacement);

            ReloadBlackPins();
            ReloadBlackWords();
            ReloadBlackUsers();
            ReloadReplacements();
        }

        public class DataGridItemBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected internal virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class ValueRemarkItem : DataGridItemBase
        {
            private string _value;
            public string value
            {
                get => _value;
                set
                {
                    if (_value == value)
                        return;
                    _value = value;
                    OnPropertyChanged("value");
                }
            }

            private bool _showLog;
            public bool showLog
            {
                get => _showLog;
                set
                {
                    if (_showLog == value)
                        return;
                    _showLog = value;
                    OnPropertyChanged("showLog");
                }
            }

            private string _remark;
            public string remark
            {
                get => _remark;
                set
                {
                    if (_remark == value)
                        return;
                    _remark = value;
                    OnPropertyChanged("remark");
                }
            }
        }
        public ObservableCollection<ValueRemarkItem> blackUsers = new ObservableCollection<ValueRemarkItem>();
        public void ReloadBlackUsers()
        {
            blackUsers.Clear();
            if (File.Exists(fileFullName_BlackUser))
            {
                string[] lines = File.ReadAllLines(fileFullName_BlackUser);
                string[] parts;
                foreach (string l in lines)
                {
                    parts = l.Split('\t');
                    if (parts.Length < 2)
                        continue;
                    if (parts[0].Length <= 0)
                        continue;
                    if (parts.Length == 2)
                    {
                        blackUsers.Add(new ValueRemarkItem()
                        { value = parts[0], showLog = true, remark = parts[1], });
                    }
                    else // if (parts.Length > 2)
                    {
                        if (bool.TryParse(parts[1], out bool v))
                            blackUsers.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = v, remark = parts[2], });
                        else
                            blackUsers.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = true, remark = parts[2], });
                    }
                }
            }
        }
        public void SaveBlackUsers()
        {
            using (Stream fs = File.Create(fileFullName_BlackUser))
            using (StreamWriter sr = new StreamWriter(fs))
            {
                ValueRemarkItem bui;
                for (int i = 0, iv = blackUsers.Count; i < iv; i++)
                {
                    bui = blackUsers[i];
                    sr.WriteLine(bui.value + "\t" + bui.showLog.ToString() + "\t" + bui.remark);
                }
                sr.Flush();
            }
        }
        public bool HaveBlackUser(string id, out ValueRemarkItem foundUserInfo)
        {
            ValueRemarkItem found = blackUsers.FindFirst(i => i.value == id);
            foundUserInfo = found;
            return found != null;
        }
        //public bool AddBlackUser(string id, string remark)
        //{
        //    ValueRemarkItem found = blackUsers.FindFirst(i => i.value == id);
        //    if (found != null)
        //        return false;

        //    blackUsers.Add(new ValueRemarkItem()
        //    { value = id, remark = remark, });
        //    return true;
        //}
        public bool UpdateBlackUser(string oriId, string newId, bool showLog, string newRemark)
        {
            ValueRemarkItem found = blackUsers.FindFirst(i => i.value == oriId);
            if (found == null)
                return false;

            found.value = newId;
            found.showLog = showLog;
            found.remark = newRemark;
            return true;
        }
        public bool DeleteBlackUser(string id)
        {
            ValueRemarkItem found = blackUsers.FindFirst(i => i.value == id);
            if (found == null)
                return false;

            blackUsers.Remove(found);
            return true;
        }



        public ObservableCollection<ValueRemarkItem> blackWords = new ObservableCollection<ValueRemarkItem>();
        public ObservableCollection<ValueRemarkItem> blackPins = new ObservableCollection<ValueRemarkItem>();
        public void ReloadBlackWords()
        {
            blackWords.Clear();
            if (File.Exists(fileFullName_BlackKeyWord))
            {
                string[] lines = File.ReadAllLines(fileFullName_BlackKeyWord);
                string[] parts;
                foreach (string l in lines)
                {
                    parts = l.Split('\t');
                    if (parts.Length < 2)
                        continue;
                    if (parts[0].Length <= 0)
                        continue;
                    if (parts.Length == 2)
                    {
                        blackWords.Add(new ValueRemarkItem()
                        { value = parts[0], showLog = true, remark = parts[1], });
                    }
                    else // if (parts.Length > 2)
                    {
                        if (bool.TryParse(parts[1], out bool v))
                            blackWords.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = v, remark = parts[2], });
                        else
                            blackWords.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = true, remark = parts[2], });
                    }
                }
            }
        }
        public void ReloadBlackPins()
        {
            blackPins.Clear();
            if (File.Exists(fileFullName_BlackPinYin))
            {
                string[] lines = File.ReadAllLines(fileFullName_BlackPinYin);
                string[] parts;
                foreach (string l in lines)
                {
                    parts = l.Split('\t');
                    if (parts.Length < 2)
                        continue;
                    if (parts[0].Length <= 0)
                        continue;
                    if (parts.Length == 2)
                    {
                        blackPins.Add(new ValueRemarkItem()
                        { value = parts[0], showLog = true, remark = parts[1], });
                    }
                    else // if (parts.Length > 2)
                    {
                        if (bool.TryParse(parts[1], out bool v))
                            blackPins.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = v, remark = parts[2], });
                        else
                            blackPins.Add(new ValueRemarkItem()
                            { value = parts[0], showLog = true, remark = parts[2], });
                    }
                }
            }
        }
        public void SaveBlackWords()
        {
            using (Stream fs = File.Create(fileFullName_BlackKeyWord))
            using (StreamWriter sr = new StreamWriter(fs))
            {
                ValueRemarkItem bui;
                for (int i = 0, iv = blackWords.Count; i < iv; i++)
                {
                    bui = blackWords[i];
                    sr.WriteLine(bui.value + "\t" + bui.showLog.ToString() + "\t" + bui.remark);
                }
                sr.Flush();
            }
        }
        public void SaveBlackPins()
        {
            using (Stream fs = File.Create(fileFullName_BlackPinYin))
            using (StreamWriter sr = new StreamWriter(fs))
            {
                ValueRemarkItem bui;
                for (int i = 0, iv = blackPins.Count; i < iv; i++)
                {
                    bui = blackPins[i];
                    sr.WriteLine(bui.value + "\t" + bui.showLog.ToString() + "\t" + bui.remark);
                }
                sr.Flush();
            }
        }


        //public bool HaveBlackWord(string v)
        //{
        //    ValueRemarkItem found = blackWords.FindFirst(i => i.value == v);
        //    return found != null;
        //}
        //public bool HaveBlackPin(string v)
        //{
        //    ValueRemarkItem found = blackPins.FindFirst(i => i.value == v);
        //    return found != null;
        //}

        //public bool AddBlackWord(string v, string remark)
        //{
        //    ValueRemarkItem found = blackWords.FindFirst(i => i.value == v);
        //    if (found != null)
        //        return false;

        //    blackWords.Add(new ValueRemarkItem()
        //    { value = v, remark = remark, });
        //    return true;
        //}
        //public bool AddBlackPin(string v, string remark)
        //{
        //    ValueRemarkItem found = blackPins.FindFirst(i => i.value == v);
        //    if (found != null)
        //        return false;

        //    blackPins.Add(new ValueRemarkItem()
        //    { value = v, remark = remark, });
        //    return true;
        //}

        //public bool UpdateBlackWord(string oriV, string newV, string newRemark)
        //{
        //    ValueRemarkItem found = blackWords.FindFirst(i => i.value == oriV);
        //    if (found == null)
        //        return false;

        //    found.value = newV;
        //    found.remark = newRemark;
        //    return true;
        //}
        //public bool UpdateBlackPin(string oriV, string newV, string newRemark)
        //{
        //    ValueRemarkItem found = blackPins.FindFirst(i => i.value == oriV);
        //    if (found == null)
        //        return false;

        //    found.value = newV;
        //    found.remark = newRemark;
        //    return true;
        //}

        //public bool DeleteBlackWord(string v)
        //{
        //    ValueRemarkItem found = blackWords.FindFirst(i => i.value == v);
        //    if (found == null)
        //        return false;

        //    blackWords.Remove(found);
        //    return true;
        //}
        //public bool DeleteBlackPin(string v)
        //{
        //    ValueRemarkItem found = blackPins.FindFirst(i => i.value == v);
        //    if (found == null)
        //        return false;

        //    blackPins.Remove(found);
        //    return true;
        //}


        public bool MoveUpBlackPin(ValueRemarkItem source)
        {
            if (blackPins.Contains(source))
            {
                int surIdx = blackPins.IndexOf(source);
                if (surIdx > 0)
                {
                    blackPins.Remove(source);
                    blackPins.Insert(surIdx - 1, source);
                    return true;
                }
            }
            return false;
        }
        public bool MoveDownBlackPin(ValueRemarkItem source)
        {
            if (blackPins.Contains(source))
            {
                int surIdx = blackPins.IndexOf(source);
                if (surIdx < blackPins.Count - 1)
                {
                    blackPins.Remove(source);
                    blackPins.Insert(surIdx + 1, source);
                    return true;
                }
            }
            return false;
        }


        public bool MoveUpBlackWord(ValueRemarkItem source)
        {
            if (blackWords.Contains(source))
            {
                int surIdx = blackWords.IndexOf(source);
                if (surIdx > 0)
                {
                    blackWords.Remove(source);
                    blackWords.Insert(surIdx - 1, source);
                    return true;
                }
            }
            return false;
        }
        public bool MoveDownBlackWord(ValueRemarkItem source)
        {
            if (blackWords.Contains(source))
            {
                int surIdx = blackWords.IndexOf(source);
                if (surIdx < blackWords.Count - 1)
                {
                    blackWords.Remove(source);
                    blackWords.Insert(surIdx + 1, source);
                    return true;
                }
            }
            return false;
        }





        public class ReplacementItem : DataGridItemBase
        {
            private string _find;
            public string find
            {
                get => _find;
                set
                {
                    if (_find == value)
                        return;
                    _find = value;
                    OnPropertyChanged("find");
                }
            }
            private string _replace;
            public string replace
            {
                get => _replace;
                set
                {
                    if (_replace == value)
                        return;
                    _replace = value;
                    OnPropertyChanged("replace");
                }
            }
            private string _remark;
            public string remark
            {
                get => _remark;
                set
                {
                    if (_remark == value)
                        return;
                    _remark = value;
                    OnPropertyChanged("remark");
                }
            }
        }
        public ObservableCollection<ReplacementItem> replacements = new ObservableCollection<ReplacementItem>();
        public void ReloadReplacements()
        {
            replacements.Clear();
            if (File.Exists(fileFullName_Replacement))
            {
                string[] lines = File.ReadAllLines(fileFullName_Replacement);
                string[] parts;
                foreach (string l in lines)
                {
                    parts = l.Split('\t');
                    if (parts.Length < 3)
                        continue;
                    if (parts[0].Length <= 0)
                        continue;

                    replacements.Add(new ReplacementItem()
                    { find = parts[0], replace = parts[1], remark = parts[2], });
                }
            }
        }
        public void SaveReplacements()
        {
            using (Stream fs = File.Create(fileFullName_Replacement))
            using (StreamWriter sr = new StreamWriter(fs))
            {
                ReplacementItem rep;
                for (int i = 0, iv = replacements.Count; i < iv; i++)
                {
                    rep = replacements[i];
                    sr.WriteLine(rep.find + "\t" + rep.replace + "\t" + rep.remark);
                }
                sr.Flush();
            }
        }
        public string BatchReplacement(string input)
        {
            ReplacementItem rpi;
            for (int i = 0, iv = replacements.Count; i < iv; i++)
            {
                rpi = replacements[i];
                input = input.Replace(rpi.find, rpi.replace);
            }
            return input;
        }
        public bool CheckReplacements(out string msg)
        {
            msg = null;
            if (replacements.Count <= 1)
                return true;
            ReplacementItem first, cur;
            for (int i = 0, j, iv = replacements.Count - 1, jv = replacements.Count; i < iv; i++)
            {
                first = replacements[i];
                for (j = i + 1; j < jv; j++)
                {
                    cur = replacements[j];
                    if (cur.find.Contains(first.find))
                    {
                        msg = $"关键字\"{cur.find}\"之前已存在\"{first.find}\"";
                        return false;
                    }
                    if (cur.replace.Contains(first.find))
                    {
                        msg = $"替换字\"{cur.replace}\"之前存在关键字\"{first.find}\"";
                        return false;
                    }
                    if (first.replace.Contains(cur.find))
                    {
                        msg = $"替换字\"{first.replace}\"之后存在关键字\"{cur.find}\"";
                        return false;
                    }
                }
            }
            return true;
        }
        public void AddReplacement(string find, string replace, string remark)
        {
            replacements.Add(new ReplacementItem()
            { find = find, replace = replace, remark = remark, });
        }

        public bool UpdateReplacement(ReplacementItem source, string newFind, string newReplace, string newRemark)
        {
            if (replacements.Contains(source))
            {
                source.find = newFind;
                source.replace = newReplace;
                source.remark = newRemark;
                return true;
            }
            return false;
        }
        public bool DeleteReplacement(ReplacementItem source)
        {
            if (replacements.Contains(source))
            {
                replacements.Remove(source);
                return true;
            }
            return false;
        }
        public bool MoveUpReplacement(ReplacementItem source)
        {
            if (replacements.Contains(source))
            {
                int surIdx = replacements.IndexOf(source);
                if (surIdx > 0)
                {
                    replacements.Remove(source);
                    replacements.Insert(surIdx - 1, source);
                    return true;
                }
            }
            return false;
        }
        public bool MoveDownReplacement(ReplacementItem source)
        {
            if (replacements.Contains(source))
            {
                int surIdx = replacements.IndexOf(source);
                if (surIdx < replacements.Count - 1)
                {
                    replacements.Remove(source);
                    replacements.Insert(surIdx + 1, source);
                    return true;
                }
            }
            return false;
        }




        public void Dispose()
        {
            Dispose(true);
        }
        public void Dispose(bool discardChanges)
        {
            if (!discardChanges)
            {
                // save
                SaveBlackUsers();
                SaveBlackWords();
                SaveBlackPins();
                SaveReplacements();
            }
        }

        internal bool BatchCheckBlackWords(string tx, out ValueRemarkItem foundBW)
        {
            foundBW = null;
            if (string.IsNullOrEmpty(tx))
                return false;

            string low1 = tx.ToLower(), low2;
            foreach (ValueRemarkItem vri in blackWords)
            {
                if (vri.value == null)
                    continue;
                low2 = vri.value.ToLower();
                if (low1 == low2 || low1.Contains(low2))
                {
                    foundBW = vri;
                    return false;
                }
            }
            return true;
        }

        internal bool BatchCheckBlackPins(string inText, out ValueRemarkItem foundBP, out int txStart, out int txLength)
        {
            foundBP = null;
            txStart = -1;
            txLength = -1;
            string pin = Common.SimpleStringHelper.Chinese2PINYIN.GetFullPINYIN(inText);
            foreach (ValueRemarkItem vri in blackPins)
            {
                if (pin.Contains(vri.value))
                {
                    int endIdx = pin.IndexOf(vri.value) + vri.value.Length;
                    if (endIdx < pin.Length)
                    {
                        string test = pin.Substring(endIdx, 1);
                        if (test != test.ToUpper())
                            continue;
                    }
                    foundBP = vri;
                    string sBefore = Common.SimpleStringHelper.Chinese2PINYIN.RemoveLowerPINYIN(pin.Substring(0, pin.IndexOf(vri.value)));
                    txStart = sBefore.Length;
                    string sVri = Common.SimpleStringHelper.Chinese2PINYIN.RemoveLowerPINYIN(vri.value);
                    txLength = sVri.Length;
                    return false;
                }
            }
            return true;
        }
    }
    public static class ObservableCollectionHelper
    {
        public static T FindFirst<T>(this ObservableCollection<T> source, Predicate<T> match)
        {
            foreach (T t in source)
            {
                if (match(t))
                    return t;
            }
            return default(T);
        }
    }
}
