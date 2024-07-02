using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for WindowMenuSettings.xaml
    /// </summary>
    public partial class WindowMenuSettings : Window
    {
        public WindowMenuSettings()
        {
            InitializeComponent();
        }


        public ObservableCollection<DataGridRowViewModel> editingList = new ObservableCollection<DataGridRowViewModel>();
        public void Init(ExplorerContextMenu menu)
        {
            editingList.Clear();

            rb_disable.IsChecked = menu.disabled_disabledOrHidden;
            rb_hide.IsChecked = !menu.disabled_disabledOrHidden;

            foreach (object o in menu.itemList)
                editingList.Add(new DataGridRowViewModel(o));
            dataGrid.ItemsSource = editingList;
        }


        public bool IsDisabledItemDisableOrHidden
        {
            get => rb_disable.IsChecked == true;
        }

        public class DataGridRowViewModel : INotifyPropertyChanged
        {
            public object data;  //EMItemModel  or Separator
            public DataGridRowViewModel(object data)
            {
                this.data = data;
                if (data is EMItemModel)
                {
                    EMItemModel emi = (EMItemModel)data;
                    IsUserItem = emi.IsUserItem;
                    IsEnabled = emi.IsEnabled;
                    Icon = emi.Icon;
                    Text = emi.Text;
                    CommandText = emi.CommandText;
                    SelectionCountType = emi.SelectionCountType;
                    SelectionFileType = emi.SelectionFileType;
                }
                else if (data is Separator)
                {
                    IsUserItem = (int)((Separator)data).Tag < 0;
                    IsEnabled = true;
                    Text = EMItemModelExtensions.flag_separator;
                }
            }
            private bool _IsUserItem = true;
            public bool IsUserItem
            {
                set
                {
                    _IsUserItem = value;
                    if (value)
                    {
                        _BrushRow = null;
                    }
                    else
                    {
                        Color c = Colors.LightSlateGray;
                        c.A = 120;
                        _BrushRow = new SolidColorBrush(c);
                    }
                }
                get => _IsUserItem;
            }
            private Brush _BrushRow = null;
            public Brush BrushRow
            {
                set
                {
                    _BrushRow = value;
                    RaisePropertyChanged("BrushRow");
                }
                get => _BrushRow;
            }
            private bool _IsEnabled = true;
            public bool IsEnabled
            {
                set
                {
                    _IsEnabled = value;
                    RaisePropertyChanged("IsEnabled");
                }
                get => _IsEnabled;
            }

            private BitmapSource _Icon = null;
            public BitmapSource Icon
            {
                set
                {
                    _Icon = value;
                    RaisePropertyChanged("Icon");
                }
                get => _Icon;
            }
            private string _Text;
            public string Text
            {
                set
                {
                    _Text = value;
                    RaisePropertyChanged("Text");
                }
                get => _Text;
            }
            private string _CommandText;


            public string CommandText
            {
                set
                {
                    _CommandText = value;
                    RaisePropertyChanged("CommandText");
                }
                get => _CommandText;
            }

            public SelectionCountTypes SelectionCountType { set; get; } = SelectionCountTypes.Any0;

            public SelectionFileTypes SelectionFileType { set; get; } = SelectionFileTypes.Any;


            public event PropertyChangedEventHandler? PropertyChanged;
            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }




        #region dataGrid operates

        private void cb_enableAll_checkChanged(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            foreach (DataGridRowViewModel vm in editingList)
            {
                vm.IsEnabled = cb.IsChecked == true;
            }
        }
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                tb_itemText.Text = vm.Text;
                tb_command.Text = vm.CommandText;
                if (vm.IsUserItem)
                {
                    btn_update.IsEnabled = true;
                }
                else
                {
                    btn_update.IsEnabled = false;
                }
            }
            else
            {
                tb_itemText.Text = "";
                tb_command.Text = "";
                btn_update.IsEnabled = false;
            }
        }
        private bool TryGetSelectedDGItemVM(out DataGridRowViewModel vm)
        {
            vm = null;
            object testItem = dataGrid.SelectedItem;
            if (testItem != null && testItem is DataGridRowViewModel)
            {
                vm = (DataGridRowViewModel)testItem;
                return true;
            }
            return false;
        }

        private void dataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                if (vm.IsUserItem)
                {
                    // allow edit
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion


        #region item move up, move down, delete
        private void btn_up_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                int oIdx = editingList.IndexOf(vm);
                if (oIdx > 0)
                {
                    editingList.Remove(vm);
                    editingList.Insert(oIdx - 1, vm);
                }
                dataGrid_selectNFocus(vm);
            }
        }

        private void btn_down_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                int oIdx = editingList.IndexOf(vm);
                if (oIdx < editingList.Count - 1)
                {
                    editingList.Remove(vm);
                    editingList.Insert(oIdx + 1, vm);
                }
                dataGrid_selectNFocus(vm);
            }
        }

        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                if (vm.IsUserItem)
                {
                    editingList.Remove(vm);
                }
            }
        }

        #endregion




        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            DataGridRowViewModel newVM = new DataGridRowViewModel(null)
            { IsUserItem = true, Text = tb_itemText.Text, CommandText = tb_command.Text };
            if (newVM.Text.Trim().ToLower() == EMItemModelExtensions.flag_separator)
            {
                newVM.CommandText = "";
            }
            else
            {
                dataGrid_addUpdate_trySetIcon(newVM);
            }

            if (dataGrid.SelectedItem != null)
            {
                int idxS = editingList.IndexOf((DataGridRowViewModel)dataGrid.SelectedItem);
                if (idxS < editingList.Count - 1)
                {
                    editingList.Insert(idxS + 1, newVM);
                }
                else
                {
                    editingList.Add(newVM);
                }
            }
            else
            {
                editingList.Add(newVM);
            }
            dataGrid_addUpdate_trySetIcon(newVM);
            dataGrid_selectNFocus(newVM);
        }
        private void dataGrid_selectNFocus(object item)
        {
            dataGrid.SelectedItem = item;
            dataGrid.ScrollIntoView(item);
            dataGrid.Focus();
        }
        private void dataGrid_addUpdate_trySetIcon(DataGridRowViewModel item)
        {
            EMItemModelExtensions.CommandAnalysisResult result = EMItemModelExtensions.AnalysisCommand(item.CommandText);

            switch (result.cmdType)
            {
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.New:
                    if (File.Exists(result.newTempletFile))
                        item.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(result.newTempletFile, true, false);
                    else
                        item.Icon = null;
                    break;
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.NewDir:
                    item.Icon = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
                    break;
                case EMItemModelExtensions.CommandAnalysisResult.CmdTypes.Exec:
                    if (File.Exists(result.execFile))
                        item.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(result.execFile, true, false);
                    else
                        item.Icon = null;
                    break;
            }
        }

        private void btn_update_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetSelectedDGItemVM(out DataGridRowViewModel vm))
            {
                if (vm.IsUserItem)
                {
                    // update
                    vm.Text = tb_itemText.Text.Trim();
                    vm.CommandText = tb_command.Text.Trim();
                    dataGrid_addUpdate_trySetIcon(vm);
                }
            }
        }

        private void btn_cmdCheck_Click(object sender, RoutedEventArgs e)
        {
            string testParent = @"C:\testDir";
            HashSet<string> testFiles = new HashSet<string>();
            testFiles.Add(@"C:\testDir\a.txt");
            testFiles.Add(@"C:\testDir\b.txt");
            string testResult = EMItemModelExtensions.GetRealCommand(tb_command.Text, testParent, testFiles.ToList());
            MessageBox.Show(testResult, "Example", MessageBoxButton.OK);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            previewECMenu = null;
        }
        private ExplorerContextMenu previewECMenu = null;

        private void btn_preview_Click(object sender, RoutedEventArgs e)
        {
            // 测试文件夹 ME2\pv
            string pvDir = Common.Variables.IOPath.SettingDir;
            if (!Directory.Exists(pvDir))
            {
                Directory.CreateDirectory(pvDir);
            }
            pvDir = System.IO.Path.Combine(pvDir, "ME2");
            if (!Directory.Exists(pvDir))
            {
                Directory.CreateDirectory(pvDir);
            }
            pvDir = System.IO.Path.Combine(pvDir, "pv");
            if (!Directory.Exists(pvDir))
            {
                Directory.CreateDirectory(pvDir);
            }

            // 按设定，创建文件
            List<string> subFiles = new List<string>();
            string[] parts = tb_simu_dirs.Text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder errStrBdr = new StringBuilder();
            string test;
            foreach (string d in parts)
            {
                test = System.IO.Path.Combine(pvDir, d);
                if (Directory.Exists(test))
                {
                    subFiles.Add(test);
                    continue;
                }
                try
                {
                    Directory.CreateDirectory(test);
                    subFiles.Add(test);
                }
                catch (Exception)
                {
                    errStrBdr.Append(d);
                    errStrBdr.Append(Environment.NewLine);
                }
            }
            parts = tb_simu_files.Text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string f in parts)
            {
                test = System.IO.Path.Combine(pvDir, f);
                if (File.Exists(test))
                {
                    subFiles.Add(test);
                    continue;
                }
                try
                {
                    File.WriteAllText(test, null);
                    subFiles.Add(test);
                }
                catch (Exception)
                {
                    errStrBdr.Append(f);
                    errStrBdr.Append(Environment.NewLine);
                }
            }


            if (errStrBdr.Length > 0)
            {
                MessageBox.Show($"Cant create following items:{Environment.NewLine + errStrBdr.ToString()}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (previewECMenu == null)
                {
                    previewECMenu = new ExplorerContextMenu();

                    // 从 editingList，获取setting.list，通过这个list，生成浏览菜单
                    // 保存setting后，重新初始化生产菜单；

                    setting = new Setting(false);
                    setting.data.FromSettingWindow(this);
                    previewECMenu.ReInitItems(setting);

                    // 去掉点击后执行的具体行为；
                    MenuItem mi;
                    EMItemModel miVM;
                    foreach (object o in previewECMenu.Items)
                    {
                        if (o is MenuItem)
                        {
                            mi = (MenuItem)o;

                            if (mi.Tag is EMItemModel)
                            {
                                miVM = (EMItemModel)mi.Tag;
                                miVM.actionClick = ShowCmd;
                            }
                        }
                    }
                }
                previewECMenu.ShowContextMenu(btn_preview, pvDir, subFiles.ToArray());
            }
        }
        private void ShowCmd(EMItemModel carrier)
        {
            MessageBox.Show(carrier.CommandText);
        }

        public Setting setting;
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            // save
            setting = new Setting(false);
            setting.data.FromSettingWindow(this);
            setting.Save();

            this.DialogResult = true;
            this.Close();
        }

        private void btn_cancle_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // clear temp files
            string pvDir = System.IO.Path.Combine(Common.Variables.IOPath.SettingDir, "ME2", "pr");
            if (Directory.Exists(pvDir))
            {
                Directory.Delete(pvDir, true);
            }
        }

        private void btn_TPLT_dir_Click(object sender, RoutedEventArgs e)
        {
            string dir = System.IO.Path.Combine(Common.Variables.IOPath.SettingDir, "ME2");
            if (Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            dir = System.IO.Path.Combine(dir, "TPLT");
            if (Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Process.Start("Explorer.exe", dir);
        }
    }
}
