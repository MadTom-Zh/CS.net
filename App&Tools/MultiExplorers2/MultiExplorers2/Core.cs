using MadTomDev.App;
using MadTomDev.App.Ctrls;
using MadTomDev.Data;
using MadTomDev.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MadTomDev.UI.VisualHelper;
using static System.Windows.Forms.AxHost;
using DataGrid = System.Windows.Controls.DataGrid;
using TreeView = System.Windows.Controls.TreeView;

namespace MadTomDev.App
{
    public class Core : IDisposable
    {
        private Core()
        {
            driveDector.DrivePluged += DriveDector_DrivePluged;
            transferManager.RecycleBinGenerated += TransferManager_RecycleBinGenerated;
        }


        private static Core instance = null;
        public static Core GetInstance()
        {
            if (instance == null)
            {
                instance = new Core();
            }
            return instance;
        }


        #region varables

        private Logger logger = Logger.Instance;

        public Common.IconHelper.Shell32Icons iconS32 = Common.IconHelper.Shell32Icons.Instance;
        public Common.IconHelper.FileSystem iconFS = Common.IconHelper.FileSystem.Instance;
        public Common.IconHelper.SystemIcons iconSys = Common.IconHelper.SystemIcons.Instance;

        public MainWindow mainWindow;
        public Explorer currentExplorer = null;
        public ExplorerContextMenu explorerContextMenu;
        Setting setting = Setting.Instance;

        private TransferManager transferManager = TransferManager.GetInstance();

        #endregion


        internal void InitMainWindow()
        {
            if (string.IsNullOrWhiteSpace(setting.language))
            {
                settingsLanguage.TrySetDefaultLang(Application.Current.Resources);
            }
            else
            {
                settingsLanguage.TrySetLang(Application.Current.Resources, setting.language);
            }


            #region load current layout, and layout btns
            if (setting.curLayout != null)
            {
                LayoutApply(setting.curLayout);
            }
            else
            {
                Setting.Layout defaultLayout = new Setting.Layout()
                {
                    size = new Setting.Layout.IntPair() { x = 2, y = 1 },
                    tipTx = "init layout",
                };
                defaultLayout.explorerList.Add(new Setting.Layout.Explorer()
                { posi = new Setting.Layout.IntPair() { x = 0, y = 0 } });
                defaultLayout.explorerList.Add(new Setting.Layout.Explorer()
                { posi = new Setting.Layout.IntPair() { x = 1, y = 0 } });
                LayoutApply(defaultLayout);
            }

            StackPanel sPanel = mainWindow.sPanel_layoutBtns;
            sPanel.Children.Clear();
            setting.layouts.Sort((a, b) => a.idx - b.idx);
            for (int i = 0, iv = setting.layouts.Count; i < iv; i++)
            {
                AddLayoutButton(setting.layouts[i]);
            }

            //ReloadDisksAsync();
            //ReloadHostsAsync();
            #endregion

            explorerContextMenu = new ExplorerContextMenu();
            explorerContextMenu.itemPaste.actionClick = ContextMenu_ActionPaste;
            explorerContextMenu.itemDelete.actionClick = ContextMenu_ActionDelete;
            explorerContextMenu.itemRename.actionClick = ContextMenu_ActionRename;
            explorerContextMenu.ActionOpenDir = ContextMenu_ActionOpenDir;
            explorerContextMenu.ActionNewFileDirToCreate = ContextMenu_ActionNewFileOrDirToCreate;
            explorerContextMenu.ActionManageOpenWith_setLanguage = ContextMenu_ActionManageOpenWith_setLanguage;
            explorerContextMenu.ActionCopyRaised += ExplorerContextMenu_ActionCopyRaised;
            explorerContextMenu.ActionCutRaised += ExplorerContextMenu_ActionCutRaised;

            logger.EventLevelUp += mainWindow.Logger_EventLevelUp;


            if (setting.isFileSystemWatcherEnabled)
            {
                FsWatchingStartAll();
            }
            else
            {
                FsWatchingStopAll();
            }
        }

        private void ExplorerContextMenu_ActionCopyRaised(ExplorerContextMenu cMenu, string[] files)
        {
            contextMenuExplorer.justCopyCutFiles_forSelecting = files;
            BroadcastCopy(files);
        }
        private void ExplorerContextMenu_ActionCutRaised(ExplorerContextMenu cMenu, string[] files)
        {
            contextMenuExplorer.justCopyCutFiles_forSelecting = files;
            BroadcastCut(files);
        }

        public string[] justCopyCutFiles;
        public void BroadcastCopy(string[] files)
        {
            justCopyCutFiles = files;
            FilesCopyed?.Invoke(this, files);
        }
        public void BroadcastCut(string[] files)
        {
            justCopyCutFiles = files;
            FilesCut?.Invoke(this, files);
        }





        #region layout

        internal async void LayoutApply(Setting.Layout layout)
        {
            if (layout.size == null)
                return;

            mainWindow.Cursor = Cursors.AppStarting;

            UniformGrid uGrid = mainWindow.uGrid;
            if (uGrid.Rows == 1 && uGrid.Columns == 1)
            {
                uGrid.Children.Clear();
                CellControlPanel ccp = new CellControlPanel()
                {
                    Position = new Setting.Layout.IntPair() { x = 0, y = 0 },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                uGrid.Children.Insert(0, ccp);
            }
            while (uGrid.Columns < layout.size.x)
                LayoutAddCol(uGrid, false);
            while (uGrid.Rows < layout.size.y)
                LayoutAddRow(uGrid, false);
            GetAllExplorers();

            Setting.Layout.Explorer foundE;
            for (int x, y = 0, xV = layout.size.x, yV = layout.size.y; y < yV; ++y)
            {
                for (x = 0; x < xV; ++x)
                {
                    foundE = layout.explorerList.Find(e => e.posi.x == x && e.posi.y == y);
                    if (foundE == null)
                    {
                        LayoutRemoveExplorer(allExplorers[y][x]);
                    }
                    else
                    {
                        LayoutSetUI(uGrid, foundE.posi, foundE.bgClr, foundE.url);
                    }
                }
            }
            if (layout.size.x < uGrid.Columns || layout.size.y < uGrid.Rows)
            {
                while (layout.size.x < uGrid.Columns)
                    LayoutRemoveCol(uGrid.Columns - 1, false, false);
                while (layout.size.y < uGrid.Rows)
                    LayoutRemoveRow(uGrid.Rows - 1, false, false);
                GetAllExplorers();
                //ResetCCPs();
            }
            mainWindow.Cursor = Cursors.Arrow;
        }


        private void AddLayoutButton(Setting.Layout layout)
        {
            BtnLayout btnLayout = new BtnLayout();
            btnLayout.Init(layout);
            mainWindow.sPanel_layoutBtns.Children.Add(btnLayout);
            btnLayout.ActionClick = mainWindow.LayoutBtnClickHandler;
            btnLayout.ActionRightClick = mainWindow.LayoutBtnRightClickHandler;
        }
        internal void LayoutUpdate(Setting.Layout layout)
        {
            BtnLayout btn = LayoutFindBtn(layout);
            if (btn != null)
            {
                Setting.Layout curLayout = GetLayoutSettingFromCurrent(btn, layout.c, layout.idx);
                setting.layouts[setting.layouts.IndexOf(layout)] = curLayout;
                btn.Init(curLayout);
                setting.Save();
            }
        }
        private BtnLayout LayoutFindBtn(Setting.Layout layout)
        {
            BtnLayout btn;
            for (int i = mainWindow.sPanel_layoutBtns.Children.Count - 1; i >= 0; i--)
            {
                btn = (BtnLayout)mainWindow.sPanel_layoutBtns.Children[i];
                if (btn.layout == layout)
                {
                    return btn;
                }
            }
            return null;
        }
        internal void layoutDelete(Setting.Layout layout)
        {
            BtnLayout btn = LayoutFindBtn(layout);
            if (btn != null)
            {
                mainWindow.sPanel_layoutBtns.Children.Remove(btn);
                setting.layouts.Remove(layout);
            }
        }




        public Explorer[][] GetAllExplorers()
        {
            UniformGrid uGrid = mainWindow.uGrid;
            int i = 0, r, rv = uGrid.Rows, c, cv = uGrid.Columns;
            allExplorers = new Explorer[rv][];
            UIElement ui;
            for (r = 0; r < rv; r++)
            {
                allExplorers[r] = new Explorer[cv];
                for (c = 0; c < cv; c++)
                {
                    ui = uGrid.Children[i++];
                    if (ui is Explorer)
                    {
                        allExplorers[r][c] = (Explorer)ui;
                    }
                }
            }
            return allExplorers;
        }
        public Explorer[][] allExplorers;
        public int CountExplorerAll()
        {
            int result = 0;
            foreach (Explorer[] roe in allExplorers)
            {
                foreach (Explorer e in roe)
                {
                    if (e != null)
                        result++;
                }
            }
            return result;
        }
        public int CountExplorerWithinRow(int rIdx)
        {
            int result = 0;
            if (0 <= rIdx && rIdx < allExplorers.Length)
            {
                foreach (Explorer e in allExplorers[rIdx])
                {
                    if (e != null)
                        result++;
                }
            }
            return result;
        }
        public int CountExplorerWithinCol(int cIdx)
        {
            int result = 0;
            if (allExplorers.Length > 0
                && 0 <= cIdx && cIdx < allExplorers[0].Length)
            {
                foreach (Explorer[] roe in allExplorers)
                {
                    if (roe[cIdx] != null)
                        result++;
                }
            }
            return result;
        }
        public async void LayoutSetUI(UniformGrid uGrid, Setting.Layout.IntPair posi, Color bgClr, string uri)
        {
            int idx = uGrid.Columns * posi.y + posi.x;
            UIElement child = uGrid.Children[idx];
            Explorer explorer;
            if (child is Explorer)
            {
                explorer = (Explorer)child;
                explorer.Background = new SolidColorBrush(bgClr);
            }
            else
            {
                uGrid.Children.Remove(child);
                explorer = new Explorer()
                { Background = new SolidColorBrush(bgClr), };
                uGrid.Children.Insert(idx, explorer);
                explorer.Init();
            }
            if (string.IsNullOrWhiteSpace(uri))
            {
                explorer.treeViewRoot_quickAccess.IsExpanded = true;
                explorer.treeViewRoot_thisPC.IsSelected = true;
            }
            else
            {
                explorer.NavigateTo(uri);
            }
            allExplorers[posi.y][posi.x] = explorer;
        }
        internal void LayoutRemoveRow(CellControlPanel caller, bool reGetExplorers = true)
        {
            if (CountExplorerAll() - CountExplorerWithinRow(caller.Position.y) < 2)
                return;
            LayoutRemoveRow(caller.Position.y, reGetExplorers);
        }
        private void LayoutRemoveRow(int rowIdx, bool reGetExplorers = true, bool reSetCCPs = true)
        {
            UniformGrid uGrid = mainWindow.uGrid;
            for (int i = (rowIdx + 1) * uGrid.Columns - 1, count = uGrid.Columns;
                count > 0; i--, count--)
            {
                uGrid.Children.RemoveAt(i);
            }
            uGrid.Rows -= 1;
            if (reGetExplorers)
                GetAllExplorers();
            if (reSetCCPs)
                ResetCCPs();
        }
        public void LayoutRemoveExplorer(Explorer explorer)
        {
            if (explorer == null)
                return;
            if (CountExplorerAll() < 3)
                return;

            UniformGrid uGrid = mainWindow.uGrid;
            int idx = uGrid.Children.IndexOf(explorer);
            int cols = uGrid.Columns;
            int x = idx % cols, y = idx / cols;
            CellControlPanel ccp = new CellControlPanel()
            {
                Position = new Setting.Layout.IntPair() { x = x, y = y },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            uGrid.Children.Remove(explorer);
            uGrid.Children.Insert(idx, ccp);
            allExplorers[y][x] = null;
        }

        private void ResetCCPs()
        {
            UniformGrid uGrid = mainWindow.uGrid;
            object testUI;
            for (int i = 0, r = 0, rv = uGrid.Rows, c, cv = uGrid.Columns;
                r < rv; r++)
            {
                for (c = 0; c < cv; c++)
                {
                    testUI = uGrid.Children[i++];
                    if (testUI is CellControlPanel)
                    {
                        ((CellControlPanel)testUI).Position
                            = new Setting.Layout.IntPair() { x = c, y = r };
                    }
                }
            }
        }

        internal void LayoutRemoveCol(CellControlPanel caller, bool reGetExplorers = true)
        {
            if (CountExplorerAll() - CountExplorerWithinCol(caller.Position.x) < 2)
                return;

            LayoutRemoveCol(caller.Position.x, reGetExplorers);
        }
        private void LayoutRemoveCol(int colIdx, bool reGetExplorers = true, bool reSetCCPs = true)
        {
            UniformGrid uGrid = mainWindow.uGrid;
            int colCount = uGrid.Columns;
            for (int i = uGrid.Columns * (uGrid.Rows - 1) + colIdx, count = uGrid.Rows;
                count > 0; i -= colCount, count--)
            {
                uGrid.Children.RemoveAt(i);
            }
            uGrid.Columns -= 1;
            if (reGetExplorers)
                GetAllExplorers();
            if (reSetCCPs)
                ResetCCPs();
        }

        internal void LayoutAddExplorer(CellControlPanel caller)
        {
            UniformGrid uGrid = mainWindow.uGrid;
            int idx = caller.Position.y * uGrid.Columns + caller.Position.x;

            Explorer explorer = new Explorer();
            uGrid.Children.RemoveAt(idx);
            uGrid.Children.Insert(idx, explorer);
            explorer.Init();

            // call get root items....
            explorer.treeViewRoot_quickAccess.IsExpanded = true;
            explorer.treeViewRoot_thisPC.IsSelected = true;

            allExplorers[caller.Position.y][caller.Position.x] = explorer;
            currentExplorer = explorer;
            explorer.Focus();
            explorer.dataGrid.Focus();
        }

        internal void LayoutAddRow(UniformGrid uGrid, bool reGetExplorers = true)
        {
            int r = uGrid.Rows;
            int c0 = uGrid.Columns * r;
            uGrid.Rows += 1;
            CellControlPanel ccp;
            for (int c = 0, cv = uGrid.Columns; c < cv; c++)
            {
                // 控制面板还需要位置参数；
                ccp = new CellControlPanel()
                {
                    Position = new Setting.Layout.IntPair() { x = c, y = r },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                uGrid.Children.Insert(c0 + c, ccp);
            }
            if (reGetExplorers)
                GetAllExplorers();
        }

        internal void LayoutAddCol(UniformGrid uGrid, bool reGetExplorers = true)
        {
            int c = uGrid.Columns;
            uGrid.Columns += 1;
            int newCols = uGrid.Columns;
            CellControlPanel ccp;
            for (int r = 0, rv = uGrid.Rows; r < rv; r++)
            {
                // 控制面板还需要位置参数；
                ccp = new CellControlPanel()
                {
                    Position = new Setting.Layout.IntPair() { x = c, y = r },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                uGrid.Children.Insert(newCols * (r + 1) - 1, ccp);
            }
            if (reGetExplorers)
                GetAllExplorers();
        }

        private Setting.Layout GetLayoutSettingFromCurrent(BtnLayout referBtn = null, char layoutChar = '?', int idx = 0)
        {
            UniformGrid uGrid = mainWindow.uGrid;
            Setting.Layout result = new Setting.Layout()
            {
                size = new Setting.Layout.IntPair()
                { x = uGrid.Columns, y = uGrid.Rows },
                c = layoutChar,
                bgClr = referBtn == null ? Colors.Transparent : ((SolidColorBrush)referBtn.grid.Background).Color,
                foreClr = referBtn == null ? Colors.Black : ((SolidColorBrush)referBtn.tb_char.Foreground).Color,
                sizeClr = referBtn == null ? Colors.Lime : ((SolidColorBrush)referBtn.tb_size.Foreground).Color,
                idx = idx,
                tipTx = referBtn == null
                    ? GetLangTx("txBtnLayout_layout", DateTime.Now.ToString("yyyy MMdd HHmm"))
                    : referBtn.tb_tt.Text,
            };


            Explorer e;
            Setting.Layout.Explorer es;
            for (int r = 0, rv = allExplorers.Length, c, cv = allExplorers[0].Length; r < rv; r++)
            {
                for (c = 0; c < cv; c++)
                {
                    e = allExplorers[r][c];
                    if (e != null)
                    {
                        es = new Setting.Layout.Explorer()
                        {
                            posi = new Setting.Layout.IntPair() { x = c, y = r },
                            url = e.GetTreeNodeURL(),
                        };
                        if (e.Background is SolidColorBrush)
                            es.bgClr = ((SolidColorBrush)e.Background).Color;
                        else
                            es.bgClr = Colors.Transparent;
                        result.explorerList.Add(es);
                    }
                }
            }
            return result;
        }
        internal void LayoutSaveNew()
        {
            char lc = 'A';
            if (setting.layouts.Count > 0)
            {
                lc = setting.layouts[setting.layouts.Count - 1].c;
                if (lc < 'Z')
                    lc = (char)(lc + 1);
                else
                    lc = 'A';
            }
            Setting.Layout newLayout = GetLayoutSettingFromCurrent(null, lc, setting.layouts.Count);
            setting.layouts.Add(newLayout);

            AddLayoutButton(newLayout);
        }

        #endregion



        #region reload disk, dir, host

        public class ReloadItemInfo
        {
            public string Name;
            public DriveInfoShadow diskInfo;
            public IOInfoShadow ioInfo;
            public Exception Err;
            public bool isDir;
            public bool hasSubDir;
            public bool hasSubFile;
        }
        public delegate void ReloadDirCompleteDelegate(Core sender, bool? nullFull_trueAdd_falseRemove, string basePath, List<ReloadItemInfo> dirs, List<ReloadItemInfo> files);
        public event ReloadDirCompleteDelegate ReloadDirStart;
        public event ReloadDirCompleteDelegate ReloadDirComplete;
        public delegate void FileRenamedDelegate(Core sender, string oldFullName, string newName);
        public event FileRenamedDelegate FileRenamed;

        public event Action<Core, string[]> FilesCopyed;
        public event Action<Core, string[]> FilesCut;

        public event Action<Core> RecycleBinItemFound;

        public HashSet<string> ReloadDirList = new HashSet<string>();

        public Task ReloadDisksAsync()
        {
            if (ReloadDirList.Contains("disks"))
            {
                return Task.Factory.StartNew(() =>
                {
                    while (ReloadDirList.Contains("disks"))
                    {
                        Task.Delay(50).Wait();
                    }
                });
            }

            LogStoreageAccess("disks");
            ReloadDirStart?.Invoke(this, null, null, null, null);
            ReloadDirList.Add("disks");

            return Task.Factory.StartNew(async () =>
            {
                List<ReloadItemInfo> subDirs = new List<ReloadItemInfo>();
                ReloadItemInfo rii;
                DirectoryInfo dii;
                foreach (DriveInfo dsc in DriveInfo.GetDrives())
                {
                    if (!dsc.IsReady)
                        continue;
                    try
                    {
                        dii = dsc.RootDirectory;
                        rii = new ReloadItemInfo()
                        {
                            Name = dsc.Name,
                            diskInfo = new DriveInfoShadow(dsc),
                            //ioInfo = new IOInfoShadow(dii),
                            isDir = true,
                            Err = null,
                            hasSubFile = false,
                            hasSubDir = false,
                        };
                        subDirs.Add(rii);
                        try
                        {
                            rii.hasSubFile = dii.EnumerateFiles().Any();
                            rii.hasSubDir = dii.EnumerateDirectories().Any();
                        }
                        catch (Exception err2)
                        {
                            rii.Err = err2;
                        }
                    }
                    catch (Exception err)
                    {
                        ;
                    }
                }

                ReloadDirList.Remove("disks");
                ReloadDirComplete?.Invoke(this, null, null, subDirs, null);
            });
        }
        public Task ReloadDirAsync(string dirPath)
        {
            if (ReloadDirList.Contains(dirPath))
            {
                return Task.Factory.StartNew(() =>
                {
                    while (ReloadDirList.Contains(dirPath))
                    {
                        Task.Delay(50).Wait();
                    }
                });
            }

            LogStoreageAccess(dirPath);
            ReloadDirStart?.Invoke(this, null, dirPath, null, null);
            ReloadDirList.Add(dirPath);

            return Task.Factory.StartNew(() =>
            {
                DirectoryInfo di = null;
                try
                {
                    di = new DirectoryInfo(dirPath);
                }
                catch (Exception err)
                {
                    if (setting.isLogEnabled && setting.logFlagError)
                    {
                        logger.LogError(err);
                    }
                    MessageBox.Show(err.Message);
                }
                List<ReloadItemInfo> subDirs = new List<ReloadItemInfo>();
                List<ReloadItemInfo> subFiles = new List<ReloadItemInfo>();

                if (di != null)
                {
                    try
                    {
                        foreach (FileInfo fi in di.GetFiles())
                        {
                            subFiles.Add(new ReloadItemInfo()
                            {
                                Name = fi.Name,
                                ioInfo = new IOInfoShadow(fi),
                                isDir = false,
                                Err = null,
                                hasSubFile = false,
                                hasSubDir = false,
                            });
                        }
                    }
                    catch (Exception err)
                    {
                        ;
                    }
                    ReloadItemInfo rii;
                    try
                    {
                        foreach (DirectoryInfo dii in di.GetDirectories())
                        {
                            rii = new ReloadItemInfo()
                            {
                                Name = dii.Name,
                                ioInfo = new IOInfoShadow(dii),
                                isDir = true,
                                Err = null,
                                hasSubFile = false,
                                hasSubDir = false,
                            };
                            subDirs.Add(rii);
                            try
                            {
                                rii.hasSubFile = dii.EnumerateFiles().Any();
                                rii.hasSubDir = dii.EnumerateDirectories().Any();
                            }
                            catch (Exception err2)
                            {
                                rii.Err = err2;
                                rii.ioInfo.dirError = err2;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        ;
                    }
                }

                ReloadDirList.Remove(dirPath);
                if (setting.isFileSystemWatcherEnabled)
                    FsWatchingStartAsync(dirPath);
                ReloadDirComplete?.Invoke(this, null, dirPath, subDirs, subFiles);
            });
        }


        public delegate void ReloadHostsCompleteDelegate(Core sender, List<string> hostList);
        public event ReloadHostsCompleteDelegate ReloadHostsStart;
        public event ReloadHostsCompleteDelegate ReloadHostsComplete;

        public bool ReloadHostsFlag = false;
        public Task ReloadHostsAsync()
        {
            if (ReloadHostsFlag)
            {
                return Task.Factory.StartNew(() =>
                {
                    while (ReloadHostsFlag)
                    {
                        Task.Delay(50).Wait();
                    }
                });
            }

            LogStoreageAccess("hosts");
            ReloadHostsStart?.Invoke(this, null);
            ReloadHostsFlag = true;

            return Task.Factory.StartNew(() =>
            {
                List<string> hostList = new List<string>();
                foreach (string hostName in Network.Hosts.GetVisibleComputers())
                {
                    hostList.Add(hostName);
                }

                ReloadHostsFlag = false;
                ReloadHostsComplete?.Invoke(this, hostList);
            });
        }
        public void ReloadHostsComplete_fakeCall(List<string> fakeHostNameList)
        {
            ReloadHostsComplete?.Invoke(this, fakeHostNameList);
        }
        internal void ReloadDirComplete_removeIOs_fakeCall(string basePath)
        {
            ReloadDirComplete?.Invoke(this, false, basePath, null, null);
        }

        public Task ReloadHostAsync(string hostName)
        {
            string hostFullPath = "\\\\" + hostName;
            if (ReloadDirList.Contains(hostName))
            {
                return Task.Factory.StartNew(() =>
                {
                    while (ReloadDirList.Contains(hostName))
                    {
                        Task.Delay(50).Wait();
                    }
                });
            }

            LogStoreageAccess(hostFullPath);
            ReloadDirStart?.Invoke(this, null, hostFullPath, null, null);
            ReloadDirList.Add(hostName);

            return Task.Factory.StartNew(() =>
            {
                string subDirFullName;
                DirectoryInfo subDirInfo;
                List<ReloadItemInfo> dirs = new List<ReloadItemInfo>();
                ReloadItemInfo dir;
                foreach (Network.Hosts.SHARE_INFO_1 si in Network.Hosts.EnumNetShares(hostName))
                {
                    if (!si.shi1_netname.StartsWith("ERROR=")
                        && !si.shi1_netname.EndsWith('$'))
                    {
                        subDirFullName = Path.Combine(hostFullPath, si.shi1_netname);
                        subDirInfo = new DirectoryInfo(subDirFullName);
                        dir = new ReloadItemInfo()
                        {
                            hasSubFile = false,
                            hasSubDir = false,
                            isDir = true,
                            Err = null,
                            Name = si.shi1_netname,
                            ioInfo = new IOInfoShadow(subDirInfo),
                        };
                        try
                        {
                            dir.hasSubFile = subDirInfo.EnumerateFiles().Any();
                            dir.hasSubDir = subDirInfo.EnumerateDirectories().Any();
                        }
                        catch (Exception err1)
                        {
                            dir.hasSubFile = false;
                            dir.hasSubDir = false;
                            dir.Err = err1;
                        }
                        dirs.Add(dir);
                    }
                }

                ReloadDirList.Remove(hostName);
                ReloadDirComplete?.Invoke(this, null, hostFullPath, dirs, null);
            });
        }


        Resource.DriveDetector driveDector = Resource.DriveDetector.GetInstance();

        private void DriveDector_DrivePluged(char driveLetter, bool plugedInOrOut)
        {
            if (plugedInOrOut)
                LogGeneral(GetLangTx("txLog_drivePlugedIn", driveLetter.ToString()));
            else
                LogGeneral(GetLangTx("txLog_drivePlugedOut", driveLetter.ToString()));
            List<ReloadItemInfo> diskList = new List<ReloadItemInfo>();
            string driveName = driveLetter + ":";

            LogDrivePlug(
                new DriveInfoShadow(new DriveInfo(driveName)),
                plugedInOrOut);

            if (plugedInOrOut)
            {
                ReloadDirStart?.Invoke(this, true, driveName, null, null);
                DirectoryInfo dii = new DirectoryInfo(driveName);
                ReloadItemInfo rii = new ReloadItemInfo()
                {
                    Name = driveName,
                    ioInfo = new IOInfoShadow(dii),
                    isDir = true,
                    Err = null,
                    hasSubFile = false,
                    hasSubDir = false,
                };
                try
                {
                    rii.hasSubFile = dii.EnumerateFiles().Any();
                    rii.hasSubDir = dii.EnumerateDirectories().Any();
                }
                catch (Exception err2)
                {
                    rii.Err = err2;
                }
                diskList.Add(rii);

                ReloadDirComplete?.Invoke(this, true, null, diskList, null);
            }
            else
            {
                DirectoryInfo dii = new DirectoryInfo(driveName);
                ReloadItemInfo rii = new ReloadItemInfo()
                {
                    Name = driveName,
                    ioInfo = new IOInfoShadow(dii),
                    isDir = true,
                    Err = null,
                    hasSubFile = false,
                    hasSubDir = false,
                };
                diskList.Add(rii);
                ReloadDirComplete?.Invoke(this, false, null, diskList, null);
            }
        }

        #endregion



        #region file system watcher

        public Dictionary<string, FileSystemWatcher> fsWatcherDict = new Dictionary<string, FileSystemWatcher>();

        public void FsWatchingStartAll()
        {
            foreach (Explorer[] expRow in allExplorers)
            {
                foreach (Explorer exp in expRow)
                {
                    if (exp == null)
                        continue;
                    FsWatchingStartAsync(exp.GetTreeNodeURL());
                }
            }
        }
        public void FsWatchingRemoveUnwanted()
        {
            string path;
            List<string> restPath = new List<string>();
            restPath.AddRange(fsWatcherDict.Keys);
            foreach (Explorer[] expRow in allExplorers)
            {
                foreach (Explorer exp in expRow)
                {
                    if (exp == null)
                        continue;
                    path = exp.GetTreeNodeURL();
                    if (restPath.Contains(path))
                    {
                        restPath.Remove(path);
                    }
                }
            }
            if (restPath.Count > 0)
            {

                LogGeneral(GetLangTx("txLog_removeUnwantedFS", restPath.Count.ToString()));
                foreach (string oldPath in restPath)
                {
                    FsWatchingStop(oldPath);
                }
            }
        }
        public void FsWatchingStopAll()
        {
            LogGeneral(GetLangTx("txLog_FSStopAll"));
            List<string> allPath = new List<string>();
            allPath.AddRange(fsWatcherDict.Keys);
            foreach (string p in allPath)
            {
                FsWatchingStop(p);
            }
        }

        private object FsWatchingLocker = new object();
        private int FsWatchingStart_execCount = 0;
        private bool FsWatchingStart_notRemovingUnwanted = true;
        public Task FsWatchingStartAsync(string newPath, bool autoRemoveUnwanted = true)
        {
            return Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(newPath)
                    && Directory.Exists(newPath))
                {
                    lock (FsWatchingLocker)
                    {
                        if (!fsWatcherDict.ContainsKey(newPath))
                        {

                            LogGeneral(GetLangTx("txLog_FSStartPath", newPath));
                            ++FsWatchingStart_execCount;
                            FileSystemWatcher newFsWatcher = new FileSystemWatcher(newPath)
                            { IncludeSubdirectories = false, };
                            try
                            {
                                newFsWatcher.EnableRaisingEvents = true;
                                newFsWatcher.Changed += FsWatcher_Changed;
                                newFsWatcher.Created += FsWatcher_Created;
                                newFsWatcher.Deleted += FsWatcher_Deleted;
                                newFsWatcher.Renamed += FsWatcher_Renamed;
                                newFsWatcher.Error += FsWatcher_Error;
                                fsWatcherDict.Add(newPath, newFsWatcher);
                            }
                            catch (Exception)
                            {
                            }
                            --FsWatchingStart_execCount;
                        }
                    }
                }
                if (FsWatchingStart_notRemovingUnwanted && autoRemoveUnwanted && FsWatchingStart_execCount == 0)
                {
                    FsWatchingStart_notRemovingUnwanted = false;
                    Task.Delay(50).Wait();
                    FsWatchingRemoveUnwanted();
                    FsWatchingStart_notRemovingUnwanted = true;
                }
            });
        }



        #region fs watcher events
        private void FsWatcher_Created(object sender, FileSystemEventArgs e)
        {
            string fp = e.FullPath, em = e.ToString();
            mainWindow.Dispatcher.Invoke(() =>
            {

                LogFsWatcherEvent(GetLangTx("txLog_FSFileCreated"), em);
                // FileSystemWatcher fsWatcher = (FileSystemWatcher)sender;
                ReloadDirComplete?.Invoke(this, true, fp, null, null);
            });
            FsWatcher_delayRefresh(fp);
        }
        private void FsWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            string fp = e.FullPath, em = e.ToString();
            mainWindow.Dispatcher.Invoke(() =>
            {
                LogFsWatcherEvent(GetLangTx("txLog_FSFileDeleted"), em);
                ReloadDirComplete?.Invoke(this, false, fp, null, null);
            });
            FsWatcher_delayRefresh(fp);
        }
        private void FsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            string fp = e.FullPath, em = e.ToString();
            mainWindow.Dispatcher.Invoke(() =>
            {
                LogFsWatcherEvent(GetLangTx("txLog_FSFileChanged"), em);
                ReloadDirComplete?.Invoke(this, true, fp, null, null);
            });
            FsWatcher_delayRefresh(fp);
        }
        private void FsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string ofp = e.OldFullPath, n = e.Name, em = e.ToString();
            mainWindow.Dispatcher.Invoke(() =>
            {
                LogFsWatcherEvent(GetLangTx("txLog_FSFileRenamed"), em);
                FileRenamed?.Invoke(this, ofp, n);
            });
        }
        private void FsWatcher_Error(object sender, ErrorEventArgs e)
        {
            string em = e.ToString();
            mainWindow.Dispatcher.Invoke(() =>
            {
                LogFsWatcherEvent(GetLangTx("txLog_FSFileError"), em);
                LogError(e.GetException());
            });
        }

        private DateTime FsWatcher_delayRefresh_actionTime = DateTime.MinValue;
        private string FsWatcher_delayRefresh_actionPath;
        private TimeSpan FsWatcher_delayRefresh_waitTime = new TimeSpan(0, 0, 0, 0, 500);
        private bool FsWatcher_delayRefresh_isWaiting = false;
        private void FsWatcher_delayRefresh(string actionPath)
        {
            FsWatcher_delayRefresh_actionTime = DateTime.Now;
            FsWatcher_delayRefresh_actionPath = actionPath;
            if (FsWatcher_delayRefresh_isWaiting)
                return;

            FsWatcher_delayRefresh_isWaiting = true;
            Task.Run(() =>
            {
                do
                {
                    Task.Delay(50).Wait();
                }
                while ((DateTime.Now - FsWatcher_delayRefresh_actionTime).Ticks > FsWatcher_delayRefresh_actionTime.Ticks);

                ReloadDirAsync(FsWatcher_delayRefresh_actionPath);
                FsWatcher_delayRefresh_isWaiting = false;
            });
        }


        private void LogFsWatcherEvent(string title, string eMsg)
        {
            if (setting.isLogEnabled && setting.logFlagFSWatcher)
            {
                logger.LogInfo(title, eMsg);
            }
        }
        private void LogFsWatcherEvent(string title, ErrorEventArgs e)
        {
            if (setting.isLogEnabled && setting.logFlagFSWatcher)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    logger.LogInfo(GetLangTx("txLog_FSFileError"), null);
                    logger.LogError(e.GetException());
                });
            }
        }
        #endregion


        public Task<bool> FsWatchingStop(string oldPath)
        {
            return Task.Run(() =>
            {
                if (fsWatcherDict.ContainsKey(oldPath))
                {
                    FileSystemWatcher fsWatcher = fsWatcherDict[oldPath];
                    fsWatcher.Changed -= FsWatcher_Changed;
                    fsWatcher.Created -= FsWatcher_Created;
                    fsWatcher.Deleted -= FsWatcher_Deleted;
                    fsWatcher.Renamed -= FsWatcher_Renamed;
                    fsWatcher.Error -= FsWatcher_Error;
                    fsWatcher.Dispose();
                    lock (FsWatchingLocker)
                    {
                        fsWatcherDict.Remove(oldPath);
                    }
                    return true;
                }
                return false;
            });
        }




        #endregion



        #region context menu


        internal Explorer contextMenuExplorer; // sender explorer
        internal Control contextMenuCtrl; // treeview or datagrid
        private void ContextMenu_ActionPaste(EMItemModel clickedBtn)
        {
            if (Clipboard.ContainsFileDropList())
            {
                string[] files = Utilities.ClipBoard.GetFileDrops(out DragDropEffects ddEffect);
                if (files != null && files.Length > 0)
                {
                    Utilities.ClipBoard.Clear();
                }
                List<string> targetList = new List<string>();
                for (int i = 0, iv = files.Length; i < iv; ++i)
                {
                    targetList.Add(Path.Combine(explorerContextMenu.dirFullName, Path.GetFileName(files[i])));
                }
                TransferManager.TransferTask task = transferManager.TransferTaskAdd(
                    ddEffect == DragDropEffects.Move ? TransferManager.TaskTypes.Move : TransferManager.TaskTypes.Copy,
                    files,
                    targetList.ToArray(),
                    setting.sameNameDirHandleType,
                    setting.sameNameFileHandleType,
                    null);
                TransferTaskListening(task);
                transferManager.TryStartTasksAsync();
                contextMenuExplorer.dataGrid.SelectedItems.Clear();
                contextMenuExplorer.justCopyCutFiles_time_forTimeOut = DateTime.Now;
            }
        }
        private async void ContextMenu_ActionDelete(EMItemModel clickedBtn)
        {
            Explorer_Delete(
                explorerContextMenu.subDirsNFiles.ToArray(),
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));

            if (contextMenuCtrl is TreeView && explorerContextMenu.subDirsNFiles.Count == 1)
            {
                // 此时有可能是树表中右击删除的项目，检查并手动触发删除事件
                string deletedFile = explorerContextMenu.subDirsNFiles[0];
                string deletedFileDir = Path.GetDirectoryName(deletedFile);
                if (deletedFileDir != contextMenuExplorer.GetTreeNodeURL())
                {
                    int waitCount = 20;
                    while (waitCount-- > 0 && File.Exists(deletedFile) || Directory.Exists(deletedFile))
                    {
                        await Task.Delay(50);
                    }
                    if (waitCount > 0)
                        ReloadDirComplete(this, false, deletedFile, null, null);
                }
            }
        }
        private void ContextMenu_ActionRename(EMItemModel clickedBtn)
        {
            if (contextMenuCtrl is DataGrid)
                contextMenuExplorer.DataGridTryStartRename();
            else if (contextMenuCtrl is TreeView)
                contextMenuExplorer.TreeViewTryStartRename(true);
        }
        private void ContextMenu_ActionOpenDir(string dirFullName)
        {
            contextMenuExplorer.NavigateTo(dirFullName);
        }

        public void Rename(string[] sources, string newName)
        {
            if (sources.Length > 0)
            {
                if (sources[0].Length == 3)
                {
                    // rename disk label
                    try
                    {
                        DriveInfo di = new DriveInfo(sources[0]);
                        di.VolumeLabel = newName;
                    }
                    catch (Exception err)
                    {
                        LogError(err);
                        MessageBox.Show(err.Message, GetLangTx("txLog_cantChangeDriveLabel"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // rename files
                    string[] newFullNames = new string[sources.Length];
                    if (sources.Length > 1)
                    {
                        int lastDotIdx = newName.LastIndexOf(".");
                        if (lastDotIdx > 0)
                        {
                            newName = newName.Substring(0, lastDotIdx);
                        }
                        string curSource, curFileName, curDir;
                        for (int i = 0, iv = sources.Length; i < iv; ++i)
                        {
                            curSource = sources[i];
                            curDir = Path.GetDirectoryName(curSource);
                            curFileName = Path.GetFileName(curSource);
                            if (curFileName.LastIndexOf(".") < curFileName.Length - 1)
                            {
                                newFullNames[i] = Path.Combine(curDir, newName + Path.GetExtension(curFileName));
                            }
                            else
                            {
                                newFullNames[i] = Path.Combine(curDir, newName);
                            }
                        }
                    }
                    else
                    {
                        newFullNames[0] = Path.Combine(Path.GetDirectoryName(sources[0]), newName);
                    }
                    if (newFullNames.Length > 1)
                    {
                        string curNewFile, ext, fullNamePre, testFileName;
                        for (int n = 1, i = 0, iv = newFullNames.Length; i < iv; ++i)
                        {
                            curNewFile = newFullNames[i];
                            ext = Path.GetExtension(curNewFile);
                            fullNamePre = curNewFile.Substring(0, curNewFile.Length - ext.Length);

                            do
                            {
                                testFileName = $"{fullNamePre} ({n++}){ext}";
                            }
                            while (File.Exists(testFileName) || Directory.Exists(testFileName));
                            newFullNames[i] = testFileName;
                        }
                    }
                    TransferManager.TransferTask task = transferManager.TransferTaskAdd(
                        TransferManager.TaskTypes.Move,
                        sources,
                        newFullNames,
                        setting.sameNameDirHandleType,
                        setting.sameNameFileHandleType,
                        null);
                    TransferTaskListening(task);
                    transferManager.TryStartTasksAsync();
                }
            }
        }
        private void ContextMenu_ActionNewFileOrDirToCreate(bool isFileOrDir, string newFileOrDir)
        {
            contextMenuExplorer.newFileToRename = newFileOrDir;
        }
        private void ContextMenu_ActionManageOpenWith_setLanguage(WindowMenuSettings msWin)
        {
            settingsLanguage.TrySetLang(msWin.Resources, false);
        }

        #endregion



        #region handle files drop

        public async void Explorer_HandleFileDrop(string[] filesSource, DragDropEffects action, string tarDir)
        {
            TransferManager.TaskTypes? taskType = null;
            switch (action)
            {
                case DragDropEffects.Copy:
                    taskType = TransferManager.TaskTypes.Copy;
                    break;
                case DragDropEffects.Move:
                    taskType = TransferManager.TaskTypes.Move;
                    break;
                case DragDropEffects.Link:
                    taskType = TransferManager.TaskTypes.CreateLink;
                    break;
            }
            if (taskType != null)
            {
                TransferManager.TaskTypes tt = (TransferManager.TaskTypes)taskType;
                string[] toFiles = new string[filesSource.Length];
                for (int i = 0, iv = filesSource.Length; i < iv; ++i)
                {
                    toFiles[i] = Path.Combine(tarDir, Path.GetFileName(filesSource[i]));
                }

                TransferManager.SameNameDirHandleTypes dirHT = setting.sameNameDirHandleType;
                TransferManager.SameNameFileHandleTypes fileHT = setting.sameNameFileHandleType;
                // 2023 1121 如果源和目标是同一目录，则直接用新命名；
                if (tt == TransferManager.TaskTypes.Copy
                    && Path.GetDirectoryName(filesSource[0]) == Path.GetDirectoryName(toFiles[0]))
                {
                    dirHT = TransferManager.SameNameDirHandleTypes.Rename;
                    fileHT = TransferManager.SameNameFileHandleTypes.Rename;
                }

                TransferManager.TransferTask task = transferManager.TransferTaskAdd(
                    tt,
                    filesSource,
                    toFiles,
                    dirHT,
                    fileHT,
                    null);
                TransferTaskListening(task);
                await transferManager.TryStartTasksAsync();

                switch (tt)
                {
                    case TransferManager.TaskTypes.Copy:
                        // 更新目标目录
                        WaitReload(toFiles[0], true);
                        break;
                    case TransferManager.TaskTypes.Move:
                        // 更新源目录
                        WaitReload(filesSource[0], false);
                        // 更新目标目录
                        WaitReload(toFiles[0], true);
                        break;
                    case TransferManager.TaskTypes.Delete:
                        // 更新源目录
                        WaitReload(filesSource[0], false);
                        break;
                }
                async void WaitReload(string file, bool willAddOrRemove)
                {
                    int waitCount = 20;
                    if (willAddOrRemove)
                    {
                        while (waitCount-- > 0 && !(File.Exists(file) || Directory.Exists(file)))
                        {
                            await Task.Delay(50);
                        }
                    }
                    else
                    {
                        while (waitCount-- > 0 && (File.Exists(file) || Directory.Exists(file)))
                        {
                            await Task.Delay(50);
                        }
                    }
                    if (waitCount > 0)
                    {
                        ReloadDirAsync(Path.GetDirectoryName(file));
                    }
                }
            }
        }
        #endregion


        internal void SettingFLow()
        {
            WindowSetting wndSetting = new WindowSetting();
            if (wndSetting.ShowDialog() == true)
            {
                // start or stop file-system-watcher, or do nothing
                if (setting.isFileSystemWatcherEnabled)
                {
                    FsWatchingStartAll();
                }
                else
                {
                    FsWatchingStopAll();
                }
            }
        }


        #region log

        #region filtered log
        internal void LogGeneral(string msg)
        {
            if (setting.isLogEnabled && setting.logFlagGeneral)
                mainWindow.Dispatcher.Invoke(() =>
                { logger.LogInfo(GetLangTx("txLog_general"), msg); });
        }

        internal void LogError(Exception err)
        {
            if (setting.isLogEnabled && setting.logFlagError)
                mainWindow.Dispatcher.Invoke(() =>
                { logger.LogError(err); });
        }

        internal void LogStoreageAccess(string loadingDir)
        {
            if (setting.isLogEnabled && setting.logFlagStorageAccess)
                mainWindow.Dispatcher.Invoke(() =>
                { logger.LogInfo(GetLangTx("txLog_loadDir"), loadingDir); });
        }

        internal void LogDrivePlug(DriveInfoShadow drive, bool isPlugIn)
        {
            if (setting.isLogEnabled && setting.logFlagStoragePlugInOut)
                mainWindow.Dispatcher.Invoke(() =>
                {
                    logger.LogInfo(isPlugIn
                        ? GetLangTx("txLog_plugIn")
                        : GetLangTx("txLog_plugOut"),
                    drive.ToString());
                });
        }

        internal void LogTransferTask(TransferManager.TransferTask task)
        {
            if (setting.isLogEnabled && setting.logFlagTransTask)
            {
                StringBuilder strBdr = new StringBuilder();

                strBdr.Append(GetLangTx("txLog_idNType", task.Id.ToString(), task.TaskType.ToString()));
                if (task.SourceIOs != null && task.SourceIOs.Length > 0)
                {
                    strBdr.AppendLine(GetLangTx("txLog_sources"));
                    foreach (string s in task.SourceIOs)
                        strBdr.AppendLine(s);
                }
                if (task.TargetIOs != null && task.TargetIOs.Length > 0)
                {
                    strBdr.AppendLine(GetLangTx("txLog_targets"));
                    foreach (string s in task.TargetIOs)
                        strBdr.AppendLine(s);
                }

                mainWindow.Dispatcher.Invoke(() =>
                { logger.LogInfo(GetLangTx("txLog_transferAdded"), strBdr.ToString()); });
            }
        }

        internal void LogTransferDetail(TransferManager.TransferTask task, string detail)
        {
            if (setting.isLogEnabled && setting.logFlagTransDetails)
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    logger.LogInfo(GetLangTx("txLog_transferDetails"), GetLangTx("txLog_taskIdNType", task.Id.ToString(), detail));
                });
            }
        }

        internal void LogTransferError(TransferManager.TransferTask task, Exception err)
        {
            if (setting.isLogEnabled && setting.logFlagTransError)
            {
                string title = GetLangTx("txLog_transferDetails");
                string msg = GetLangTx("txLog_taskIdNType", task.Id.ToString(), err.ToString());
                mainWindow.Dispatcher.Invoke(() =>
                { logger.Log(Logger.InfoLevels.Error, ref title, ref msg, err); });
            }
        }
        #endregion


        private WindowHandleRest windowHandleRest = null;
        private object windowHandleRest_waiting_locker = false;
        public void TransferTaskListening(TransferManager.TransferTask task)
        {
            // 2022-10-12 新建任务后，将窗体光标改为启动中；开始读取时再恢复；
            mainWindow.Cursor = Cursors.AppStarting;


            // 2022-10-09
            // refresh target path first
            if (task.TargetIOs != null && task.TargetIOs.Length > 0)
            {
                string tPath = task.TargetIOs[0];
                if (Directory.Exists(tPath))
                {
                    ReloadDirAsync(tPath);
                }
            }


            // to ui, for user view n control
            mainWindow.ListeningTransferTask(task);

            #region write log, reload after complete/cancel
            // 记录日志，如果需要；
            LogTransferTask(task);
            task.ExceptionRaised += (s, e) =>
            {
                mainWindow.Dispatcher.Invoke(() =>
                {
                    LogTransferError(s, e);
                    //LogError(e);
                });
            };
            task.EventRaised += (s1, e1) =>
             {
                 mainWindow.Dispatcher.Invoke(() =>
                 {
                     switch (e1)
                     {
                         case TransferManager.TransferTask.Events.Scanning:
                             LogTransferDetail(s1, GetLangTx("txLog_scanningFiles"));
                             break;
                         case TransferManager.TransferTask.Events.Start:
                             mainWindow.Cursor = Cursors.Arrow;
                             LogTransferDetail(s1, GetLangTx("txLog_transStart"));
                             break;
                         case TransferManager.TransferTask.Events.Paused:
                             LogTransferDetail(s1, GetLangTx("txLog_transPaused"));
                             break;
                         case TransferManager.TransferTask.Events.Resumed:
                             LogTransferDetail(s1, GetLangTx("txLog_transResumed"));
                             break;
                         case TransferManager.TransferTask.Events.Completed:
                             LogTransferDetail(s1, GetLangTx("txLog_transComplete"));
                             ReloadSourceNTarget(s1);
                             break;
                         case TransferManager.TransferTask.Events.Canceled:
                             LogTransferDetail(s1, GetLangTx("txLog_transCanceled"));
                             ReloadSourceNTarget(s1);
                             break;
                     }
                 });
             };
            void ReloadSourceNTarget(TransferManager.TransferTask task)
            {
                string sDir = Path.GetDirectoryName(task.SourceIOs[0]);
                ReloadDirAsync(sDir);
                if (task.TaskType == TransferManager.TaskTypes.Move)
                {
                    string tDir = Path.GetDirectoryName(task.TargetIOs[0]);
                    if (sDir != tDir)
                        ReloadDirAsync(tDir);
                }
            }
            //task.Progressed += (s2, e2) =>
            //{
            //};
            task.HandleRestNeeded += async (s3, e3) =>
            {
                await Task.Run(() =>
                {
                    while (true)
                    {
                        lock (windowHandleRest_waiting_locker)
                        {
                            if (!(bool)windowHandleRest_waiting_locker)
                            {
                                windowHandleRest_waiting_locker = true;
                                break;
                            }
                        }
                        Task.Delay(100).Wait();
                    }
                });


                mainWindow.Dispatcher.Invoke(() =>
                {
                    if (setting.isLogEnabled && setting.logFlagTransDetails)
                    {
                        StringBuilder strBdr = new StringBuilder();
                        strBdr.AppendLine(GetLangTx("txLog_needUserAttention"));   // "一些项目需要用户处理："
                        foreach (TransferManager.TransferTask.RestFilesData rest in e3)
                        {
                            strBdr.Append(GetLangTx("txLog_handleSource", s3.TaskType.ToString(), rest.io.fullName));

                            if (rest.deleteSource) strBdr.Append(GetLangTx("txLog_handleDeleteSource", rest.deleteSource.ToString()));
                            if (rest.sur != null) strBdr.Append(GetLangTx("txLog_handleSur", rest.sur));
                            if (rest.tar != null) strBdr.Append(GetLangTx("txLog_handleTar", rest.tar));
                            if (rest.targetFullName != null) strBdr.Append(GetLangTx("txLog_handleTargetFullName", rest.targetFullName));
                            if (rest.newName != null) strBdr.Append(GetLangTx("txLog_handleNewName", rest.newName));
                            if (rest.err != null) strBdr.Append(GetLangTx("txLog_handleError", rest.err.ToString()));
                            strBdr.AppendLine();
                        }
                        LogTransferDetail(s3, strBdr.ToString());
                    }

                    windowHandleRest = new WindowHandleRest();
                    windowHandleRest.Init(s3, e3);
                    windowHandleRest.ShowDialog();
                    windowHandleRest = null;

                    s3.ResumeAsync();

                    lock (windowHandleRest_waiting_locker)
                    {
                        windowHandleRest_waiting_locker = false;
                    }
                });
            };
            #endregion
        }


        #endregion


        #region language

        public SettingsLanguage settingsLanguage = SettingsLanguage.GetInstance();

        public string GetLangTx(string key, params string[] words)
        {
            if (words != null && words.Length > 0)
            {
                return SettingsLanguage.GetTx(Application.Current.Resources[key].ToString(), words);
            }
            else
            {
                return Application.Current.Resources[key].ToString();
            }
        }
        public void TrySetLang(string langName)
        {
            settingsLanguage.TrySetLang(Application.Current.Resources, langName);
        }


        #endregion


        internal void BeforeExiting()
        {
            LogGeneral(GetLangTx("txLog_appSaveSettingBeforeExiting"));
            // save layouts          
            setting.curLayout = GetLayoutSettingFromCurrent();
            setting.Save();
        }
        public void Dispose()
        {
            driveDector?.Dispose();
        }


        #region call from explorer
        internal void Explorer_Paste(string intoDir)
        {
            if (string.IsNullOrWhiteSpace(intoDir))
                return;

            string[] inFiles = Utilities.ClipBoard.GetFileDrops(out DragDropEffects copyOrMove);
            if (inFiles != null && inFiles.Length > 0)
            {
                Utilities.ClipBoard.Clear();
                TransferManager.TaskTypes taskType = TransferManager.TaskTypes.Move;
                bool inSameDir = intoDir == Path.GetDirectoryName(inFiles[0]);
                if (copyOrMove == DragDropEffects.Copy)
                    taskType = TransferManager.TaskTypes.Copy;

                if (inSameDir && taskType == TransferManager.TaskTypes.Move)
                    return;

                string[] toFiles = new string[inFiles.Length];

                // 2022 1209 如果源和目标处于同一目录，则自动使用新名称
                string tarFullName;
                List<string> protentialList = new List<string>();
                string newFullName;
                for (int i = 0, iv = inFiles.Length; i < iv; ++i)
                {
                    tarFullName = Path.Combine(intoDir, Path.GetFileName(inFiles[i]));
                    if (inSameDir)
                    {
                        newFullName = Utilities.CSharpWapper.AutoNewFullName(tarFullName, protentialList);
                        toFiles[i] = newFullName;
                        protentialList.Add(newFullName);
                    }
                    else
                    {
                        toFiles[i] = tarFullName;
                    }
                }

                TransferManager.TransferTask task;
                if (inSameDir)
                {
                    task = transferManager.TransferTaskAdd(
                        taskType,
                        inFiles,
                        toFiles,
                        TransferManager.SameNameDirHandleTypes.Rename,
                        TransferManager.SameNameFileHandleTypes.Rename,
                        null);
                }
                else
                {
                    task = transferManager.TransferTaskAdd(
                        taskType,
                        inFiles,
                        toFiles,
                        setting.sameNameDirHandleType,
                        setting.sameNameFileHandleType,
                        null);
                }
                TransferTaskListening(task);
                transferManager.TryStartTasksAsync();
            }
        }

        internal void Explorer_Delete(IList dataGridSelectedItems, bool isPermanentDelete = false)
        {
            if (dataGridSelectedItems == null || dataGridSelectedItems.Count <= 0)
                return;

            object testItem = dataGridSelectedItems[0];
            if (testItem is VM.DataGridRowModle_dirNFile)
            {
                List<string> inFiles = new List<string>();
                foreach (VM.DataGridRowModle_dirNFile dgFile in dataGridSelectedItems)
                {
                    inFiles.Add(dgFile.iois.fullName);
                }
                Explorer_Delete(inFiles.ToArray(), isPermanentDelete);
            }
        }

        internal void Explorer_Delete(VM.TreeViewModelDir tnDir, bool isPermanentDelete = false)
        {
            Explorer_Delete(new string[] { tnDir.dirInfo.fullName }, isPermanentDelete);
        }
        private void Explorer_Delete(string[] files, bool isPermanentDelete = false)
        {
            if (isPermanentDelete)
            {
                if (MessageBox.Show(
                        GetLangTx("txMsg_questionShiftDelete"),
                        GetLangTx("txMsg_warning"),
                        MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.No)
                {
                    return;
                }
            }
            //else if (files != null && files.Length > 0 && files[0].StartsWith("\\\\"))
            //{
            //    if (MessageBox.Show(
            //            GetLangTx("txMsg_questionDeleteRemote"),
            //            GetLangTx("txMsg_warning"),
            //            MessageBoxButton.YesNo, MessageBoxImage.Warning)
            //        == MessageBoxResult.No)
            //    {
            //        return;
            //    }
            //}
            else if (Path.GetFileName(Path.GetDirectoryName(files[0])) == Utilities.CSharpWapper.RecycleBinName)
            {
                if (MessageBox.Show(
                        GetLangTx("txMsg_questionRecycleBinItemsDelete"),
                        GetLangTx("txMsg_warning"),
                        MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.No)
                {
                    return;
                }
                isPermanentDelete = true;
            }

            TransferManager.TransferTask task = transferManager.TransferTaskAdd(
                TransferManager.TaskTypes.Delete,
                files,
                null,
                setting.sameNameDirHandleType,
                setting.sameNameFileHandleType,
                isPermanentDelete);
            TransferTaskListening(task);
            transferManager.TryStartTasksAsync();
        }

        private void TransferManager_RecycleBinGenerated(TransferManager tMgr, string binDir)
        {
            RecycleBinItemFound?.Invoke(this);
        }
        internal void CheckRecycleBin()
        {
            bool trashFound = false;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            string cycleDirName = Utilities.CSharpWapper.RecycleBinName;
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady
                    && drive.DriveType != DriveType.CDRom
                    && drive.DriveType != DriveType.Network)
                {
                    if (Directory.Exists(Path.Combine(drive.Name, cycleDirName)))
                    {
                        trashFound = true;
                        break;
                    }
                }
            }
            if (trashFound)
            {
                RecycleBinItemFound?.Invoke(this);
            }
        }
        internal void ClearRecycleBin()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            string cycleDirName = Utilities.CSharpWapper.RecycleBinName;
            string binDir;
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady
                    && drive.DriveType != DriveType.CDRom
                    && drive.DriveType != DriveType.Network)
                {
                    binDir = Path.Combine(drive.Name, cycleDirName);
                    if (Directory.Exists(binDir))
                    {
                        try
                        {
                            Directory.Delete(binDir, true);
                            ReloadDirComplete?.Invoke(this, false, binDir, null, null);
                        }
                        catch (Exception err)
                        {
                            LogError(err);
                        }
                    }
                }
            }
        }

        #endregion



        #region 全局变量

        internal bool isDragingFile;
        internal bool isPreventDropOnce = false;




        #endregion



    }
}
