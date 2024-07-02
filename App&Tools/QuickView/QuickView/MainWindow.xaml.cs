using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

using MadTomDev.Common;

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
            //windDbg.Show();
            ApplyBackgroundColor();
        }

        //WindowDebug windDbg = new WindowDebug();

        Brush foreBrush;
        private void ApplyBackgroundColor()
        {
            Color bgClr = setting.BG;
            gridMain.Background = new SolidColorBrush(bgClr);

            float test = (bgClr.R + bgClr.G + bgClr.B) / 3f * (bgClr.A / 255f);
            Color foreClr;
            if (test > 128) foreClr = Colors.Black;
            else foreClr = Colors.White;
            foreBrush = new SolidColorBrush(foreClr);
            tb_fileName.Foreground = foreBrush;
            tb_sizeInfo.Foreground = foreBrush;
            bdr_sliptter.BorderBrush = foreBrush;

            TitledMedia tm;
            if (grid_mainView.Children.Count > 0)
            {
                tm = (TitledMedia)grid_mainView.Children[0];
                tm.SetTitleColor(foreBrush);
            }
            for (int i = stackPanel_thum.Children.Count - 1; i >= 0; i--)
            {
                if (stackPanel_thum.Children[i] is TitledMedia)
                    ((TitledMedia)stackPanel_thum.Children[i]).SetTitleColor(foreBrush);
            }

            tb_videoPosiT.Foreground = foreBrush;
            tb_videoPosi.Foreground = foreBrush;
            tb_videoSpeedT.Foreground = foreBrush;
            tb_videoSpeed.Foreground = foreBrush;
            tb_videoVolumeT.Foreground = foreBrush;
            tb_videoVolume.Foreground = foreBrush;

            bdr_videoControl.BorderBrush = foreBrush;
            bdr_dropZone.BorderBrush = foreBrush;
            tb_fileListPosi.Foreground = foreBrush;

            tb_dropZone.Foreground = foreBrush;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ResumeAllMedia();
            bdr_thum_SizeChanged(null, null);
            tb_input.Focus();
            if (timer == null)
            {
                timer = new Timer(timerTick);
                timer.Change(1000, 1000);
            }
            uiLink_videoSpeed = setting.PlaySpeed;
            uiLink_videoVolume = setting.PlayVolume;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                // unload all medias
                CloseAllMedia();
            }
            else
            {
                TryResetStillWaitNResumeAll();
            }
        }
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            TryResetStillWaitNResumeAll();
        }
        private bool isClosingAllMedia = false;
        private async void CloseAllMedia()
        {
            isClosingAllMedia = true;
            TitledMedia tm;
            if (grid_mainView.Children.Count > 0)
            {
                if (grid_mainView.Children[0] is TitledMedia)
                {
                    tm = (TitledMedia)grid_mainView.Children[0];
                    tm.Close();
                }
                grid_mainView.Children.RemoveAt(0);

            }
            UIElement ui;
            for (int i = stackPanel_thum.Children.Count - 1; i >= 0; i--)
            {
                ui = stackPanel_thum.Children[i];
                if (ui is TitledMedia)
                {
                    tm = (TitledMedia)stackPanel_thum.Children[i];
                    tm.Close();
                    stackPanel_thum.Children.RemoveAt(i);
                }
            }
            await Task.Delay(100);
            isClosingAllMedia = false;
        }
        private void ResumeAllMedia()
        {
            if (grid_mainView.Children.Count <= 0)
            {
                View(viewIndex, out FileInfo missing);
            }
        }


        private double uiLink_videoSpeed
        {
            get => Math.Round(pgb_videoSpeed.Value * 10) / 10;
            set
            {
                pgb_videoSpeed.Value = value;
                tb_videoSpeed.Text = value.ToString("0.0");
            }
        }
        private double uiLink_videoVolume
        {
            get => pgb_videoVolume.Value;
            set
            {
                pgb_videoVolume.Value = value;
                tb_videoVolume.Text = ((int)(value * 100)).ToString("0");
            }
        }

        private Settings setting = Settings.GetInstance();
        public ViewHistory history;
        private string dropedDir;
        private List<FileInfo> mediaFileList
            = new List<FileInfo>();

        #region drop folder, create file list, init history, init view

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            dropedDir = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null || files.Length != 1)
                {
                    e.Effects = DragDropEffects.None;
                }
                else
                {
                    if (Directory.Exists(files[0]))
                    {
                        dropedDir = files[0];
                        e.Effects = DragDropEffects.Link;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void Border_DragOver(object sender, DragEventArgs e)
        {
            if (dropedDir == null)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Link;
            }
            e.Handled = true;
        }
        private async void Border_Drop(object sender, DragEventArgs e)
        {
            if (dropedDir != null)
            {
                this.Cursor = Cursors.Wait;
                await Task.Delay(1);

                ReloadMediaFiles();
                FileInfo outMediaFile;
                View(0, out outMediaFile);
                if (history == null)
                {
                    history = new ViewHistory(setting.ViewHistoryCount, "\\")
                    { IgnoreDuplicatedReport = false, };
                    if (outMediaFile != null)
                        history.Write_History(outMediaFile.FullName, null, new ActionInfo()
                        { action = ActionInfo.ActionTypes.ViewNext, index = viewIndex, mediaFile = outMediaFile, });
                }
                Sort_rebuildDirs();

                this.Cursor = Cursors.Arrow;
            }
        }

        SimpleStringHelper.Checker_starNQues ReloadMediaFiles_fileNameChecker = new SimpleStringHelper.Checker_starNQues();
        private bool ReloadMediaFiles_blockFiles = false;
        private long ReloadMediaFiles_limitMinFileSizeByte = 0;
        private bool ReloadMediaFiles_isLimitMinFileSize = false;
        private void ReloadMediaFiles()
        {
            List<string> ReloadMediaFiles_blockFileList = setting.GetBlockFileList();
            ReloadMediaFiles_blockFiles = (ReloadMediaFiles_blockFileList.Count > 0);
            ReloadMediaFiles_fileNameChecker.ClearPatterns();
            foreach (string f in ReloadMediaFiles_blockFileList)
            {
                ReloadMediaFiles_fileNameChecker.AddPattern(f);
            }

            ReloadMediaFiles_limitMinFileSizeByte = (long)(setting.LimitMinFileSize * 1024);
            ReloadMediaFiles_isLimitMinFileSize = (ReloadMediaFiles_limitMinFileSizeByte > 0);
            mediaFileList.Clear();
            ReloadMediaFiles_Loop(new DirectoryInfo(dropedDir), 0);
        }
        private void ReloadMediaFiles_Loop(DirectoryInfo baseDir, int level)
        {
            if (setting.CmnLoopSubs)
            {
                try
                {
                    int nextLevel = level + 1;
                    string testDirName,
                        dn0 = setting.Sort0DirName.ToLower(),
                        dn1 = setting.Sort1DirName.ToLower();
                    foreach (DirectoryInfo subDi in baseDir.GetDirectories())
                    {
                        if (level == 0)
                        {
                            testDirName = subDi.Name.ToLower();
                            if (testDirName == dn0 || testDirName == dn1)
                                continue;
                        }
                        ReloadMediaFiles_Loop(subDi, nextLevel);
                    }
                }
                catch (Exception err)
                {
                    HandleError(err);
                }
            }
            try
            {
                foreach (FileInfo fi in baseDir.GetFiles())
                {
                    if (ReloadMediaFiles_blockFiles)
                    {
                        if (ReloadMediaFiles_fileNameChecker.Check(fi.Name))
                            continue;
                    }
                    if (ReloadMediaFiles_isLimitMinFileSize)
                    {
                        if (fi.Length <= ReloadMediaFiles_limitMinFileSizeByte)
                            continue;
                    }
                    mediaFileList.Add(fi);
                }
            }
            catch (Exception err)
            {
                HandleError(err);
            }
        }

        #endregion


        #region settings, error handle, key/mouse response

        private void btn_setting_Click(object sender, RoutedEventArgs e)
        {
            WindowSetting winSetting = new WindowSetting()
            { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, };
            winSetting.ShowDialog();
            ApplyBackgroundColor();

            if (setting.StillWaitMin > 0)
            {
                StillWaitCountDown_sec = setting.StillWaitMin * 60;
            }

            if (history == null)
            {
                history = new ViewHistory(setting.ViewHistoryCount, "\\")
                { IgnoreDuplicatedReport = false, };
            }
            if (setting.ViewHistoryCount != history.MaxLength)
            {
                history = new ViewHistory(setting.ViewHistoryCount, "\\")
                { IgnoreDuplicatedReport = false, };
            }
            Sort_rebuildDirs();
            tb_input.Focus();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            WindowHelp win = new WindowHelp()
            { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, };
            win.ShowDialog();
        }

        private void HandleError(Exception err)
        {
            if (setting.CmnErrorMsgBox)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(err.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        private void HandleErrors(List<Exception> errs)
        {
            if (errs == null || errs.Count == 0)
                return;

            if (setting.CmnErrorMsgBox)
            {
                StringBuilder msgsBdr = new StringBuilder();
                foreach (Exception err in errs)
                {
                    msgsBdr.AppendLine(err.Message);
                }
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(msgsBdr.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }

        }


        private bool tb_input_Pressed = false;
        private void tb_input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (tb_input_Pressed)
                return;

            TryResetStillWaitNResumeAll();

            if (Keyboard.IsKeyDown(Key.LeftCtrl)
                || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.Z)
                {
                    Undo();
                    tb_input_Pressed = true;
                }
                else if (e.Key == Key.Y)
                {
                    Redo();
                    tb_input_Pressed = true;
                }
            }
            else
            {
                if (e.Key == setting.ViewPrevKey)
                {
                    View(viewIndex - 1, out FileInfo mediaFile);
                    if (mediaFile != null)
                        history.Write_History(mediaFile.FullName, null, new ActionInfo()
                        { action = ActionInfo.ActionTypes.ViewPrev, index = viewIndex, mediaFile = mediaFile, });
                    tb_input_Pressed = true;
                }
                else if (e.Key == setting.ViewNextKey)
                {
                    View(viewIndex + 1, out FileInfo mediaFile);
                    if (mediaFile != null)
                        history.Write_History(mediaFile.FullName, null, new ActionInfo()
                        { action = ActionInfo.ActionTypes.ViewNext, index = viewIndex, mediaFile = mediaFile, });
                    tb_input_Pressed = true;
                }
                else if (e.Key == setting.Sort0Key)
                {
                    Sort(false, out int idx, out FileInfo mediaFile);
                    if (mediaFile != null)
                        history.Write_History(mediaFile.FullName, null, new ActionInfo()
                        { action = ActionInfo.ActionTypes.Sort0, index = idx, mediaFile = mediaFile, });
                    tb_input_Pressed = true;
                }
                else if (e.Key == setting.Sort1Key)
                {
                    Sort(true, out int idx, out FileInfo mediaFile);
                    if (mediaFile != null)
                        history.Write_History(mediaFile.FullName, null, new ActionInfo()
                        { action = ActionInfo.ActionTypes.Sort1, index = idx, mediaFile = mediaFile, });
                    tb_input_Pressed = true;
                }
            }
            e.Handled = true;
            tb_input.Focus();
        }
        private void tb_input_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            tb_input_Pressed = false;
        }

        #region mouse actions

        private bool mainView_isMouseDown = false;
        private TitledMedia mainView_vBox = null;
        private Thickness mainView_vBoxMarginOri;
        private Point mainView_startMousePoint;
        private DateTime mainView_leftMouseDownTime = DateTime.MinValue;
        private DateTime mainView_midMouseDownTime = DateTime.MinValue;
        private void grid_mainView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (grid_mainView.Children.Count != 1)
                return;
            mainView_vBox = (TitledMedia)grid_mainView.Children[0];
            DateTime now = DateTime.Now;

            if (e.ChangedButton == MouseButton.Middle)
            {
                if ((now - mainView_midMouseDownTime).TotalMilliseconds <= GetDoubleClickTime())
                {
                    mainView_midMouseDownTime = DateTime.MinValue;
                    mainView_vBox.SetZoomOrigin(
                        new Size(grid_mainView.ActualWidth, grid_mainView.ActualHeight));
                    return;
                }

                mainView_midMouseDownTime = now;
                mainView_startMousePoint = Mouse.GetPosition(grid_mainView);
                mainView_vBoxMarginOri = mainView_vBox.Margin;
                mainView_isMouseDown = true;
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                if ((now - mainView_leftMouseDownTime).TotalMilliseconds <= GetDoubleClickTime())
                {
                    mainView_leftMouseDownTime = DateTime.MinValue;
                    mi_open_Click(null, null);
                    return;
                }

                mainView_leftMouseDownTime = now;
            }
        }
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();

        private void grid_mainView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mainView_vBox = null;
            mainView_isMouseDown = false;
            if (e.ChangedButton == MouseButton.Right)
            {
                pauseCtrlFocus = true;
            }
        }
        private void tb_fileName_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                pauseCtrlFocus = true;
            }
        }
        private void grid_mainView_MouseLeave(object sender, MouseEventArgs e)
        {
            mainView_vBox = null;
            mainView_isMouseDown = false;
        }
        private void grid_mainView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mainView_isMouseDown)
            {
                Point curMP = Mouse.GetPosition(grid_mainView);
                if (curMP.X != mainView_startMousePoint.X
                    && curMP.Y != mainView_startMousePoint.Y)
                    mainView_vBox.Move(
                        mainView_vBoxMarginOri,
                        new Size(grid_mainView.ActualWidth, grid_mainView.ActualHeight),
                        mainView_startMousePoint, curMP);
            }
        }

        private void grid_mainView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (grid_mainView.Children.Count != 1)
                return;

            mainView_vBox = (TitledMedia)grid_mainView.Children[0];

            if (e.Delta > 0)
                mainView_vBox.SetZoom(
                    new Size(grid_mainView.ActualWidth, grid_mainView.ActualHeight),
                    e.GetPosition(grid_mainView), 1.3);
            else
                mainView_vBox.SetZoom(
                    new Size(grid_mainView.ActualWidth, grid_mainView.ActualHeight),
                    e.GetPosition(grid_mainView), 0.76923);
        }

        #endregion

        #endregion


        #region view, sort, undo, redo


        private Size thumSize;
        private void bdr_thum_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            thumSize.Height = bdr_thum.ActualHeight;
            thumSize.Width = (bdr_thum.ActualWidth - bdr_sliptter.ActualWidth) / setting.ViewThumbnailCount;
            if (thumSize.Width / thumSize.Height > 1.6)
                thumSize.Width = thumSize.Height * 1.6;

            foreach (UIElement ui in stackPanel_thum.Children)
            {
                if (ui is TitledMedia)
                    ((TitledMedia)ui).SetSize(thumSize);
                else if (ui is Border)
                    ((Border)ui).Height = thumSize.Height - 6;
            }
        }

        private int viewIndex = int.MinValue;
        private List<TitledMedia> viewBoxList = new List<TitledMedia>();
        private TitledMedia curMainViewBox = null;
        private void View(int index, out FileInfo viewingFile)
        {
            curMainViewBox = null;
            viewingFile = null;
            tb_fileName.Text = "";
            // check index
            int mediaFileListCount = mediaFileList.Count;
            if (index < 0) viewIndex = mediaFileListCount - 1;
            else if (index >= mediaFileListCount) viewIndex = 0;
            else viewIndex = index;

            if (viewIndex < 0) viewIndex = 0;

            // remove all vBox ui
            if (grid_mainView.Children.Count > 0)
            {
                TitledMedia tm = (TitledMedia)grid_mainView.Children[0];
                tm.SetSize(thumSize);
                tm.TitleVisibility = Visibility.Visible;
                grid_mainView.Children.Clear();
            }
            for (int i = stackPanel_thum.Children.Count - 1; i >= 0; i--)
            {
                if (stackPanel_thum.Children[i] is TitledMedia)
                    stackPanel_thum.Children.RemoveAt(i);
            }

            // check count, reduce/increase
            int viewCount = setting.ViewThumbnailCount + 1;
            TitledMedia vBox;
            while (viewBoxList.Count < viewCount)
            {
                vBox = new TitledMedia() { MediaFile = null, };
                vBox.SetTitleColor(foreBrush);
                vBox.SetSize(thumSize);
                viewBoxList.Add(vBox);
            }
            while (viewBoxList.Count > viewCount)
                viewBoxList.RemoveAt(0);

            // mute and normal speed all
            foreach (TitledMedia vb in viewBoxList)
            {
                vb.mediaElement.IsMuted = true;
                vb.mediaElement.SpeedRatio = 1;
            }


            // get current uri list
            int idxMid = setting.ViewThumbnailCount / 2;
            FileInfo[] fileList = new FileInfo[viewCount];
            for (int i = viewIndex - idxMid, iv = viewIndex + idxMid, c = 0;
                i <= iv; i++)
            {
                if (i < 0 || i >= mediaFileList.Count)
                    fileList[c++] = null;
                else
                    fileList[c++] = mediaFileList[i];
            }

            // sort ui
            List<TitledMedia> tmpVBList = new List<TitledMedia>();
            tmpVBList.AddRange(viewBoxList.ToArray());
            TitledMedia[] sortVBArray = new TitledMedia[viewCount];
            TitledMedia foundVB;
            FileInfo curFile;
            for (int i = 0, iv = fileList.Length; i < iv; i++)
            {
                curFile = fileList[i];
                if (curFile == null)
                {
                    continue;
                }

                foundVB = tmpVBList.Find(vb => vb.MediaFile != null && vb.MediaFile.FullName == fileList[i].FullName);
                if (foundVB != null)
                {
                    sortVBArray[i] = foundVB;
                    tmpVBList.Remove(foundVB);
                }
            }
            for (int i = 0, j = 0, iv = tmpVBList.Count; i < iv; i++)
            {
                for (; j < viewCount; j++)
                {
                    if (sortVBArray[j] == null)
                    {
                        sortVBArray[j] = tmpVBList[i];
                        break;
                    }
                }
            }

            // add ui
            for (int i = 0; i < viewCount; i++)
            {
                if (i < idxMid)
                    stackPanel_thum.Children.Insert(i, sortVBArray[i]);
                else if (i == idxMid)
                {
                    curMainViewBox = sortVBArray[i];
                    curMainViewBox.TitleVisibility = Visibility.Collapsed;
                    grid_mainView.Children.Add(curMainViewBox);
                    curMainViewBox.SetZoomFull(
                        new Size(grid_mainView.ActualWidth, grid_mainView.ActualHeight));
                    curMainViewBox.mediaElement.SpeedRatio = uiLink_videoSpeed;
                    curMainViewBox.mediaElement.IsMuted = false;
                    curMainViewBox.mediaElement.Volume = uiLink_videoVolume;
                    viewingFile = fileList[i];
                }
                else
                    stackPanel_thum.Children.Add(sortVBArray[i]);
            }

            // load/unload uri
            for (int i = 0; i < viewCount; i++)
            {
                if (sortVBArray[i].MediaFile != fileList[i])
                    sortVBArray[i].MediaFile = fileList[i];
            }

            // progress
            if (mediaFileListCount == 0)
            {
                pgb_fileListPosi.Value = 0;
                pgb_fileListPosi.Maximum = 0;
                tb_fileListPosi.Text = "0 / 0";
            }
            else
            {
                pgb_fileListPosi.Maximum = mediaFileListCount - 1;
                pgb_fileListPosi.Value = viewIndex;
                tb_fileListPosi.Text = (viewIndex + 1).ToString() + " / " + mediaFileListCount.ToString();
            }

            // show size info
            if (mediaFileListCount > 0 && curMainViewBox != null)
            {
                ShowMediaInfo(curMainViewBox.MediaElement, curMainViewBox.MediaFile);
                tb_fileName.Text = curMainViewBox.Title;
            }
            else
            {
                ShowMediaInfo(null, null);
            }

        }

        //private bool ShowMediaInfo_setting = false;
        private DateTime ShowMediaInfo_startTime = DateTime.MinValue;
        private async void ShowMediaInfo(MediaElement me, FileInfo mediaFile)
        {
            //if (ShowMediaInfo_setting)
            //    return;
            //ShowMediaInfo_setting = true;

            ShowMediaInfo_startTime = DateTime.Now;
            DateTime curTime = ShowMediaInfo_startTime;

            StringBuilder strBdr = new StringBuilder();
            if (mediaFile != null)
            {
                strBdr.Append(SimpleStringHelper.UnitsOfMeasure.GetShortString(mediaFile.Length, "B", 1024));
            }
            if (me != null)
            {
                if (strBdr.Length > 0)
                {
                    strBdr.Append("  ");
                }
                int counter = 0;
                bool isFirstWait = true;
                while (counter++ < 60)
                {
                    if (curTime != ShowMediaInfo_startTime)
                        return;
                    if (me.HasVideo)
                    {
                        if (me.NaturalDuration.HasTimeSpan)
                            AddTimeInfo(strBdr, me.NaturalDuration.TimeSpan);
                        if (strBdr.Length > 0)
                            strBdr.Append(Environment.NewLine);
                        AddSizeInfo(strBdr, me.NaturalVideoWidth, me.NaturalVideoHeight);
                        break;
                    }
                    else if (me.HasAudio)
                    {
                        if (me.NaturalDuration.HasTimeSpan)
                            AddTimeInfo(strBdr, me.NaturalDuration.TimeSpan);
                        break;
                    }
                    else
                    {
                        if (isFirstWait)
                        {
                            tb_sizeInfo.Text = strBdr.ToString();
                        }
                        isFirstWait = false;
                        await Task.Delay(5);
                    }
                }
            }
            tb_sizeInfo.Text = strBdr.ToString();
            //ShowMediaInfo_setting = false;            

            void AddTimeInfo(StringBuilder strBdr, TimeSpan ts)
            {
                if (ts.TotalHours > 1)
                    strBdr.Append(ts.ToString(@"hh\:mm:ss.fff"));
                else
                    strBdr.Append(ts.ToString(@"mm:ss.fff"));
            }
            void AddSizeInfo(StringBuilder strBdr, int width, int height)
            {
                strBdr.Append(width);
                strBdr.Append(" * ");
                strBdr.Append(height);
            }
        }
        private void stackPanel_thum_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mp = e.GetPosition(stackPanel_thum);
            double xRight = stackPanel_thum.ActualWidth - mp.X;
            double halfPanelWidth = (stackPanel_thum.ActualWidth - bdr_sliptter.ActualWidth) / 2;
            double rate;
            bool isForward;
            if (mp.X >= 0 && mp.X <= halfPanelWidth)
            {
                isForward = false;
                rate = 1 - mp.X / halfPanelWidth;
            }
            else if (xRight >= 0 && xRight <= halfPanelWidth)
            {
                isForward = true;
                rate = 1 - xRight / halfPanelWidth;
            }
            else return;

            int idxOffset = (int)(setting.ViewThumbnailCount / 2 * rate) + 1;
            FileInfo mediaFile;
            if (isForward)
                View(viewIndex + idxOffset, out mediaFile);
            else
                View(viewIndex - idxOffset, out mediaFile);

            if (mediaFile != null)
                history.Write_History(mediaFile.FullName, null, new ActionInfo()
                { action = ActionInfo.ActionTypes.ViewPrev, index = viewIndex, });
        }

        private void Sort(bool upOrDown, out int idx, out FileInfo mediaFile)
        {
            idx = -1; mediaFile = null;
            if (grid_mainView.Children.Count != 1)
                return;

            mainView_vBox = (TitledMedia)grid_mainView.Children[0];
            idx = viewIndex; mediaFile = mainView_vBox.MediaFile;

            MoveFileAsync(mediaFile.FullName, System.IO.Path.Combine(
                upOrDown ? dirSort1 : dirSort0,
                mediaFile.Name));
            mediaFileList.RemoveAt(viewIndex);
            View(viewIndex, out FileInfo missing);
        }
        private bool MoveFileAsync_working = false;
        private object MoveFileAsync_workQueueLocker = new object();
        public struct MoveFilePair
        {
            public string source;
            public string target;
        }
        private Queue<MoveFilePair> MoveFileAsync_workQueue = new Queue<MoveFilePair>();
        private List<Exception> MoveFileAsync_errorList = new List<Exception>();
        private void MoveFileAsync(string source, string target)
        {
            lock (MoveFileAsync_workQueueLocker)
            {
                MoveFileAsync_workQueue.Enqueue(
                    new MoveFilePair() { source = source, target = target, });
            }
            if (MoveFileAsync_working)
                return;

            MoveFileAsync_working = true;
            Task.Run(async () =>
            {
                await Task.Delay(5);

                bool doLoop = true;
                MoveFilePair task;
                while (doLoop)
                {
                    List<MoveFilePair> taskList = new List<MoveFilePair>();
                    lock (MoveFileAsync_workQueueLocker)
                    {
                        while (MoveFileAsync_workQueue.Count > 0)
                        {
                            taskList.Add(MoveFileAsync_workQueue.Dequeue());
                        }
                    }

                    while (taskList.Count > 0)
                    {
                        task = taskList[0];
                        taskList.RemoveAt(0);
                        try
                        {
                            if (File.Exists(task.target))
                            {
                                // 当分类文件中已经存在同名文件时，先将这个已存在文件重名，然后再移动；
                                int oldIdx = 0;
                                string oldTar;
                                do
                                {
                                    oldTar = task.target + ".old" + oldIdx++;
                                }
                                while (File.Exists(oldTar));
                                File.Move(task.target, oldTar);
                            }
                            File.Move(task.source, task.target);
                        }
                        catch (Exception err)
                        {
                            MoveFileAsync_errorList.Add(err);
                        }
                    }

                    // show error
                    if (MoveFileAsync_errorList.Count > 0)
                    {
                        HandleErrors(MoveFileAsync_errorList);
                    }
                    MoveFileAsync_errorList.Clear();

                    // process next batch
                    await Task.Delay(5);
                    lock (MoveFileAsync_workQueueLocker)
                    {
                        doLoop = MoveFileAsync_workQueue.Count > 0;
                    }
                }

                MoveFileAsync_working = false;
            });
        }
        private string dirSort0, dirSort1;
        private void Sort_rebuildDirs()
        {
            if (dropedDir == null)
                return;
            try
            {
                dirSort0 = System.IO.Path.Combine(dropedDir, setting.Sort0DirName);
                if (!Directory.Exists(dirSort0))
                    Directory.CreateDirectory(dirSort0);
                dirSort1 = System.IO.Path.Combine(dropedDir, setting.Sort1DirName);
                if (!Directory.Exists(dirSort1))
                    Directory.CreateDirectory(dirSort1);
            }
            catch (Exception err)
            {
                HandleError(err);
            }
        }

        private class ActionInfo
        {
            public enum ActionTypes
            { ViewNext, ViewPrev, Sort0, Sort1, }
            public ActionTypes action;
            public int index;
            public FileInfo mediaFile;
        }

        private void Undo()
        {
            if (history.CanGetPrev)
            {
                ViewHistory.Item curDone = history.Read();
                ViewHistory.Item preDone = history.Get_Entry(-1);
                ActionInfo aInfo = (ActionInfo)curDone.tag;
                bool isSort = false;
                string oriFile = null, tarFile = null;
                FileInfo missing;
                switch (aInfo.action)
                {
                    case ActionInfo.ActionTypes.ViewNext:
                        View(((ActionInfo)preDone.tag).index, out missing);
                        break;
                    case ActionInfo.ActionTypes.ViewPrev:
                        View(((ActionInfo)preDone.tag).index, out missing);
                        break;
                    case ActionInfo.ActionTypes.Sort1:
                        isSort = true;
                        oriFile = System.IO.Path.Combine(dirSort1, System.IO.Path.GetFileName(curDone.fullName));
                        tarFile = curDone.fullName;
                        break;
                    case ActionInfo.ActionTypes.Sort0:
                        isSort = true;
                        oriFile = System.IO.Path.Combine(dirSort0, System.IO.Path.GetFileName(curDone.fullName));
                        tarFile = curDone.fullName;
                        break;
                }
                if (isSort)
                {
                    try
                    {
                        File.Move(oriFile, tarFile);
                        WaitFileMove(tarFile);
                        mediaFileList.Insert(aInfo.index, ((ActionInfo)curDone.tag).mediaFile);
                        View(aInfo.index, out missing);
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void WaitFileMove(string tarFile)
        {
            while (!File.Exists(tarFile))
            {
                Thread.Sleep(10);
            }
        }

        private void Redo()
        {
            if (history.CanGetNext)
            {
                ViewHistory.Item furDone = history.Get_Entry(1);
                ActionInfo aInfo = (ActionInfo)furDone.tag;
                switch (aInfo.action)
                {
                    case ActionInfo.ActionTypes.ViewNext:
                        View(aInfo.index, out FileInfo missing);
                        break;
                    case ActionInfo.ActionTypes.ViewPrev:
                        View(aInfo.index, out missing);
                        break;
                    case ActionInfo.ActionTypes.Sort0:
                        Sort(false, out int missingInt, out missing);
                        break;
                    case ActionInfo.ActionTypes.Sort1:
                        Sort(true, out missingInt, out missing);
                        break;
                }
            }
        }

        #endregion



        #region video play, speed, volume

        private System.Threading.Timer timer = null;
        private bool CheckMainViewHasAV()
        {
            if (curMainViewBox == null)
                return false;
            if (curMainViewBox.mediaElement.HasVideo || curMainViewBox.mediaElement.HasAudio)
            {
                if (curMainViewBox.mediaElement.NaturalDuration.HasTimeSpan)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        private bool CheckMainViewHaveMedia()
        {
            if (curMainViewBox == null)
                return false;
            if (curMainViewBox.mediaElement.Source == null)
                return false;
            return true;
        }
        private void timerTick(object state)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!CheckMainViewHasAV())
                {
                    if (StillWaitCountDown_sec >= 0
                        && setting.StillWaitMin > 0)
                    {
                        if (--StillWaitCountDown_sec < 0)
                        {
                            CloseAllMedia();
                        }
                    }
                    return;
                }

                pgb_videoPosi.Maximum = curMainViewBox.mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
                TimeSpan posi = curMainViewBox.mediaElement.Position;
                pgb_videoPosi.Value = posi.TotalMilliseconds;
                tb_videoPosi.Text = ((int)posi.TotalHours).ToString() + ":" + posi.ToString(@"mm\:ss");
            });
        }
        private int StillWaitCountDown_sec = 99999;
        private void StillWaitResetTime()
        {
            StillWaitCountDown_sec = setting.StillWaitMin * 60;
        }
        private void TryResetStillWaitNResumeAll()
        {
            if (setting.StillWaitMin > 0 && !isClosingAllMedia)
            {
                StillWaitResetTime();
                ResumeAllMedia();
            }
        }

        private void pgb_videoPosi_MouseEnter(object sender, MouseEventArgs e)
        {
            if (CheckMainViewHasAV())
                pgb_videoPosi.Cursor = Cursors.Cross;
            else
                pgb_videoPosi.Cursor = Cursors.Arrow;
        }
        private void pgb_videoPosi_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }
        private void pgb_videoPosi_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckMainViewHasAV())
            {
                Point mp = e.GetPosition(pgb_videoPosi);
                double ratio = 1 - mp.Y / pgb_videoPosi.ActualHeight;
                if (ratio < 0) ratio = 0;
                else if (ratio > 1) ratio = 1;

                curMainViewBox.mediaElement.Position
                    = TimeSpan.FromTicks((long)(curMainViewBox.mediaElement.NaturalDuration.TimeSpan.Ticks * ratio));
            }
        }

        private void pgb_videoSpeed_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }
        private void pgb_videoSpeed_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point mp = e.GetPosition(pgb_videoSpeed);
                double ratio = 1 - mp.Y / pgb_videoSpeed.ActualHeight;
                if (ratio < 0) ratio = 0;
                else if (ratio > 1) ratio = 1;
                uiLink_videoSpeed = pgb_videoSpeed.Maximum * ratio;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                uiLink_videoSpeed = 1;
            }
            else return;

            setting.PlaySpeed = uiLink_videoSpeed;
            if (CheckMainViewHasAV())
                curMainViewBox.mediaElement.SpeedRatio = uiLink_videoSpeed;
        }

        private void pgb_videoVolume_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
        }
        private void pgb_videoVolume_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mp = e.GetPosition(pgb_videoVolume);
            double ratio = 1 - mp.Y / pgb_videoVolume.ActualHeight;
            if (ratio < 0) ratio = 0;
            else if (ratio > 1) ratio = 1;
            uiLink_videoVolume = ratio;
            setting.PlayVolume = ratio;

            if (CheckMainViewHasAV())
                curMainViewBox.mediaElement.Volume = uiLink_videoVolume;
        }


        #endregion


        #region copy image, copy file, open file, open dir, batch move

        private void grid_mainView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu cm = grid_mainView.ContextMenu;
            MenuItem mi = (MenuItem)cm.Items[0];
            mi.IsEnabled = CheckMainViewHaveMedia();
        }

        private void mi_copy_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckMainViewHaveMedia())
            {
                return;
            }

            MediaElement ctrl = curMainViewBox.mediaElement;
            int width = (int)Math.Round(ctrl.ActualWidth);
            int height = (int)Math.Round(ctrl.ActualHeight);
            RenderTargetBitmap renderTargetBitmap =
                new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(ctrl);
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }
            renderTargetBitmap.Render(dv);
            Clipboard.SetImage(renderTargetBitmap);
        }

        private void mi_copyFile_Click(object sender, RoutedEventArgs e)
        {
            if (curMainViewBox == null || curMainViewBox.MediaFile == null)
                return;

            Data.Utilities.ClipBoard.SetFileDrags(true, false, curMainViewBox.MediaFile.FullName);
        }
        private void mi_copyFilePath_Click(object sender, RoutedEventArgs e)
        {
            if (curMainViewBox == null || curMainViewBox.MediaFile == null)
                return;

            Clipboard.SetText(curMainViewBox.MediaFile.FullName);
        }

        private void mi_open_Click(object sender, RoutedEventArgs e)
        {
            if (curMainViewBox == null || curMainViewBox.MediaFile == null)
                return;

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(curMainViewBox.MediaFile.FullName)
            { UseShellExecute = true, };
            p.Start();
        }

        private void mi_openDir_Click(object sender, RoutedEventArgs e)
        {
            if (curMainViewBox == null || curMainViewBox.MediaFile == null)
                return;

            Process.Start("Explorer.exe", "/select," + curMainViewBox.MediaFile);
        }


        #region batch move
        private void mi_movePrevsToSort0_Click(object sender, RoutedEventArgs e)
        {
            TryBatchMove(viewIndex, true, true);
        }
        private void TryBatchMove(int index, bool movePrevsOrNexts, bool toSort0_orSort1)
        {
            FileInfo curFi;
            try
            {
                string tarDir = toSort0_orSort1 ? dirSort0 : dirSort1;
                if (movePrevsOrNexts)
                {
                    for (int i = index; i >= 0; --i, --viewIndex)
                    {
                        curFi = mediaFileList[i];
                        //curFi.MoveTo(System.IO.Path.Combine(tarDir, curFi.Name));
                        MoveFileAsync(curFi.FullName, System.IO.Path.Combine(tarDir, curFi.Name));
                        mediaFileList.RemoveAt(i);
                    }
                    ++viewIndex;
                }
                else
                {
                    for (int i = mediaFileList.Count - 1; i >= index; --i)
                    {
                        curFi = mediaFileList[i];
                        //curFi.MoveTo(System.IO.Path.Combine(tarDir, curFi.Name));
                        MoveFileAsync(curFi.FullName, System.IO.Path.Combine(tarDir, curFi.Name));
                        mediaFileList.RemoveAt(i);
                    }
                    --viewIndex;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(this, err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                history.Clear();
                View(viewIndex, out curFi);
            }
        }

        private void mi_movePrevsToSort1_Click(object sender, RoutedEventArgs e)
        {
            TryBatchMove(viewIndex, true, false);
        }

        private void mi_moveNextsToSort0_Click(object sender, RoutedEventArgs e)
        {
            TryBatchMove(viewIndex, false, true);
        }

        private void mi_moveNextsToSort1_Click(object sender, RoutedEventArgs e)
        {
            TryBatchMove(viewIndex, false, false);
        }
        #endregion

        #endregion


        private bool pauseCtrlFocus = false;
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                return;
            }
            Task.Run(() =>
            {
                if (pauseCtrlFocus)
                    return;
                Thread.Sleep(10);
                this.Dispatcher.Invoke(() => { tb_input.Focus(); });
            });
        }

        private void cm_mainView_Closed(object sender, RoutedEventArgs e)
        {
            pauseCtrlFocus = false;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            setting.Save();
        }
    }
}
