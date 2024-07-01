using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MadTomDev.Data;
using System.Reflection;
using MadTomDev.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace MadTomDev.Data
{
    /// <summary>
    /// 使用方法
    /// 变更语言（替换字符串）的前提，是UI的文本，都已数据字典中的字符串，以动态加载方式显示；
    /// 第一次，执行SaveToFile方法，将字典中的字符串键值全部读取并保存；
    /// 设定语言时，先执行ReloadAllLangFiles方法读取所有预设语言文本，或者单独读取需要的文本文件；
    /// 要设定语言时，用TrySetDefaultLang，按当前系统语言寻找匹配并设定；
    /// 或者使用TrySetLang来设定指定的语言；
    /// 
    /// </summary>
    public class SettingsLanguage
    {
        private SettingsLanguage() { }
        private static Dictionary<string, SettingsTxt> _langDicts
             = new Dictionary<string, SettingsTxt>();

        public string DefaultFullLanguagesDirPath
        {
            get
            {
                string value = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                return value;
            }
        }
        public bool ContainsLangFile(string langName)
        {
            return _langDicts.ContainsKey(langName);
        }
        private static SettingsLanguage _Instance = null;
        public static SettingsLanguage GetInstance()
        {
            if (_Instance == null)
                _Instance = new SettingsLanguage();
            return _Instance;
        }

        public delegate void LanguageChangedDelgate(SettingsLanguage sender, string langName);
        public event LanguageChangedDelgate NewLanguageLoaded;
        public event LanguageChangedDelgate NewLanguageSet;

        public string[] ListStoredLanguages()
        {
            List<string> result = new List<string>();
            foreach (FileInfo fi in new DirectoryInfo(DefaultFullLanguagesDirPath).GetFiles("*.txt"))
            {
                result.Add(fi.Name.Substring(0, fi.Name.Length - 4));
            }
            return result.ToArray();
        }
        public bool HaveStoredLanguage(string langName)
        {
            return File.Exists(Path.Combine(DefaultFullLanguagesDirPath, langName + ".txt"));
        }

        private bool ReLoadLangFile(string langName)
        {
            string langFileFullName = Path.Combine(DefaultFullLanguagesDirPath, langName + ".txt");
            if (!File.Exists(langFileFullName))
                return false;

            if (_langDicts.ContainsKey(langName))
                _langDicts.Remove(langName);
            SettingsTxt lang = new SettingsTxt(langFileFullName);
            _langDicts.Add(langName, lang);
            NewLanguageLoaded?.Invoke(this, langName);
            return true;
        }
        public void SaveToFile(ResourceDictionary appDict, string targetFileFullName)
        {
            using (FileStream fs = new FileStream(targetFileFullName, FileMode.Create))
            {
                string newLine = Environment.NewLine;
                byte[] data = Encoding.UTF8.GetBytes(@"// Using UTF-8 to save this file, if you edit it;
// the setter will auto seeks fields, names to set the string value;
// version 2019-05-05 by longtombbj;

");
                fs.Write(data, 0, data.Length);
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    object value;
                    foreach (object key in appDict.Keys)
                    {
                        value = appDict[key];
                        if (value is string)
                        {
                            sw.WriteLine(key + "\t" + value.ToString());
                        }
                    }
                }
                //fs.Flush();
            }
        }

        public int TrySetDefaultLang(ResourceDictionary appDict, bool addIfNotExists = true)
        {
            string curLangName = System.Globalization.CultureInfo.CurrentCulture.Name;
            return TrySetLang(appDict, curLangName, addIfNotExists);
        }

        public string LastLangName { private set; get; }
        public int TrySetLang(ResourceDictionary appDict, string langName, bool addIfNotExists = true)
        {
            LastLangName = langName;
            if (ReLoadLangFile(langName))
            {
                SettingsTxt lang = _langDicts[langName];
                object value;
                int result = 0;
                foreach (string key in lang.Keys)
                {
                    if (appDict.Contains(key))
                    {
                        if (appDict[key] is string)
                        {
                            appDict[key] = lang[key];
                            ++result;
                        }
                    }
                    else if (addIfNotExists)
                    {
                        appDict.Add(key, lang[key]);
                    }
                }
                NewLanguageSet?.Invoke(this, langName);
                return result;
            }
            else
            {
                return 0;
            }
        }
        public int TrySetLang(ResourceDictionary appDict, bool addIfNotExists = true)
        {
            if (string.IsNullOrWhiteSpace(LastLangName))
                return 0;
            return TrySetLang(appDict, LastLangName, addIfNotExists);
        }

        public static string GetTx(string formatTx, params string[] words)
        {
            string result = formatTx.Replace("{nl}", Environment.NewLine);
            for (int i = 0, iv = words.Length; i < iv; ++i)
            {
                result = result.Replace("{" + i + "}", words[i]);
            }
            return result;
        }
    }
}
