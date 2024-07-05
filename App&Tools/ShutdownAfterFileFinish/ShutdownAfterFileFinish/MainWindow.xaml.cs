using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using static System.Net.WebRequestMethods;

using MadTomDev.Common;
using File = System.IO.File;
using System.Diagnostics;
using Path = System.IO.Path;
using System.Media;

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
            dg_watchingDirs.ItemsSource = dgWatchingDirSource;
            dg_watchingFiles.ItemsSource = dgWatchingFileSource;
            SettingLoad();
            tb_endingScriptFile_TextChanged(null, null);

            if (!Directory.Exists(loggerDir))
                Directory.CreateDirectory(loggerDir);
            logger = new Logger()
            {
                BaseDir = loggerDir,
                BaseFileNamePre = "SAFF",
                IncrementType = Logger.IncrementTypeEnums.Size5M,
            };

            btn_watchingStop.IsEnabled = false;
            tb_currentState.Text = "Idle";
        }
        private string loggerDir = Common.Variables.IOPath.LogDir;

        #region setting load & save

        private void SettingLoad()
        {
            settings.Load();

            dgWatchingDirSource.Clear();
            foreach (string dir in settings.watchintDirs)
                dgWatchingDirSource.Add(new DGDataItem() { Tx = dir, });
            cb_watchingSubDirs.IsChecked = settings.withinSubDirs;
            dgWatchingFileSource.Clear();
            foreach (DGDataItemFile file in settings.watchintFiles)
                dgWatchingFileSource.Add(new DGDataItemFile(file));
            tb_endingScriptFile.Text = settings.endingScriptFile;
            tb_timerInterval.Text = settings.timerIntervalSec.ToString();
            sld_timerInterval.Value = settings.timerIntervalSec;
            cb_writeLog.IsChecked = settings.writeLog;
            cb_endingSound.IsChecked = settings.endingSound;
        }
        private bool SettingSave_needSave = false;
        private void SettingSave()
        {
            if (SettingSave_needSave)
            {
                settings.watchintDirs.Clear();
                foreach (DGDataItem dir in dgWatchingDirSource)
                    settings.watchintDirs.Add(dir.Tx);
                settings.withinSubDirs = cb_watchingSubDirs.IsChecked == true;
                settings.watchintFiles.Clear();
                foreach (DGDataItemFile file in dgWatchingFileSource)
                    settings.watchintFiles.Add(new DGDataItemFile(file));
                settings.endingScriptFile = tb_endingScriptFile.Text;
                settings.timerIntervalSec = int.Parse(tb_timerInterval.Text);
                settings.timerIntervalSec = (int)sld_timerInterval.Value;
                settings.writeLog = cb_writeLog.IsChecked == true;
                settings.endingSound = cb_endingSound.IsChecked == true;

                settings.Save();
                SettingSave_needSave = false;
            }
        }


        private Settings settings = Settings.GetInstance();
        public class Settings
        {

            private Data.SettingXML xml;
            private static string xmlFile;
            private Settings()
            {
                xml = new Data.SettingXML();
                xmlFile = Common.Variables.IOPath.SettingDir;
                if (!Directory.Exists(xmlFile))
                    Directory.CreateDirectory(xmlFile);
                xmlFile = Path.Combine(xmlFile, "SDaFF.xml");
            }
            private static Settings instance;
            public static Settings GetInstance()
            {
                if (instance == null)
                {
                    instance = new Settings();
                }
                return instance;
            }

            public List<string> watchintDirs = new List<string>();
            public List<DGDataItemFile> watchintFiles = new List<DGDataItemFile>();
            public bool withinSubDirs;
            public string? endingScriptFile;
            public int timerIntervalSec;
            public bool writeLog;
            public bool endingSound;

            public void Load()
            {
                watchintDirs.Clear();
                watchintFiles.Clear();
                endingScriptFile = null;
                timerIntervalSec = 5;
                writeLog = false;
                endingSound = false;
                if (File.Exists(xmlFile))
                {
                    xml.Reload(xmlFile);
                    Data.SettingXML.Node rootNode = xml.rootNode;
                    foreach (Data.SettingXML.Node dirNode in rootNode["WatchingDirs"])
                        watchintDirs.Add(dirNode.Text);

                    bool outVb;
                    foreach (Data.SettingXML.Node dirNode in rootNode["WatchingFiles"])
                        watchintFiles.Add(new DGDataItemFile()
                        {
                            Tx = dirNode.Text,
                            IsExisting = bool.TryParse(dirNode.attributes["IsExisting"], out outVb) ? outVb : false,
                            IsLocked = bool.TryParse(dirNode.attributes["IsLocked"], out outVb) ? outVb : false,
                        });

                    endingScriptFile = rootNode["EndingScriptFile"][0].Text;
                    if (int.TryParse(rootNode.attributes["TimerIntervalSec"], out int vI) && vI > 0)
                        timerIntervalSec = vI;
                    if (bool.TryParse(rootNode.attributes["WithinSubDirs"], out bool vB))
                        withinSubDirs = vB;
                    if (bool.TryParse(rootNode.attributes["IsWritingLog"], out vB))
                        writeLog = vB;
                    if (bool.TryParse(rootNode.attributes["IsUsingendingSound"], out vB))
                        endingSound = vB;
                }
                else
                {
                    watchintFiles.Add(new DGDataItemFile() { Tx = "*.aac", IsExisting = true, IsLocked = false, });
                    watchintFiles.Add(new DGDataItemFile() { Tx = "*.m4v", IsExisting = true, IsLocked = false, });
                    watchintFiles.Add(new DGDataItemFile() { Tx = "*.mp4", IsExisting = false, IsLocked = true, });
                }
            }
            public void Save()
            {
                MadTomDev.Data.SettingXML.Node rootNode = xml.rootNode;
                rootNode.Clear();
                rootNode.attributes.Clear();

                foreach (string dir in watchintDirs)
                {
                    rootNode.Children.Add(new Data.SettingXML.Node()
                    {
                        nodeName = "WatchingDirs",
                        Text = dir,
                    });
                }
                Data.SettingXML.Node newXmlNode;
                foreach (DGDataItemFile file in watchintFiles)
                {
                    newXmlNode = new Data.SettingXML.Node()
                    {
                        nodeName = "WatchingFiles",
                        Text = file.Tx,
                    };
                    newXmlNode.attributes.AddUpdate("IsExisting", file.IsExisting.ToString());
                    newXmlNode.attributes.AddUpdate("IsLocked", file.IsLocked.ToString());

                    rootNode.Children.Add(newXmlNode);
                }
                rootNode.Children.Add(new Data.SettingXML.Node()
                {
                    nodeName = "EndingScriptFile",
                    Text = endingScriptFile,
                });
                rootNode.attributes.AddUpdate("WithinSubDirs", withinSubDirs.ToString());
                rootNode.attributes.AddUpdate("TimerIntervalSec", timerIntervalSec.ToString());
                rootNode.attributes.AddUpdate("IsWritingLog", writeLog.ToString());
                rootNode.attributes.AddUpdate("IsUsingendingSound", endingSound.ToString());
                xml.Save(xmlFile);
            }

        }

        #endregion


        #region user operates


        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain.SelectedIndex == 1)
            {
                SettingSave();
                if (!File.Exists(settings.endingScriptFile))
                {
                    MessageBox.Show(this, "Script file not exists!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                if (tabMain.SelectedIndex == 0)
                    SettingSave_needSave = true;
                btn_watchingStop_Click(null, null);
            }
        }


        private Ookii.Dialogs.Wpf.VistaFolderBrowserDialog folderDialog;
        private void btn_watchingAddDir_Click(object sender, RoutedEventArgs e)
        {
            if (folderDialog == null)
            {
                folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
                { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), };
            }

            if (folderDialog.ShowDialog() == true)
            {
                string newDir = folderDialog.SelectedPath;
                bool toAdd = true;
                foreach (DGDataItem dir in dgWatchingDirSource)
                {
                    if (dir.Tx == newDir)
                    {
                        MessageBox.Show(this, "Already in watching list!", "Stop",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        toAdd = false;
                        break;
                    }
                }
                if (toAdd)
                {
                    dgWatchingDirSource.Add(new DGDataItem() { Tx = newDir, });
                }
            }

        }

        private void dg_watchingDirs_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // do nothing
        }

        private void dg_watchingFiles_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // remove duplicated items
            DGDataItemFile f1, f2;
            string t1, t2;
            for (int j, i = 0, iv = dgWatchingFileSource.Count - 1; i < iv; ++i)
            {
                f1 = dgWatchingFileSource[i];
                t1 = f1.Tx.ToLower();
                for (j = i + 1; j <= iv; ++j)
                {
                    f2 = dgWatchingFileSource[j];
                    if (t1 == f2.Tx.ToLower())
                    {
                        dgWatchingFileSource.RemoveAt(j);
                        --i; --j; --iv;
                    }
                }
            }
            // select only lock or exist, can't make sure which is select later, de-select if multi-selected            
            for (int j, i = 0, iv = dgWatchingFileSource.Count - 1; i < iv; ++i)
            {
                f1 = dgWatchingFileSource[i];
                if (f1.IsExisting && f1.IsLocked)
                {
                    f1.IsExisting = false;
                    f1.IsLocked = false;
                }
            }
        }

        private Ookii.Dialogs.Wpf.VistaOpenFileDialog fileDialog;
        private void btn_endingScriptFileBrows_Click(object sender, RoutedEventArgs e)
        {
            if (fileDialog == null)
            {
                fileDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            }

            string oriFile = tb_endingScriptFile.Text;
            if (File.Exists(oriFile))
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(oriFile);
                fileDialog.FileName = Path.GetFileName(oriFile);
            }
            else
            {
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            if (fileDialog.ShowDialog() == true)
            {
                settings.endingScriptFile = fileDialog.FileName;
                tb_endingScriptFile.Text = settings.endingScriptFile;
            }
        }

        private void btn_viewLogs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Explorer.exe", loggerDir);
        }

        private void sld_timerInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tb_timerInterval.Text = sld_timerInterval.Value.ToString();
            settings.timerIntervalSec = (int)sld_timerInterval.Value;
        }

        private void btn_watchingStart_Click(object sender, RoutedEventArgs e)
        {
            if (isWatcherRunning)
                return;

            checkingPassCounter = 0;
            checkingStatue = CheckingStatues.None;

            checker.ClearPatterns();
            foreach (DGDataItemFile flt in settings.watchintFiles)
            {
                if (flt.IsExisting || flt.IsLocked)
                    checker.AddPattern(flt.Tx);
            }

            watcherIntervalMS = settings.timerIntervalSec * 1000;
            WatcherStartAsync();
            SetTimerUIStart();
        }

        private void btn_watchingStop_Click(object sender, RoutedEventArgs e)
        {
            if (!isWatcherRunning)
                return;

            WatcherStop();
            SetTimerUIStop();
        }
        private void SetTimerUIStart()
        {
            Dispatcher.Invoke(() =>
            {
                tb_currentState.Text = "Watching in action";
                btn_watchingStart.IsEnabled = false;
                btn_watchingStop.IsEnabled = true;
            });
        }
        private void SetTimerUIStop()
        {
            Dispatcher.Invoke(() =>
            {
                tb_currentState.Text = "Idle";
                btn_watchingStart.IsEnabled = true;
                btn_watchingStop.IsEnabled = false;
            });
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            btn_watchingStop_Click(null, null);
            SettingSave();
        }

        #endregion


        #region datagrid VM

        public ObservableCollection<DGDataItem> dgWatchingDirSource = new ObservableCollection<DGDataItem>();
        public ObservableCollection<DGDataItemFile> dgWatchingFileSource = new ObservableCollection<DGDataItemFile>();

        public class DGDataItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            public void RaisePropertyChangedEvent(string? propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string _Tx;
            public string Tx
            {
                get => _Tx;
                set
                {
                    _Tx = value;
                    RaisePropertyChangedEvent("Tx");
                }
            }
        }
        public class DGDataItemFile : DGDataItem
        {
            private bool _IsLocked = true;
            public bool IsLocked
            {
                get => _IsLocked;
                set
                {
                    _IsLocked = value;
                    RaisePropertyChangedEvent("IsLocked");
                }
            }
            private bool _IsExisting = false;

            public DGDataItemFile()
            {
            }
            public DGDataItemFile(DGDataItemFile file)
            {
                Tx = file.Tx;
                IsLocked = file.IsLocked;
                IsExisting = file.IsExisting;
            }


            public bool IsExisting
            {
                get => _IsExisting;
                set
                {
                    _IsExisting = value;
                    RaisePropertyChangedEvent("IsExisting");
                }
            }
        }

        #endregion


        #region timer, check
        private int checkingPassCounter = 0;
        private List<FileInfo> checkingFileListPre = new List<FileInfo>();
        private List<FileInfo> checkingFileListCur = new List<FileInfo>();
        private FileInfo existFile, lockedFile, writeTimeChangedFile;
        //private string lastCheckedFile = null;
        private enum CheckingStatues
        {
            None, GetingFileList, CompairingFileList,
            CheckingFileExist, ReCheckFileExist,
            CheckingFileChange, ReCheckingFileChange,
            CheckingFileLock, ReCheckingFileLock,
        }
        private CheckingStatues checkingStatue = CheckingStatues.None;

        /// <summary>
        /// 0-deleted; 1-lastWriteTimeChanged; 2-writeLocked;   /// ? 3-readError;
        /// </summary>
        private int lastCheckedFileState = 0;

        private bool isWatcherRunning = false;
        private int watcherIntervalMS;
        private void WatcherStartAsync()
        {
            Task.Run(() =>
            {
                isWatcherRunning = true;
                while (true)
                {
                    if (!isWatcherRunning)
                        return;

                    WatcherTick();
                    Task.Delay(watcherIntervalMS).Wait();
                }
            });
        }
        private void WatcherStop()
        {
            isWatcherRunning = false;
        }
        private async void WatcherTick()
        {
            switch (checkingStatue)
            {
                case CheckingStatues.None:
                    checkingStatue = CheckingStatues.GetingFileList;
                    SetCurrentAction("Re-geting file full list.");

                    checkingFileListCur.Clear();
                    GetFileInfoList(ref checkingFileListCur);
                    checkingFileListCur.Sort(CompairFileInfo);
                    break;
                case CheckingStatues.GetingFileList:
                    checkingStatue = CheckingStatues.CompairingFileList;
                    SetCurrentAction($"Compairing file list.[{checkingFileListCur.Count} files]");
                    if (!CheckFileInfoListEqual(checkingFileListCur, checkingFileListPre))
                    {
                        checkingPassCounter = 0;
                        checkingStatue = CheckingStatues.None;
                    }
                    checkingFileListPre.Clear();
                    for (int i = 0, iv = checkingFileListCur.Count; i < iv; ++i)
                    {
                        checkingFileListPre.Add(checkingFileListCur[i]);
                    }
                    break;
                case CheckingStatues.CompairingFileList:
                    checkingStatue = CheckingStatues.CheckingFileExist;

                    existFile = null;
                    SimpleStringHelper.Checker_starNQues existChecker = new SimpleStringHelper.Checker_starNQues();
                    foreach (DGDataItemFile flt in settings.watchintFiles)
                    {
                        if (flt.IsExisting)
                        {
                            existChecker.ClearPatterns();
                            existChecker.AddPattern(flt.Tx);
                            foreach (FileInfo a in checkingFileListCur)
                            {
                                if (existChecker.Check(a.Name))
                                {
                                    existFile = a;
                                    break;
                                }
                            }
                        }
                        if (existFile != null)
                            break;
                    }
                    if (existFile != null)
                    {
                        SetCurrentAction($"Temporary File detected.[{existFile.Name}]");
                        if (settings.writeLog)
                            LogMsg($"Temporary file [{existFile.FullName}].");

                        checkingPassCounter = 0;
                        //checkingStatue = CheckingStatues.CheckingFileExist;
                    }
                    else
                    {
                        checkingStatue = CheckingStatues.ReCheckFileExist;
                    }
                    break;
                case CheckingStatues.CheckingFileExist:
                    checkingStatue = CheckingStatues.ReCheckFileExist;
                    if (existFile != null && File.Exists(existFile.FullName))
                    {
                        checkingStatue = CheckingStatues.CheckingFileExist;
                    }
                    else
                    {
                        checkingPassCounter = 0;
                        checkingStatue = CheckingStatues.None;
                        existFile = null;
                    }
                    break;
                case CheckingStatues.ReCheckFileExist:
                    checkingStatue = CheckingStatues.CheckingFileChange;

                    List<FileInfo> tmpFileList = new List<FileInfo>();
                    bool isFileGenerated = false;
                    FileInfo? foundFile;
                    GetFileInfoList(ref tmpFileList);
                    foreach (FileInfo f in tmpFileList)
                    {
                        foundFile = checkingFileListCur.Find(a => a.FullName == f.FullName);
                        if (foundFile == null)
                        {
                            SetCurrentAction($"File generate detected.[{f.Name}]");
                            if (settings.writeLog)
                                LogMsg($"Generated file [{f.FullName}].");

                            checkingPassCounter = 0;
                            isFileGenerated = true;
                            writeTimeChangedFile = null;
                            break;
                        }
                        else
                        {
                            if (f.LastWriteTime != foundFile.LastWriteTime)
                            {
                                SetCurrentAction($"File write time changed.[{f.Name}]");
                                if (settings.writeLog)
                                    LogMsg($"Changing file [{f.FullName}] at [{f.LastWriteTime.ToString("yyMMdd HH:mm:ss")}].");

                                checkingPassCounter = 0;
                                writeTimeChangedFile = f;
                                break;
                            }
                            else if (f.Length != foundFile.Length)
                            {
                                // not possible ?
                                SetCurrentAction($"File size changed.[{f.Name}]");
                                if (settings.writeLog)
                                    LogMsg($"Changing file [{f.FullName}] size to [{f.Length}].");

                                checkingPassCounter = 0;
                                writeTimeChangedFile = f;
                                break;
                            }
                            else
                            {
                                writeTimeChangedFile = null;
                            }
                        }
                    }
                    if (writeTimeChangedFile == null)
                    {
                        if (isFileGenerated)
                        {
                            checkingStatue = CheckingStatues.None;
                        }
                        else
                        {
                            checkingStatue = CheckingStatues.ReCheckingFileChange;
                        }
                    }
                    else
                    {
                        //checkingStatue = CheckingStatues.CheckingFileWriteTime;
                    }
                    break;

                case CheckingStatues.CheckingFileChange:
                    checkingStatue = CheckingStatues.ReCheckingFileChange;
                    FileInfo tmpFileInfo = new FileInfo(writeTimeChangedFile.FullName);

                    if (tmpFileInfo.LastWriteTime != writeTimeChangedFile.LastWriteTime)
                    {
                        SetCurrentAction($"File write time changed.[{tmpFileInfo.Name}]");
                        if (settings.writeLog)
                            LogMsg($"Changing file [{tmpFileInfo.FullName}] at [{tmpFileInfo.LastWriteTime.ToString("yyMMdd HH:mm:ss")}].");

                        checkingPassCounter = 0;
                        writeTimeChangedFile = tmpFileInfo;
                        checkingStatue = CheckingStatues.CheckingFileChange;
                    }
                    else if (tmpFileInfo.Length != writeTimeChangedFile.Length)
                    {
                        // not possible ?
                        SetCurrentAction($"File size changed.[{tmpFileInfo.Name}]");
                        if (settings.writeLog)
                            LogMsg($"Changing file [{tmpFileInfo.FullName}] size to [{tmpFileInfo.Length}].");

                        checkingPassCounter = 0;
                        writeTimeChangedFile = tmpFileInfo;
                        checkingStatue = CheckingStatues.CheckingFileChange;
                    }
                    else
                    {
                        writeTimeChangedFile = null;
                    }

                    break;
                case CheckingStatues.ReCheckingFileChange:
                    checkingStatue = CheckingStatues.CheckingFileLock;
                    foreach (FileInfo f in checkingFileListCur)
                    {
                        if (!File.Exists(f.FullName))
                        {
                            checkingPassCounter = 0;
                            checkingStatue = CheckingStatues.None;
                            SetCurrentAction($"File missing detected.[Missing {f.Name}]");
                            if (settings.writeLog)
                                LogMsg($"Missing file [{f.FullName}].");
                            break;
                        }
                        if (CheckFileWriteLocked(f))
                        {
                            checkingPassCounter = 0;
                            SetCurrentAction($"File lock detected.[{f.Name}]");
                            if (settings.writeLog)
                                LogMsg($"Locked file [{f.FullName}].");
                            lockedFile = f;
                            checkingStatue = CheckingStatues.ReCheckingFileLock;
                            break;
                        }
                    }
                    //await Task.Delay(settings.timerIntervalSec * 1000);

                    break;
                case CheckingStatues.ReCheckingFileLock:
                    // lockedFile.Exists does not work when it's gone after;
                    if (File.Exists(lockedFile.FullName))
                    {
                        if (!CheckFileWriteLocked(lockedFile))
                        {
                            checkingPassCounter = 0;
                            checkingStatue = CheckingStatues.None;
                            SetCurrentAction($"Lock released.[{lockedFile.Name}]");
                            if (settings.writeLog)
                                LogMsg($"Lock released [{lockedFile.FullName}].");
                        }
                    }
                    else
                    {
                        checkingPassCounter = 0;
                        checkingStatue = CheckingStatues.None;
                        SetCurrentAction($"Locked file missing detected.[{lockedFile.Name}]");
                        if (settings.writeLog)
                            LogMsg($"Missing locked file [{lockedFile.FullName}].");
                    }
                    //await Task.Delay(settings.timerIntervalSec * 1000);
                    break;
                case CheckingStatues.CheckingFileLock:
                    ++checkingPassCounter;
                    if (checkingPassCounter >= 3)
                    {

                        // **** end of check ****
                        WatcherStop();
                        checkingPassCounter = 0;
                        SetTimerUIStop();

                        if (settings.endingSound)
                        {
                            SystemSounds.Asterisk.Play();
                        }

                        SetCurrentAction("No changing, run ending-script.");
                        RunEndingScript();
                    }
                    else
                    {
                        SetCurrentAction($"All pass for [{checkingPassCounter}] times, run next loop.");
                        //await Task.Delay(settings.timerIntervalSec * 1000);
                    }
                    checkingStatue = CheckingStatues.None;
                    break;
            }
        }
        private void GetFileInfoList(ref List<FileInfo> list)
        {
            foreach (string dir in settings.watchintDirs)
            {
                if (Directory.Exists(dir))
                {
                    GetFileInfoLoop(ref list, new DirectoryInfo(dir), settings.withinSubDirs);
                }
            }
        }

        SimpleStringHelper.Checker_starNQues checker = new SimpleStringHelper.Checker_starNQues();
        private void GetFileInfoLoop(ref List<FileInfo> list, DirectoryInfo baseDir, bool withinSubDir = true)
        {
            bool filtePass;
            try
            {
                foreach (FileInfo file in baseDir.GetFiles())
                {
                    filtePass = true;
                    if (settings.watchintFiles.Count > 0)
                    {
                        if (!checker.Check(file.Name))
                        {
                            filtePass = false;
                            continue;
                        }
                    }
                    if (filtePass)
                    {
                        list.Add(file);
                    }
                }
            }
            catch (Exception err)
            {
                if (settings.writeLog)
                {
                    logger.Log(err);
                }
            }
            if (withinSubDir)
            {
                try
                {
                    foreach (DirectoryInfo dir in baseDir.GetDirectories())
                    {
                        GetFileInfoLoop(ref list, dir, withinSubDir);
                    }
                }
                catch (Exception err)
                {
                    if (settings.writeLog)
                    {
                        logger.Log(err);
                    }
                }
            }
        }
        private int CompairFileInfo(FileInfo a, FileInfo b)
        {
            return a.FullName.CompareTo(b.FullName);
        }
        private bool CheckFileInfoListEqual(List<FileInfo> listA, List<FileInfo> listB)
        {
            if (listA.Count != listB.Count)
                return false;
            FileInfo a, b;
            for (int i = 0, iv = listA.Count; i < iv; ++i)
            {
                a = listA[i]; b = listB[i];
                if (a.FullName != b.FullName)
                {
                    if (settings.writeLog)
                        LogMsg($"File name list dismatch.");
                    return false;
                }
                //if (a.LastWriteTime != b.LastWriteTime)
                //    return false;
                //if (a.Length != b.Length)
                //{
                //    if (settings.writeLog)
                //        LogMsg($"File size changed [{a.Length}] -> [{b.Length}].");
                //    return false;
                //}
            }
            return true;
        }
        private bool CheckFileWriteLocked(FileInfo file)
        {
            Stream s = null;
            bool result = false;
            try
            {
                s = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception)
            {
                result = true;
            }
            finally
            {
                s?.Close();
                s?.Dispose();
            }
            return result;
        }

        private void SetCurrentAction(string msg)
        {
            Dispatcher.Invoke(() =>
            { tb_currentAction.Text = msg; });
            if (settings.writeLog)
                LogMsg(msg);
        }

        #endregion


        #region ending script, log
        private void RunEndingScript()
        {
            if (File.Exists(settings.endingScriptFile))
            {
                if (settings.writeLog)
                    LogMsg($"Running script [{settings.endingScriptFile}].");
                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(settings.endingScriptFile),
                        FileName = settings.endingScriptFile,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    },

                };
                p.Start();
                while (!p.StandardOutput.EndOfStream)
                {
                    LogMsg(p.StandardOutput.ReadLine());
                }
                p.Exited += (s1, e1) =>
                {
                    SetCurrentAction("End of running script.");
                };
            }
            else
            {
                SetCurrentAction("Ending script file not exists!");
            }
        }

        Logger logger;

        private Brush tb_endingScriptFile_background_nor = SystemColors.WindowBrush;
        private Brush tb_endingScriptFile_background_err = new SolidColorBrush(Colors.Orange);
        private void tb_endingScriptFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filePath = tb_endingScriptFile.Text;
            if (File.Exists(filePath))
            {
                tb_endingScriptFile.Background = tb_endingScriptFile_background_nor;
            }
            else
            {
                tb_endingScriptFile.Background = tb_endingScriptFile_background_err;
            }
        }


        private void LogMsg(string? msg)
        {
            if (msg == null)
                return;
            Dispatcher.Invoke(() =>
            {
                logger.Log(msg);
            });
        }

        #endregion

    }
}
