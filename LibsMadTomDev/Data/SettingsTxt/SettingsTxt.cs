using System;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MadTomDev.Data
{
    public class SettingsTxt
    {
        private string _SettingsFileFullName = AppDomain.CurrentDomain.BaseDirectory + "Settings.txt";
        public string SettingsFileFullName
        {
            set
            {
                if (_SettingsFileFullName != value)
                {
                    //Save();
                    _SettingsFileFullName = value;
                    ReLoad();
                }
            }
            get
            {
                return _SettingsFileFullName;
            }
        }
        public Dictionary<string, string> Settings = new Dictionary<string, string>();

        private static SettingsTxt _instance;
        public static SettingsTxt GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SettingsTxt();
                _instance.ReLoad();
            }
            return _instance;
        }

        public SettingsTxt()
        {
        }
        public SettingsTxt(string settingFileFullName)
        {
            SettingsFileFullName = settingFileFullName;
        }


        public const string FlagNewLine = "{NewLine}";
        public const string FlagParameter = "{Parameter}";

        public void ReLoad()
        {
            if (File.Exists(SettingsFileFullName))
            {
                string tmp, key, value;
                int tabIdx;
                foreach (string line in File.ReadAllLines(SettingsFileFullName))
                {
                    tmp = line.TrimStart();
                    if (string.IsNullOrWhiteSpace(line)
                        || tmp.StartsWith("//"))
                    {
                        continue;
                    }
                    if (tmp.Contains("\t"))
                    {
                        tabIdx = tmp.IndexOf('\t');
                        key = tmp.Substring(0, tabIdx).Trim();
                        if (key.Length <= 0) continue;
                        value = tmp.Substring(tabIdx + 1);
                        if (value.Contains(FlagNewLine))
                            value = value.Replace(FlagNewLine, Environment.NewLine);

                        if (Settings.ContainsKey(key))
                        {
                            Settings[key] = value;
                        }
                        else
                        {
                            Settings.Add(key, value);
                        }
                    }
                }
            }
        }

        public void Save()
        {
            using (FileStream fs = new FileStream(SettingsFileFullName, FileMode.Create))
            {
                Save(fs);
                fs.Close();
            }
        }
        public void Save(Stream outputStream)
        {
            string newLine = Environment.NewLine;
            byte[] data;
            foreach (string key in Settings.Keys)
            {
                data = Encoding.UTF8.GetBytes(key + "\t" + ReplaceWithFlags(Settings[key]) + newLine);
                outputStream.Write(data, 0, data.Length);
            }
            outputStream.Flush();
        }
        private string ReplaceWithFlags(string source)
        {
            if (source != null && source.Contains(Environment.NewLine))
                source = source.Replace(Environment.NewLine, FlagNewLine);

            return source;
        }

        public IEnumerable<string> Keys
        {
            get            
            {
                return Settings.Keys;
            }
        }
        public string this[string key]
        {
            set
            {
                if (Settings.ContainsKey(key)) Settings[key] = value;
                else Settings.Add(key, value);
            }
            get
            {
                if (Settings.ContainsKey(key)) return Settings[key];
                else return null;
            }
        }
        public bool HasKey(string key)
        {
            return Settings.ContainsKey(key);
        }

        public void SetUpdateValue(string key, string value)
        {
            if (Settings.ContainsKey(key))
                Settings[key] = value;
            else Settings.Add(key, value);
        }

        public string GetValue(string key)
        {
            return this[key];
        }
        public string GetValue(string key, params string[] subValues)
        {
            return GetText(this[key], subValues);
        }
        public static string GetText(string baseValue, params string[] subValues)
        {
            if (subValues == null || subValues.Length == 0)
                return baseValue;

            string[] parts = baseValue.Split(new string[] { FlagParameter }, StringSplitOptions.None);
            if ((parts.Length - 1) != subValues.Length)
                throw new ArgumentOutOfRangeException("Insufficient sub values, [" + subValues.Length + "] inputs, but [" + (parts.Length - 1) + "] sub values needed.");

            StringBuilder result = new StringBuilder();
            int i = 0;
            foreach (string sv in subValues)
            {
                result.Append(parts[i++]);
                result.Append(sv);
            }
            result.Append(parts[i]);
            return result.ToString();
        }

        private const string stringSplit_keyValue = "{tab}";
        private const string stringSplit_newLine = "{newLine}";
        private const string stringReplace_keyValue = "{@tab}";
        private const string stringReplace_newLine = "{@newLine}";
        public string SerializedString
        {
            get
            {
                StringBuilder result = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in Settings)
                {
                    result.Append(kv.Key
                        .Replace(stringSplit_keyValue, stringReplace_keyValue)
                        .Replace(stringSplit_newLine, stringReplace_newLine));
                    result.Append(stringSplit_keyValue);
                    result.Append(kv.Value
                        .Replace(stringSplit_keyValue, stringReplace_keyValue)
                        .Replace(stringSplit_newLine, stringReplace_newLine));
                    result.Append(stringSplit_newLine);
                }
                if (result.Length > 0) return result.ToString(0, result.Length - stringSplit_newLine.Length);
                else return null;
            }
            set
            {
                Settings.Clear();
                string[] kvStrPairs = value.Split(new string[] { stringSplit_newLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] kv;
                string key, vlu;
                foreach (string kvStr in kvStrPairs)
                {
                    kv = kvStr.Split(new string[] { stringSplit_keyValue }, StringSplitOptions.None);
                    if (kv.Length != 2) continue;
                    key = kv[0].Replace(stringReplace_keyValue, stringSplit_keyValue)
                        .Replace(stringReplace_newLine, stringSplit_newLine);
                    vlu = kv[1].Replace(stringReplace_keyValue, stringSplit_keyValue)
                       .Replace(stringReplace_newLine, stringSplit_newLine);
                    if (Settings.ContainsKey(key)) Settings[key] = vlu;
                    else Settings.Add(key, vlu);
                }
            }
        }

        public void Clear()
        {
            Settings.Clear();
        }

    }
}
