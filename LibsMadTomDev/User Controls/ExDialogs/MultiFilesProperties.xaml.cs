using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Path = System.IO.Path;

namespace MadTomDev.UI
{
    /// <summary>
    /// Interaction logic for MultiFilesProperties.xaml
    /// </summary>
    public partial class MultiFilesProperties : Window
    {
        public MultiFilesProperties()
        {
            InitializeComponent();
        }

        Common.IconHelper.FileSystem iconFS;
        Common.IconHelper.Shell32Icons iconS32;


        /// <summary>
        /// 初始化内容，设置多个文件、文件夹；
        /// </summary>
        public void Init(params string[] filesNDirs)
        {
            int countFiles = 0, countDirs = 0;
            Data.IOInfoShadow io, ioFileTmp = null;
            if (filesNDirs != null && filesNDirs.Length > 1)
            {
                dataGridItemSource.Clear();
                iconFS = Common.IconHelper.FileSystem.Instance;
                iconS32 = Common.IconHelper.Shell32Icons.Instance;

                foreach (string f in filesNDirs)
                {
                    if (File.Exists(f))
                    {
                        io = new Data.IOInfoShadow(new FileInfo(f));
                        ++countFiles;
                        if (ioFileTmp == null)
                        {
                            ioFileTmp = io;
                        }
                    }
                    else if (Directory.Exists(f))
                    {
                        io = new Data.IOInfoShadow(new DirectoryInfo(f));
                        ++countDirs;
                    }
                    else
                    {
                        continue;
                    }

                    dataGridItemSource.Add(new DataGridItemViewModel(io));
                }
                if (countDirs > 0 && countFiles > 0)
                {
                    SetTitle1_dirs();
                    SetTitle2_files();
                }
                else if (countDirs > 0)
                {
                    SetTitle1_dirs();
                    SetTitle2(null, null);
                }
                else if (countFiles > 0)
                {
                    SetTitle1_files();
                    SetTitle2(null, null);
                }
                else
                {
                    SetTitle1(null, null);
                    SetTitle2(null, null);
                }
                dataGrid.ItemsSource = dataGridItemSource;
            }
            else
            {
                dataGrid.ItemsSource = null;
            }

            #region pre methods
            void SetTitle1_dirs()
            {
                if (countDirs == 1)
                {
                    SetTitle1(iconFS.GetDirIcon(false), "1 Folder");
                }
                else if (countDirs > 1)
                {
                    SetTitle1(iconS32.GetCustomIcon(0), $"{countDirs} Folders");
                }
            }
            void SetTitle1_files()
            {
                if (countFiles == 1)
                {
                    SetTitle1(iconFS.GetIcon(ioFileTmp.fullName, false, false), "1 File");
                }
                else if (countFiles > 1)
                {
                    SetTitle1(iconS32.GetIcon(54, false), $"{countFiles} Files");
                }
            }
            void SetTitle2_files()
            {
                if (countFiles == 1)
                {
                    SetTitle2(iconFS.GetIcon(ioFileTmp.fullName, false, false), "1 File");
                }
                else if (countFiles > 1)
                {
                    SetTitle2(iconS32.GetIcon(54, false), $"{countFiles} Files");
                }
            }
            void SetTitle1(BitmapSource icon, string labelTx)
            {
                img_icon1.Source = icon;
                tb_count1.Text = labelTx;
            }
            void SetTitle2(BitmapSource icon, string labelTx)
            {
                img_icon2.Source = icon;
                tb_count2.Text = labelTx;
            }
            #endregion
        }

        private ObservableCollection<DataGridItemViewModel> dataGridItemSource = new ObservableCollection<DataGridItemViewModel>();
        private class DataGridItemViewModel : INotifyPropertyChanged
        {
            public Data.IOInfoShadow ioInfo;
            private static string datetimeTxFormat = "yyyy-MM-dd HH:mm:ss.fff";
            public DataGridItemViewModel(Data.IOInfoShadow ioInfo)
            {
                this.ioInfo = ioInfo;
                if (ioInfo.wasFile)
                    this.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(ioInfo.fullName, true, false);
                else
                    this.Icon = Common.IconHelper.FileSystem.Instance.GetDirIcon(true);
                Name = ioInfo.name;
                NamePrefix = Name.Contains('.') ? Name.Substring(0, Name.LastIndexOf('.')) : Name;
                Ext = ioInfo.extension;
                string sizeTx = Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(ioInfo.length, "B", 1024);
                int idxSpace = sizeTx.IndexOf(' ');
                SizeNumTx = sizeTx.Substring(0, idxSpace);
                SizeUnitTx = sizeTx.Substring(idxSpace + 1);

                CreateTime = ioInfo.creationTime;
                ModifyTime = ioInfo.lastWriteTime;
                AccessTime = ioInfo.lastAccessTime;

                IsReadOnly = ioInfo.attributes.readOnly;
                IsHidden = ioInfo.attributes.hidden;
                CheckSetRowBrush(false, true);
            }

            public void CheckSetRowBrush(bool checkModify, bool checkTime)
            {
                if (checkModify && checkTime)
                {
                    bool cM = CheckModify();
                    bool cT = CheckTime();
                    if (cM && cT)
                        RowBrush = null;
                    else if (cM) // time error
                        RowBrush = new SolidColorBrush(Colors.LightYellow);
                    else if (cT) // edited
                        RowBrush = new SolidColorBrush(Colors.LightBlue);
                    else // edited, and time error
                        RowBrush = new SolidColorBrush(Colors.Orange);
                }
                else if (checkModify)
                {
                    if (CheckModify())
                        RowBrush = null;
                    else
                        RowBrush = new SolidColorBrush(Colors.LightBlue);
                }
                else if (checkTime)
                {
                    if (CheckTime())
                        RowBrush = null;
                    else
                        RowBrush = new SolidColorBrush(Colors.LightYellow);
                }
                else
                {
                    RowBrush = null;
                }
                bool CheckModify()
                {
                    if (Name != ioInfo.name)
                        return false;
                    if (CreateTime != ioInfo.creationTime)
                        return false;
                    if (ModifyTime != ioInfo.lastWriteTime)
                        return false;
                    if (AccessTime != ioInfo.lastAccessTime)
                        return false;
                    if (_IsReadOnly != ioInfo.attributes.readOnly)
                        return false;
                    if (_IsHidden != ioInfo.attributes.hidden)
                        return false;
                    return true;
                }
                bool CheckTime()
                {
                    if (ModifyTime < CreateTime)
                        return false;
                    if (AccessTime < ModifyTime)
                        return false;
                    return true;
                }
            }
            public BitmapSource Icon { set; get; }
            private string _Name;
            public string Name
            {
                set
                {
                    _Name = value;
                    RaisePropertyChanged("Name");
                }
                get => _Name;
            }
            public string NamePrefix { set; get; }
            public string Ext { set; get; }
            public string SizeNumTx { set; get; }
            public string SizeUnitTx { set; get; }

            #region datetimes, set/get datetimes, get text

            private DateTime _CreateTime;
            public DateTime CreateTime
            {
                get => _CreateTime;
                set
                {
                    _CreateTime = value;
                    CreateTimeTx = value.ToString(datetimeTxFormat);
                }
            }
            private string _CreateTimeTx;
            public string CreateTimeTx
            {
                private set
                {
                    _CreateTimeTx = value;
                    RaisePropertyChanged("CreateTimeTx");
                }
                get => _CreateTimeTx;
            }

            private DateTime _ModifyTime;
            public DateTime ModifyTime
            {
                get => _ModifyTime;
                set
                {
                    _ModifyTime = value;
                    ModifyTimeTx = value.ToString(datetimeTxFormat);
                }
            }
            private string _ModifyTimeTx;
            public string ModifyTimeTx 
            {
                private set
                {
                    _ModifyTimeTx = value;
                    RaisePropertyChanged("ModifyTimeTx");
                }
                get => _ModifyTimeTx;
            }

            private DateTime _AccessTime;


            public DateTime AccessTime
            {
                get => _AccessTime;
                set
                {
                    _AccessTime = value;
                    AccessTimeTx = value.ToString(datetimeTxFormat);
                }
            }
            private string _AccessTimeTx;
            public string AccessTimeTx
            { 
                private set
                {
                    _AccessTimeTx = value;
                    RaisePropertyChanged("AccessTimeTx");
                }
                get => _AccessTimeTx;
            }

            #endregion

            private bool _IsReadOnly = false;
            public bool IsReadOnly
            {
                set
                {
                    _IsReadOnly = value;
                    RaisePropertyChanged("IsReadOnly");
                }
                get => _IsReadOnly;
            }
            private bool _IsHidden = false;
            public bool IsHidden
            {
                set
                {
                    _IsHidden = value;
                    RaisePropertyChanged("IsHidden");
                }
                get => _IsHidden;
            }

            private SolidColorBrush _RowBrush;
            public SolidColorBrush RowBrush 
            {
                set
                {
                    _RowBrush = value;
                    RaisePropertyChanged("RowBrush");
                }
                get => _RowBrush;
            }


            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }



        #region editing name, dateTime

        private void grid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (TryStartEdit(e.Column.Header.ToString()))
            {
                e.Cancel = true;
            }
        }
        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (dataGrid.SelectedItems.Count > 0)
                {
                    TryStartEdit(dataGrid.CurrentCell.Column.Header.ToString());
                }
            }
        }
        private bool TryStartEdit(string colHeaderTx)
        {
            if (colHeaderTx.StartsWith("Name"))
            {
                StartEdit(0);
                return true;
            }
            else if (colHeaderTx.StartsWith("Create"))
            {
                StartEdit(1);
                return true;
            }
            else if (colHeaderTx.StartsWith("Modify"))
            {
                StartEdit(2);
                return true;
            }
            else if (colHeaderTx.StartsWith("Access"))
            {
                StartEdit(3);
                return true;
            }
            return false;
        }

        private int preEditingDataType = -1;
        DataGridItemViewModel firstEditItem;
        /// <summary>
        /// 开始(批量)编辑
        /// </summary>
        /// <param name="dataType">0-Name 1-CreateAt 2-ModifyAt 3-AccessAt</param>
        private void StartEdit(int dataType)
        {
            if (dataGrid.SelectedItems == null || dataGrid.SelectedItems.Count <= 0)
                return;

            preEditingDataType = dataType;
            firstEditItem = (DataGridItemViewModel)dataGrid.CurrentCell.Item;


            DataGridCell cell = null;
            FrameworkElement cellContent = dataGrid.CurrentColumn.GetCellContent(dataGrid.CurrentItem);
            if (cellContent != null)
            {
                cell = cellContent.Parent as DataGridCell;
            }
            if (cell != null)
            {
                Point pt = cell.PointToScreen(new Point());
                pt.X -= this.Left + dataGrid.Margin.Left - 6;
                pt.Y -= this.Top + dataGrid.Margin.Top + mainGrid.RowDefinitions[0].ActualHeight + 14;
                switch (dataType)
                {
                    case 0:
                        // 名称（仅限前缀），
                        tb_nameEdit.Text = firstEditItem.NamePrefix;
                        ShowFocusEditCtrl(tb_nameEdit, pt.X + 18, pt.Y, cell.ActualWidth - 14, cell.ActualHeight);
                        break;
                    case 1:
                        dtUpDown_dateTimeEdit.Value = firstEditItem.CreateTime;
                        ShowFocusEditCtrl(dtUpDown_dateTimeEdit, pt.X, pt.Y, cell.ActualWidth + 2, cell.ActualHeight);
                        break;
                    case 2:
                        dtUpDown_dateTimeEdit.Value = firstEditItem.ModifyTime;
                        ShowFocusEditCtrl(dtUpDown_dateTimeEdit, pt.X, pt.Y, cell.ActualWidth + 2, cell.ActualHeight);
                        break;
                    case 3:
                        dtUpDown_dateTimeEdit.Value = firstEditItem.AccessTime;
                        ShowFocusEditCtrl(dtUpDown_dateTimeEdit, pt.X, pt.Y, cell.ActualWidth + 2, cell.ActualHeight);
                        break;
                }
            }
            void ShowFocusEditCtrl(Control ctrl, double left, double top, double width, double height)
            {
                ctrl.Margin = new Thickness(left, top, 0, 0);
                ctrl.Width = width;
                ctrl.Height = height;
                ctrl.Visibility = Visibility.Visible;
                ctrl.Focus();
                if (ctrl is TextBox)
                {
                    TextBox tb = (TextBox)ctrl;
                    tb.SelectionStart = 0;
                    tb.SelectionLength = tb.Text.Length;
                }
                else if (ctrl is DateTimeUpDown)
                {
                    DateTimeUpDown dtud = (DateTimeUpDown)ctrl;
                    dtud.SeekSegment();
                }
            }
        }
        private void tb_nameEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            string rightName = Data.Utilities.FilePath.CorrectorName(tb_nameEdit.Text, out HashSet<char> illChars);
            if (rightName != tb_nameEdit.Text)
            {
                int s = tb_nameEdit.SelectionStart - tb_nameEdit.Text.Length + rightName.Length;
                tb_nameEdit.Text = rightName;
                if (s < 0) s = 0;
                else if (s > rightName.Length) s = rightName.Length;
                tb_nameEdit.SelectionStart = s;
            }
        }
        private void StopEdit(bool cancel)
        {
            tb_nameEdit.Visibility = Visibility.Collapsed;
            dtUpDown_dateTimeEdit.Visibility = Visibility.Collapsed;

            if (!cancel)
            {
                switch (preEditingDataType)
                {
                    case 0:
                        // check name legal -- check N' correct when changed
                        string newNamePrefix = tb_nameEdit.Text.Trim();

                        if (dataGrid.SelectedItems.Count == 1 && newNamePrefix == firstEditItem.NamePrefix)
                            break;

                        ChangeName(firstEditItem, newNamePrefix);
                        firstEditItem.CheckSetRowBrush(true, true);
                        foreach (DataGridItemViewModel vm in dataGrid.SelectedItems)
                        {
                            if (vm == firstEditItem)
                                continue;
                            ChangeName(vm, newNamePrefix);
                            vm.CheckSetRowBrush(true, true);
                        }
                        break;
                    case 1:
                        // just set times
                        foreach (DataGridItemViewModel vm in dataGrid.SelectedItems)
                        {
                            vm.CreateTime = dtUpDown_dateTimeEdit.Value;
                            vm.CheckSetRowBrush(true, true);
                        }
                        break;
                    case 2:
                        foreach (DataGridItemViewModel vm in dataGrid.SelectedItems)
                        {
                            vm.ModifyTime = dtUpDown_dateTimeEdit.Value;
                            vm.CheckSetRowBrush(true, true);
                        }
                        break;
                    case 3:
                        foreach (DataGridItemViewModel vm in dataGrid.SelectedItems)
                        {
                            vm.AccessTime = dtUpDown_dateTimeEdit.Value;
                            vm.CheckSetRowBrush(true, true);
                        }
                        break;
                }
                //.CheckSetRowBrush(true, true);
            }
            preEditingDataType = -1;
            void ChangeName(DataGridItemViewModel vm, string newNamePrefix)
            {
                if (newNamePrefix.Length == 0)
                {
                    if (!string.IsNullOrEmpty(vm.Ext))
                    {
                        vm.NamePrefix = TryGetNewNamePrefix("", vm.Ext);
                        vm.Name = vm.Ext;
                    }
                }
                else
                {
                    vm.NamePrefix = TryGetNewNamePrefix(newNamePrefix, vm.Ext);
                    vm.Name = vm.NamePrefix + vm.Ext;
                }
            }
            string TryGetNewNamePrefix(string namePrefix, string ext)
            {
                // set new name if needed
                DataGridItemViewModel found;
                int count = 0;
                string newPrefix = namePrefix;
                do
                {
                    found = dataGridItemSource.Where(x => x.Name == newPrefix + ext).FirstOrDefault();
                    if (found != null)
                    {
                        ++count;
                        newPrefix = namePrefix + (namePrefix.Length > 0 ? " " : "") + $"({count})";
                    }
                }
                while (found != null);
                return newPrefix;
            }
        }

        private void dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (preEditingDataType >= 0)
                StopEdit(false);
        }
        private void grid_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            StopEdit(false);
        }
        private void tb_nameEdit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                StopEdit(true);
            else if (e.Key == Key.Enter)
                StopEdit(false);
        }
        private void dtUpDown_dateTimeEdit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                StopEdit(true);
            else if (e.Key == Key.Enter)
                StopEdit(false);
        }
        private void tb_nameEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            StopEdit(false);
        }
        private void dtUpDown_dateTimeEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            StopEdit(false);
        }

        #endregion



        #region editing read-only, hidden

        private void cb_readOnlyAll_CheckChanged(object sender, RoutedEventArgs e)
        {
            foreach (DataGridItemViewModel vm in dataGridItemSource)
            {
                vm.IsReadOnly = cb_readOnlyAll.IsChecked == true;
            }
        }

        private void cb_hiddenAll_CheckChanged(object sender, RoutedEventArgs e)
        {
            foreach (DataGridItemViewModel vm in dataGridItemSource)
            {
                vm.IsHidden = cb_hiddenAll.IsChecked == true;
            }
        }

        #endregion


        private void btn_apply_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fi = null;
            DirectoryInfo di = null;
            FileAttributes att;
            foreach (DataGridItemViewModel vm in dataGridItemSource)
            {
                try
                {
                    #region change read-only, hidden
                    att = FileAttributes.Offline;
                    if (vm.IsReadOnly != vm.ioInfo.attributes.readOnly)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            TryInitFi(vm.ioInfo.fullName);
                            att = fi.Attributes;
                            if (vm.IsReadOnly)
                                att |= FileAttributes.ReadOnly;
                            else
                                att -= FileAttributes.ReadOnly;
                        }
                        else
                        {
                            TryInitDi(vm.ioInfo.fullName);
                            att = di.Attributes;
                            if (vm.IsReadOnly)
                                att |= FileAttributes.ReadOnly;
                            else
                                att -= FileAttributes.ReadOnly;
                        }
                    }
                    if (vm.IsHidden != vm.ioInfo.attributes.hidden)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            TryInitFi(vm.ioInfo.fullName);
                            if (att == FileAttributes.Offline)
                                att = fi.Attributes;
                            if (vm.IsHidden)
                                att |= FileAttributes.Hidden;
                            else
                                att -= FileAttributes.Hidden;
                        }
                        else
                        {
                            TryInitDi(vm.ioInfo.fullName);
                            if (att == FileAttributes.Offline)
                                att = di.Attributes;
                            if (vm.IsHidden)
                                att |= FileAttributes.Hidden;
                            else
                                att -= FileAttributes.Hidden;
                        }
                    }
                    if (att != FileAttributes.Offline)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            fi.Attributes = att;
                        }
                        else
                        {
                            di.Attributes = att;
                        }
                    }

                    #endregion

                    #region change times

                    if (vm.CreateTime != vm.ioInfo.creationTime)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            TryInitFi(vm.ioInfo.fullName);
                            fi.CreationTime = vm.CreateTime;
                        }
                        else
                        {
                            TryInitDi(vm.ioInfo.fullName);
                            di.CreationTime = vm.CreateTime;
                        }
                    }
                    if (vm.ModifyTime != vm.ioInfo.lastWriteTime)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            TryInitFi(vm.ioInfo.fullName);
                            fi.LastWriteTime = vm.ModifyTime;
                        }
                        else
                        {
                            TryInitDi(vm.ioInfo.fullName);
                            di.LastWriteTime = vm.ModifyTime;
                        }
                    }
                    if (vm.AccessTime != vm.ioInfo.lastWriteTime)
                    {
                        if (vm.ioInfo.wasFile)
                        {
                            TryInitFi(vm.ioInfo.fullName);
                            fi.LastAccessTime = vm.AccessTime;
                        }
                        else
                        {
                            TryInitDi(vm.ioInfo.fullName);
                            di.LastAccessTime = vm.AccessTime;
                        }
                    }

                    #endregion

                    if (vm.Name != vm.ioInfo.name)
                    {
                        string destName;
                        if (vm.ioInfo.wasFile)
                        {
                            destName = Path.Combine(Path.GetDirectoryName(vm.ioInfo.fullName), vm.Name);
                            File.Move(vm.ioInfo.fullName, destName);
                            fi = new FileInfo(destName);
                        }
                        else
                        {
                            destName = Path.Combine(Path.GetDirectoryName(vm.ioInfo.fullName), vm.Name);
                            Directory.Move(vm.ioInfo.fullName, destName);
                            di = new DirectoryInfo(destName);
                        }
                    }


                    if (fi != null)
                    {
                        vm.ioInfo = new Data.IOInfoShadow(fi);
                        vm.CheckSetRowBrush(true, true);
                    }
                    else if (di != null)
                    {
                        vm.ioInfo = new Data.IOInfoShadow(di);
                        vm.CheckSetRowBrush(true, true);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Error changing \"{vm.ioInfo.name}\"{Environment.NewLine}" + err.Message, "Error");
                }
                finally
                {
                    fi = null;
                    di = null;
                }
            }
            void TryInitFi(string fileFullName)
            {
                if (fi == null)
                {
                    fi = new FileInfo(fileFullName);
                }
            }
            void TryInitDi(string dirFullName)
            {
                if (di == null)
                {
                    di = new DirectoryInfo(dirFullName);
                }
            }
        }
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            btn_apply_Click(null, null);
            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
