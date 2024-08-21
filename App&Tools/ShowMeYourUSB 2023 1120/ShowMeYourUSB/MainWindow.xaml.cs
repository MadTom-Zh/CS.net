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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        NotifyIcon notifyIcon;
        private void Window_Initialized(object sender, EventArgs e)
        {
            if (logger == null)
            {
                logger = new Common.Logger()
                {
                    BaseDir = Common.Variables.IOPath.LogDir,
                    BaseFileNamePre = "SMYU",
                    IncrementType = Common.Logger.IncrementTypeEnums.TimeHour,
                };
            }


            cb_init();
            drives_init();
            notifyIcon_init();

        }

        #region notify icon

        public WindowState[] windowStates = new WindowState[2];

        private void Window_StateChanged(object sender, EventArgs e)
        {
            windowStates[0] = windowStates[1];
            windowStates[1] = this.WindowState;

            if (setting.IsWndMiniHideTaskBtn && this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
        private void notifyIcon_init()
        {
            if (notifyIcon == null)
            {
                notifyIcon = NotifyIcon.Create();
                notifyIcon.Icon = iconSet.GetIconNotify();
                notifyIcon.DoubleClick += (s1, e1) =>
                {
                    ShowActivateWindow();
                };
                ContextMenuStrip niCMenu = notifyIcon.ContextMenuStrip;
                NotifyIconCommand notifyIconCMCmd = new NotifyIconCommand();
                niCMenu.Items.Add(new ContextMenuStrip.MenuItem()
                {
                    Text = "显示窗口",
                    Command = notifyIconCMCmd,
                    CommandParameter = new object[] { this, "showWindow" },
                });
                //niCMenu.Items.Add(new ContextMenuStrip.MenuItem());//new Separator()
                niCMenu.Items.Add(new ContextMenuStrip.MenuItem()
                {
                    Text = "退出",
                    Command = notifyIconCMCmd,
                    CommandParameter = new object[] { this, "exit" },
                });
            }
        }
        public class NotifyIconCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            MainWindow window;
            string p;
            private bool TryGetPars(object parameter)
            {
                if (parameter is object[])
                {
                    object[] pars = (object[])parameter;
                    MainWindow window;
                    if (pars.Length > 0 && pars[0] is MainWindow)
                    {
                        window = (MainWindow)pars[0];
                        if (pars.Length > 1 && pars[1] is string)
                        {
                            this.window = window;
                            this.p = (string)pars[1];
                            return true;
                        }
                        else return false;
                    }
                    else return false;
                }
                return false;
            }
            public bool CanExecute(object parameter)
            {
                if (TryGetPars(parameter))
                {
                    if (p == "showWindow")
                    {
                        if (window.WindowState == WindowState.Minimized)
                            return true;
                        if (window.Visibility == Visibility.Visible)
                            return false;
                        else
                            return true;
                    }
                    else  //  exit
                    {
                        return true;
                    }
                }
                //CanExecuteChanged?.Invoke(this, null);
                return false;
            }

            public void Execute(object parameter)
            {
                if (TryGetPars(parameter))
                {
                    if (p == "showWindow")
                    {
                        window.ShowActivateWindow();
                    }
                    else  //  exit
                    {
                        window.Close();
                    }
                }
            }
        }
        public void ShowActivateWindow()
        {
            if (WindowState != WindowState.Minimized)
                WindowState = WindowState.Minimized;

            Show();
            WindowState = windowStates[0];
            Activate();
        }
        #endregion

        #region basic settings

        Setting setting = Setting.GetInstance();
        private bool isIniting = true;
        private void cb_init()
        {
            cb_hideTrayIcon.IsChecked = setting.IsListeningHideTray;
            cb_hideTaskBtn.IsChecked = setting.IsWndMiniHideTaskBtn;
            cb_logPluged.IsChecked = setting.IsLogPluged;
            cb_copyFileTree.IsChecked = setting.IsLogFileTree;
            cb_copyFiles.IsChecked = setting.IsCopyFiles;

            isIniting = false;
        }
        private void cb_hideTrayIcon_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting)
                return;

            setting.IsListeningHideTray = (bool)cb_hideTrayIcon.IsChecked;
        }
        private void cb_hideTaskBtn_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting)
                return;

            setting.IsWndMiniHideTaskBtn = (bool)cb_hideTaskBtn.IsChecked;
        }

        private void cb_logPluged_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting)
                return;

            setting.IsLogPluged = (bool)cb_logPluged.IsChecked;
            cb_checkChain();
        }

        private void cb_copyFileTree_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting)
                return;

            setting.IsLogFileTree = (bool)cb_copyFileTree.IsChecked;
            cb_checkChain();
        }

        private void cb_copyFiles_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (isIniting)
                return;

            setting.IsCopyFiles = (bool)cb_copyFiles.IsChecked;
        }
        private void cb_checkChain()
        {
            if (cb_logPluged.IsChecked == true)
            {
                cb_copyFileTree.IsEnabled = true;
                if (cb_copyFileTree.IsChecked == true)
                {
                    cb_copyFiles.IsEnabled = true;
                }
                else
                {
                    cb_copyFiles.IsEnabled = false;
                }

                btn_startListening.IsEnabled = true;
            }
            else
            {
                cb_copyFileTree.IsEnabled = false;
                cb_copyFiles.IsEnabled = false;

                btn_startListening.IsEnabled = false;
            }
        }

        #endregion


        #region init drive list, drive plug listening, !! trigger logging

        Resource.DriveDetector dd = Resource.DriveDetector.GetInstance();
        IconSet iconSet = IconSet.GetInstance();

        ObservableCollection<DGCurDriveItem> curDriveList = new();
        public class DGCurDriveItem : INotifyPropertyChanged
        {
            #region properties

            private string _name;
            public string name
            {
                set
                {
                    _name = value;
                    RaisePropertyChangedEvent("name");
                }
                get => _name;
            }
            private BitmapSource _icon;
            public BitmapSource icon
            {
                set
                {
                    _icon = value;
                    RaisePropertyChangedEvent("icon");
                }
                get => _icon;
            }
            private double _usageRateV;
            public double usageRateV
            {
                set
                {
                    _usageRateV = value;
                    RaisePropertyChangedEvent("usageRateV");
                }
                get => _usageRateV;
            }
            private string _usageRateTx;
            public string usageRateTx
            {
                set
                {
                    _usageRateTx = value;
                    RaisePropertyChangedEvent("usageRateTx");
                }
                get => _usageRateTx;
            }
            public Guid guid
            {
                set;
                get;
            }

            #endregion

            public Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo;

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public DGCurDriveItem(Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo)
            {
                RefreshRate(advDriveInfo);
            }
            public void RefreshRate(Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo)
            {
                this.advDriveInfo = advDriveInfo;
                IconSet iconSet = IconSet.GetInstance();
                if (advDriveInfo == null)
                {
                    name = null;
                    icon = iconSet.IconDriveOffline;
                    usageRateV = 0;
                    usageRateTx = "--";
                    guid = Guid.Empty;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(advDriveInfo.driveInfo.volumeLabel))
                        name = $"({advDriveInfo.driveInfo.name})";
                    else
                        name = $"{advDriveInfo.driveInfo.volumeLabel} ({advDriveInfo.driveInfo.name})";
                    icon = iconSet.GetDriveIcon(advDriveInfo.driveInfo.name);
                    usageRateV = (double)(advDriveInfo.driveInfo.totalSize - advDriveInfo.driveInfo.availableFreeSpace) / advDriveInfo.driveInfo.totalSize;
                    usageRateTx = usageRateV == 0 ? "0 %" : usageRateV.ToString("P2");
                    guid = advDriveInfo.DeviceId;
                }
            }
        }

        private void drives_init()
        {
            Data.AdvancedDriveInfoHelper advDIHelper = Data.AdvancedDriveInfoHelper.GetInstance();
            DriveInfo[] curDrives = DriveInfo.GetDrives();
            foreach (DriveInfo di in curDrives)
            {
                if (di.IsReady)
                {
                    curDriveList.Add(new DGCurDriveItem(
                        advDIHelper.GetAdvDriveInfo(di.Name[0])));
                }
            }
            dd.DrivePluged += Dd_DrivePluged;

            dg_currentDrives.ItemsSource = curDriveList;

            Data.AdvancedDriveInfoHelper.AdvDriveInfo foundAdvDI;
            foreach (Guid id in setting.ExcludingList)
            {
                foundAdvDI = advDIHelper.GetAdvDriveInfo_byDeviceId(id);
                if (foundAdvDI == null)
                {
                    excludingDriveList.Add(new DGExcludingDriveItem("?:\\", false, id));
                }
                else
                {
                    excludingDriveList.Add(new DGExcludingDriveItem(foundAdvDI));
                }
            }
            dg_excludingDrives.ItemsSource = excludingDriveList;
        }
        private void dg_currentDrives_GotFocus(object sender, RoutedEventArgs e)
        {
            // refresh usage rate
            DriveInfo[] curDrives = DriveInfo.GetDrives();
            Data.AdvancedDriveInfoHelper.AdvDriveInfo sample;
            DGCurDriveItem curDGCDI;
            int i, iv = curDriveList.Count;
            foreach (DriveInfo di in curDrives)
            {
                if (di.IsReady)
                {
                    sample = Data.AdvancedDriveInfoHelper.GetInstance().GetAdvDriveInfo(di.Name[0]);
                    for (i = 0; i < iv; i++)
                    {
                        curDGCDI = curDriveList[i];
                        if (curDGCDI.advDriveInfo.DeviceId == sample.DeviceId)
                        {
                            curDGCDI.RefreshRate(sample);
                        }
                    }
                }
            }
        }

        Data.AdvancedDriveInfoHelper advDriveInfoHelper = Data.AdvancedDriveInfoHelper.GetInstance();
        private void Dd_DrivePluged(char driveLetter, bool plugedInOrOut)
        {
            if (plugedInOrOut)
            {
                advDriveInfoHelper.Refresh();
                Data.AdvancedDriveInfoHelper.AdvDriveInfo advDI = advDriveInfoHelper.GetAdvDriveInfo(driveLetter);
                if (advDI == null)
                    return;

                bool isNotExcluded = true;
                Dispatcher.Invoke(() =>
                {
                    curDriveList.Add(new DGCurDriveItem(advDI));

                    foreach (DGExcludingDriveItem dgEDI in excludingDriveList)
                    {
                        if (dgEDI.guid == advDI.DeviceId)
                        {
                            dgEDI.SetOnline(advDI);
                            isNotExcluded = false;
                            break;
                        }
                    }
                });

                if (isNotExcluded && isListening)
                {
                    SetPlugInCountInfo(true);
                    StartLoggingNCopyingAsync(driveLetter, plugedInOrOut);
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    logger.Log($"USB [{driveLetter}:] Plug out, at {GetTimeNowFullString()}");

                    DGCurDriveItem curDGCDI;
                    int i, iv = curDriveList.Count;
                    for (i = 0; i < iv; i++)
                    {
                        curDGCDI = curDriveList[i];
                        if (curDGCDI.advDriveInfo.VolumeLetter == driveLetter)
                        {
                            curDriveList.Remove(curDGCDI);
                            break;
                        }
                    }

                    DGExcludingDriveItem excludingDGCDI;
                    iv = excludingDriveList.Count;
                    for (i = 0; i < iv; i++)
                    {
                        excludingDGCDI = excludingDriveList[i];
                        if (excludingDGCDI.isOnLine && excludingDGCDI.advDriveInfo.VolumeLetter == driveLetter)
                        {
                            excludingDGCDI.SetOffLine();
                            break;
                        }
                    }
                });

                if (workingDriveLetterList.Contains(driveLetter))
                    workingDriveLetterList.Remove(driveLetter);
            }
        }

        public int PlugInCount = 0;
        private void SetPlugInCountInfo(bool isPlusOne)
        {
            Dispatcher.Invoke(() =>
            {
                if (isPlusOne)
                    PlugInCount++;
                lb_plugInCount.Content = PlugInCount;

                if (notifyIcon != null)
                {
                    if (PlugInCount == 0)
                    {
                        notifyIcon.Icon = iconSet.GetIconNotify();
                    }
                    else
                    {
                        notifyIcon.Icon = iconSet.GetIconNotifyWithNum(PlugInCount);
                    }
                }
            });
        }

        private void btn_plugInCount_reset_Click(object sender, RoutedEventArgs e)
        {
            PlugInCount = 0;
            SetPlugInCountInfo(false);
        }

        #endregion


        #region excluding drives

        ObservableCollection<DGExcludingDriveItem> excludingDriveList = new();
        public class DGExcludingDriveItem : INotifyPropertyChanged
        {
            #region properties

            private string _name;
            public string name
            {
                set
                {
                    _name = value;
                    RaisePropertyChangedEvent("name");
                }
                get => _name;
            }
            private BitmapSource _icon;
            public BitmapSource icon
            {
                set
                {
                    _icon = value;
                    RaisePropertyChangedEvent("icon");
                }
                get => _icon;
            }
            private bool _isOnLine;
            public bool isOnLine
            {
                set
                {
                    _isOnLine = value;
                    RaisePropertyChangedEvent("isOnLine");
                }
                get => _isOnLine;
            }

            public Guid guid
            {
                set;
                get;
            }

            #endregion

            public Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo;

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChangedEvent(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public DGExcludingDriveItem(string driveName, bool isOnline, Guid guid)
            {
                _name = driveName;
                _isOnLine = isOnLine;
                icon = IconSet.GetInstance().IconDriveOffline;
                this.guid = guid;
            }
            public DGExcludingDriveItem(Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo)
            {
                guid = advDriveInfo.DeviceId;
                SetOnline(advDriveInfo);
            }
            public void SetOnline(Data.AdvancedDriveInfoHelper.AdvDriveInfo advDriveInfo)
            {
                if (guid != advDriveInfo.DeviceId)
                    throw new ArgumentException("Device ID not match.");

                this.advDriveInfo = advDriveInfo;
                if (string.IsNullOrWhiteSpace(advDriveInfo.driveInfo.volumeLabel))
                    name = $"({advDriveInfo.driveInfo.name})";
                else
                    name = $"{advDriveInfo.driveInfo.volumeLabel} ({advDriveInfo.driveInfo.name})";
                icon = IconSet.GetInstance().GetDriveIcon(advDriveInfo.driveInfo.name);

                isOnLine = true;
            }
            public void SetOffLine()
            {
                advDriveInfo = null;
                name = "?:\\";
                isOnLine = false;
                icon = IconSet.GetInstance().IconDriveOffline;
            }
        }
        private void btn_addExcluding_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<DGExcludingDriveItem> foundExList;
            foreach (DGCurDriveItem dgi in dg_currentDrives.SelectedItems)
            {
                foundExList = excludingDriveList.Where(e => e.guid == dgi.guid);
                if (foundExList.Count() > 0)
                    continue;

                setting.AddExcluding(dgi.guid);
                excludingDriveList.Add(new DGExcludingDriveItem(dgi.advDriveInfo));
                if (workingDriveLetterList.Contains(dgi.advDriveInfo.VolumeLetter))
                    workingDriveLetterList.Remove(dgi.advDriveInfo.VolumeLetter);
            }
            setting.Save();
        }

        private void btn_removeExcluding_Click(object sender, RoutedEventArgs e)
        {
            List<DGExcludingDriveItem> toRemoveList = new List<DGExcludingDriveItem>();
            foreach (DGExcludingDriveItem dgi in dg_excludingDrives.SelectedItems)
                toRemoveList.Add(dgi);
            foreach (DGExcludingDriveItem i in toRemoveList)
            {
                setting.RemoveExcluding(i.guid);
                excludingDriveList.Remove(i);
            }

            setting.Save();
        }

        #endregion


        #region log event, log file-tree, copy files

        private Common.Logger logger;
        private Common.Logger loggerTree;
        private SynchronizedCollection<char> workingDriveLetterList = new SynchronizedCollection<char>();
        private void StartLoggingNCopyingAsync(char driveLetter, bool plugedInOrOut)
        {
            if (setting.IsLogPluged)
            {
                workingDriveLetterList.Add(driveLetter);
                if (plugedInOrOut)
                {
                    string nowString = GetTimeNowFullString();
                    Data.AdvancedDriveInfoHelper.AdvDriveInfo advDI = advDriveInfoHelper.GetAdvDriveInfo(driveLetter);
                    logger.Log($"USB [{advDI.driveInfo.volumeLabel}({driveLetter}:\\)] Plug in, at {nowString}");
                    logger.Log($"    DeviceID: {advDI.DeviceId}", false);
                    logger.Log($"    VolumeSerialNumber: {advDI.VolumeInformation.VolumeSerialNumber}", false);
                    logger.Log($"    TotalSize: {advDI.driveInfo.driveType}", false);
                    logger.Log($"    FileSystem: {advDI.VolumeInformation.fileSystem}", false);
                    logger.Log($"    TotalSize: {advDI.driveInfo.totalSize}", false);
                    logger.Log($"    UsedSize: {advDI.driveInfo.totalSize - advDI.driveInfo.totalFreeSpace}", false);

                    if (setting.IsLogFileTree)
                    {
                        if (loggerTree == null)
                        {
                            loggerTree = new Common.Logger()
                            {
                                BaseDir = AppDomain.CurrentDomain.BaseDirectory,
                                BaseFileNamePre = "tree",
                                IncrementType = Common.Logger.IncrementTypeEnums.Size10M,
                            };
                        }
                        loggerTree.TryReBuildLogFileName();
                        loggerTree.Log($"Start logging file-tree of [{driveLetter}:\\]");
                        LogTreeLoopDir(loggerTree, driveLetter, new DirectoryInfo(driveLetter + ":"), 0);

                        if (setting.IsCopyFiles)
                        {
                            CopyFileLoop(
                                new DirectoryInfo(driveLetter + ":"),
                                System.IO.Path.Combine(
                                   AppDomain.CurrentDomain.BaseDirectory,
                                   driveLetter.ToString() + " " + DateTime.Now.ToString("yyyy MMdd HHmmss")));
                        }
                    }
                }
                else
                {
                    workingDriveLetterList.Remove(driveLetter);
                    logger.Log($"USB [{driveLetter}:] Plug out, at {GetTimeNowFullString()}");
                }

                logger.NewLine();
                loggerTree.NewLine();
                logger.Flush();
                loggerTree.Flush();
            }
        }
        private void LogTreeLoopDir(Common.Logger loggerTree, char driveLetter, DirectoryInfo dirInfo, int level)
        {
            if (workingDriveLetterList.Contains(driveLetter))
            {
                int lCount = level;
                StringBuilder foreSpaceBdr = new StringBuilder();
                while (lCount-- > 0)
                {
                    foreSpaceBdr.Append("    ");
                }
                string foreSpace = foreSpaceBdr.ToString();
                try
                {
                    DirectoryInfo[] subDs = dirInfo.GetDirectories();
                    FileInfo[] subFs = dirInfo.GetFiles();

                    foreach (DirectoryInfo subD in subDs)
                    {
                        loggerTree.Log(foreSpace + GetDirInfoString(subD), false);
                        LogTreeLoopDir(loggerTree, driveLetter, subD, level + 1);
                    }
                    foreach (FileInfo subF in subFs)
                    {
                        loggerTree.Log(foreSpaceBdr.ToString() + GetFileInfoString(subF), false);

                    }
                }
                catch (Exception err)
                {
                    loggerTree.Log($"{foreSpace}Err: {err.ToString()}");
                }
            }
            else
            {
                loggerTree.Log($"Logging cancel at {GetTimeNowFullString()}");
            }

            string GetDirInfoString(DirectoryInfo di)
            {
                StringBuilder result = new StringBuilder();
                Data.IOInfoShadow iois = new Data.IOInfoShadow(di);
                result.Append(iois.name);
                result.Append("\t");
                result.Append(iois.length);
                result.Append("\t");
                result.Append(iois.attributes.ToShortString7());
                result.Append("\t");
                result.Append("C:" + GetTimeString(iois.creationTime));
                result.Append("\t");
                result.Append("M:" + GetTimeString(iois.lastWriteTime));

                return result.ToString();
            }
            string GetFileInfoString(FileInfo fi)
            {
                StringBuilder result = new StringBuilder();
                Data.IOInfoShadow iois = new Data.IOInfoShadow(fi);
                result.Append(iois.name);
                result.Append("\t");
                result.Append(iois.length);
                result.Append("\t");
                result.Append(iois.attributes.ToShortString7());
                result.Append("\t");
                result.Append("C:" + GetTimeString(iois.creationTime));
                result.Append("\t");
                result.Append("M:" + GetTimeString(iois.lastWriteTime));

                return result.ToString();
            }
            string GetTimeString(DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        private string GetTimeNowFullString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void CopyFileLoop(DirectoryInfo dirInfo, string targetDir)
        {
            try
            {
                if (!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
            }
            catch (Exception err0)
            {
                logger.Log($"Error creating dir: {err0.ToString()}");
                return;
            }

            foreach (FileInfo subFi in dirInfo.GetFiles())
            {
                try
                {
                    subFi.CopyTo(System.IO.Path.Combine(targetDir, subFi.Name));
                }
                catch (Exception err1)
                {
                    logger.Log($"Error copying file: {err1.ToString()}");
                }
            }

            try
            {
                foreach (DirectoryInfo subDi in dirInfo.GetDirectories())
                {
                    CopyFileLoop(subDi, System.IO.Path.Combine(targetDir, subDi.Name));
                }
            }
            catch (Exception err2)
            {
                logger.Log($"Error copying dir: {err2.ToString()}");
            }
        }

        #endregion



        private bool isListening = false;
        private string btn_startListening_txStart = "立即启动监听";
        private string btn_startListening_txStop = "(监听中)停止监听";
        private void btn_startListening_Click(object sender, RoutedEventArgs e)
        {
            if (isListening)
            {
                isListening = false;
                btn_startListening.Background = Brushes.LightGreen;
                btn_startListening.Content = btn_startListening_txStart;
            }
            else
            {
                if (setting.IsListeningHideTray)
                {
                    this.Hide();

                    // hide tray icon
                    notifyIcon?.Dispose();
                }

                setting.Save();
                isListening = true;
                btn_startListening.Background = Brushes.OrangeRed;
                btn_startListening.Content = btn_startListening_txStop;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            setting.Save();
            notifyIcon?.Dispose();
        }

        private void btn_viewLogDir_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Explorer.exe", logger.BaseDir);
        }

        private void btn_viewFileDir_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
