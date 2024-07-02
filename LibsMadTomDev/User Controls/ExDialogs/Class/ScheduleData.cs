using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;

namespace MadTomDev.UI.Class
{
    public class ScheduleData
    {
        public string title;
        public enum ScheduleTypes
        {
            /// <summary>
            /// 一次性
            /// </summary>
            Once,
            /// <summary>
            /// 每天
            /// </summary>
            EveryDay,
            /// <summary>
            /// 每周
            /// </summary>
            EveryWeek,
            /// <summary>
            /// 每月
            /// *如设定启动天数为31，则大月均不会触发
            /// </summary>
            EveryMonth,
            /// <summary>
            /// 其他时间间隔，如设定3天为72小时等，可精确到秒
            /// </summary>
            OtherInterval,
        }
        /// <summary>
        /// 定时类型
        /// </summary>
        public ScheduleTypes scheduleType = ScheduleTypes.Once;

        /// <summary>
        /// 启动时间 (含启动日期)
        /// *当采用特殊间隔时间时，以此时间点为基准，计算其他激活时间点；
        /// </summary>
        public DateTime startTime = DateTime.Now;
        /// <summary>
        /// 每周启动的星期(号)数
        /// *当定时类型设定为EveryWeek时，此字段有效；
        /// </summary>
        public List<int> startDaysInWeek = new List<int>();
        /// <summary>
        /// 每月启动的日(号)数
        /// *当定时类型设定为EveryMonth时，此字段有效；
        /// </summary>
        public List<int> startDaysInMonth = new List<int>();
        /// <summary>
        /// 定时的特殊间隔时间
        /// *最短为1分钟！！！
        /// *当定时类型设定为OtherInterval时，此字段有效；
        /// </summary>
        public TimeSpan otherInterval;

        public List<DateTime> GetTriggerPoints(DateTime beforeTime)
        {
            return GetTriggerPoints(DateTime.Now, beforeTime);
        }
        public List<DateTime> GetTriggerPoints(DateTime fromTime, DateTime beforeTime)
        {
            return GetTriggerPoints(fromTime, beforeTime, int.MaxValue);
        }
        public List<DateTime> GetTriggerPoints(DateTime fromTime, DateTime beforeTime, int count)
        {
            List<DateTime> result = new List<DateTime>();
            if (count == 0)
                return result;
            DateTime testTime;
            switch (scheduleType)
            {
                case ScheduleTypes.Once:
                    if (fromTime <= startTime && startTime <= beforeTime)
                        result.Add(startTime);
                    break;
                case ScheduleTypes.EveryDay:
                    DateTime baseTime = new DateTime(
                        fromTime.Year, fromTime.Month, fromTime.Day,
                        startTime.Hour, startTime.Minute, startTime.Second);
                    int days = (int)(beforeTime - fromTime).TotalDays + 1;
                    for (int i = 0; i < days; ++i)
                    {
                        testTime = baseTime.AddDays(i);
                        if (fromTime <= testTime && testTime <= beforeTime)
                        {
                            result.Add(testTime);
                            if (--count <= 0)
                                break;
                        }
                    }
                    break;
                case ScheduleTypes.EveryWeek:
                    if (startDaysInWeek.Count <= 0)
                        break;
                    SortAndRemoveDuplis(ref startDaysInWeek);

                    // 找到fromTime之前的第一个时间
                    testTime = startTime;
                    testTime = testTime.AddDays(-(int)((startTime - fromTime).Days) - 1);

                    // 向后循环，直到找到全部时间点
                    while (true)
                    {
                        if (beforeTime < testTime)
                            break;

                        // 0-sunday... 6-saturday
                        if (fromTime <= testTime
                            && startDaysInWeek.Contains((int)testTime.DayOfWeek))
                        {
                            result.Add(testTime);
                            if (--count <= 0)
                                break;
                        }
                        testTime = testTime.AddDays(1);
                    }
                    break;
                case ScheduleTypes.EveryMonth:
                    if (startDaysInMonth.Count <= 0)
                        break;
                    SortAndRemoveDuplis(ref startDaysInMonth);

                    int year = fromTime.Year;
                    int month = fromTime.Month;
                    TimeSpan time = new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second);
                    bool exitWhile = false;
                    while (true)
                    {
                        try
                        {
                            foreach (int day in startDaysInMonth)
                            {
                                testTime = new DateTime(year, month, day) + time;
                                if (fromTime <= testTime && testTime <= beforeTime)
                                {
                                    result.Add(testTime);
                                    if (--count <= 0)
                                    {
                                        exitWhile = true;
                                        break;
                                    }
                                }
                                if (beforeTime < testTime)
                                {
                                    exitWhile = true;
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (exitWhile)
                            break;
                        if (++month > 12)
                        {
                            ++year;
                            month = 1;
                        }
                    }
                    break;
                case ScheduleTypes.OtherInterval:
                    if (otherInterval.TotalMinutes < 1)
                        throw new Exception("最短特殊间隔为1分钟。");
                    // add current
                    if (fromTime <= startTime && startTime <= beforeTime)
                    {
                        result.Add(startTime);

                        // find before
                        testTime = startTime - otherInterval;
                        while (fromTime <= testTime)
                        {
                            result.Insert(0, testTime);
                            testTime -= otherInterval;
                        }
                        int countOffset = result.Count - count;
                        if (countOffset == 0)
                        {
                            break;
                        }
                        else if (countOffset > 0)
                        {
                            while (countOffset-- > 0)
                            {
                                result.RemoveAt(result.Count - 1);
                            }
                        }
                        else
                        {
                            count += countOffset;
                        }

                        // find after
                        testTime = startTime + otherInterval;
                        while (testTime <= beforeTime)
                        {
                            result.Add(testTime);
                            if (--count <= 0)
                                break;
                            testTime += otherInterval;
                        }
                    }
                    break;
            }
            return result;
        }
        public List<DateTime> GetTriggerPoints(DateTime fromTime, int count)
        {
            return GetTriggerPoints(fromTime, DateTime.MaxValue, count);
        }

        public static void SortAndRemoveDuplis(ref List<int> list)
        {
            if (list.Count <= 1)
                return;
            list.Sort();
            int pre = list[0], cur;
            for (int i = 1, iv = list.Count; i < iv; ++i)
            {
                cur = list[i];
                if (pre == cur)
                {
                    list.RemoveAt(i);
                    --i; --iv;
                }
                pre = cur;
            }
        }

        public XmlNode ToXmlNode(XmlDocument xmlDoc)
        {
            XmlNode result = xmlDoc.CreateElement("ScheduleData");

            // start time/date
            XmlAttribute att = xmlDoc.CreateAttribute("startTime");
            att.Value = startTime.ToString("yyyy-MM-dd HH:mm:ss");
            result.Attributes.Append(att);

            // type
            att = xmlDoc.CreateAttribute("scheduleType");
            att.Value = scheduleType.ToString();
            result.Attributes.Append(att);

            // days in week
            StringBuilder strBdr = new StringBuilder();
            SortAndRemoveDuplis(ref startDaysInWeek);
            if (startDaysInWeek.Count > 0)
            {
                foreach (int d in startDaysInWeek)
                {
                    strBdr.Append(d + ",");
                }
                strBdr.Remove(strBdr.Length - 1, 1);
            }
            att = xmlDoc.CreateAttribute("startDaysInWeek");
            att.Value = strBdr.ToString();
            result.Attributes.Append(att);

            // days in month
            strBdr.Clear();
            SortAndRemoveDuplis(ref startDaysInMonth);
            if (startDaysInMonth.Count > 0)
            {
                foreach (int d in startDaysInMonth)
                {
                    strBdr.Append(d + ",");
                }
                strBdr.Remove(strBdr.Length - 1, 1);
            }
            att = xmlDoc.CreateAttribute("startDaysInMonth");
            att.Value = strBdr.ToString();
            result.Attributes.Append(att);

            // other interval
            att = xmlDoc.CreateAttribute("otherIntervalDays");
            att.Value = ((int)otherInterval.TotalDays).ToString();
            result.Attributes.Append(att);
            att = xmlDoc.CreateAttribute("otherIntervalTime");
            att.Value = otherInterval.ToString(@"hh\:mm\:ss");
            result.Attributes.Append(att);

            // title
            result.AppendChild(xmlDoc.CreateCDataSection(title));

            return result;
        }
        /// <summary>
        /// 从XML加载信息
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns>返回码，
        /// 0-pass，1-startTime错误，2-scheduleType错误，3-startDaysInWeek错误，
        /// 4-startDaysInMonth错误，5-otherIntervalDays错误，6-otherIntervalTime错误，
        /// 7-title错误</returns>
        public int FromXmlNode(XmlNode xmlNode)
        {
            int result = 0;
            XmlAttribute att = xmlNode.Attributes["startTime"];
            if (att != null)
            {
                if (DateTime.TryParse(att.Value, out DateTime vD))
                {
                    startTime = vD;
                }
                else result = 1;
            }
            else result = 1;

            att = xmlNode.Attributes["scheduleType"];
            if (att != null)
            {
                if (Enum.TryParse(att.Value, out ScheduleTypes vS))
                {
                    scheduleType = vS;
                }
                else result = 2;
            }
            else result = 2;

            att = xmlNode.Attributes["startDaysInWeek"];
            if (att != null)
            {
                if (TryGetIntList(att.Value, ref startDaysInWeek))
                {
                    SortAndRemoveDuplis(ref startDaysInWeek);
                }
                else result = 3;
            }
            else result = 3;

            att = xmlNode.Attributes["startDaysInMonth"];
            if (att != null)
            {
                if (TryGetIntList(att.Value, ref startDaysInMonth))
                {
                    SortAndRemoveDuplis(ref startDaysInMonth);
                }
                else result = 4;
            }
            else result = 4;

            bool TryGetIntList(string str, ref List<int> list)
            {
                list.Clear();
                string[] parts = str.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                int outI;
                foreach (string p in parts)
                {
                    if (int.TryParse(p, out outI))
                    {
                        list.Add(outI);
                    }
                    else return false;
                }
                return true;
            }

            att = xmlNode.Attributes["otherIntervalDays"];
            int otherIntervalDays = -1;
            if (att != null)
            {
                if (!int.TryParse(att.Value, out otherIntervalDays))
                    result = 5;
            }
            else result = 5;
            if (otherIntervalDays >= 0)
            {
                att = xmlNode.Attributes["otherIntervalTime"];
                if (att != null)
                {
                    if (TimeSpan.TryParse(att.Value, out TimeSpan vTS))
                    {
                        otherInterval = new TimeSpan(
                            otherIntervalDays,
                            vTS.Hours, vTS.Minutes, vTS.Seconds);
                    }
                    else result = 6;
                }
                else result = 6;
            }

            if (xmlNode.ChildNodes.Count > 0
                && xmlNode.ChildNodes[0] is XmlCDataSection)
            {
                title = ((XmlCDataSection)xmlNode.ChildNodes[0]).Value;
            }
            else result = 7;

            return result;
        }
        public static ScheduleData CreateFromXmlNode(XmlNode xmlNode, out int rc)
        {
            ScheduleData result = new ScheduleData();
            rc = result.FromXmlNode(xmlNode);
            return result;
        }

        public ScheduleData Clone()
        {
            ScheduleData result = new ScheduleData()
            {
                title = title,
                scheduleType = scheduleType,
                startTime = startTime,
                startDaysInWeek = CloneIntList(startDaysInWeek),
                startDaysInMonth = CloneIntList(startDaysInMonth),
                otherInterval = otherInterval,
            };
            return result;

            List<int> CloneIntList(List<int> source)
            {
                List<int> result = new List<int>();
                foreach (int i in source)
                    result.Add(i);
                return result;
            }
        }


        public string GetCycleDescription(bool withTimeCheck = true)
        {
            switch (scheduleType)
            {
                case ScheduleTypes.Once:
                    if (withTimeCheck)
                        return $"一次性({(DateTime.Now < startTime ? "等待" : "已执行")})";
                    else
                        return "一次性";
                case ScheduleTypes.EveryDay:
                    return "每天";
                case ScheduleTypes.EveryWeek:
                    return $"每周({ListToString(startDaysInWeek, true)})";
                case ScheduleTypes.EveryMonth:
                    return $"每月({ListToString(startDaysInMonth)})";
                    break;
                case ScheduleTypes.OtherInterval:
                    return $"特殊间隔({OtherIntvToString()})";
                default:
                    return "未知类型";
            }
            string ListToString(List<int> list, bool isWeekDays = false)
            {
                StringBuilder strBdr = new StringBuilder();
                if (isWeekDays)
                {
                    foreach (int d in list)
                    {
                        if (d == 0)
                            strBdr.Append(7);
                        else
                            strBdr.Append(d);
                        strBdr.Append(",");
                    }
                }
                else
                {
                    foreach (int d in list)
                    {
                        strBdr.Append(d);
                        strBdr.Append(",");
                    }
                }
                if (strBdr.Length > 0)
                    strBdr.Remove(strBdr.Length - 1, 1);
                return strBdr.ToString();
            }
            string OtherIntvToString()
            {
                StringBuilder strBdr = new StringBuilder();
                int days = (int)otherInterval.TotalDays;
                if (days >= 1)
                {
                    strBdr.Append(days);
                    strBdr.Append("天, ");
                }
                strBdr.Append(otherInterval.ToString(@"hh\:mm\:dd"));
                return strBdr.ToString();
            }
        }
    }
}
