using System;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace MadTomDev.Common
{
    public class Logger
    {
        #region static
        public static void StaticLog(string info)
        {
            StaticLog(info, AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Logs");
        }
        public static void StaticLog(Exception err)
        {
            StaticLog(err.ToString());
        }

        public static void StaticLog(string info, string toFolder)
        {
            StaticLog(info, toFolder, null);
        }
        public static void StaticLog(Exception err, string toFolder)
        {
            StaticLog(err.ToString(), toFolder);
        }

        public static void StaticLog(string info, string toFolder, string filePrefix)
        {
            string baseDir = toFolder;
            if (Directory.Exists(baseDir) == false)
            {
                Directory.CreateDirectory(baseDir);
            }
            DateTime now = DateTime.Now;
            string fileName = string.IsNullOrEmpty(filePrefix) ? "" : (filePrefix + ".");
            fileName += now.ToString("yyyy_MM_dd.HH") + ".txt";
            File.AppendAllText(baseDir + "\\" + fileName, now.ToString("HH:mm:ss:fff") + " " + info + Environment.NewLine);
        }
        public static void StaticLog(Exception err, string toFolder, string filePrefix)
        {
            StaticLog(err.ToString(), toFolder, filePrefix);
        }

        #endregion

        private string _BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        public string BaseDir
        {
            set
            {
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }
                _BaseDir = value;
                ReBuild_logFileFullName();
            }
            get
            {
                return _BaseDir;
            }
        }
        private string _BaseFileNamePrefix = "log";
        public string logFileSuffix = ".txt";


        /// <summary>
        /// default is "log",
        /// file name will be like "log.2020-09-07 11_00.txt"(if BaseFileNameSuf is ".txt")
        /// </summary>
        public string BaseFileNamePre
        {
            set
            {
                if (value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                {
                    _BaseFileNamePrefix = value;
                    ReBuild_logFileFullName();
                }
                else throw new InvalidCastException("File name \"" + value + "\" contains invalid char(s)!");
            }
            get
            {
                return _BaseFileNamePrefix;
            }
        }
        /// <summary>
        /// default is ".txt"
        /// </summary>
        public string BaseFileNameSuf
        {
            set
            {
                if (value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
                {
                    logFileSuffix = value;
                    ReBuild_logFileFullName();
                }
                else throw new InvalidCastException("File name suffix \"" + value + "\" contains invalid char(s)!");
            }
            get
            {
                return logFileSuffix;
            }
        }
        public enum IncrementTypeEnums
        {
            Size1M,
            Size2M,
            Size5M,
            Size10M,
            Size20M,
            Size50M,
            Size100M,
            TimeDay,
            TimeHour,
            TimeMinute,
        }

        private long baseLogFileSize;
        private IncrementTypeEnums _IncrementType;
        public IncrementTypeEnums IncrementType
        {
            set
            {
                if (_IncrementType.ToString().ToLower().StartsWith("size"))
                {
                    if (value.ToString().ToLower().StartsWith("size") == false)
                        LogFileIndex = -1;
                    else
                        LogFileIndex = 0;
                }
                _IncrementType = value;
            }
            get
            {
                return _IncrementType;
            }
        }

        private DateTime logFileTime;
        private string logFileFullName;
        public string CurrentWritingLogFileFullName
        {
            get { return logFileFullName; }
        }
        private int LogFileIndex = -1;
        public void TryReBuildLogFileName()
        {
            ReBuild_logFileFullName();
        }
        private void ReBuild_logFileFullName()
        {
            logFileTime = DateTime.Now;
            char separChar = Path.DirectorySeparatorChar;
            string doubleSepar = "" + separChar + separChar;
            if (_IncrementType.ToString().ToLower().StartsWith("size") == true)
            {
                if (baseLogFileSize > 0) LogFileIndex++;
                logFileFullName = _BaseDir + separChar
                    + ((_BaseFileNamePrefix == null || _BaseFileNamePrefix.Length == 0) ? "" : (_BaseFileNamePrefix + "."))
                    + logFileTime.ToString("yyyy-MM-dd HH_mm") + "." + LogFileIndex.ToString("00#") + logFileSuffix;
                baseLogFileSize = 0;
            }
            else
            {
                logFileFullName = _BaseDir + separChar
                    + ((_BaseFileNamePrefix == null || _BaseFileNamePrefix.Length == 0) ? "" : (_BaseFileNamePrefix + "."))
                    + logFileTime.ToString("yyyy-MM-dd HH_mm") + logFileSuffix;
            }
            while (logFileFullName.Contains(doubleSepar))
            {
                logFileFullName = logFileFullName.Replace(doubleSepar, "" + separChar);
            }
        }
        public Logger()
        {
            cacheQueue = new Queue<string>();
            IncrementType = IncrementTypeEnums.Size2M;
            ReBuild_logFileFullName();
        }

        private void CheckIncresment(long newTextLength)
        {
            //if (!File.Exists(logFileFullName))
            //{
            //    return;
            //}
            string typeString = IncrementType.ToString().ToLower();
            if (typeString.StartsWith("size"))
            {
                long sizeAfter = baseLogFileSize + newTextLength;

                switch (IncrementType)
                {
                    case IncrementTypeEnums.Size1M:
                        if (sizeAfter > 1048576) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size2M:
                        if (sizeAfter > 2097152) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size5M:
                        if (sizeAfter > 5242880) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size10M:
                        if (sizeAfter > 10485760) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size20M:
                        if (sizeAfter > 20971520) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size50M:
                        if (sizeAfter > 52428800) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.Size100M:
                        if (sizeAfter > 104857600) ReBuild_logFileFullName();
                        break;
                }
            }
            if (typeString.StartsWith("time"))
            {
                DateTime now = DateTime.Now;
                switch (IncrementType)
                {
                    case IncrementTypeEnums.TimeDay:
                        if ((now.Day != logFileTime.Day)) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.TimeHour:
                        if ((now.Day != logFileTime.Day) && (now.Hour != logFileTime.Hour)) ReBuild_logFileFullName();
                        break;
                    case IncrementTypeEnums.TimeMinute:
                        if ((now.Day != logFileTime.Day) && (now.Hour != logFileTime.Hour) && (now.Minute != logFileTime.Minute)) ReBuild_logFileFullName();
                        break;
                }
            }
        }


        /// <summary>
        /// default with "Environment.NewLine";
        /// you can double it, so an extra blank line will help manual reading;
        /// </summary>
        public string LogLineEndwith = Environment.NewLine;

        private bool _IsBusy = false;
        /// <summary>
        /// No matter busy or not, use 'Log()' freely, msgs will enqueue for later writing
        /// </summary>
        public bool IsBusy { get { return _IsBusy; } }
        public void Log(string msg, bool withTimeStamp = true)
        {            
            while (cacheQueue.Count > 1000)
                Thread.Sleep(10);

            lock (cacheQueue)
            {
                if (withTimeStamp)
                    cacheQueue.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg);
                else
                    cacheQueue.Enqueue(msg);
            }
            if (_IsBusy)
                return;

            _IsBusy = true;
            _Log_Loop();
        }
        public void NewLine()
        {
            while (cacheQueue.Count > 1000)
                Thread.Sleep(10);

            lock (cacheQueue)
                cacheQueue.Enqueue("");
            if (_IsBusy)
                return;

            _IsBusy = true;
            _Log_Loop();
        }
        private Queue<string> cacheQueue;
        private void _Log_Loop()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
            {
                using (var stream = File.Open(logFileFullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StringBuilder msgBdr = new StringBuilder();
                    while (cacheQueue.Count > 0)
                    {
                        msgBdr.Clear();
                        lock (cacheQueue)
                            msgBdr.Append(cacheQueue.Dequeue());
                        msgBdr.Append(LogLineEndwith);
                        int msgLength = msgBdr.Length;
                        CheckIncresment(msgLength);
                        //File.AppendAllText(logFileFullName, msgBdr.ToString());
                        byte[] data = Encoding.UTF8.GetBytes(msgBdr.ToString());
                        stream.Write(data, 0, data.Length);
                        baseLogFileSize += msgLength;
                    }
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
                _IsBusy = false;
            }));
        }
        public void Flush()
        {
            while (_IsBusy)
            {
                Thread.Sleep(10);
            }
            _IsBusy = true;
            _Log_Loop();
        }
        public void Log(Exception err, bool withTimeStamp = true)
        {
            Log(err.ToString(), withTimeStamp);
        }

        public class LogFileInfo
        {
            public string fullName;
            public string name;
            public DateTime timeOnName;
        }

        public static List<LogFileInfo> GetLogFiles(string logDirNameOrFullName, string fileSuffix = ".txt")
        {
            string pathSeparator = "" + Path.DirectorySeparatorChar;
            string pathSeparatorDoubled = pathSeparator + pathSeparator;
            if (logDirNameOrFullName.Contains(pathSeparator) == false)
                logDirNameOrFullName
                    = AppContext.BaseDirectory
                    + pathSeparator + logDirNameOrFullName;
            while (logDirNameOrFullName.Contains(pathSeparatorDoubled))
                logDirNameOrFullName.Replace(pathSeparatorDoubled, pathSeparator);

            List<LogFileInfo> result = new List<LogFileInfo>();
            //Regex timeStrChecker = new Regex(@"^\d{4,5}-\d{1,2}-\d{1,2} \d{2}:\d{2}:\d{2}$");
            string logFileName;
            string logFileName_mid;
            foreach (string logFileFullName in Directory.GetFiles(logDirNameOrFullName, "*" + fileSuffix))
            {
                logFileName
                    = logFileFullName.Substring(
                        logFileFullName.LastIndexOf(pathSeparator) + 1);

                logFileName_mid = logFileName.Substring(logFileName.IndexOf('.'));
                try
                {
                    result.Add(
                        new LogFileInfo()
                        {
                            timeOnName = DateTime.Parse(logFileName_mid),
                            fullName = logFileFullName,
                            name = logFileName,
                        });
                }
                catch (Exception) { }
            }
            return result;
        }
    }
}
