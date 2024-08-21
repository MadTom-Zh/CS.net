using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MadTomDev.App
{
    class Setting : Data.SettingsTxt
    {
        private Setting() { }
        private static Setting instance = null;
        public static new Setting GetInstance()
        {
            if (instance == null)
            {
                instance = new Setting()
                {
                    SettingsFileFullName = Path.Combine(
                        Common.Variables.IOPath.SettingDir,
                        "ShowMeYourUSB.txt"),
                };
                instance.ReLoad();
            }
            return instance;
        }
        public bool IsListeningHideTray
        {
            get => TryGetBool(base["IsListeningHideTray"], false);
            set => base["IsListeningHideTray"] = value.ToString();
        }
        public bool IsWndMiniHideTaskBtn
        {
            get => TryGetBool(base["IsWndMiniHideTaskBtn"], true);
            set => base["IsWndMiniHideTaskBtn"] = value.ToString();
        }

        
        private bool TryGetBool(string str, bool failDefault)
        {
            if (bool.TryParse(str, out bool result))
            {
                return result;
            }
            return failDefault;
        }

        public bool IsLogPluged
        {
            get => TryGetBool(base["IsLogPluged"], true);
            set => base["IsLogPluged"] = value.ToString();
        }
        public bool IsLogFileTree
        {
            get => TryGetBool(base["IsLogFileTree"], true);
            set => base["IsLogFileTree"] = value.ToString();
        }
        public bool IsCopyFiles
        {
            get => TryGetBool(base["IsCopyFiles"], false);
            set => base["IsCopyFiles"] = value.ToString();
        }

        private bool _ExcludingList_notInited = true;
        private HashSet<Guid> _ExcludingList = new HashSet<Guid>();
        public HashSet<Guid> ExcludingList
        {
            get
            {
                if (_ExcludingList_notInited)
                {
                    string test = base["ExcludingList"];
                    if (test != null && test.Length > 0)
                    {
                        string[] parts = test.Split('|');
                        foreach (string p in parts)
                        {
                            if (Guid.TryParse(p, out Guid r))
                            {
                                _ExcludingList.Add(r);
                            }
                        }
                    }
                    _ExcludingList_notInited = false;
                }
                return _ExcludingList;
            }
        }
        public void AddExcluding(Guid guid)
        {
            _ExcludingList.Add(guid);
        }
        public void RemoveExcluding(Guid guid)
        {
            _ExcludingList.Remove(guid);
        }

        public new void Save()
        {
            StringBuilder elBdr = new StringBuilder();
            foreach (Guid g in _ExcludingList)
            {
                elBdr.Append(g);
                elBdr.Append("|");
            }
            if (elBdr.Length > 0)
                elBdr.Remove(elBdr.Length - 1, 1);
            base["ExcludingList"] = elBdr.ToString();

            base.Save();
        }
    }
}
