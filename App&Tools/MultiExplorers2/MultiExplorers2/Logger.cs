using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MadTomDev.App
{
    public class Logger : Common.Logger
    {
        private static Logger instance = null;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger()
                    {
                        BaseDir = Common.Variables.IOPath.LogDir,
                        BaseFileNamePre = "ME2",
                        IncrementType = IncrementTypeEnums.Size10M,
                    };
                }
                return instance;
            }
        }

        public enum InfoLevels
        { None = 0, Info = 1, Warning = 2, Error = 3, }

        /// <summary>
        /// 相当于LogInfo() 记录普通消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="withTimeStamp"></param>
        public new void Log(string msg, bool withTimeStamp = true)
        {
            LogInfo("Info", msg);
        }
        /// <summary>
        /// 相当于LogError() 记录错误
        /// </summary>
        /// <param name="err"></param>
        /// <param name="withTimeStamp"></param>
        public new void Log(Exception err, bool withTimeStamp = true)
        {
            LogError(err);
        }

        public void LogInfo(string title, string msg)
        {
            Log(InfoLevels.Info, ref title, ref msg);
        }
        public void LogWarning(string title, string msg)
        {
            Log(InfoLevels.Warning, ref title, ref msg);
        }
        public void LogError(Exception err)
        {
            string title = err.Message, msg = err.ToString();
            Log(InfoLevels.Error, ref title, ref msg, err);
        }

        public InfoLevels _MaxInfoLevel = InfoLevels.None;
        public InfoLevels MaxInfoLevel
        {
            get => _MaxInfoLevel;
            set
            {
                _MaxInfoLevel = value;
                EventLevelUp?.Invoke(this, _MaxInfoLevel);
            }
        }
        public delegate void EventLevelUpDelegate(Logger sender, InfoLevels newLevel);
        public event EventLevelUpDelegate EventLevelUp;

        public void Log(InfoLevels infoLevel, ref string title, ref string msg, Exception err = null)
        {            
            base.Log($"{infoLevel}  {title}  {msg}");
            DateTime now = DateTime.Now;
            ItemViewModel item = new ItemViewModel()
            {
                time = now,
                timeTx = now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                title = title,
                description = msg,
                tag = err,
            };
            switch (infoLevel)
            {
                default:
                case InfoLevels.Info:
                    item.icon = StaticResource.SysIconInfo16;
                    break;
                case InfoLevels.Warning:
                    item.icon = StaticResource.SysIconWarning16;
                    break;
                case InfoLevels.Error:
                    item.icon = StaticResource.SysIconError16;
                    break;
            }            
            LimitAddLogItems(item);
            TrySetmaxInfoLevel(infoLevel);
        }

        private int LimitAddLogItems_maxItems = 1000;
        private void LimitAddLogItems(ItemViewModel item)
        {
            while (logItems.Count >= LimitAddLogItems_maxItems)
            {
                logItems.RemoveAt(0);
            }
            logItems.Add(item);
        }
        private void TrySetmaxInfoLevel(InfoLevels newIL)
        {
            if (_MaxInfoLevel < newIL)
            {
                MaxInfoLevel = newIL;
            }
        }
        public void ClearLogItems()
        {
            logItems.Clear();
            MaxInfoLevel = InfoLevels.None;
        }

        public ObservableCollection<ItemViewModel> logItems = new ObservableCollection<ItemViewModel>();
        public class ItemViewModel
        {
            public DateTime time;
            public string timeTx { set; get; }
            public BitmapSource icon { set; get; }
            public string title { set; get; }
            public string description { set; get; }
            public object tag;
        }
    }
}
