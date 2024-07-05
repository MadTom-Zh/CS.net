using MadTomDev.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MadTomDev.App.Ctrls
{
    /// <summary>
    /// Interaction logic for BtnTransferTask.xaml
    /// </summary>
    public partial class BtnTransferTask : UserControl
    {
        public BtnTransferTask()
        {
            InitializeComponent();
        }
        TransferManager transferManager = TransferManager.GetInstance();
        public TransferManager.TransferTask task;
        private string pathSource, pathTarget;

        public Action<BtnTransferTask> ActionComplete;

        public void Init(TransferManager.TransferTask task)
        {
            // 复制-绿色，移动-蓝色，删除-红色
            this.task = task;
            switch (task.TaskType)
            {
                case TransferManager.TaskTypes.Copy:
                    btn.BorderBrush = Brushes.DarkGreen;
                    progressBarBrush_going = Brushes.Lime;
                    break;
                case TransferManager.TaskTypes.Move:
                    btn.BorderBrush = Brushes.DarkBlue;
                    progressBarBrush_going = Brushes.SkyBlue;
                    if (!task.needDataCopy)
                    {
                        img_rightIcon.Visibility = Visibility.Collapsed;
                        rect_idcRight.Visibility = Visibility.Collapsed;
                        tb_right.Visibility = Visibility.Collapsed;
                    }
                    break;
                case TransferManager.TaskTypes.Delete:
                    btn.BorderBrush = Brushes.DarkRed;
                    progressBarBrush_going = Brushes.Orange;

                    img_rightIcon.Visibility = Visibility.Collapsed;
                    rect_idcRight.Visibility = Visibility.Collapsed;
                    tb_right.Visibility = Visibility.Collapsed;
                    break;
                case TransferManager.TaskTypes.CreateLink:
                    btn.BorderBrush = Brushes.DarkMagenta;
                    progressBarBrush_going = Brushes.Magenta;
                    break;
            }
            rect_progress.Fill = progressBarBrush_going;

            pathSource = task.SourceIOs[0];
            pathTarget = (task.TargetIOs == null || task.TargetIOs.Length <= 0) ? pathSource : task.TargetIOs[0];






            SetDiskLightLeft();
            SetIconNText(img_leftIcon, tb_left, pathSource);
            if (img_rightIcon.Visibility == Visibility.Visible)
            {
                SetDiskLightRight();
                SetIconNText(img_rightIcon, tb_right, pathTarget);
            }

            void SetIconNText(Image picBox, TextBlock tb, string path)
            {
                if (path.StartsWith("\\\\"))
                {
                    picBox.Source = Common.IconHelper.Shell32Icons.Instance.GetIcon(275, false);
                    tb.Text = Utilities.FilePath.GetUNCHostName(path);
                }
                else
                {
                    if (path.Length >= 3)
                    {
                        picBox.Source = Common.IconHelper.FileSystem.Instance.GetIcon(path.Substring(0, 3), true, true);
                        tb.Text = path.Substring(0, 3);
                    }
                    else
                    {
                        picBox.Source = null;
                        tb.Text = path;
                    }
                }
            }

            task.EventRaised += Task_EventRaised;
            task.ExceptionRaised += Task_ExceptionRaised;
            task.Progressed += Task_Progressed;

            // animation
            Storyboard.SetTarget(storyboard_blinkErr, rect_err);
            Storyboard.SetTargetProperty(storyboard_blinkErr, new PropertyPath(OpacityProperty));
            storyboard_blinkErr.Children.Add(new DoubleAnimation(1, 0, new Duration(new TimeSpan(0, 0, 0, 0, 300))) { SpeedRatio = 0.5, });
        }


        #region 按照任务执行过程中发生的事件，实时变化按钮显示

        private void SetDiskLightLeft()
        {
            SetDiskLight(rect_idcLeft, pathSource);
        }
        private void SetDiskLightRight()
        {
            SetDiskLight(rect_idcRight, pathTarget);
        }
        private void SetDiskLight(Rectangle light, string testPath)
        {
            if (transferManager.CheckFlagDiskWriting(testPath) == true)
                light.Fill = Brushes.Tomato;
            else if (transferManager.CheckFlagDiskReading(testPath) == true)
                light.Fill = Brushes.LawnGreen;
            else
                light.Fill = Brushes.DarkGray;
        }

        private Brush progressBarBrush_going, progressBarBrush_paused = Brushes.DarkGray;

        private void Task_EventRaised(TransferManager.TransferTask sender, TransferManager.TransferTask.Events e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e)
                {
                    case TransferManager.TransferTask.Events.Scanning:
                        SetDiskLightLeft();
                        break;
                    case TransferManager.TransferTask.Events.Start:
                        SetDiskLightLeft();
                        if (img_rightIcon.Visibility == Visibility.Visible)
                        {
                            SetDiskLightRight();
                        }
                        break;
                    case TransferManager.TransferTask.Events.Paused:
                        taskBarState = TaskBarStates.Paused;
                        rect_progress.Fill = progressBarBrush_paused;
                        SetDiskLightLeft();
                        if (img_rightIcon.Visibility == Visibility.Visible)
                        {
                            SetDiskLightRight();
                        }
                        break;
                    case TransferManager.TransferTask.Events.Resumed:
                        taskBarState = TaskBarStates.Running;
                        rect_progress.Fill = progressBarBrush_going;
                        SetDiskLightLeft();
                        if (img_rightIcon.Visibility == Visibility.Visible)
                        {
                            SetDiskLightRight();
                        }
                        break;
                    case TransferManager.TransferTask.Events.Completed:
                    case TransferManager.TransferTask.Events.Canceled:
                        SetDiskLightLeft();
                        if (img_rightIcon.Visibility == Visibility.Visible)
                        {
                            SetDiskLightRight();
                        }
                        sender.EventRaised -= Task_EventRaised;
                        sender.ExceptionRaised -= Task_ExceptionRaised;
                        sender.Progressed -= Task_Progressed;
                        ActionComplete?.Invoke(this);
                        break;
                }
            });
        }
        private bool Task_Progressed_isShowingInfo = false;
        private DateTime Task_Progressed_preCalSpeedTime = DateTime.MinValue;
        private long Task_Progressed_preCalSpeedSize = 0;
        private void Task_Progressed(TransferManager.TransferTask sender, TransferManager.TransferTask.ProgressData e)
        {
            if (Task_Progressed_isShowingInfo)
                return;

            Dispatcher.Invoke(() =>
            {
                // progress bar width = 56
                Task_Progressed_isShowingInfo = true;
                //double filesTotalSizeRate = (double)sender.filesSizeTransfered / sender.filesSizeTotal;
                taskBarProgressTotalValue = (double)sender.filesSizeTransfered / sender.filesSizeTotal;
                taskBarState = TaskBarStates.Running;
                rect_progress.Width = 56d * taskBarProgressTotalValue;

                if (isCMenuOpened)
                {
                    pgb_files.Value = (double)sender.filesCountTransfered / sender.filesCountTotal;
                    tb_files.Text = $"{sender.filesCountTransfered.ToString("###,###,###,##0")} / {sender.filesCountTotal.ToString("###,###,###,##0")}";

                    DateTime now = DateTime.Now;
                    TimeSpan dataTransTimeEs = now - Task_Progressed_preCalSpeedTime;
                    if (dataTransTimeEs.TotalMilliseconds > 300)
                    {
                        long dataTransed = sender.filesSizeTransfered - Task_Progressed_preCalSpeedSize;
                        double speedV = (double)dataTransed * 1000 / dataTransTimeEs.TotalMilliseconds;
                        tb_speed.Text = $"{Common.SimpleStringHelper.UnitsOfMeasure.GetShortString((long)speedV, "B", 1024)}/s";

                        Task_Progressed_preCalSpeedTime = now;
                        Task_Progressed_preCalSpeedSize = sender.filesSizeTransfered;
                    }

                    tb_curFileName.Text = e.ioInfo.name;
                    pgb_curFileSize.Value = (double)e.fileSizeTransfered / e.ioInfo.length;
                    tb_curFileSize.Text = $"{(e.fileSizeTransfered / 1048576d).ToString("###,###,###,##0.00")} / {(e.ioInfo.length / 1048576d).ToString("###,###,###,##0.00")}";
                }
                Task_Progressed_isShowingInfo = false;
            });
        }


        #region for taskBar progress

        public enum TaskBarStates
        { Starting, Running,Paused,Error, }
        public TaskBarStates taskBarState = TaskBarStates.Starting;
        public double taskBarProgressTotalValue = 0;

        #endregion

        #region 只有在右键菜单显示的时候，才更新其中的进度信息
        private void btn_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Point mp = Mouse.GetPosition(btn);
            cMenu.HorizontalOffset = -mp.X;
            cMenu.VerticalOffset = -mp.Y - cMenu.ActualHeight;

            if (task.flagPause == true)
            {
                menuItem_play.Header = "Continue";
                menuItem_play.IsEnabled = true;
                menuItem_pause.IsEnabled = false;
            }
            else
            {
                menuItem_play.Header = "Start";
                menuItem_play.IsEnabled = false;
                menuItem_pause.IsEnabled = true;
            }
        }
        internal void CloseContextMenu()
        {
            if (cMenu.IsOpen)
                cMenu.IsOpen = false;
        }

        private bool isCMenuOpened = false;
        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            isCMenuOpened = true;
        }
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            isCMenuOpened = false;
        }
        #endregion


        Storyboard storyboard_blinkErr = new Storyboard();
        private void Task_ExceptionRaised(TransferManager.TransferTask sender, Exception e)
        {
            Dispatcher.Invoke(() =>
            {
                // play a anima, show there is a err
                taskBarState = TaskBarStates.Error;
                storyboard_blinkErr.Begin();
                MessageBox.Show(e.ToString(),Core.GetInstance().GetLangTx("txMsg_transferError"), MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
        #endregion


        private void menuItem_play_Click(object sender, RoutedEventArgs e)
        {
            task.ResumeAsync();
        }
        private void menuItem_pause_Click(object sender, RoutedEventArgs e)
        {
            task.PauseAsync();
        }

        private void menuItem_cancel_Click(object sender, RoutedEventArgs e)
        {
            task.CancelAsync();
        }


        private void Button_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (task.flagPause == true)
                task.ResumeAsync();
            else if (task.HasStarted)
                task.PauseAsync();
            else
                task.StartAsync();
        }

    }
}
