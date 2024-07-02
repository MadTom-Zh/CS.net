using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MadTomDev.UI
{
    public class Setting : Data.SettingXML
    {
        private static string _fileSetting;
        public static string fileSetting
        {
            get
            {
                if (_fileSetting == null)
                {
                    InitStaticVars();
                }
                return _fileSetting;
            }
        }

        private static string _dirTempletes;
        public static string dirTempletes
        {
            get
            {
                if (_dirTempletes == null)
                {
                    InitStaticVars();
                }
                return _dirTempletes;
            }
        }
        private static void InitStaticVars()
        {
            _fileSetting = Common.Variables.IOPath.SettingDir;
            _dirTempletes = Path.Combine(_fileSetting, "ME2");
            if (!Directory.Exists(_fileSetting))
                Directory.CreateDirectory(_fileSetting);
            _fileSetting = Path.Combine(_fileSetting, "ME2.ECM.xml");

            if (!Directory.Exists(_dirTempletes))
                Directory.CreateDirectory(_dirTempletes);
            _dirTempletes = Path.Combine(_dirTempletes, "TPLT");
            if (!Directory.Exists(_dirTempletes))
                Directory.CreateDirectory(_dirTempletes);
        }
        public Setting(bool loadFile = true)
        {
            base.xmlFile = fileSetting;

            if (loadFile)
                Reload();
            else
                data = new DataSet(this)
                { disableItemDisableOrHidden = true, };
        }

        public DataSet data;
        /// <summary>
        /// 请不要只用此方法
        /// </summary>
        /// <param name="fileName"></param>
        public new void Reload(string fileName) { }

        private static string flag_inst = "inst";
        private static string flag_appPath = "appPath";
        private static string flag_userName = "userName";
        private static string flag_disableOrHidden = "disableOrHidden";
        private static string flag_defOrders = "defOrders";
        private static string flag_cust = "cust";
        private static string flag_index = "index";
        private static string flag_enable = "enable";
        private static string flag_whenS = "whenS";
        private static string flag_whenF = "whenF";
        public void Reload()
        {
            data = null;
            if (File.Exists(fileSetting))
                base.Reload();

            // declear
            // root
            //    inst  appPath, disableOrHidden, defOrders
            //       cust  index, enable, whenS, whenF, txt, cmd
            Node? instNode = rootNode.Children.Find(
                n => n.nodeName == flag_inst
                && n.attributes[flag_appPath] == this.GetType().Assembly.Location
                && n.attributes[flag_userName] == Environment.UserName);
            if (instNode != null)
            {
                data = new DataSet(this);
                if (bool.TryParse(instNode.attributes[flag_disableOrHidden], out bool b))
                    data.disableItemDisableOrHidden = b;
                else
                    data.disableItemDisableOrHidden = false;

                // defOrders, (x,y),(x,y),,
                string testStr = instNode.attributes[flag_defOrders];
                if (!string.IsNullOrWhiteSpace(testStr))
                {
                    string[] strParts = testStr.Split(new string[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0, iv = strParts.Length; i < iv; ++i)
                    {
                        testStr = strParts[i].Trim();
                        if (testStr.Length > 2)
                        {
                            System.Drawing.Point? testPt = TryParse(testStr);
                            if (testPt != null)
                            {
                                data.defaultItemOrderList.Add((System.Drawing.Point)testPt);
                            }
                        }
                    }
                    System.Drawing.Point? TryParse(string str)
                    {
                        int cIdx = str.IndexOf(',');
                        if (cIdx > 0)
                        {
                            if (int.TryParse(str.Substring(0, cIdx), out int x)
                                && int.TryParse(str.Substring(cIdx + 1), out int y))
                            {
                                return new System.Drawing.Point(x, y);
                            }
                        }
                        return null;
                    }
                }

                // cust
                DataSet.CustomItemData cusItem;
                WindowMenuSettings.DataGridRowViewModel dataGridVM;
                EMItemModel vm;
                SelectionCountTypes scType;
                foreach (Node n in instNode.Children)
                {
                    if (n.Children.Count < 2)
                        continue;

                    testStr = n.attributes[flag_index];
                    if (int.TryParse(testStr, out int i))
                    {
                        cusItem = new DataSet.CustomItemData();
                        cusItem.InsertIndex = i;


                        testStr = n.Children[0].Text;
                        if (testStr.ToLower() == EMItemModelExtensions.flag_separator)
                        {
                            dataGridVM = new WindowMenuSettings.DataGridRowViewModel(EMItemModelExtensions.NewSeparator());
                        }
                        else
                        {
                            vm = new EMItemModel();

                            //enable, whenS, whenF,
                            if (bool.TryParse(n.attributes[flag_enable], out bool b1))
                                vm.IsEnabled = b1;
                            else
                                vm.IsEnabled = false;

                            testStr = n.attributes[flag_whenS];
                            if (!string.IsNullOrWhiteSpace(testStr)
                                && Enum.TryParse(typeof(SelectionCountTypes), testStr, out object? o)
                                && o != null)
                            {
                                vm.SelectionCountType = (SelectionCountTypes)o;
                            }
                            else
                            {
                                vm.SelectionCountType = SelectionCountTypes.None;
                            }

                            testStr = n.attributes[flag_whenF];
                            if (!string.IsNullOrWhiteSpace(testStr)
                                && Enum.TryParse(typeof(SelectionFileTypes), testStr, out object? o2)
                                && o2 != null)
                            {
                                vm.SelectionFileType = (SelectionFileTypes)o2;
                            }
                            else
                            {
                                vm.SelectionFileType = SelectionFileTypes.Any;
                            }

                            //  txt, cmd
                            if (n.Children.Count >= 2)
                            {
                                vm.Text = n.Children[0].Text;
                                vm.CommandText = n.Children[1].Text;
                            }

                            // icon
                            EMItemModelExtensions.CommandAnalysisResult cmdAnal = EMItemModelExtensions.AnalysisCommand(vm.CommandText);
                            if (cmdAnal.cmdType == EMItemModelExtensions.CommandAnalysisResult.CmdTypes.New
                                && !string.IsNullOrEmpty(cmdAnal.newTempletFile))
                            {
                                if (File.Exists(cmdAnal.newTempletFile))
                                    vm.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(cmdAnal.newTempletFile, true, false);
                                else
                                    vm.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(cmdAnal.newTempletFile, true, true);
                            }
                            else if (cmdAnal.cmdType == EMItemModelExtensions.CommandAnalysisResult.CmdTypes.NewDir)
                            {
                                vm.Icon = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
                            }
                            else if (cmdAnal.cmdType == EMItemModelExtensions.CommandAnalysisResult.CmdTypes.Exec
                                && !string.IsNullOrEmpty(cmdAnal.execFile))
                            {
                                vm.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(cmdAnal.execFile, true, false);
                                //vm.IconDisabled
                                //    = QuickGraphics.Image.GetTransparentImage(
                                //        QuickGraphics.Image.GetGrayImage(vm.Icon), 0.5f, true);
                            }

                            dataGridVM = new WindowMenuSettings.DataGridRowViewModel(vm);
                        }

                        cusItem.vm = dataGridVM;
                        data.customItemOrderList.Add(cusItem);
                    }
                }
            }
            else
            {
                data = new DataSet(this)
                { disableItemDisableOrHidden = true, };
            }
        }
        public new void Save()
        {
            if (File.Exists(fileSetting))
                base.Reload();

            //Node? instNode = rootNode.Children.Find(
            //    n => n.nodeName == flag_inst
            //    && n.attributes[flag_appPath] == this.GetType().Assembly.Location);
            Node? instNode = rootNode.Children.Find(
                n => n.nodeName == flag_inst
                && n.attributes[flag_appPath] == this.GetType().Assembly.Location
                && n.attributes[flag_userName] == Environment.UserName);
            if (instNode == null)
            {
                instNode = new Node() { nodeName = flag_inst };
                instNode.attributes.AddUpdate(flag_appPath, this.GetType().Assembly.Location);
                instNode.attributes.AddUpdate(flag_userName, Environment.UserName);
                rootNode.Add(instNode);
            }
            instNode.attributes.AddUpdate(flag_disableOrHidden, data.disableItemDisableOrHidden.ToString());
            StringBuilder strBdr = new StringBuilder();
            for (int i = 0, iv = data.defaultItemOrderList.Count; i < iv; ++i)
            {
                strBdr.Append("(");
                strBdr.Append(data.defaultItemOrderList[i].ToString());
                strBdr.Append("),");
            }
            if (strBdr.Length > 0)
                strBdr.Remove(strBdr.Length - 1, 1);
            instNode.attributes.AddUpdate(flag_defOrders, strBdr.ToString());

            instNode.Children.Clear();
            DataSet.CustomItemData cust;
            Node custNode;
            for (int i = 0, iv = data.customItemOrderList.Count; i < iv; ++i)
            {
                cust = data.customItemOrderList[i];
                custNode = new Node() { nodeName = flag_cust, };
                instNode.Add(custNode);

                // index, enable, whenS, whenF,
                custNode.attributes.AddUpdate(flag_index, cust.InsertIndex.ToString());
                custNode.attributes.AddUpdate(flag_enable, cust.vm.IsEnabled.ToString());
                custNode.attributes.AddUpdate(flag_whenS, cust.vm.SelectionCountType.ToString());
                custNode.attributes.AddUpdate(flag_whenF, cust.vm.SelectionFileType.ToString());

                // txt, cmd
                custNode.Add(new Node() { Text = cust.vm.Text, });
                custNode.Add(new Node() { Text = cust.vm.CommandText, });
            }

            base.Save();
        }

        // 首先将标准选项进性排序，设定可用性
        // 其次，按位置插入用户选项，包括设定可用性

        public class DataSet
        {
            public bool disableItemDisableOrHidden;
            public List<System.Drawing.Point> defaultItemOrderList = new List<System.Drawing.Point>();
            public List<CustomItemData> customItemOrderList = new List<CustomItemData>();


            private Setting parent = null;
            private WindowMenuSettings settingWnd = null;
            public DataSet(Setting parent)
            {
                this.parent = parent;
            }
            public void FromSettingWindow(WindowMenuSettings settingWindow)
            {
                this.settingWnd = settingWindow;
                disableItemDisableOrHidden = settingWindow.IsDisabledItemDisableOrHidden;
                List<WindowMenuSettings.DataGridRowViewModel> defaultList = new List<WindowMenuSettings.DataGridRowViewModel>();
                WindowMenuSettings.DataGridRowViewModel vm;
                int i, iv;
                for (i = 0, iv = settingWindow.editingList.Count; i < iv; ++i)
                {
                    vm = settingWindow.editingList[i];
                    if (!vm.IsUserItem)
                    {
                        defaultList.Add(vm);
                    }
                }

                // default order :
                // open
                // seperator
                // manageOpenWith
                // seperator
                // cut
                // copy
                // paste
                // seperator
                // createShotcut
                // delete
                // rename
                // seperator
                // properties

                List<int> simuOriIndexlist = new List<int>();
                simuOriIndexlist.AddRange(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
                int curIdx, oriIdx;
                for (i = 0, iv = defaultList.Count; i < iv; ++i)
                {
                    curIdx = GetOriIndex(defaultList[i]);
                    oriIdx = simuOriIndexlist.IndexOf(curIdx);
                    if (curIdx == oriIdx)
                        continue;

                    defaultItemOrderList.Add(new System.Drawing.Point(oriIdx, i));
                    simuOriIndexlist.Remove(curIdx);
                    simuOriIndexlist.Insert(i, curIdx);
                }
                EMItemModel vmEMIM;
                int GetOriIndex(WindowMenuSettings.DataGridRowViewModel vm)
                {
                    if (vm.data is EMItemModel)
                    {
                        vmEMIM = (EMItemModel)vm.data;
                        if (vmEMIM.name == "open") return 0;
                        if (vmEMIM.name == "manageOpenWith") return 2;
                        if (vmEMIM.name == "cut") return 4;
                        if (vmEMIM.name == "copy") return 5;
                        if (vmEMIM.name == "paste") return 6;
                        if (vmEMIM.name == "createShotcut") return 8;
                        if (vmEMIM.name == "delete") return 9;
                        if (vmEMIM.name == "rename") return 10;
                        if (vmEMIM.name == "properties") return 12;
                    }
                    else if (vm.data is Separator)
                    {
                        return (int)((Separator)vm.data).Tag;
                    }
                    return -1;
                }

                // save custom items
                for (i = 0, iv = settingWindow.editingList.Count; i < iv; ++i)
                {
                    vm = settingWindow.editingList[i];
                    if (vm.IsUserItem)
                    {
                        customItemOrderList.Add(new CustomItemData()
                        { InsertIndex = i, vm = vm, });
                    }
                }
            }

            public void FromXmlSetting()
            {
            }
            public class CustomItemData
            {
                public int InsertIndex;
                public WindowMenuSettings.DataGridRowViewModel vm;
            }
        }
    }
}
