using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for WindowHandleRest.xaml
    /// </summary>
    public partial class WindowHandleRest : Window
    {
        public WindowHandleRest()
        {
            InitializeComponent();
        }

        internal void Init(TransferManager.TransferTask transferTask, List<TransferManager.TransferTask.RestFilesData> restList)
        {
            DGVM vm;
            foreach (TransferManager.TransferTask.RestFilesData r in restList)
            {
                vm = new DGVM(transferTask, r) { ActionOptChanged = countCheck, };
                dataGridSource.Add(vm);
                if (vm.OptNewEnabled)
                {
                    ++countCheck_newMax;
                    if (vm.OptNew) ++countCheck_new;
                }
                if (vm.OptCombineOverEnabled)
                {
                    ++countCheck_combinMax;
                    if (vm.OptCombineOver) ++countCheck_combin;
                }
                if (vm.OptDeleteSourceEnabled)
                {
                    ++countCheck_delMax;
                    if (vm.OptDeleteSource) ++countCheck_del;
                }
                if (vm.OptSkipEnabled)
                {
                    ++countCheck_skipMax;
                    if (vm.OptSkip) ++countCheck_skip;
                }
            }

            dataGrid.ItemsSource = dataGridSource;
        }

        private ObservableCollection<DGVM> dataGridSource = new ObservableCollection<DGVM>();
        private class DGVM : INotifyPropertyChanged
        {
            private TransferManager.TransferTask task;
            private TransferManager.TransferTask.RestFilesData rest;
            public DGVM(TransferManager.TransferTask task, TransferManager.TransferTask.RestFilesData rest)
            {
                this.task = task;
                this.rest = rest;
            }

            public BitmapSource TaskIcon
            {
                get
                {
                    switch (task.TaskType)
                    {
                        case TransferManager.TaskTypes.Copy:
                            return StaticResource.UIIconPlus32;
                        case TransferManager.TaskTypes.Move:
                            return StaticResource.UIIconArrowRight32;
                        case TransferManager.TaskTypes.CreateLink:
                            return Common.IconHelper.Shell32Icons.Instance.GetIcon(263);
                        case TransferManager.TaskTypes.Delete:
                            return Common.IconHelper.Shell32Icons.Instance.GetIcon(133);
                    }
                    return null;
                }
                set { }
            }
            public string TaskTypeTx
            {
                get => task.TaskType.ToString();
                set { }
            }
            public BitmapSource SourceIcon
            {
                get
                {
                    if (rest.io.wasExists)
                        return Common.IconHelper.FileSystem.Instance.GetIcon(rest.io.fullName, true, !rest.io.wasFile);
                    else
                        return null;
                }
                set { }
            }
            public string SourceTx
            {
                get
                {
                    return rest.io.fullName;
                }
                set { }
            }

            public BitmapSource TargetIcon
            {
                get
                {
                    if (System.IO.File.Exists(TargetTx))
                        return Common.IconHelper.FileSystem.Instance.GetIcon(rest.io.fullName, true, false);
                    else if (System.IO.Directory.Exists(TargetTx))
                        return Common.IconHelper.FileSystem.Instance.GetIcon(rest.io.fullName, true, true);
                    else
                        return null;
                }
                set { }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string _TargetTx = "0";
            public string TargetTx
            {
                get
                {
                    if (_TargetTx == "0")
                    {
                        switch (task.TaskType)
                        {
                            default:
                            case TransferManager.TaskTypes.Delete:
                                _TargetTx = null;
                                break;
                            case TransferManager.TaskTypes.Copy:
                                if (rest.newName == null)
                                {
                                    _TargetTx = rest.targetFullName;
                                }
                                else
                                {
                                    _TargetTx = System.IO.Path.Combine(
                                        System.IO.Path.GetDirectoryName(rest.targetFullName),
                                        rest.newName);
                                }
                                break;
                            case TransferManager.TaskTypes.Move:
                                _TargetTx = rest.tar;
                                break;
                            case TransferManager.TaskTypes.CreateLink:
                                _TargetTx = rest.targetFullName;
                                break;
                        }
                        if (string.IsNullOrWhiteSpace(_TargetTx))
                            TargetTxBtnOpacity = 0.001;
                    }
                    return _TargetTx;
                }
                set { }
            }
            public double TargetTxBtnOpacity
            {
                get; set;
            } = 1d;

            public string SizeATx
            {
                get => Common.SimpleStringHelper.UnitsOfMeasure.GetShortString(rest.io.length, "B", 1024);
                set { }
            }
            private long? _SizeB = -1;
            public long? SizeB
            {
                get
                {
                    if (_SizeB < 0)
                    {
                        LoadBInfo();
                    }
                    return _SizeB;
                }
            }
            public DateTime? _MTimeB = DateTime.MinValue;
            public DateTime? MTimeB
            {
                get
                {
                    if (_MTimeB == DateTime.MinValue)
                    {
                        LoadBInfo();
                    }
                    return _MTimeB;
                }
            }
            private void LoadBInfo()
            {
                if (System.IO.File.Exists(TargetTx))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(TargetTx);
                    _SizeB = fi.Length;
                    _MTimeB = fi.LastWriteTime;
                }
                else
                {
                    _SizeB = null;
                    _MTimeB = null;
                }
            }
            public string SizeBTx
            {
                get
                {
                    if (SizeB != null && SizeB >= 0)
                        return Common.SimpleStringHelper.UnitsOfMeasure.GetShortString((long)SizeB, "B", 1024);
                    else
                        return "---";
                }
                set { }
            }
            public FontWeight SizeATxFW
            {
                get
                {
                    if (SizeB >= 0 && rest.io.length > SizeB)
                        return FontWeights.Bold;
                    else
                        return FontWeights.Normal;
                }
                set { }
            }
            public FontWeight SizeBTxFW
            {
                get
                {
                    if (SizeB >= 0 && rest.io.length < SizeB)
                        return FontWeights.Bold;
                    else
                        return FontWeights.Normal;
                }
                set { }
            }
            private static string FormatTx_datetime = "yyyy-MM-dd HH:mm:ss";
            public string MTimeATx
            {
                get => rest.io.lastWriteTime.ToString(FormatTx_datetime);
                set { }
            }
            public string MTimeBTx
            {
                get
                {
                    if (MTimeB == null)
                    {
                        return "---";
                    }
                    else
                    {
                        return ((DateTime)MTimeB).ToString(FormatTx_datetime);
                    }
                }
                set { }
            }
            public FontWeight MTimeATxFW
            {
                get
                {
                    if (MTimeB == null)
                    {
                        return FontWeights.Normal;
                    }
                    else
                    {
                        if ((DateTime)MTimeB < rest.io.lastWriteTime)
                            return FontWeights.Bold;
                        else
                            return FontWeights.Normal;
                    }
                }
                set { }
            }
            public FontWeight MTimeBTxFW
            {
                get
                {
                    if (MTimeB == null)
                    {
                        return FontWeights.Normal;
                    }
                    else
                    {
                        if (rest.io.lastWriteTime < (DateTime)MTimeB)
                            return FontWeights.Bold;
                        else
                            return FontWeights.Normal;
                    }
                }
                set { }
            }


            public string ProblemTx
            {
                get
                {
                    if (rest.err != null)
                    {
                        return rest.err.Message;
                    }
                    else
                    {
                        switch (task.TaskType)
                        {
                            default:
                            case TransferManager.TaskTypes.Delete:
                                break;
                            case TransferManager.TaskTypes.Copy:
                            case TransferManager.TaskTypes.Move:
                            case TransferManager.TaskTypes.CreateLink:
                                if (System.IO.File.Exists(TargetTx))
                                    return $"目标 文件 已经存在";
                                else if (System.IO.Directory.Exists(TargetTx))
                                    return $"目标 文件夹 已经存在";
                                break;
                        }

                        return "未知问题";
                    }
                }
                set { }
            }

            public bool OptNewEnabled
            {
                get
                {
                    switch (task.TaskType)
                    {
                        case TransferManager.TaskTypes.Copy:
                        case TransferManager.TaskTypes.Move:
                        case TransferManager.TaskTypes.CreateLink:
                            return true;
                        default:
                        case TransferManager.TaskTypes.Delete:
                            return false;
                    }
                }
                set { }
            }
            public bool OptNew
            {
                get
                {
                    return rest.restFilesHandleType == TransferManager.TransferTask.RestFilesHandleTypes.Rename;
                }
                set
                {
                    if (value)
                    {
                        OptChangedCheck(OptIdxCurChecked(), 0);
                        rest.restFilesHandleType = TransferManager.TransferTask.RestFilesHandleTypes.Rename;
                        RaiseOptionsChanged();
                    }
                }
            }
            private void RaiseOptionsChanged()
            {
                RaisePropertyChangedEvent("OptNew");
                RaisePropertyChangedEvent("OptCombineOver");
                RaisePropertyChangedEvent("OptDeleteSource");
                RaisePropertyChangedEvent("OptSkip");
            }
            private int OptIdxCurChecked()
            {
                if (OptNew) return 0;
                else if (OptCombineOver) return 1;
                else if (OptDeleteSource) return 2;
                else return 3;
            }

            public Action<int, bool> ActionOptChanged;
            private void OptChangedCheck(int preTreeOpt, int curTreeOpt)
            {
                if (ActionOptChanged != null)
                {
                    ActionOptChanged.Invoke(preTreeOpt, false);
                    ActionOptChanged.Invoke(curTreeOpt, true);
                }
            }
            private bool? _OptCombineOverEnabled = null;
            public bool OptCombineOverEnabled
            {
                get
                {
                    if (_OptCombineOverEnabled == null)
                    {
                        switch (task.TaskType)
                        {
                            case TransferManager.TaskTypes.Copy:
                                if (System.IO.Path.GetDirectoryName(rest.io.fullName)
                                    == System.IO.Path.GetDirectoryName(rest.targetFullName))
                                {
                                    if (rest.newName != null && rest.newName == rest.io.name)
                                        return false;
                                    else
                                        return true;
                                }
                                else
                                {
                                    return true;
                                }
                            case TransferManager.TaskTypes.Move:
                                if (rest.sur == rest.tar)
                                    return false;
                                else
                                    return true;
                            case TransferManager.TaskTypes.CreateLink:
                                return true;
                            default:
                            case TransferManager.TaskTypes.Delete:
                                return false;
                        }
                    }
                    return _OptCombineOverEnabled == true;
                }
                set { }
            }
            public bool OptCombineOver
            {
                get
                {
                    return rest.restFilesHandleType == TransferManager.TransferTask.RestFilesHandleTypes.Overwrite
                        || rest.restFilesHandleType == TransferManager.TransferTask.RestFilesHandleTypes.combine;
                }
                set
                {
                    if (value)
                    {
                        OptChangedCheck(OptIdxCurChecked(), 1);
                        if (rest.io.wasFile)
                            rest.restFilesHandleType = TransferManager.TransferTask.RestFilesHandleTypes.Overwrite;
                        else
                            rest.restFilesHandleType = TransferManager.TransferTask.RestFilesHandleTypes.combine;
                        RaiseOptionsChanged();
                    }
                }
            }

            private bool? _OptDeleteSourceEnabled = null;
            public bool OptDeleteSourceEnabled
            {
                get
                {
                    if (_OptDeleteSourceEnabled == null)
                    {
                        switch (task.TaskType)
                        {
                            case TransferManager.TaskTypes.Move:
                                _OptDeleteSourceEnabled = true;
                                break;
                            default:
                            case TransferManager.TaskTypes.Copy:
                            case TransferManager.TaskTypes.CreateLink:
                            case TransferManager.TaskTypes.Delete:
                                _OptDeleteSourceEnabled = false;
                                break;
                        }
                    }
                    return _OptDeleteSourceEnabled == true;
                }
                set { }
            }
            public bool OptDeleteSource
            {
                get
                {
                    return rest.restFilesHandleType == TransferManager.TransferTask.RestFilesHandleTypes.SelfDelete;
                }
                set
                {
                    if (value)
                    {
                        OptChangedCheck(OptIdxCurChecked(), 2);
                        rest.restFilesHandleType = TransferManager.TransferTask.RestFilesHandleTypes.SelfDelete;
                        RaiseOptionsChanged();
                    }
                }
            }
            public bool OptSkipEnabled
            {
                get
                {
                    return true;
                }
                set { }
            }
            public bool OptSkip
            {
                get
                {
                    return rest.restFilesHandleType == TransferManager.TransferTask.RestFilesHandleTypes.Skip;
                }
                set
                {
                    if (value)
                    {
                        OptChangedCheck(OptIdxCurChecked(), 3);
                        rest.restFilesHandleType = TransferManager.TransferTask.RestFilesHandleTypes.Skip;
                        RaiseOptionsChanged();
                    }
                }
            }
        }


        #region click source, click target

        private void btn_source_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null)
            {
                Process.Start("Explorer.exe", "/select," + dataGrid_currentItem.SourceTx);
            }
        }

        private void btn_target_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null)
            {
                string target = dataGrid_currentItem.TargetTx;
                if (!string.IsNullOrWhiteSpace(target))
                {
                    if (System.IO.File.Exists(target) || System.IO.Directory.Exists(target))
                        Process.Start("Explorer.exe", "/select," + dataGrid_currentItem.SourceTx);
                    else
                        MessageBox.Show("File or directory not exists.", "No file", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
        }

        #endregion


        #region checkBox header check changed

        private int countCheck_newMax, countCheck_new;
        private int countCheck_combinMax, countCheck_combin;
        private int countCheck_delMax, countCheck_del;
        private int countCheck_skipMax, countCheck_skip;

        public void countCheck(int optIdx, bool isIncrease)
        {
            switch (optIdx)
            {
                case 0:
                    countCheck_new += isIncrease ? 1 : -1; break;
                case 1:
                    countCheck_combin += isIncrease ? 1 : -1; break;
                case 2:
                    countCheck_del += isIncrease ? 1 : -1; break;
                case 3:
                    countCheck_skip += isIncrease ? 1 : -1; break;
            }

            CheckOptAll(ref countCheck_newMax, ref countCheck_new, cb_newNameAll);
            CheckOptAll(ref countCheck_combinMax, ref countCheck_combin, cb_overWriteAll);
            CheckOptAll(ref countCheck_delMax, ref countCheck_del, cb_deleteSourceAll);
            CheckOptAll(ref countCheck_skipMax, ref countCheck_skip, cb_skipAll);

            void CheckOptAll(ref int max, ref int cur, CheckBox cb)
            {
                if (cur == 0)
                    cb.IsChecked = false;
                else if (cur == max)
                    cb.IsChecked = true;
            }
        }


        private void cb_newNameAll_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            // as mouse up? check state already changed
            bool cbChecked = cb_newNameAll.IsChecked == true;

            foreach (DGVM vm in dataGridSource)
            {
                if (vm.OptNewEnabled && !vm.OptNew)
                {
                    vm.OptNew = cbChecked;
                }
            }
        }
        private void cb_overWriteAll_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            bool cbChecked = cb_overWriteAll.IsChecked == true;
            foreach (DGVM vm in dataGridSource)
            {
                if (vm.OptCombineOverEnabled && !vm.OptCombineOver)
                {
                    vm.OptCombineOver = cbChecked;
                }
            }
        }


        private void cb_deleteSourceAll_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            bool cbChecked = cb_deleteSourceAll.IsChecked == true;
            foreach (DGVM vm in dataGridSource)
            {
                if (vm.OptDeleteSourceEnabled && !vm.OptDeleteSource)
                {
                    vm.OptDeleteSource = cbChecked;
                }
            }
        }
        private void cb_skipAll_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            bool cbChecked = cb_skipAll.IsChecked == true;
            foreach (DGVM vm in dataGridSource)
            {
                if (vm.OptSkipEnabled && !vm.OptSkip)
                {
                    vm.OptSkip = cbChecked;
                }
            }
        }


        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null && dataGrid_currentItem.OptNewEnabled)
            {
                dataGrid_currentItem.OptNew = true;
            }
        }
        private void btn_combine_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null && dataGrid_currentItem.OptCombineOverEnabled)
            {
                dataGrid_currentItem.OptCombineOver = true;
            }
        }
        private void btn_deleteSource_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null && dataGrid_currentItem.OptDeleteSourceEnabled)
            {
                dataGrid_currentItem.OptDeleteSource = true;
            }
        }
        private void btn_skip_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_GetCurrentItem() != null && dataGrid_currentItem.OptSkipEnabled)
            {
                dataGrid_currentItem.OptSkip = true;
            }
        }
        private DGVM dataGrid_currentItem;


        private DGVM dataGrid_GetCurrentItem()
        {
            if (dataGrid.CurrentCell != null)
            {
                if (dataGrid.CurrentCell.Item is DGVM)
                    dataGrid_currentItem = (DGVM)dataGrid.CurrentCell.Item;
                else
                    dataGrid_currentItem = null;
            }
            else
            {
                dataGrid_currentItem = null;
            }
            return dataGrid_currentItem;
        }

        #endregion



        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            flagCancel = false;
            this.Close();
        }
        private bool flagCancel = true;
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (flagCancel)
            {
                foreach (DGVM vm in dataGridSource)
                {
                    vm.OptSkip = true;
                }
            }
        }
    }
}
