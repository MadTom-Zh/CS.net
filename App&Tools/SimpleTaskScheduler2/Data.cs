using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using MadTomDev.UI.Class;

namespace MadTomDev.App
{
    public class Data
    {
        #region view module
        public class VMDataSchedule : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            public void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            // 序号  名称  最近时间  动作


            private string _NoTx;
            public string NoTx
            {
                get => _NoTx;
                set
                {
                    _NoTx = value;
                    RaisePropertyChangedEvent("NoTx");
                }
            }
            public string Title
            {
                get
                {
                    if (scheduleData == null)
                        scheduleData = new ScheduleData();
                    return scheduleData.title;
                }
                set
                {
                    if (scheduleData == null)
                        scheduleData = new ScheduleData();
                    scheduleData.title = value;
                    RaisePropertyChangedEvent("Title");
                }
            }


            private string _NextTimeTx;
            public string NextTimeTx
            {
                get => _NextTimeTx;
            }
            private DateTime _NextTime;
            public DateTime NextTime
            {
                get => _NextTime;
                set
                {
                    _NextTime = value;
                    RaisePropertyChangedEvent("NextTime");


                    _NextTimeTx = "";
                    RaisePropertyChangedEvent("NextTimeTx");
                }
            }
            public DateTime UpdateNextTime()
            {
                List<DateTime> tList = scheduleData.GetTriggerPoints(DateTime.Now, 1);
                if (tList.Count > 0)
                    NextTime = tList[0];
                else
                    NextTime = scheduleData.startTime;
                return _NextTime;
            }


            private string _CmdDescription;
            public string CmdDescription
            {
                get => _CmdDescription;
            }
            private string _Cmd;
            public string Cmd
            {
                get => _Cmd;
                set
                {
                    _Cmd = value;
                    RaisePropertyChangedEvent("Cmd");

                    if (_Cmd == null)
                        return;

                    string[] lines = _Cmd.Replace("\r\n", "\r").Replace("\n", "\r").Split('\r');

                    if (lines.Length > 1)
                    {
                        StringBuilder strBdr = new StringBuilder();
                        strBdr.Append($"({lines.Length} lines) ");
                        string l, testCmd;
                        int spaceIdx;
                        for (int i = 0, iv = Math.Min(3, lines.Length); i < iv; ++i)
                        {
                            l = lines[i];
                            spaceIdx = l.IndexOf(" ");
                            if (spaceIdx > 0)
                                testCmd = l.Substring(0, spaceIdx);
                            else
                                testCmd = l;
                            if (testCmd.Length > 8)
                                strBdr.Append(testCmd.Substring(0, 8) + ".. ");
                            else
                                strBdr.Append(testCmd + ", ");
                        }
                        if (strBdr.Length > 1)
                            strBdr.Remove(strBdr.Length - 2, 2);
                        _CmdDescription = strBdr.ToString();
                    }
                    else
                        _CmdDescription = _Cmd;
                    RaisePropertyChangedEvent("CmdDescription");
                }
            }

            public ScheduleData scheduleData;
            public void SyncVM_toData()
            {
                if (scheduleData == null)
                    scheduleData = new ScheduleData();
            }
            public void SyncVM_fromData()
            {
                RaisePropertyChangedEvent("Title");
                List<DateTime> tList = scheduleData.GetTriggerPoints(DateTime.Now, 1);
                if (tList.Count > 0)
                    NextTime = tList[0];
                else
                    NextTime = scheduleData.startTime;
            }


            public XmlNode ToXmlNode(XmlDocument xmlDoc)
            {
                // cd title
                // schedule
                // cds cmd

                XmlNode result = xmlDoc.CreateElement("TaskSchel");
                result.AppendChild(xmlDoc.CreateCDataSection(Title));
                SyncVM_toData();
                result.AppendChild(scheduleData.ToXmlNode(xmlDoc));
                string[] cmdLines = _Cmd.Replace("\r\n", "\r").Replace("\n", "\r").Split("\r");
                foreach (string l in cmdLines)
                    result.AppendChild(xmlDoc.CreateCDataSection(l));
                return result;
            }
            public void FromXmlNode(XmlNode xmlNode)
            {
                Title = xmlNode.ChildNodes[0].Value;
                scheduleData = ScheduleData.CreateFromXmlNode(xmlNode.ChildNodes[1], out int rc);
                SyncVM_fromData();
                StringBuilder cmdBdr = new StringBuilder();
                for (int i = 2, iv = xmlNode.ChildNodes.Count; i < iv; ++i)
                {
                    cmdBdr.AppendLine(xmlNode.ChildNodes[i].Value);
                }
                if (cmdBdr.Length > 0)
                {
                    int nlLen = Environment.NewLine.Length;
                    cmdBdr.Remove(cmdBdr.Length - nlLen, nlLen);
                }
                Cmd = cmdBdr.ToString();
            }
            public static VMDataSchedule CreateFromXmlNode(XmlNode xmlNode)
            {
                VMDataSchedule result = new VMDataSchedule();
                result.FromXmlNode(xmlNode);
                return result;
            }

            internal VMDataSchedule Clone()
            {
                VMDataSchedule result = new VMDataSchedule()
                {
                    NoTx = NoTx,
                    Title = Title,
                    NextTime = NextTime,
                    Cmd = Cmd,
                    scheduleData = scheduleData.Clone(),
                };
                return result;
            }

        }

        public class VMDataTask : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            public void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            internal void UpdateCountDown()
            {
                TimeSpan ts = _StartTime - DateTime.Now;
                if (ts.TotalMilliseconds > 0)
                {
                    CountDownTx = ts.ToString(@"d\.\ hh\:mm\:ss");
                }
                else
                {
                    CountDownTx = "---";
                }
            }

            // 倒计时  启动时间  所属计划  周期  动作

            private string _CountDownTx;
            public string CountDownTx
            {
                get => _CountDownTx;
                set
                {
                    _CountDownTx = value;
                    RaisePropertyChangedEvent("CountDownTx");
                }
            }


            private DateTime _StartTime;
            public DateTime StartTime
            {
                get => _StartTime;
                set
                {
                    _StartTime = value;
                    RaisePropertyChangedEvent("StartTime");
                    RaisePropertyChangedEvent("StartTimeTx");
                }
            }
            public string StartTimeTx
            {
                get => _StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            }



            private VMDataSchedule _VMSchelduleData;
            public VMDataSchedule VMSchelduleData
            {
                get => _VMSchelduleData;
                set
                {
                    _VMSchelduleData = value;
                    RaisePropertyChangedEvent("Title");
                    RaisePropertyChangedEvent("CmdDescription");
                    RaisePropertyChangedEvent("CycleDescription");
                }
            }

            public string Title
            {
                get
                {
                    if (_VMSchelduleData != null)
                        return _VMSchelduleData.Title;
                    return null;
                }
            }
            public string CycleDescription
            {
                get
                {
                    if (_VMSchelduleData != null)
                        return _VMSchelduleData.scheduleData.GetCycleDescription();
                    return null;
                }
            }
            public string CmdDescription
            {
                get
                {
                    if (_VMSchelduleData != null)
                        return _VMSchelduleData.CmdDescription;
                    return null;
                }
            }
        }

        #endregion

        #region scheldule list
        private string _FileScheldule = null;
        private string FileScheldule
        {
            get
            {
                if (_FileScheldule == null)
                {
                    string settingDir = Common.Variables.IOPath.SettingDir;
                    if (!Directory.Exists(settingDir))
                        Directory.CreateDirectory(settingDir);
                    _FileScheldule = Path.Combine(settingDir, "STS2.Schel.xml");
                }
                return _FileScheldule;
            }
        }
        public List<VMDataSchedule> ReloadSchelduleList()
        {
            List<VMDataSchedule> result = new List<VMDataSchedule>();
            if (File.Exists(FileScheldule))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(FileScheldule);
                if (xmlDoc.ChildNodes.Count > 0)
                {
                    foreach (XmlNode xn in xmlDoc.ChildNodes[1].ChildNodes)
                        result.Add(VMDataSchedule.CreateFromXmlNode(xn));
                }
            }
            return result;
        }
        public void SaveSchelduleLists(ObservableCollection<VMDataSchedule>[] lists)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            XmlNode xmlRoot = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(xmlRoot);
            foreach (ObservableCollection<VMDataSchedule> list in lists)
            {
                foreach (VMDataSchedule vm in list)
                    xmlRoot.AppendChild(vm.ToXmlNode(xmlDoc));
            }
            xmlDoc.Save(FileScheldule);
        }

        #endregion

        #region settings
        private string _FileSetting = null;
        private string FileSetting
        {
            get
            {
                if (_FileSetting == null)
                {
                    string settingDir = Common.Variables.IOPath.SettingDir;
                    if (!Directory.Exists(settingDir))
                        Directory.CreateDirectory(settingDir);
                    _FileSetting = Path.Combine(settingDir, "STS2.Setting.txt");
                }
                return _FileSetting;
            }
        }


        public double settingTaskScoutDays = 8;
        public int settingTimerIntervalSecs = 1;
        public DateTime settingLastTaskStartTime;


        public void ReloadSettings()
        {
            if (File.Exists(FileSetting))
            {
                string[] lines = File.ReadAllLines(FileSetting);
                foreach (string l in lines)
                {
                    if (l.StartsWith("taskScoutDays\t"))
                    {
                        if (!double.TryParse(l.Substring(14), out settingTaskScoutDays))
                            settingTaskScoutDays = 8;
                    }
                    else if (l.StartsWith("timerIntervalsSecs\t"))
                    {
                        if (!int.TryParse(l.Substring(19), out settingTimerIntervalSecs))
                            settingTaskScoutDays = 1;
                    }
                    else if (l.StartsWith("lastTaskStartTime\t"))
                    {
                        if (!DateTime.TryParse(l.Substring(18), out settingLastTaskStartTime))
                            settingLastTaskStartTime = DateTime.Now;
                    }
                }
            }
        }
        public void SaveSettings()
        {
            using (FileStream fs = File.OpenWrite(FileSetting))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("taskScoutDays\t" + settingTaskScoutDays);
                sw.WriteLine("timerIntervalsSecs\t" + settingTimerIntervalSecs);
                sw.WriteLine("lastTaskStartTime\t" + settingLastTaskStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                sw.Flush();
            }
        }
        #endregion
    }
}
