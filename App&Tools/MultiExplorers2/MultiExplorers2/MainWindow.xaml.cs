using MadTomDev.App.Ctrls;
using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using ToolTip = System.Windows.Controls.ToolTip;
using Window = System.Windows.Window;

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
            this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            taskBar = this.TaskbarItemInfo;
            SetTaskBarProgressBarStyle(TaskBarProgressStyles.GreenFlow);

            core = Core.GetInstance();
            core.mainWindow = this;

            taskBarAnimaTimer = new Timer(taskBarAnimaTimer_tick);
            taskBarAnimaTimer.Change(Timeout.Infinite, Timeout.Infinite);
            core.InitMainWindow();

            core.ReloadDirStart += Core_ReloadDirStart;
            core.ReloadHostsStart += Core_ReloadHostsStart;

            core.ReloadDirComplete += Core_ReloadDirComplete;
            core.ReloadHostsComplete += Core_ReloadHostsComplete;

            core.RecycleBinItemFound += Core_RecycleBinItemFound;

            core.LogGeneral(core.GetLangTx("txLog_ME2Started"));

            core.CheckRecycleBin();

            SetTaskBarProgressBarStyle(TaskBarProgressStyles.None);


        }


        Core core;
        Setting setting = Setting.Instance;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                core.LogGeneral(core.GetLangTx("txLog_ME2WindowMinimized"));
                core.FsWatchingStopAll();
            }
            else
            {
                core.LogGeneral(core.GetLangTx("txLog_ME2WindowRestored_", this.WindowState.ToString()));
                if (setting.isFileSystemWatcherEnabled)
                    core.FsWatchingStartAll();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_ME2Closing"));
            core.BeforeExiting();
        }

        #region top menu buttons
        private void btn_explorer_Click(object sender, RoutedEventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_ME2RunExplorer"));
            Process.Start("explorer.exe");
        }
        private void btn_clearRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this,
                core.GetLangTx("txMsg_questionRecycleBinItemsDelete"),
                core.GetLangTx("txMsg_warning"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                == MessageBoxResult.Yes)
            {
                this.Cursor = Cursors.Wait;
                btn_clearRecycleBin.IsEnabled = false;
                core.LogGeneral(core.GetLangTx("txLog_ME2ClearRecycleBin"));
                core.ClearRecycleBin();
                img_clearRecycleBin.Source = StaticResource.UIIconRecycleBinEmpty32;
                this.Cursor = Cursors.Arrow;
            }
        }
        private void Core_RecycleBinItemFound(Core c)
        {
            Dispatcher.Invoke(()=>
            {
                img_clearRecycleBin.Source = StaticResource.UIIconRecycleBinFull32;
                btn_clearRecycleBin.IsEnabled = true;
            });
        }

        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_ME2StartSetting"));
            core.SettingFLow();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(
                System.IO.Path.Combine("Docs", "Help.mht"))
            { UseShellExecute = true, };
            p.Start();


        }
        private void btn_info_Click(object sender, RoutedEventArgs e)
        {
            core.LogGeneral(core.GetLangTx("txLog_ME2CheckInfo"));
            WindowLog winLog = new WindowLog();
            winLog.ShowDialog();
        }
        public void Logger_EventLevelUp(Logger sender, Logger.InfoLevels newLevel)
        {
            Dispatcher.Invoke(() =>
            {
                core.LogGeneral(core.GetLangTx("txLog_ME2EventLevelup"));
                switch (newLevel)
                {
                    case Logger.InfoLevels.None:
                        btn_info.Background = Brushes.Transparent;
                        break;
                    case Logger.InfoLevels.Info:
                        btn_info.Background = Brushes.LightSkyBlue;
                        break;
                    case Logger.InfoLevels.Warning:
                        btn_info.Background = Brushes.Yellow;
                        break;
                    case Logger.InfoLevels.Error:
                        btn_info.Background = Brushes.Orange;
                        break;
                }
            });
        }

        #endregion


        #region layouts
        private void btn_addRow_Click(object sender, RoutedEventArgs e)
        {
            core.LayoutAddRow(uGrid);
        }

        private void btn_addCol_Click(object sender, RoutedEventArgs e)
        {
            core.LayoutAddCol(uGrid);
        }

        private void btn_addLayout_Click(object sender, RoutedEventArgs e)
        {
            core.LayoutSaveNew();
        }
        internal void LayoutBtnClickHandler(BtnLayout sender)
        {
            if (sPanel_layoutBtns_isMovintBtn_just)
            {
                sPanel_layoutBtns_isMovintBtn_just = false;
                return;
            }
            core.LayoutApply(sender.layout);
        }

        #region move layout button

        private Point sPanel_layoutBtns_downedMBPoint;
        private MouseButton? sPanel_layoutBtns_downedMB = null;
        private bool sPanel_layoutBtns_isMovintBtn = false;
        private bool sPanel_layoutBtns_isMovintBtn_just = false;
        private void sPanel_layoutBtns_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            sPanel_layoutBtns_downedMBPoint = e.GetPosition(sPanel_layoutBtns);
            sPanel_layoutBtns_downedMB = e.ChangedButton;
        }

        private BtnLayout sPanel_layoutBtns_movingBtn = null;
        private int sPanel_layoutBtns_movingBtn_oriIdx;
        private void sPanel_layoutBtns_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sPanel_layoutBtns_isMovintBtn)
            {
                if (sPanel_layoutBtns_movingBtn == null)
                {
                    // 获取正在拖动的按钮
                    sPanel_layoutBtns_movingBtn = GetBtnAtMousePoint(sPanel_layoutBtns_downedMBPoint);
                    sPanel_layoutBtns_movingBtn.Opacity = 0.6d;
                    sPanel_layoutBtns_movingBtn_oriIdx = sPanel_layoutBtns.Children.IndexOf(sPanel_layoutBtns_movingBtn);
                }
                else
                {
                    // 检查当前按钮是否为刚移动上去的按钮，是的话，把拖动按钮放在这个位置；
                    Point mp = e.GetPosition(sPanel_layoutBtns);
                    BtnLayout curBtn = GetBtnAtMousePoint(mp);
                    if (curBtn != null && curBtn != sPanel_layoutBtns_movingBtn)
                    {
                        int curBtnIdx = sPanel_layoutBtns.Children.IndexOf(curBtn);
                        sPanel_layoutBtns.Children.Remove(sPanel_layoutBtns_movingBtn);

                        sPanel_layoutBtns.Children.Insert(curBtnIdx, sPanel_layoutBtns_movingBtn);

                        //if (curBtnIdx < sPanel_layoutBtns_movingBtn_oriIdx)
                        //{
                        //    // 向左边移动
                        //    sPanel_layoutBtns.Children.Insert(curBtnIdx, sPanel_layoutBtns_movingBtn);
                        //}
                        //else
                        //{
                        //    // 向右边移动
                        //    if (curBtnIdx == sPanel_layoutBtns.Children.Count )
                        //    {
                        //        sPanel_layoutBtns.Children.Add(sPanel_layoutBtns_movingBtn);
                        //    }
                        //    else
                        //    {
                        //    }
                        //}
                    }
                }
            }
            else
            {
                if (sPanel_layoutBtns_downedMB == MouseButton.Left)
                {
                    Point curMP = e.GetPosition(sPanel_layoutBtns);
                    if (Math.Abs(curMP.X - sPanel_layoutBtns_downedMBPoint.X) > 3
                        || Math.Abs(curMP.Y - sPanel_layoutBtns_downedMBPoint.Y) > 3)
                    {
                        sPanel_layoutBtns_isMovintBtn = true;
                        sPanel_layoutBtns_isMovintBtn_just = true;
                    }
                    else
                    {
                        sPanel_layoutBtns_isMovintBtn = false;
                    }
                }
                else
                {
                    sPanel_layoutBtns_isMovintBtn = false;
                }
            }

            BtnLayout GetBtnAtMousePoint(Point mp)
            {
                DependencyObject v = UI.VisualHelper.GetSubVisual(sPanel_layoutBtns, mp);
                try
                {
                    FrameworkElement c = (FrameworkElement)v;
                    while (c != null && !(c is BtnLayout))
                    {
                        if (c.Parent != null && c.Parent is FrameworkElement)
                            c = (FrameworkElement)c.Parent;
                        else
                            c = null;
                    }
                    if (c != null)
                    {
                        return (BtnLayout)c;
                    }
                }
                catch (Exception) { }
                return null;
            }
        }

        private void sPanel_layoutBtns_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sPanel_layoutBtns_isMovintBtn)
            {
                List<Setting.Layout> newList = new List<Setting.Layout>();
                Setting.Layout curL;
                for (int i = 0, iv = sPanel_layoutBtns.Children.Count; i < iv; ++i)
                {
                    curL = ((BtnLayout)sPanel_layoutBtns.Children[i]).layout;
                    curL.idx = i;
                    newList.Add(curL);
                }
                setting.layouts = newList;
                setting.Save();
            }

            if (sPanel_layoutBtns_movingBtn != null)
                sPanel_layoutBtns_movingBtn.Opacity = 1d;
            sPanel_layoutBtns_movingBtn = null;
            sPanel_layoutBtns_isMovintBtn = false;
            sPanel_layoutBtns_downedMB = null;
        }

        private void sPanel_layoutBtns_MouseLeave(object sender, MouseEventArgs e)
        {
            sPanel_layoutBtns_PreviewMouseUp(null, null);
        }

        #endregion

        #region contextMenu layout btn

        private BtnLayout LayoutBtn_rightClicked;
        internal void LayoutBtnRightClickHandler(BtnLayout sender)
        {
            LayoutBtn_rightClicked = sender;
            ContextMenu cm = (ContextMenu)FindResource("cMenu_layoutBtns");
            cm.PlacementTarget = sender;
            cm.IsOpen = true;
        }

        private void MenuItem_layoutBtns_update_Click(object sender, RoutedEventArgs e)
        {
            core.LayoutUpdate(LayoutBtn_rightClicked.layout);
        }
        private void MenuItem_layoutBtns_appearance_Click(object sender, RoutedEventArgs e)
        {
            WindowLayoutBtnStyle wndBtnSetting = new WindowLayoutBtnStyle()
            { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            wndBtnSetting.Init(LayoutBtn_rightClicked);
            wndBtnSetting.ShowDialog();
        }
        private void MenuItem_layoutBtns_delete_Click(object sender, RoutedEventArgs e)
        {
            core.layoutDelete(LayoutBtn_rightClicked.layout);
        }

        #endregion

        #endregion


        #region loading path infoBtn
        private void Core_ReloadDirStart(Core sender, bool? nullFull_trueAdd_falseRemove, string basePath, List<Core.ReloadItemInfo> dirs, List<Core.ReloadItemInfo> files)
        {
            // all disks, null, false
            if (nullFull_trueAdd_falseRemove == null)
                DriveAccessAddLoadingSign(basePath, false);
        }
        private void Core_ReloadHostsStart(Core sender, List<string> hostList)
        {
            DriveAccessAddLoadingSign(null, true);
        }
        private void Core_ReloadDirComplete(Core sender, bool? nullFull_trueAdd_falseRemove, string basePath, List<Core.ReloadItemInfo> dirs, List<Core.ReloadItemInfo> files)
        {
            if (nullFull_trueAdd_falseRemove == null)
                DriveAccessRemoveLoadingSign(basePath, false);
        }
        private void Core_ReloadHostsComplete(Core sender, List<string> hostList)
        {
            DriveAccessRemoveLoadingSign(null, true);
        }

        private void DriveAccessAddLoadingSign(string loadingPath, bool isNetwork)
        {
            Dispatcher.Invoke(() =>
            {
                string newBtnKey = null;
                BtnDirAccess newBtn = null;

                if (string.IsNullOrWhiteSpace(loadingPath))
                {
                    if (isNetwork)
                    {
                        // network
                        newBtnKey = "Network";
                        newBtn = new BtnDirAccess()
                        { IconType = BtnDirAccess.IconTypes.Network, LoadingPath = newBtnKey, };
                    }
                    else
                    {
                        // all disks
                        newBtnKey = "PC";
                        newBtn = new BtnDirAccess()
                        { IconType = BtnDirAccess.IconTypes.PC, LoadingPath = newBtnKey, };
                    }
                }
                else
                {
                    if (isNetwork)
                    {
                        // not possible
                    }
                    else
                    {
                        newBtnKey = loadingPath;
                        if (loadingPath.StartsWith("\\\\"))
                        {
                            int rootSidx = loadingPath.IndexOf("\\", 3);
                            if (0 < rootSidx)
                            {
                                // host
                                newBtn = new BtnDirAccess()
                                { IconType = BtnDirAccess.IconTypes.Host, LoadingPath = newBtnKey, };
                            }
                            else
                            {
                                int rootSidx2 = loadingPath.IndexOf("\\", rootSidx + 1);
                                if (0 < rootSidx2)
                                {
                                    // host root
                                    newBtn = new BtnDirAccess()
                                    { IconType = BtnDirAccess.IconTypes.HostRoot, LoadingPath = newBtnKey, };
                                }
                                else
                                {
                                    // net dir
                                    newBtn = new BtnDirAccess()
                                    { IconType = BtnDirAccess.IconTypes.Dir, LoadingPath = newBtnKey, };
                                }
                            }
                        }
                        else
                        {
                            if (loadingPath.Length <= 3)
                            {
                                // disk
                                newBtn = new BtnDirAccess()
                                { IconType = BtnDirAccess.IconTypes.Custom, LoadingPath = newBtnKey, };
                                newBtn.Icon = Common.IconHelper.FileSystem.Instance.GetIcon(loadingPath, true, true);
                            }
                            else
                            {
                                // local dir
                                newBtn = new BtnDirAccess()
                                { IconType = BtnDirAccess.IconTypes.Dir, LoadingPath = newBtnKey, };
                            }
                        }
                    }
                }

                if (newBtnKey != null)
                {
                    sPanel_taskBtns.Children.Add(newBtn);
                }
            });
        }
        private void DriveAccessRemoveLoadingSign(string doneLoadingPath, bool isNetwork)
        {
            Dispatcher.Invoke(() =>
            {
                string key;
                if (string.IsNullOrWhiteSpace(doneLoadingPath))
                {
                    key = isNetwork ? "Network" : "PC";
                }
                else
                {
                    key = doneLoadingPath;
                }
                object o;
                BtnDirAccess btn;
                for (int i = 0, iv = sPanel_taskBtns.Children.Count; i < iv; ++i)
                {
                    o = sPanel_taskBtns.Children[i];
                    if (o is BtnDirAccess)
                    {
                        btn = (BtnDirAccess)o;
                        if (btn.LoadingPath == key)
                        {
                            sPanel_taskBtns.Children.Remove(btn);
                            if (btn.ToolTip != null)
                            {
                                ToolTip tt = (ToolTip)btn.ToolTip;
                                tt.IsOpen = false;
                            }
                            break;
                        }
                    }
                }
            });
        }

        #endregion


        #region trans infoBtn

        public void ListeningTransferTask(TransferManager.TransferTask task)
        {
            BtnTransferTask newBtn = new BtnTransferTask();
            sPanel_taskBtns.Children.Add(newBtn);
            newBtn.ActionComplete = ListeningTransferTask_removeBtn;
            task.CheckSetNeedTransfer();
            newBtn.Init(task);
            UpdateTaskBarProgress();
        }


        private async void ListeningTransferTask_removeBtn(BtnTransferTask oldBtn)
        {
            oldBtn.CloseContextMenu();
            if (sPanel_taskBtns.Children.Count == 1)
            {
                // even removeed "oldBtn", children.count will still be ONE !
                SetTaskBarClear();
            }
            else
            {
                UpdateTaskBarProgress();
            }
            sPanel_taskBtns.Children.Remove(oldBtn);
        }

        #endregion

        #region taskbar show progress


        #region taskbar progress

        internal System.Windows.Shell.TaskbarItemInfo taskBar;


        //public object taskBarQueueLock = new object();
        //public Queue<TaskBarStyleData> taskBarQueue= new Queue<TaskBarStyleData>();
        //public class TaskBarStyleData
        //{
        //    public TaskBarProgressStyles style;
        //    public double progress;
        //    public string descption;
        //}
             

        public enum TaskBarProgressStyles
        { None, GreenFlow, Green, Yellow, Red, }
        public void SetTaskBarProgressBarStyle(TaskBarProgressStyles style)
        {
            Dispatcher?.Invoke(async () =>
            {
                switch (style)
                {
                    case TaskBarProgressStyles.None:
                        await Task.Delay(50);
                        taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        break;
                    case TaskBarProgressStyles.GreenFlow:
                        taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                        break;
                    case TaskBarProgressStyles.Green:
                        taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        break;
                    case TaskBarProgressStyles.Yellow:
                        taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                        break;
                    case TaskBarProgressStyles.Red:
                        taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        break;
                }
            });
        }
        public void SetTaskBarProgress(double value)
        {
            Dispatcher?.Invoke(() =>
            {
                taskBar.ProgressValue = value;
            });
        }
        public void SetTaskBarDescription(string descp)
        {
            Dispatcher?.Invoke(() =>
            {
                taskBar.Description = descp;
            });
        }

        public void SetTaskBarClear()
        {
            Dispatcher?.Invoke(() =>
            {
                taskBar.ProgressValue = 0d;
                taskBar.Description = "";
                taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            });
        }
        #endregion




        private Timer taskBarAnimaTimer;
        private bool taskBarAnimaTimer_disable = false;
        private BtnTransferTask taskBarAnimaTimer_selectedTaskBtn;
        private void UpdateTaskBarProgress()
        {
            if (sPanel_taskBtns.Children.Count == 0)
            {
                taskBarAnimaTimer_disable = true;
                taskBarAnimaTimer.Change(Timeout.Infinite, Timeout.Infinite);
                taskBarAnimaTimer_selectedTaskBtn = null;
                SetTaskBarClear();
            }
            else
            {
                BtnTransferTask btn, btnErr = null, btnRunning = null, btnPaused = null, btnStarting = null;
                foreach (UIElement ui in sPanel_taskBtns.Children)
                {
                    if (ui is BtnTransferTask)
                    {
                        btn = (BtnTransferTask)ui;
                        if (btnErr == null && btn.taskBarState == BtnTransferTask.TaskBarStates.Error)
                        {
                            btnErr = btn;
                        }
                        else if (btnRunning == null && btn.taskBarState == BtnTransferTask.TaskBarStates.Running)
                        {
                            btnRunning = btn;
                        }
                        else if (btnPaused == null && btn.taskBarState == BtnTransferTask.TaskBarStates.Paused)
                        {
                            btnPaused = btn;
                        }
                        else if (btnStarting == null && btn.taskBarState == BtnTransferTask.TaskBarStates.Starting)
                        {
                            btnStarting = btn;
                        }
                    }
                }
                if (btnErr != null) taskBarAnimaTimer_selectedTaskBtn = btnErr;
                else if (btnRunning != null) taskBarAnimaTimer_selectedTaskBtn = btnRunning;
                else if (btnPaused != null) taskBarAnimaTimer_selectedTaskBtn = btnPaused;
                else if (btnStarting != null) taskBarAnimaTimer_selectedTaskBtn = btnStarting;

                if (taskBarAnimaTimer_selectedTaskBtn == null)
                {
                    taskBarAnimaTimer_disable = true;
                    taskBarAnimaTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    SetTaskBarDescription($"");
                    SetTaskBarProgressBarStyle(TaskBarProgressStyles.None);
                    SetTaskBarProgress(0);
                }
                else
                {
                    taskBarAnimaTimer_disable = false;
                    taskBarAnimaTimer.Change(0, 200);
                }
            }
        }
        private void taskBarAnimaTimer_tick(object o)
        {
            if (taskBarAnimaTimer_disable)
                return;
            if (taskBarAnimaTimer_selectedTaskBtn == null
                || taskBarAnimaTimer_selectedTaskBtn.Parent == null)
            {
                SetTaskBarDescription($"");
                SetTaskBarProgressBarStyle(TaskBarProgressStyles.None);
                SetTaskBarProgress(0);
                return;
            }


            try
            {
                // sometimes taskBarAnimaTimer_selectedTaskBtn will be null
                string descTail = $"[{taskBarAnimaTimer_selectedTaskBtn.task.TaskType}],{Environment.NewLine}taskID [{taskBarAnimaTimer_selectedTaskBtn.task.Id}].";
                switch (taskBarAnimaTimer_selectedTaskBtn.taskBarState)
                {
                    case BtnTransferTask.TaskBarStates.Starting:
                        SetTaskBarDescription($"Starting {descTail}");
                        SetTaskBarProgress(0);
                        SetTaskBarProgressBarStyle(TaskBarProgressStyles.GreenFlow);
                        break;
                    case BtnTransferTask.TaskBarStates.Running:
                        SetTaskBarDescription($"Transfering {descTail}");
                        SetTaskBarProgressBarStyle(TaskBarProgressStyles.Green);
                        SetTaskBarProgress(taskBarAnimaTimer_selectedTaskBtn.taskBarProgressTotalValue);
                        break;
                    case BtnTransferTask.TaskBarStates.Paused:
                        SetTaskBarDescription($"Paused {descTail}");
                        SetTaskBarProgressBarStyle(TaskBarProgressStyles.Yellow);
                        break;
                    case BtnTransferTask.TaskBarStates.Error:
                        SetTaskBarDescription($"Error {descTail}");
                        SetTaskBarProgressBarStyle(TaskBarProgressStyles.Red);
                        break;
                }
            }
            catch (Exception err)
            {
                SetTaskBarDescription($"");
                SetTaskBarProgressBarStyle(TaskBarProgressStyles.None);
                SetTaskBarProgress(0);
            }

        }

        #endregion

        private void btn_debug_Click(object sender, RoutedEventArgs e)
        {
            UI.OutputWindow.Output("Open output window.");
            if (!UI.OutputWindow.HasBtn("List FsWatcher"))
            {
                UI.OutputWindow.AddSetBtn("List FsWatcher", debug_listFsWatcher);
            }
            if (!UI.OutputWindow.HasBtn("ClipBoard"))
            {
                UI.OutputWindow.AddSetBtn("ClipBoard", debug_CheckClipboard);
            }
            if (!UI.OutputWindow.HasBtn("Load all label"))
            {
                UI.OutputWindow.AddSetBtn("Load all label", debug_LoadAllLabel);
            }
        }
        private void debug_listFsWatcher()
        {
            StringBuilder strBdr = new StringBuilder();
            strBdr.Append($"Found {core.fsWatcherDict.Count} filesystem watcher(s), watching:{Environment.NewLine}");
            foreach (string path in core.fsWatcherDict.Keys)
            {
                strBdr.AppendLine(path);
            }
            UI.OutputWindow.Output(strBdr.ToString());
        }
        private void debug_CheckClipboard()
        {
            StringBuilder strBdr = new StringBuilder();

            string[] files = Utilities.ClipBoard.GetFileDrops(out DragDropEffects ddE);
            if (files == null || files.Length <= 0)
            {
                strBdr.AppendLine("No file in ClipBoard.");
            }
            else
            {
                Utilities.ClipBoard.Clear();
                strBdr.AppendLine($"file(s) in ClipBoard for {ddE}-ing:");
                foreach (string f in files)
                {
                    strBdr.AppendLine(f);
                }
            }
            UI.OutputWindow.Output(strBdr.ToString());
        }

        private void debug_LoadAllLabel()
        {
            UI.OutputWindow.Output("start loading labels");
            core.settingsLanguage.SaveToFile(this.Resources, "test.txt");
            UI.OutputWindow.Output("loading labels complete.");
        }


        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (core.isDragingFile)
            {
                core.isPreventDropOnce = true;
                core.isDragingFile = false;
            }
        }

    }
}
