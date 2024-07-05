using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MadTomDev.App
{
    public class Setting
    {
        private Setting() { Reload(); }
        private static Setting instance = new Setting();
        public static Setting Instance
        { get => instance; }

        private string file = Path.Combine(Common.Variables.IOPath.SettingDir, "MultiExplorer2.xml");
        public void Reload()
        {
            curLayout = null;
            layouts.Clear();
            if (!File.Exists(file))
                return;

            // Root
            //    Layouts
            //       C (current)
            //       L idx size char foreClr bgClr
            //          TipTx
            //          E posi bgClr uri
            //          E ...
            //    Setting  isFSWEnabled  sameDirHandleType  sameFileHandleType  isLogEnabled logFlags


            Data.SettingXML xml = Data.SettingXML.FromFile(file);

            #region layouts

            Data.SettingXML.Node xmlUserNode, xmlNode = null, xn1;
            //List<Data.SettingXML.Node> xmlNodes;


            xmlUserNode = xml.rootNode["User"].Find(n => n.attributes["name"] == Environment.UserName);
            if (xmlUserNode != null)
                xmlNode = xmlUserNode["Layouts"].FirstOrDefault();

            if (xmlNode != null)
            {
                //object test = xmlNode["C"];
                xn1 = xmlNode["C"].FirstOrDefault();
                if (xn1 != null)
                {
                    curLayout = LayoutFromXmlNode(xn1);
                }
                foreach (Data.SettingXML.Node nL in xmlNode["L"])
                {
                    layouts.Add(LayoutFromXmlNode(nL));
                }
            }
            #endregion
            Layout LayoutFromXmlNode(Data.SettingXML.Node lNode)
            {
                Layout l = new Layout()
                {
                    idx = int.Parse(lNode.attributes["idx"]),
                    size = Layout.IntPair.Parse(lNode.attributes["size"]),
                    c = lNode.attributes["char"][0],
                    sizeClr = TryGetColor(lNode.attributes["sizeClr"], Colors.Lime),
                    foreClr = TryGetColor(lNode.attributes["foreClr"], Colors.Black),
                    bgClr = TryGetColor(lNode.attributes["bgClr"], Colors.Gray),
                    //tipTx //explorerList
                };
                xn1 = lNode["TipTx"].FirstOrDefault();
                if (xn1 != null)
                    l.tipTx = xn1.Text;

                foreach (Data.SettingXML.Node nE in lNode["E"])
                {
                    l.explorerList.Add(new Layout.Explorer()
                    {
                        posi = Layout.IntPair.Parse(nE.attributes["posi"]),
                        url = nE.attributes["url"],
                        bgClr = (Color)ColorConverter.ConvertFromString(nE.attributes["bgClr"]),
                    });
                }

                return l;
            }
            Color TryGetColor(string txValue, Color defaultClr)
            {
                if (string.IsNullOrEmpty(txValue))
                    return defaultClr;
                else
                    return (Color)ColorConverter.ConvertFromString(txValue);
            }

            if (xmlUserNode != null)
                xmlNode = xmlUserNode["Settings"].FirstOrDefault();
            if (xmlNode != null)
            {
                if (bool.TryParse(xmlNode.attributes["isFileSystemWatcherEnabled"], out bool b))
                    isFileSystemWatcherEnabled = b;
                if (Enum.TryParse(typeof(Data.TransferManager.SameNameDirHandleTypes), xmlNode.attributes["sameNameDirHandleType"], out object o))
                    sameNameDirHandleType = (Data.TransferManager.SameNameDirHandleTypes)o;
                if (Enum.TryParse(typeof(Data.TransferManager.SameNameFileHandleTypes), xmlNode.attributes["sameNameFileHandleType"], out object o1))
                    sameNameFileHandleType = (Data.TransferManager.SameNameFileHandleTypes)o1;
                if (bool.TryParse(xmlNode.attributes["isLogEnabled"], out bool b1))
                    isLogEnabled = b1;
                string testStr = xmlNode.attributes["logFlag"];
                if (!string.IsNullOrWhiteSpace(testStr))
                    logFlag = testStr;
                testStr = xmlNode.attributes["language"];
                if (!string.IsNullOrWhiteSpace(testStr))
                    language = testStr;
            }
        }

        public void Save()
        {
            Data.SettingXML xml;
            if (File.Exists(file))
                xml = Data.SettingXML.FromFile(file);
            else
                xml = new Data.SettingXML();


            Data.SettingXML.Node xmlUserNode = xml.rootNode["User"].Find(n => n.attributes["name"] == Environment.UserName);
            if (xmlUserNode == null)
            {
                xmlUserNode = new Data.SettingXML.Node() { nodeName = "User", };
                xmlUserNode.attributes.AddUpdate("name", Environment.UserName);
                xml.rootNode.Add(xmlUserNode);
            }

            #region layouts

            Data.SettingXML.Node layoutsNode;
            layoutsNode = xmlUserNode["Layouts"].FirstOrDefault();
            if (layoutsNode == null)
            {
                layoutsNode = new Data.SettingXML.Node() { nodeName = "Layouts", };
                xmlUserNode.Add(layoutsNode);
            }
            layoutsNode.Children.Clear();

            Data.SettingXML.Node lN, xN1;
            if (curLayout != null)
            {
                xN1 = LayoutToXmlNode(curLayout);
                xN1.nodeName = "C";
                layoutsNode.Add(xN1);
            }
            for (int i = 0,iv = layouts.Count;i<iv;++i)
            {
                layouts[i].idx = i;
                layoutsNode.Add(LayoutToXmlNode(layouts[i]));
            }
            Data.SettingXML.Node LayoutToXmlNode(Layout l)
            {
                lN = new Data.SettingXML.Node()
                { nodeName = "L", };

                lN.attributes.AddUpdate("idx", l.idx.ToString());
                lN.attributes.AddUpdate("size", l.size.ToString());
                lN.attributes.AddUpdate("char", l.c.ToString());
                lN.attributes.AddUpdate("sizeClr", l.sizeClr.ToString());
                lN.attributes.AddUpdate("foreClr", l.foreClr.ToString());
                lN.attributes.AddUpdate("bgClr", l.bgClr.ToString());

                if (!string.IsNullOrEmpty(l.tipTx))
                {
                    xN1 = new Data.SettingXML.Node() { nodeName = "TipTx", Text = l.tipTx, };
                    lN.Add(xN1);
                }
                foreach (Layout.Explorer e in l.explorerList)
                {
                    xN1 = new Data.SettingXML.Node() { nodeName = "E", };
                    lN.Add(xN1);
                    xN1.attributes.AddUpdate("posi", e.posi.ToString());
                    xN1.attributes.AddUpdate("url", e.url);
                    xN1.attributes.AddUpdate("bgClr", e.bgClr.ToString());
                }
                return lN;
            }
            #endregion

            #region settings
            Data.SettingXML.Node settingsNode;

            settingsNode = xmlUserNode["Settings"].FirstOrDefault();
            if (settingsNode == null)
            {
                settingsNode = new Data.SettingXML.Node() { nodeName = "Settings", };
                xmlUserNode.Add(settingsNode);
            }
            settingsNode.attributes.AddUpdate("isFileSystemWatcherEnabled", isFileSystemWatcherEnabled.ToString());
            settingsNode.attributes.AddUpdate("sameNameDirHandleType", sameNameDirHandleType.ToString());
            settingsNode.attributes.AddUpdate("sameNameFileHandleType", sameNameFileHandleType.ToString());
            settingsNode.attributes.AddUpdate("isLogEnabled", isLogEnabled.ToString());
            settingsNode.attributes.AddUpdate("logFlag", logFlag);
            settingsNode.attributes.AddUpdate("language", language);

            #endregion

            xml.Save(file);
        }

        #region layouts

        public Layout curLayout;
        public List<Layout> layouts = new List<Layout>();
        public class Layout
        {
            public int idx;
            public IntPair size;
            public char c;
            public Color sizeClr;
            public Color foreClr;
            public Color bgClr;
            public string tipTx;
            public List<Explorer> explorerList = new List<Explorer>();

            public class Explorer
            {
                public IntPair posi;
                public Color bgClr;
                public string url;
            }
            public class IntPair
            {
                /// <summary>
                /// for size x is width
                /// </summary>
                public int x;
                /// <summary>
                /// for size y is height
                /// </summary>
                public int y;
                public new string ToString()
                {
                    return $"{x},{y}";
                }
                public static IntPair Parse(string v)
                {
                    int cmaIdx = v.IndexOf(',');
                    return new IntPair()
                    {
                        x = int.Parse(v.Substring(0, cmaIdx)),
                        y = int.Parse(v.Substring(cmaIdx + 1))
                    };
                }
            }
        }

        #endregion


        #region settings

        public bool isFileSystemWatcherEnabled = true;

        public Data.TransferManager.SameNameDirHandleTypes sameNameDirHandleType = Data.TransferManager.SameNameDirHandleTypes.Combine;
        public Data.TransferManager.SameNameFileHandleTypes sameNameFileHandleType = Data.TransferManager.SameNameFileHandleTypes.Ask;

        public bool isLogEnabled = false;
        private string logFlag = "1101101";

        private bool? _logFlagGeneral = null;
        public bool logFlagGeneral
        {
            get
            {
                if (_logFlagGeneral == null)
                {
                    _logFlagGeneral = GetLogFlag(0);
                }
                return _logFlagGeneral == true;
            }
            set
            {
                _logFlagGeneral = value;
                SetLogFlag(0, ref value);
            }
        }
        private bool GetLogFlag(int idx, bool defaultValue = false)
        {
            if (idx >= 0 && idx < logFlag.Length)
            {
                return logFlag[idx] == '1';
            }
            return defaultValue;
        }
        private void SetLogFlag(int idx, ref bool value)
        {
            if (idx >= logFlag.Length)
            {
                StringBuilder strBdr = new StringBuilder();
                strBdr.Append(logFlag);
                for (int i = 0, iv = idx - logFlag.Length; i < iv; ++i)
                {
                    strBdr.Append('0');
                }
                strBdr.Append(value ? "1" : "0");
                logFlag = strBdr.ToString();
            }
            else
            {
                if (idx == 0)
                {
                    logFlag = $"{(value ? "1" : "0")}{logFlag.Substring(1)}";
                }
                else if (idx > 0 && idx < logFlag.Length - 1)
                {
                    logFlag = $"{logFlag.Substring(0, idx)}{(value ? "1" : "0")}{logFlag.Substring(idx + 1)}";
                }
                else
                {
                    logFlag = $"{logFlag.Substring(0, idx)}{(value ? "1" : "0")}";
                }
            }
        }

        private bool? _logFlagError = null;
        public bool logFlagError
        {
            get
            {
                if (_logFlagError == null)
                {
                    _logFlagError = GetLogFlag(1);
                }
                return _logFlagError == true;
            }
            set
            {
                _logFlagError = value;
                SetLogFlag(1, ref value);
            }
        }
        private bool? _logFlagStorageAccess = null;
        public bool logFlagStorageAccess
        {
            get
            {
                if (_logFlagStorageAccess == null)
                {
                    _logFlagStorageAccess = GetLogFlag(2);
                }
                return _logFlagStorageAccess == true;
            }
            set
            {
                _logFlagStorageAccess = value;
                SetLogFlag(2, ref value);
            }
        }
        private bool? _logFlagStoragePlugInOut = null;
        public bool logFlagStoragePlugInOut
        {
            get
            {
                if (_logFlagStoragePlugInOut == null)
                {
                    _logFlagStoragePlugInOut = GetLogFlag(3);
                }
                return _logFlagStoragePlugInOut == true;
            }
            set
            {
                _logFlagStoragePlugInOut = value;
                SetLogFlag(3, ref value);
            }
        }
        private bool? _logFlagTransTask = null;
        public bool logFlagTransTask
        {
            get
            {
                if (_logFlagTransTask == null)
                {
                    _logFlagTransTask = GetLogFlag(4);
                }
                return _logFlagTransTask == true;
            }
            set
            {
                _logFlagTransTask = value;
                SetLogFlag(4, ref value);
            }
        }
        private bool? _logFlagTransDetails = null;
        public bool logFlagTransDetails
        {
            get
            {
                if (_logFlagTransDetails == null)
                {
                    _logFlagTransDetails = GetLogFlag(5);
                }
                return _logFlagTransDetails == true;
            }
            set
            {
                _logFlagTransDetails = value;
                SetLogFlag(5, ref value);
            }
        }
        private bool? _logFlagTransError = null;

        public bool logFlagTransError
        {
            get
            {
                if (_logFlagTransError == null)
                {
                    _logFlagTransError = GetLogFlag(6);
                }
                return _logFlagTransError == true;
            }
            set
            {
                _logFlagTransError = value;
                SetLogFlag(6, ref value);
            }
        }

        private bool? _logFlagFSWatcher = null;
        public bool logFlagFSWatcher
        {
            get
            {
                if (_logFlagFSWatcher == null)
                {
                    _logFlagFSWatcher = GetLogFlag(7);
                }
                return _logFlagFSWatcher == true;
            }
            set
            {
                _logFlagFSWatcher = value;
                SetLogFlag(7, ref value);
            }
        }

        private string _language = null;
        public string language
        {
            get => _language;
            set
            {
                _language = value;
            }
        }


        #endregion
    }
}
