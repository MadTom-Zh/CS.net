using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using MadTomDev.Data;
using MadTomDev.Common;
using System.Threading;
using System.ComponentModel;

namespace MadTomDev.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (combo_secCode.Items.Count == 0)
                combo_secCode.Items.Add("");
            combo_secCode.Items[0] = ((MenuItem)combo_secCode.ContextMenu.Items[0]).Name.Replace("_", "-");
            combo_secCode.SelectedItem = combo_secCode.Items[0];

            cb_all.IsChecked = true;
        }

        #region data define
        public class DataGridItem : INotifyPropertyChanged
        {
            public IOInfoShadow ioInfo;
            public TextFileDecodeHelper.FileCodeInfo fcInfo;
            private UpdateStatus _status;
            public UpdateStatus status
            {
                get => _status;
                set
                {
                    _status = value;
                    StatusTx = _status.ToString();
                }
            }
            public string File { set; get; }
            public string Suffix { set; get; }
            public string SizeTx { set; get; }
            public string Code { set; get; }
            public double ConfV { set; get; }
            public string ConfT { set; get; }
            private bool _ToUpdate;
            public bool ToUpdate
            {
                set
                {
                    if (_ToUpdate == value)
                        return;
                    _ToUpdate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ToUpdate"));
                }
                get => _ToUpdate;
            }
            private string _StatusTx;
            public string StatusTx
            {
                set
                {
                    if (_StatusTx == value)
                        return;
                    _StatusTx = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusTx"));
                }
                get => _StatusTx;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
        public enum UpdateStatus
        { Idle, Updating, Error, Complete, }
        #endregion


        #region delete row, change update switch
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            DataGridItem item = (DataGridItem)e.Row.Item;
            item.ToUpdate = !item.ToUpdate;
            e.Row.Item = item;
            e.Cancel = true;
        }
        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    DataGridItem item;
                    for (int i = dataGrid.SelectedItems.Count - 1; i >= 0; i--)
                    {
                        item = (DataGridItem)dataGrid.SelectedItems[i];
                        dataGrid.Items.Remove(item);
                        fullNameList.Remove(item.ioInfo.fullName);
                    }
                    break;
                case Key.A:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        dataGrid.SelectAll();
                    }
                    break;
            }
        }
        #endregion


        #region ui automation
        private async void cb_all_Checked(object sender, RoutedEventArgs e)
        {
            await WaitUIReady();
            bool all = cb_all.IsChecked == true;
            cb_srt.IsEnabled = !all;
            cb_ass.IsEnabled = !all;
        }
        private async void cb_limitSize_Checked(object sender, RoutedEventArgs e)
        {
            await WaitUIReady();
            num_size.IsEnabled = cb_limitSize.IsChecked == true;
        }
        private Task WaitUIReady()
        {
            return Task.Run(() =>
            {
                bool notAllReady;
                do
                {
                    notAllReady = false;
                    if (num_size == null)
                        notAllReady = true;
                    else if (cb_all == null)
                        notAllReady = true;
                    else if (cb_srt == null)
                        notAllReady = true;
                    else if (cb_ass == null)
                        notAllReady = true;
                    else if (cb_limitSize == null)
                        notAllReady = true;
                    else if (cb_loopSubs == null)
                        notAllReady = true;
                    else if (dataGrid == null)
                        notAllReady = true;
                    Thread.Sleep(5);
                }
                while (notAllReady);
            });
        }
        #endregion


        #region drag N drop


        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            fullNameList.Clear();
            dataGrid.Items.Clear();
        }


        private bool bdr_dropZone_canDrop = false;
        private DragDropEffects bdr_dropZone_overEffect;
        private string[] bdr_dropZone_filesNDirs;
        private void bdr_dropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bdr_dropZone_filesNDirs = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effects = DragDropEffects.Link;
                bdr_dropZone_overEffect = DragDropEffects.Link;
                bdr_dropZone_canDrop = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                bdr_dropZone_overEffect = DragDropEffects.None;
                bdr_dropZone_canDrop = false;
            }
            e.Handled = true;
        }
        private void bdr_dropZone_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = bdr_dropZone_overEffect;
            e.Handled = true;
        }

        private async void bdr_dropZone_Drop(object sender, DragEventArgs e)
        {
            if (bdr_dropZone_canDrop)
            {
                this.Cursor = Cursors.AppStarting;
                if (txChecker == null)
                    txChecker = new SimpleStringHelper.Checker_starNQues();
                txChecker.ClearPatterns();
                txChecker.AddPattern("*");
                if (cb_all.IsChecked != true)
                {
                    txChecker.ClearPatterns();
                    if (cb_srt.IsChecked == true)
                    {
                        txChecker.AddPattern("*.srt");
                    }
                    if (cb_ass.IsChecked == true)
                    {
                        txChecker.AddPattern("*.ass");
                    }
                }
                if (!txChecker.HavePattern)
                {
                    MessageBox.Show("请选择至少一个后缀");
                    return;
                }
                bool isLimitSize = cb_limitSize.IsChecked == true;
                long sizeLimit = (long)(1048576 * num_size.Value);
                bool loop = cb_loopSubs.IsChecked == true;
                await Task.Run(() =>
                {
                    List<FileInfo> files = new List<FileInfo>();
                    List<DirectoryInfo> dirs = new List<DirectoryInfo>();
                    foreach (string item in bdr_dropZone_filesNDirs)
                    {
                        if (File.Exists(item))
                            files.Add(new FileInfo(item));
                        else if (Directory.Exists(item))
                            dirs.Add(new DirectoryInfo(item));
                    }
                    bdr_dropZone_recLoop(files, dirs, isLimitSize, sizeLimit, loop);
                });
                this.Cursor = Cursors.Arrow;
            }
        }
        private SimpleStringHelper.Checker_starNQues txChecker;
        private HashSet<string> fullNameList = new HashSet<string>();
        private double ConfThreshold = 0.9;
        private void bdr_dropZone_recLoop(
            List<FileInfo> files, List<DirectoryInfo> dirs,
            bool isLimitSize, long sizeLimit,
            bool loopSubs)
        {
            foreach (FileInfo fi in files)
            {
                if (fullNameList.Contains(fi.FullName))
                    continue;
                if (!txChecker.Check(fi.Name))
                    continue;
                if (isLimitSize && fi.Length > sizeLimit)
                    continue;

                TextFileDecodeHelper.FileCodeInfo fcInfo = TextFileDecodeHelper.AnalyzeFile(fi.FullName);
                UpdateStatus status = UpdateStatus.Idle;

                fullNameList.Add(fi.FullName);
                Dispatcher.Invoke(() =>
                {
                    dataGrid.Items.Add(new DataGridItem()
                    {
                        ioInfo = new IOInfoShadow(fi),
                        fcInfo = fcInfo,
                        status = status,
                        StatusTx = status.ToString(),
                        File = fi.Name,
                        Suffix = fi.Extension,
                        SizeTx = SimpleStringHelper.UnitsOfMeasure.GetShortString(fi.Length, "B", 1024),
                        Code = fcInfo.EncodingName,
                        ConfV = fcInfo.Confidence,
                        ConfT = fcInfo.Confidence.ToString("P"),
                        ToUpdate = fcInfo.Confidence > ConfThreshold && fcInfo.EncodingName.ToLower() != "utf-8",
                    });
                });
            }
            if (loopSubs)
            {
                List<FileInfo> subFiles;
                List<DirectoryInfo> subDirs;
                foreach (DirectoryInfo di in dirs)
                {
                    subFiles = new List<FileInfo>();
                    subDirs = new List<DirectoryInfo>();
                    subFiles.AddRange(di.GetFiles());
                    subDirs.AddRange(di.GetDirectories());
                    bdr_dropZone_recLoop(subFiles, subDirs, isLimitSize, sizeLimit, loopSubs);
                }
            }
        }

        #endregion


        private bool isWorking = false;
        private bool bakcup = false;
        private bool cancelFlag = false;
        private async void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (isWorking)
            {
                cancelFlag = true;
            }
            else
            {
                this.Cursor = Cursors.AppStarting;
                btn_start.Content = "停止";
                isWorking = true;
                bakcup = cb_backup.IsChecked == true;
                srtItems.Clear();
                foreach (object i in dataGrid.Items)
                {
                    srtItems.Add((DataGridItem)i);
                }
                secCoding = (string)combo_secCode.SelectedItem;
                await Task.Run(Working);
                btn_start.Content = "开始";
                this.Cursor = Cursors.Arrow;
            }
        }
        private List<DataGridItem> srtItems = new List<DataGridItem>();
        private string secCoding;
        private void Working()
        {
            Encoding tarEncoding;
            foreach (DataGridItem i in srtItems)
            {
                if (cancelFlag)
                {
                    MessageBox.Show("已取消");
                    break;
                }
                if (!i.ToUpdate)
                    continue;

                i.status = UpdateStatus.Updating;
                if (bakcup)
                    File.Copy(i.ioInfo.fullName, i.ioInfo.fullName + ".bak");
                try
                {
                    tarEncoding = i.fcInfo.Encoding;
                    if (i.ConfV < ConfThreshold || i.fcInfo.Encoding == null)
                    {
                        tarEncoding = Encoding.GetEncoding(secCoding);
                    }
                    File.WriteAllText(
                        i.ioInfo.fullName,
                        File.ReadAllText(i.ioInfo.fullName, tarEncoding),
                        Encoding.UTF8);
                    i.status = UpdateStatus.Complete;
                }
                catch (Exception)// err)
                {
                    i.status = UpdateStatus.Error;
                }
            }
            isWorking = false;
            if (!cancelFlag)
            {
                MessageBox.Show("已完成");
            }
            cancelFlag = false;
        }

        private void combo_secCode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                combo_secCode.ContextMenu.IsOpen = true;
            }
        }
        private void secCodeItemSelected(object sender, RoutedEventArgs e)
        {
            MenuItem mItem = (MenuItem)sender;
            if (mItem != null)
            {
                combo_secCode.Items[0] = mItem.Name.Replace("_", "-");
                combo_secCode.SelectedItem = combo_secCode.Items[0];
            }
        }

    }
}
